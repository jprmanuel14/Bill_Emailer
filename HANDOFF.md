# Handoff — BillColl_Mail / BillColl_Main

Continuation notes. Read this first before touching the code.

Last updated: 2026-10-02, after the legacy-directory and stale-worktree cleanup.
Runtime verified against: PostgreSQL `BCAT` on localhost:5432, Kestrel on localhost:5000.
**Everything below is now running clean — see section 0 for how to start it.**

**Most recent change:** `BCAutomation/`, `BCMailService/`, `Setup/` and `.kilo/worktrees/`
were deleted (~522 MB). They were all build output and merged agent worktrees — **no
source and no unmerged work was lost**, and the ambiguous duplicate-controller problem
described in the old P3 entry turned out never to have existed. Read section 3 before
section 10; both were corrected, so the old "delete the legacy project" advice is gone.

**Prior change:** section 9 — the role check moved off `SettingsController.Index`
and onto a controller-wide `[AdminOnly]` filter, so all **22 routed data endpoints**
(not just the page) are now gated. The file did not compile when it was written; two
compile errors were found and fixed, and the whole thing was then verified at runtime.
**Read section 9 before section 10** — it records that the denial is a **302 redirect to
`/Account/AccessDenied`, not a 403**, which contradicts section 8.2.

**Prior changes:** section 8 — three items actioned: `SQLTransHelper.cs` deleted,
`/Settings` given a server-side role check (now superseded by section 9),
`client.js` dead branch removed. Section 2.7 — the Utilities nav item was deleting
itself from the sidebar; read that if the navigation misbehaves.

---

## 0. Start it

```powershell
.\run-local.ps1
```

That is the whole thing. It sets `ASPNETCORE_ENVIRONMENT=Development` (required, or
user-secrets are not loaded and startup validation fails), then starts the web app on
<http://localhost:5000>. The `BillingMail.Plugin` runs as a hosted background service inside
the web app, so this starts **both**.

| | |
|---|---|
| URL | <http://localhost:5000> |
| Login | `admin@pwc.com` — local login, no password, SSO off in Development (any username is accepted; see section 6) |
| Emails | written to `captured_emails\*.eml` rather than sent (see below) |
| Logs | `BillColl_Main\logs\` |

To run the plugin on its own instead:

```powershell
.\run-local.ps1 -Plugin          # one batch, then exits
```

**If it will not start, read this.** The overwhelmingly most common cause is a previous
instance still holding port 5000, which surfaces only as a bare
`System.IO.IOException: Failed to bind to address http://127.0.0.1:5000: address already in use`
buried in a stack trace. `run-local.ps1` now checks the port first and prints the offending
PID, but if you started the app some other way:

```powershell
Stop-Process -Id <pid> -Force      # the PID is in the "already in use" message
```

**"The application is not running" when it demonstrably is.** `run-local.ps1` is often
launched as an agent-managed background process, and the default lifetime is *session* — the
app dies silently the moment that session ends, with no error and nothing left in the log.
Check for the listener before assuming a crash:

```powershell
Get-NetTCPConnection -State Listen -LocalPort 5000
Invoke-WebRequest http://localhost:5000/Account/Index -UseBasicParsing
```

If the listener is there and returns 200, the app is fine and the process simply needs a
persistent lifetime so it outlives the session that started it.

Also note the batch worker log lines contain `Errors=0`, which a naive search for `ERR`
matches. Those are not failures — there were no errors in the log on 2026-10-01.

`Development` is mandatory. With `--no-launch-profile` and no environment variable you get
`Production`, user-secrets are not loaded, and startup aborts with
`Database configuration is incomplete`.

**Why emails are captured instead of sent.** The real SMTP relay `10.139.108.22:25` is not
reachable from this machine, so every send times out after ~20s and logs a `SocketException`.
`appsettings.Development.json` sets `BillingMail:EmailCapturePath`, which makes the dispatcher
write each message to a `.eml` file and report success. Batch summary is then
`Pending=4, Sent=4, Errors=0` and the log is clean. Remove that key to attempt real SMTP.

---

## 1. The topology you need to remember

The application now spans **two different database engines**. This is the single most
important thing to keep in mind, and it is why a single connection string is not enough.

| Concern | Where it lives | Provider |
|---|---|---|
| BCAT (`Group`, `Contacts`, `User_Role`, `Engagement_Secretary`, `Log_Admin`, `Secretary_Logs`, `EngagementTeams`, `tbl_Log_Clients`, …) | primary database | **switchable** — PostgreSQL today |
| PH Report Databank (`tblEntityMapping` → LoS, `tblGroupCode`, `tblGroupTypeCode`, `tblBill`) | linked server | **always SQL Server** |
| FinApps (`FinAppsDM`) | linked server | **always SQL Server** |
| HRIS (`viewEmployeeProfile`) | linked server | **always SQL Server** |

The legacy `BCAutomation` project linked all three (`[BCAT]`, `[PH_MLAAPP002S]`, `[PH_MLAAPP004S]`).
BCAT has since moved to PostgreSQL; the other two have not.

**Consequence:** a SQL Server linked server cannot reach PostgreSQL. When the primary is
PostgreSQL, BCAT tables must be referenced **locally** (`dbo."User_Role"`), not through
`[BCAT].[dbo].[User_Role]`. `DbConnectionFactory.BcatTable()` encapsulates this — use it.

Linked server names differ per environment (STAGE renames them), so they are configuration
values, not hardcoded SQL identifiers.

---

## 2. What is already done (do not redo)

### 2.1 Duplicate route fix
`SettingsController.Get_GroupAndLOS` and `ClientsController.Get_GroupAndLOS` both declared
`[Route("groupcodelist")]`, so every Clients page load returned
`AmbiguousMatchException` (HTTP 500). The duplicate action was deleted; `/groupcodelist` is
now served solely by `ClientsController`, delegating to `IGroup`.

### 2.2 `GroupSQLRepository.GetGroupCodeList` mapping
Was mapping `Group_Id` / `Group_Type` / `Group_Code_WO_Desc` all to the same value.
Now sources `dbo."Group"` where `Code` is the real group code (`01001`, `01002`, `02001`) —
this is the value persisted in `Contacts.GroupType`, and equals the legacy
`chOfficeCode + chGroupCode`.

### 2.3 `/losbygroupcodelist` route
The route did not exist at all in this project (only in legacy `BCAutomation`), so
`client.js` was erroring. It now exists and queries the shared SQL Server layer. Returns
`[]` when `Connections:SharedSql:ConnectionString` is not configured.

### 2.4 Foundation: two-connection model
New files:

- `Services/ConnectionOptions.cs` — `ConnectionsOptions`, `PrimaryConnectionOptions`,
  `SharedSqlConnectionOptions`, `LinkedServerOptions`
- `Services/SharedSqlConnectionFactory.cs` — always-SQL Server connection for the shared estate
- `Services/ISharedSql.cs` + `Services/SharedSqlRepository.cs` — first consumer is
  `GetLOSByGroupCode` (`tblEntityMapping`)
- `Services/DbValues.cs` — `CoerceId`, see trap #1 below

Modified:

- `Services/DbConnectionFactory.cs` — reads `Connections:Primary:*` first, falls back to the
  legacy `BillingMail:*` / `myAppDBConnection` keys. Adds `BcatTable()`, `DatabankTable()`,
  `FinAppsTable()`, `HrisTable()` and `LinkedServers`, so no SQL embeds a server name.
- `Startup.cs` — registers `SharedSqlConnectionFactory`, `IUserRole`,
  `IEngagementSecretary`, `ISharedSql`; provider + connection string now prefer
  `Connections:Primary`; adds `ValidateDatabaseConfiguration` fail-fast check.
- `appsettings.json` — new `Connections` section; secrets removed.
- `appsettings.Production.json` — new blank template with SSO enabled.
- `BillingMail.Plugin/BillingMailServiceExtensions.cs` — applies `Connections:Primary:*` over
  `BillingMail:*` so the worker and the web app cannot diverge.

All fallbacks are in place, so existing `BillingMail:*` config keeps working.

### 2.5 Ten BCAT-only Settings actions ported
New: `Services/IUserRole.cs` + `UserRoleRepository.cs`,
`Services/IEngagementSecretary.cs` + `EngagementSecretaryRepository.cs`.

Ported off `SQLTrans` and verified end-to-end (add / read / view / update / delete):
`add_the_role`, `delete_role`, `view_role`, `update_role`, `load_role`,
`add_engagement_secretary`, `delete_engagement_secretary`,
`validate_engagement_secretary`, `validate_adding_engagement_secretary`,
`update_engagement_secretary`.

Dual writes (data + audit log) are now in an explicit DB transaction instead of one
multi-statement `ExecuteParamQuery`.

### 2.6 Batch 2: the ten remaining `SQLTrans` actions ported (2026-10-01)

`SettingsController` no longer contains a single `SQLTrans` / `SqlParameter` reference. All
ten actions plus the three private helpers were moved behind two new services.

New:

- `Services/IEmployeeDirectory.cs` + `EmployeeDirectoryRepository.cs` — the HRIS lookups that
  replaced the three private helpers (`viewEmployeeProfile`). HRIS stayed on SQL Server, so
  this runs over the shared SQL Server connection. **When that connection is not configured it
  reports `IsConfigured == false` and degrades quietly** — `GetNameByEmployeeCode` returns the
  code it was given, `GetStaff` returns an empty list — rather than throwing. That is why
  `/stafflist*` returns `[]` locally instead of HTTP 500.
- `Services/ISettingsRepository.cs` + `SettingsRepository.cs` — `admin_logs`,
  `engagement_secretary_logs`, `load_engagement_secretary`, `view_engagement_secretary`,
  `view_reports_new`, `download_admin_logs`, `download_secretary_logs`. Reads come from the
  **primary** database, so they work on both providers. The databank-only group description
  (`tblGroupCode` + `tblGroupTypeCode`) is gone; descriptions now resolve through `IGroup` on
  `dbo."Group"`. Date formatting moved out of SQL into C#, so `FORMAT` is no longer a
  SQL-Server-only dependency.
- `SettingsController.stafflist_for_engagement_users` — `client.js:2821` called a route that
  did not exist in this project. Added; same data as `/stafflist`.

Also fixed:

- **`BillingMail` config keys did not bind.** `appsettings.json` used `Mail`, `DisplayName`,
  `Password`, `Host`, `Port`, but `BillingMailOptions` binds `SenderEmail`, `SenderName`,
  `SmtpHost`, `SmtpPort`. Every deployer-set SMTP value was silently ignored and the
  hardcoded class defaults won. Renamed in `appsettings.json` and
  `appsettings.Production.json`.
- `DbConnectionFactory.AddParameters` only understands `IDbDataParameter`, a list of them, or
  an anonymous object. Passing a `Dictionary<string, object>` binds **nothing** and the query
  dies with `column "clientcode" does not exist`. Use an anonymous object.

`Services/SQLTransHelper.cs` was completely unreferenced after this port, so it has now been
**deleted** (see section 8.1). It was hardcoded to `System.Data.SqlClient` and could only ever
read an empty `myAppDBConnection`, so it could not work.

### 2.7 Navigation: the Utilities item deleted itself from the sidebar (2026-10-01)

Reported as *"when I click Utilities the navigation menu disappears."* Four independent
defects, all client-side role gating that never agreed with the server.

| # | Defect | Effect |
|---|---|---|
| 1 | `HomeController.Index` never set `ViewBag.ModelJson`, but `Home/Index.cshtml` rendered the input anyway → `value=''` | `JSON.parse('')` threw at `site.js:136`, **aborting the rest of the file**. Nav highlight never applied, and `openUtilities()` threw on the same line so the click was dead |
| 2 | `Clients/Index.cshtml` had no `#jsonData` at all, and `localStorage("user")` is never written (the form posts straight to `Account.LoginManual`; `site.js`'s `login()` called `/sign_in_new`, which **does not exist** in this project) | `site.js:102` took the `x == null` branch and added `d-none` to `#util-nav` — **the Utilities item was deleted from the menu**, with no navigation |
| 3 | `dashboard.js:28-64` had a third copy of the same hide, plus `JSON.parse(localStorage.getItem("user"))` which threw on every Home load | Same, from a different file |
| 4 | `utilities.js:142-179` had a fourth copy, which also forced `window.location = '/Home'` | Admin landing on `/Settings` was bounced back to `/Home` with the nav item hidden |

The role itself resolved to `"0"` for everyone: `SettingsController.Index` only overrode the
default when HRIS returned an employee code, and HRIS is unreachable locally.

#### What changed

**One source of truth.** New `Services/IUserContext.cs`, `CurrentUser.cs`, `UserContext.cs`,
registered at `Startup.cs:176`. `_Layout.cshtml` injects it (`@inject`), so role resolution
lives in exactly one place and the nav cannot drift from the page.

**`#jsonData` emitted once, by the layout** (`Views/Shared/_Layout.cshtml:29`). Deleted the
two per-view copies in `Home/Index.cshtml` and `Settings/Index.cshtml`. Also changed
`value='@Html.Raw(json)'` → `value="@json"`: `Html.Raw` inside a single-quoted attribute was
an XSS vector if a display name contained an apostrophe.

**Sidebar is server-rendered.** The `<li id="util-nav">` is emitted only when
`currentUser.IsAdmin`, and the anchor is now a real link
(`asp-controller="Settings" asp-action="Index"` → `href="/Settings"`). Clicking Utilities
navigates with **no JavaScript at all**.

**JS deduplicated.** `site.js` now has one null-safe `readUserContext()` + `isAdmin()`, and a
`validateAccess()` that only guards navigation. Dead `login()` and `openUtilities()` deleted.
The `util-nav` hide was removed from `utilities.js` and `dashboard.js` entirely. This also
fixed a `user_Role` vs `User_Role` casing mismatch between two copies of the same block.

Every `JSON.parse` of `#jsonData` is now guarded and returns `null` on malformed input, so a
missing payload can no longer abort a script.

---

## 3. What is still broken — in priority order

### ~~P1 — `/Settings` has no server-side role check~~ RESOLVED (2026-10-01)

Was: `SettingsController` carried `[Authorize]` and nothing else, so any authenticated user
could reach `/Settings` by typing the URL; only the sidebar link was gated.

Fixed in two steps. First an action-level check on `Index` (section 8.2) — which closed
only the page. Then, in the current pass, a controller-wide `[AdminOnly]` filter
(section 9) which closes **every** action on the controller, including the role-granting
writes. The nav gate is not the only thing deciding what is reachable, and that is now
verified rather than asserted. Note the denial is a 302 to `/Account/AccessDenied`, not a
403 — see section 9.3.

### ~~P1 — The dev admin grant is a manual database insert, per environment~~ RESOLVED (dev-only convenience)

`admin@pwc.com` is only an admin because of this row in `dbo."User_Role"`:

```
 Id |  E_Code  |   EmployeeName   | User_Role
----+----------+------------------+-----------
   7 | DEVADMIN | admin@pwc.com    | 1
```

**This grant is development-only and is scoped in code to be inert everywhere else.**
`UserContext.DevFallbackEnabled` requires **both** conditions:

| Condition | Source | When it is off |
|---|---|---|
| `IHostEnvironment.IsDevelopment()` | `ASPNETCORE_ENVIRONMENT` | any `Production`/`Staging` host, even if SSO was misconfigured |
| `!AuthSettings:EnableSso` | `appsettings.*.json` or `AuthSettings__EnableSso` | SSO switched on |

Both are **configuration**, so moving between local dev and a deployed environment never
needs a code change — the fallback simply stops applying. Under SSO the only path that runs
is the original HRIS employee-code lookup, so login behaves exactly as it did before.

When the fallback is active, a one-time warning is logged so it is never a silent bypass:

```
DEV role fallback is ACTIVE (Environment=Development, EnableSso=False). User_Role rows are
matched on signed-in name as well as employee code. This must never be enabled outside
local development.
```

Verified across all three configurations on 2026-10-01 — see section 7.

**This is now load-bearing.** Because `/Settings` is server-enforced (section 9), a
new environment with no `DEVADMIN` row will correctly deny `/Settings` to everyone —
including every endpoint on the controller, not just the page. See item 8 in section 10.

**Consequences to remember:**

- The grant is **data, not code**. It does not carry to STAGE or PROD. Under SSO there, the
  HRIS path is authoritative and the `DEVADMIN` row is ignored entirely — which is correct.
- The email is in `EmployeeName`, not `E_Code`, because `E_Code` is `varchar(10)` and
  `admin@pwc.com` does not fit (`value too long for type character varying(10)`).
- `DEVADMIN` is a placeholder for the same reason. If you add a second dev account you will
  hit the same 10-character wall. Cleanly fixing this means widening `E_Code` via a migration,
  or adding a dedicated `Email` column — do not paper over it by shortening the placeholder.
- In **Development**, the name fallback means anyone who can get an entry into `User_Role`
  can grant themselves admin. That table is admin-editable from the Utilities page, so treat
  write access to it as equivalent to admin. Outside Development the fallback does not run.

### ~~P0 — Ten actions still on `SQLTrans`~~ RESOLVED

See section 2.6. `SettingsController` has no `SQLTrans` reference left. The file was deleted
in section 8.1.


### P1 — ~~Config footgun~~ DONE

`BillingMail.Plugin` used to read `BillingMail:*` directly while the web app preferred
`Connections:Primary:*`, so the two could diverge. Resolved: the plugin now applies
`Connections:Primary:*` over its bound `BillingMail:*` values, making `Connections:Primary`
authoritative for both. See section 4.4.

### P1 — ~~Linked server names hardcoded~~ DONE

All 24 hardcoded references are now resolved from configuration via
`DbConnectionFactory.BcatTable()` / `.DatabankTable()` / `.FinAppsTable()` / `.HrisTable()`.

| File | Was | Now |
|---|---|---|
| `Services/BillingMailStatusSQLRepository.cs` | 18 | config-driven |
| `Services/ExceptionSQLRepository.cs` | 3 | config-driven |
| `Services/LogsSQLRepository.cs` | 2 | config-driven |
| `Services/GroupSQLRepository.cs` | 1 | config-driven |

`[dbo].[tblScheduledMail_v2]` and `[dbo].[tbBCATData]` in the SQL Server branches were
also normalised to `BcatTable(...)` — they were inconsistently local while the rest of the
same query used `[BCAT]`. Same tables, consistent server resolution.

The last 14 references, in `SettingsController`, are gone — see section 2.6.

### ~~P2 — `client.js:2115` is a dead branch~~ RESOLVED (2026-10-01)

Was:

```js
if (i.group_Code_WO_Desc == v[0].designated_Group_Id) {
```

`v` comes from `ClientsController.GetContactInfo`, which returns
`dataProtector.Unprotect(reference).Split("|")` — so `v[0]` is an **opaque token**, not a
group code. The comparison was always false and the edit-contact group field
(`myInputEdit`) was always blank.

The comparison is gone. **Note the field is now populated by the existing dropdown rather
than by this lookup** — see the caveat in section 8.3. Unrelated to connections.

### P2 — ~~Plaintext secrets in the repository~~ DONE (rotation still required)

`appsettings.json` no longer contains the database password or `AzureAd.ClientSecret`; both
are blank and supplied through user-secrets (local) or environment variables (deployed).
`appsettings.Production.json` was added as a blank template.

**Rotating the old values is still outstanding** — they were committed in plain text, so
they must be considered leaked. See section 4.3 item 3.

### P3 — DataProtection keys are not persisted

Cookies will not survive a restart, and will break across multiple instances, until a
shared key ring is configured. Not addressed.

### ~~P3 — Legacy `BCAutomation` project is still in the tree~~ RESOLVED (2026-10-02)

Was recorded here as "contains a near-identical `ClientsController.cs` (~1200 lines) with
the same route names … greps are ambiguous." **That was already false when it was
written**, and it stayed in the document long enough to send the next person looking for a
duplicate that does not exist.

What was actually on disk: `BCAutomation/` held **zero source files** — no `.cs`, no
`.csproj` — only `bin/` and `obj/` build output plus an empty `BCT_NGC_PROD/`. 545 files,
212 MB, zero tracked, and never committed (`git log --all -- BCAutomation` returns
nothing, so there is no history to lose). `ClientsController.cs` existed in exactly one
place, `BillColl_Main/Controllers/`. The legacy source had already been deleted by
someone; only the compiler output was left behind.

Deleted in the same pass: `BCMailService/` (26 files, 0.4 MB, also source-free),
`Setup/` (empty), and `.kilo/worktrees/` — two stale agent worktrees (`hexagonal-othnielia`,
`paint-caravan`) that were the *actual* source of duplicate filenames, carrying 326 `.cs`
files and 309 MB. Both were fully merged before removal (`hexagonal-othnielia` was 0
commits ahead of `main`; `paint-caravan`'s HEAD was `3963ca1`, `main`'s tip) and
`paint-caravan` was verified clean before `git worktree remove`. **~522 MB reclaimed.**

**Nothing needs a git-history caveat.** Every deleted path was either untracked, gitignored,
or already merged into `main`. `git status` was clean before and after.

If a future `.kilo/` worktree reappears and is stale, the check is
`git rev-list --count main..<branch>` — delete with `git branch -d`, never `-D`, so an
unmerged branch refuses to delete rather than silently losing work.

---

## 4. STAGE configuration

### 4.1 What a deployer sets — the complete list

**The goal: deployers change configuration only. No code changes, ever.**

There is exactly one source of truth: the `Connections` section. Every environment-specific
value the application reads is listed below. Anything not in this table is fixed in code and
must not be edited per environment.

#### Via environment variables (recommended — works on IIS, containers, systemd, Azure App Service)

`Host.CreateDefaultBuilder` already calls `AddEnvironmentVariables()`, and the double
underscore `__` is the section separator. No extra plumbing needed.

| Environment variable | Required | Purpose |
|---|---|---|
| `Connections__Primary__Provider` | **yes** | `PostgreSQL` or `SQLServer` |
| `Connections__Primary__ConnectionString` | **yes** | BCAT connection string |
| `Connections__SharedSql__ConnectionString` | no | SQL Server connection to databank/FinApps/HRIS. Empty ⇒ LoS auto-fill disabled, nothing throws |
| `Connections__LinkedServers__Bcat` | **yes** | BCAT linked server name |
| `Connections__LinkedServers__Databank` | **yes** | PH Report Databank linked server name |
| `Connections__LinkedServers__FinApps` | **yes** | FinApps linked server name |
| `Connections__LinkedServers__Hris` | **yes** | HRIS linked server name |
| `AzureAd__ClientSecret` | for SSO | Azure AD client secret |
| `AzureAd__TenantId` | for SSO | Azure AD tenant id |
| `AzureAd__ClientId` | for SSO | Azure AD client id |
| `AzureAd__Domain` | for SSO | Redirect URI host |
| `AuthSettings__EnableSso` | **yes** | `true` in STAGE/PROD. See warning below |
| `BillingMail__SenderEmail` | no | Sender address |
| `BillingMail__SenderName` | no | Sender display name |
| `BillingMail__SmtpHost` / `BillingMail__SmtpPort` | no | SMTP host and port |
| `BillingMail__EnableSsl` | no | `true` for STARTTLS |
| `BillingMail__EmailCapturePath` | no | Write `.eml` files here instead of sending. **Set this for local demos** — the real relay `10.139.108.22:25` is unreachable off-network |

#### Or via `appsettings.Production.json`

`BillColl_Main/appsettings.Production.json` exists as a template with every value blank.
A deployer can fill it in instead of using environment variables. Do not do both for the
same key — environment variables win, which is what you want for secrets.

#### Local development

Secrets live in **user-secrets**, not in any committed file:

```powershell
dotnet user-secrets --project .\BillColl_Main\BillColl_Main.csproj set "Connections:Primary:ConnectionString" "<value>"
```

User-secrets are only loaded when `ASPNETCORE_ENVIRONMENT=Development`. Running with
`--no-launch-profile` and no environment variable defaults to **Production**, where
user-secrets are ignored and startup will fail validation if nothing else is configured.
For local runs use either the `BillColl_Main` launch profile or
`$env:ASPNETCORE_ENVIRONMENT='Development'`.

### 4.2 Fail-fast validation

`Startup.ValidateDatabaseConfiguration` runs before anything is registered. If anything is
missing the application will not start, and says exactly what to set:

```
Unhandled exception. System.InvalidOperationException: Database configuration is incomplete:
  - No primary connection string. Set Connections:Primary:ConnectionString (env var Connections__Primary__ConnectionString).
  - Missing linked server name Connections:LinkedServers:Hris.
```

On a successful start it logs the resolved non-secret configuration, so a deployer can
confirm what the application actually picked up:

```
Database configuration resolved. Provider=PostgreSQL, SharedSql=not configured,
LinkedServers: Bcat=BCAT, Databank=STAGE-DATABANK-01, FinApps=STAGE-FINAPPS-01,
Hris=STAGE-HRIS-01. Environment=Production
```

This is intentional: a misconfigured deployment fails immediately and visibly instead of
half-working and only failing later on whichever request first touches a missing connection.

### 4.3 ⚠️ Review before promoting to STAGE/PROD

1. **`AuthSettings:EnableSso` is `true` in `appsettings.Production.json`.** When SSO is
   disabled, `AccountController.LoginManual` issues a cookie session for
   `AuthSettings:DevDefaultEmail` **without checking any password or querying the
   database**. That is a deliberate local-development convenience and it is an
   authentication bypass. It must be `true` in any shared environment. If STAGE genuinely
   needs the local login, that is a decision to make explicitly, not by default.
2. **`AzureAd:Domain`** must match the host the application is actually served from,
   because it is used to build the OIDC redirect URI.
3. **Secrets were removed from `appsettings.json` in this repo.** The previous values for
   `BillingMail:ConnectionString` (database password) and `AzureAd:ClientSecret` were
   committed in plain text. Treat both as compromised and **rotate them**. The database
   password currently used in development is `Osomatsu@14` on `localhost:5432`.
4. **When `Primary:Provider` is `SQLServer`, the linked servers must exist on that box.**
   `BcatTable()` emits `[<Bcat>].[dbo].[...]`, which requires a linked server literally
   named as configured. This is the legacy behaviour, preserved deliberately.
5. **Database names are fixed in code**, only the server names are configurable:
   `PH Report Databank`, `FinAppsDM`, `HRIS`. If an environment renames the *database*
   and not just the server, that needs a code change — flag it rather than working around
   it.
6. **`DataProtection` keys are not persisted.** Cookies will not survive a restart or a
   multi-instance deployment until a key ring is shared. Open issue, not addressed yet.
7. **Logging writes to a local `logs/` folder** via `UseBillingMailFileLogging("logs")`.
   On a locked-down production host that write may fail; move to a real sink if so.

### 4.4 Single source of truth for the database

The web application and the billing mail worker must never point at different databases.
`Connections:Primary:*` is now authoritative for both:

- Web app: `DbConnectionFactory` reads `Connections:Primary:*`, falling back to
  `BillingMail:*` and `ConnectionStrings:myAppDBConnection` when absent.
- Billing mail worker: `BillingMail.Plugin` binds `BillingMail:*`, then a second
  `Configure<BillingMailOptions>` pass overwrites `DatabaseProvider` and
  `ConnectionString` from `Connections:Primary:*` when those are present.

So deployers set `Connections:Primary:*` and the worker follows automatically. The legacy
`BillingMail:DatabaseProvider` and `BillingMail:ConnectionString` keys still work as a
fallback, but setting only those is now the less obvious path — prefer `Connections:Primary:*`.

---

## 5. Traps discovered while testing

These cost real debugging time. They will bite again on any new query.

1. **`42883: operator does not exist: integer = text`.** An `Id` arriving as a query string
   is bound as `text` by Npgsql, but the column is `integer`. SQL Server coerces implicitly;
   PostgreSQL does not. Use `DbValues.CoerceId(...)` before binding. Applies to reads via
   `FillDataTable` and writes via Dapper.
2. **`42804: column "Is_Default" is of type boolean but expression is of type integer`.**
   SQL Server `bit` literal `1`; PostgreSQL needs `TRUE`. Check `Is_Default` and any other
   bit/boolean column when writing inserts.
3. **`GETDATE()`** → `CURRENT_DATE` / `CURRENT_TIMESTAMP` on the PostgreSQL side.
   `CURRENT_TIMESTAMP` is preferred; it is what `LogsSQLRepository` already does.
4. **String concatenation** `+` → `||` on PostgreSQL.
5. **`IIF`**, **`FORMAT`**, **`TOP n`** are SQL Server only. `FORMAT` in particular is used
   in `admin_logs` and `engagement_secretary_logs` for date formatting; on PostgreSQL use
   `TO_CHAR` and move the formatting out of SQL if possible.
6. **Tables absent from the migrated schema** (verified against PostgreSQL `BCAT`):
   `tblGroupTypeCode`, `tblEntityMapping`, and every `tblGroupCode` column except
   `Id`, `chOfficeCode`, `chGroupCode`, `vcGroupDesc` — there is no `chGroupType` and no
   `sdTermDate`. Do not port the legacy `tblGroupCode` join; use `dbo."Group"`.
7. **Present in PostgreSQL and safe to use:** `User_Role`, `Log_Admin`,
   `Engagement_Secretary`, `Secretary_Logs` — all identity-backed with sequences, so
   inserts do not supply `Id`. Note the sequences are **not reset by `TRUNCATE`-free
   deletes**; a test that assumes the first insert gets `Id = 1` is wrong. Always read the
   `id` back from `load_role` / `load_engagement_secretary`.
8. **An aliased column cannot be referenced unquoted in `ORDER BY` on PostgreSQL.** `SELECT
   TO_CHAR(x) AS "Time" ... ORDER BY Time DESC` fails with `column "time" does not exist`,
   because the unquoted `Time` folds to lowercase while the table column is `"Time"`. Order
   by the **base** column and quote it, or drop the alias.
9. **`DbConnectionFactory.AddParameters` does not understand `Dictionary<string, object>`.**
   It handles `IDbDataParameter`, an enumerable of them, and anonymous objects — a dictionary
   falls through to property reflection, binds nothing, and you get
   `column "clientcode" does not exist`. Always pass an anonymous object.
10. **`dbo` is the schema and the search path is not set.** `\d "User_Role"` in `psql` fails
    with *Did not find any relation*; the tables are in the `dbo` schema. Use
    `\d dbo."User_Role"` or query `information_schema.tables`.
11. **An uncaught exception in `BillingMailWorker` kills the entire web application**, not
    just the worker, because `HostOptions.BackgroundServiceExceptionBehavior` defaults to
    `StopHost`. `BackgroundService.ExecuteAsync` must never let an exception escape. In
    particular `await Task.Delay(interval, stoppingToken)` throws `OperationCanceledException`
    on *every* normal shutdown, so it must be caught — otherwise every clean stop logs
    `BackgroundService failed` followed by `FTL ... configured to StopHost` and looks like a
    crash. A bill-collection web app must not go down because the mail worker hiccuped.
12. **Two instances on one port.** The second one dies at bind time, and if the first is still
    running the app looks broken. Always stop the previous instance.
13. **`ForbidResult` under cookie authentication is a 302, not a 403.** The cookie handler
    turns a forbid into a redirect to `AccessDeniedPath` (`/Account/AccessDenied`), which
    then returns 200. Any test asserting 403 on a `[Forbid]` path is asserting the wrong
    thing. Assert the `Location` header, or follow it.
14. **This project has implicit usings disabled**, so a new file that uses `Task`,
    `CancellationToken` or anything in `System.Collections.Generic` needs explicit
    `using` directives or it will not compile.
15. **`FilterContext` exposes no `ControllerContext` or `ActionContext`.** In an
    `IAsyncActionFilter`, reach the principal and the request services through
    `context.HttpContext` — `context.HttpContext.User` and
    `context.HttpContext.RequestServices`. Both mistakes are compile errors, so the build
    catches them — but only if you run it. See section 9.4.

---

## 6. How to run and verify

```powershell
# build
dotnet build .\BillColl_Main\BillColl_Main.csproj -p:UseAppHost=false -o "$env:TEMP\bcbuild"

# run
dotnet run --project .\BillColl_Main\BillColl_Main.csproj --no-launch-profile
```

If the build fails with `MSB3021` / `MSB3027` "file is locked", the app is still running —
stop it first. Use `-p:UseAppHost=false -o <temp>` to build without touching the locked
`bin` output.

Dev authentication is local login (no SSO), enabled by
`AuthSettings:EnableSso = false` in `appsettings.Development.json`. It issues a session
without checking a password — never enable it in a shared environment.

`--no-launch-profile` gives you **Production**, where user-secrets are not loaded and
startup validation will fail. For local runs either use the `BillColl_Main` launch profile
or set the environment first:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run --project .\BillColl_Main\BillColl_Main.csproj
```

```powershell
$jar = "$env:TEMP\bc.jar"
$page = curl.exe -s -c $jar http://localhost:5000/Account/Index
$tok = [regex]::Match($page, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value
curl.exe -s -b $jar -c $jar -o NUL -X POST http://localhost:5000/Account/LoginManual `
  --data-urlencode "Username=admin@pwc.com" --data-urlencode "__RequestVerificationToken=$tok"
curl.exe -s -b $jar http://localhost:5000/load_role
```

Server-side errors go to `BillColl_Main/logs/`. Npgsql errors appear as
`Npgsql.PostgresException` with a SQLSTATE and a `POSITION` offset — quoting the SQL around
`POSITION` identifies the failing clause quickly.

Database inspection (the `;` in the connection string is a separator, so the real password
is `Osomatsu@14`):

```powershell
$env:PGPASSWORD='Osomatsu@14'
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -h localhost -p 5432 -U postgres -d BCAT -f query.sql
```

Write multi-line SQL to a file and use `-f`; quoting identifiers in `psql -c` from
PowerShell is unreliable.

### Verifying the admin gate needs two logins

`AccountController.LoginManual` accepts **any** username in Development, so you can prove the
role check without touching the database: sign in as `admin@pwc.com` (has the `DEVADMIN`
`User_Role` row) and as any other address (resolves to `User_Role "0"`), each with its own
cookie jar. `regular.user@pwc.com` is the one used in section 7.

```powershell
function Login([string]$jar, [string]$user) {
  $page = curl.exe -s -c $jar http://localhost:5000/Account/Index
  $tok = [regex]::Match($page, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value
  curl.exe -s -b $jar -c $jar -o NUL -X POST http://localhost:5000/Account/LoginManual `
    --data-urlencode "Username=$user" --data-urlencode "__RequestVerificationToken=$tok"
}
Login "$env:TEMP\adm.jar" 'admin@pwc.com'
Login "$env:TEMP\usr.jar" 'regular.user@pwc.com'

curl.exe -s -o NUL -w '%{http_code}' -b "$env:TEMP\adm.jar" http://localhost:5000/add_the_role   # admin
curl.exe -s -D - -o NUL      -b "$env:TEMP\usr.jar" http://localhost:5000/add_the_role            # non-admin
```

Assert **302 with `Location: .../Account/AccessDenied`** for the non-admin, not 403 — see
section 9.3. To prove the *write* was blocked rather than merely hidden, re-query
`dbo."User_Role"` afterwards and confirm the row count is unchanged.

Note the billing mail worker logs SMTP connection timeouts to `10.139.108.22:25` in this
environment. Those are gone under Development now that `EmailCapturePath` is set — see
section 0.

---

## 7. Currently verified working

**Every route below returned HTTP 200 on 2026-10-01**, re-verified after the batch 2 port.
All 25 were exercised against the running app with an authenticated session:

`/Settings`, `/Settings/Index`, `/Home`, `/Clients`, `/get_name_from_username`,
`/groupcodelist`, `/losbygroupcodelist`, `/load_role`, `/load_reason`,
`/load_deletion_reason`, `/view_exception`, `/viewlogreport_clientengagement`,
`/admin_logs`, `/engagement_secretary_logs`, `/load_engagement_secretary`,
`/stafflist`, `/stafflist_executive`, `/stafflist_partner`,
`/stafflist_for_engagement_users`, `/validate_engagement_secretary`,
`/validate_adding_engagement_secretary`, `/download_admin_logs`,
`/download_secretary_logs`, `/download_viewlogreport_clientengagement`.

Note which controller each route lives on, because that decides who can reach it:

- **`SettingsController`** — everything from `/get_name_from_username` to
  `/download_secretary_logs` in the list above. **Admin-only since section 9.** These were
  verified with an admin session and still return 200; a non-admin is redirected to
  `/Account/AccessDenied`.
- **`ClientsController`** — `/load_reason`, `/load_deletion_reason`,
  `/viewlogreport_clientengagement`, `/view_exception`. Deliberately **not** admin-gated,
  because regular users can reach `/Clients`.
- **`ReportController`** — `/download_viewlogreport_clientengagement`.

Write round-trips verified end to end (insert → read → update → read → delete → read),
for both `User_Role` and `Engagement_Secretary`, with the audit rows landing in
`Log_Admin` / `Secretary_Logs`.

`view_reports_new` was checked for the LOS match, the no-LOS case, an unparseable date
(ignored rather than thrown) and an empty client code (`[]`).

Two endpoints legitimately return `[]` rather than data, because HRIS and the databank are
only reachable from the shared SQL Server estate, which is not configured locally:
`/losbygroupcodelist`, `/stafflist`, `/stafflist_executive`, `/stafflist_partner`,
`/stafflist_for_engagement_users`. This is the designed behaviour — see section 2.6.

Build: 0 warnings, 0 errors.

The plugin was verified two ways: as a hosted service inside the web app
(`Pending=4, Sent=4, Errors=0`, messages written to `captured_emails\`), and standalone via
`.\run-local.ps1 -Plugin` (both the single-batch and the database-reset modes).

Startup configuration was verified three ways:

1. `Production` with nothing configured → fails fast with the actionable message.
2. `Development` with user-secrets → starts, logs the resolved configuration.
3. `Production` with **environment variables only** (STAGE server names supplied) → starts
   and logs `Databank=STAGE-DATABANK-01, FinApps=STAGE-FINAPPS-01, Hris=STAGE-HRIS-01`.
   This is the path deployers will use.

### Navigation re-verified after the section 2.7 fix (2026-10-01)

Authenticated as `admin@pwc.com` with the `DEVADMIN` grant in place:

| Check | Result |
|---|---|
| `/`, `/Home`, `/Clients`, `/Settings` | all 200 |
| `#jsonData` | valid JSON on **all three** pages, `User_Role: "1"` |
| `<li id="util-nav">` | present on all three |
| Utilities anchor | `<a ... href="/Settings">` — a real link, no JS needed |
| `/load_role`, `/admin_logs`, `/get_name_from_username` | 200 |
| Regular-user path | `User_Role: "0"` → `util-nav` absent, correctly |

Both branches were exercised. The admin branch was confirmed with a temporary stub that was
then removed; the regular-user branch is what `admin@pwc.com` renders without the grant.

Build: 0 warnings, 0 errors.

### Development-only admin grant verified across all three configurations (2026-10-01)

Each run used the same database and the same `DEVADMIN` `User_Role` row. Only the
environment and `AuthSettings:EnableSso` changed. **No code change between any of them.**

| # | `ASPNETCORE_ENVIRONMENT` | `EnableSso` | `User_Role` rendered | Utilities nav item | DEV warning logged |
|---|---|---|---|---|---|
| A | `Development` | `false` | `"1"` | present | yes |
| B | `Production` | `false` | `"0"` | absent | no |
| C | `Production` | `true` (appsettings default) | n/a — never reaches a page | n/a | no |

Test C redirects `/Account/Index` and `/Home` straight to
`https://login.microsoftonline.com/.../authorize?...` — the OIDC challenge is unchanged and
the manual login form is not offered at all, which is the pre-existing SSO behaviour.

So switching between local dev and a deployed environment is configuration only, and the
dev grant cannot leak into STAGE or PROD.

### Controller-wide admin enforcement verified (2026-10-01, after section 9)

Run against a live instance started with `.\run-local.ps1`, PostgreSQL `BCAT`, an
authenticated cookie session per user. Admin = `admin@pwc.com` (has the `DEVADMIN` row);
non-admin = `regular.user@pwc.com` (no `User_Role` row, and HRIS is unreachable so it
resolves to `User_Role "0"`). Note the local dev login accepts **any** username
(`AccountController.LoginManual`), which is what makes this two-user test possible.

| Request | Anonymous | `admin@pwc.com` | `regular.user@pwc.com` |
|---|---|---|---|
| `/`, `/Home`, `/Clients` | 302 → login | 200 | 200 |
| `/Settings` | 302 → login | **200** | **302 → `/Account/AccessDenied`** |
| `/load_role`, `/admin_logs`, `/engagement_secretary_logs` | 302 → login | 200 | 302 → AccessDenied |
| `/view_role`, `/view_reports_new` | 302 → login | 200 | 302 → AccessDenied |
| `/stafflist`, `/download_admin_logs` | 302 → login | 200 | 302 → AccessDenied |
| `/add_the_role`, `/update_role?Id=1`, `/delete_role?Id=1` | 302 → login | — | **302 → AccessDenied** |
| `/add_engagement_secretary`, `/update_engagement_secretary?Id=1`, `/delete_engagement_secretary?Id=1` | 302 → login | — | **302 → AccessDenied** |

Two checks beyond status codes, because a 302 alone does not prove the *write* was blocked:

- **`User_Role` was re-queried after the blocked writes.** Still exactly 3 rows
  (`E10001` → `2`, `E10002` → `1`, `DEVADMIN` → `1`). Nothing was inserted, so the filter
  short-circuits before the action body runs — the privilege-escalation path in section 8.2
  is closed, not merely hidden.
- **Sidebar rendering on `/Home`:** admin → `id='util-nav'` present, `User_Role=1`;
  non-admin → `util-nav` absent, `User_Role=0`. The filter and the nav still agree, because
  both resolve through `IUserContext`.

The `—` cells are deliberate: the write endpoints were **not** called as the admin, because
doing so would insert and update real rows in `User_Role` / `Engagement_Secretary`. They are
covered by the write round-trips already recorded above in section 7. The filter is identical
for every action on the controller and the admin column proves it admits admins.

Plugin worker in the same run: `Billing Mail Background Worker started`, batch
`Pending=0, Sent=0, Errors=0` (the queue was already drained by the earlier runs), next run
in 300s. The DEV fallback warning is logged once at startup, as designed.

Build: 0 warnings, 0 errors.

### Still not verified

| Check | Expected | Notes |
|---|---|---|
| Edit-contact dialog | group dropdown usable | `myInputEdit` stays blank, by design now — see 8.3 |
| `/Settings` under **SSO** with a real employee code | 200 for an admin | not exercised; needs STAGE + HRIS |

Everything else in section 7 was re-verified and still holds.

---

## 8. Immediate Code Fixes (2026-10-01)

Three items from section 3 were actioned. **Build: 0 warnings, 0 errors** (at the time this
was written — item 8.2 was subsequently superseded, see section 9). None of the three had
been exercised against a running instance at this point; the runtime evidence now lives in
section 7.

### 8.1 Deleted `Services/SQLTransHelper.cs`

The file is gone. Verified by grep that no `SQLTrans` reference remains anywhere in
`BillColl_Main` (only prose mentions in this document). Nothing referenced it since the
batch 2 port, and the build confirms it.

### 8.2 Server-side role check on `SettingsController.Index` — SUPERSEDED by section 9

An action-level check was added to `Index`:

```csharp
var currentUser = userContext.Resolve(User);
if (!currentUser.IsAdmin)
{
    return Forbid();
}
```

An `IsAdmin` check inside the action was chosen over an `[Authorize(Policy = "IsAdmin")]`
policy, deliberately: **no policy named `IsAdmin` exists in `Startup.cs`**, so the policy
attribute would compile and then deny *everyone*, including real admins, because an
unregistered policy name fails closed. That is a silent-lockout failure mode with an
attractive-looking attribute.

Two things this version got wrong, both now fixed in section 9:

1. **It guarded only the page.** The twenty-plus data endpoints on the controller remained
   reachable by any authenticated user typing the URL — including `add_the_role`,
   `delete_role`, `update_role`, `add_engagement_secretary`, `delete_engagement_secretary`
   and `update_engagement_secretary`. A regular user could no longer *see* the Utilities
   page but could still grant themselves admin. This is now closed by `[AdminOnly]`.
2. **`Forbid()` does not return 403 here.** It returns a 302 to
   `/Account/AccessDenied?ReturnUrl=…`. See section 9.3.

### 8.3 `client.js` dead branch removed — with a caveat

The always-false comparison at the old line 2115 is gone. **This is a partial fix, and the
field is still not correctly populated.** Removing the comparison makes the always-false
branch stop pretending to do work; it does not make `myInputEdit` show the contact's real
group.

The remaining code sets `myInputEdit` from `filteredGroup`, which is initialised to `''` and
now has nothing that ever assigns to it — so the field is blank by construction rather than
by accident. Populating it properly means having the server return the contact's group
description alongside the contact, not reverse-engineering it client-side from an opaque
token. The dropdown (`groupListEdit`) is populated and usable, so an operator can select the
group manually; the field is not silently wrong, it is just not pre-filled.

The removed loop was also a no-op `groupCodeList.map(...)` whose only body was the dead
comparison, so deleting it loses nothing else. Left as a comment rather than deleted outright
so the next reader knows the lookup is missing on purpose, not by accident.

---

## 9. Controller-wide admin enforcement (2026-10-01, verified)

This is the pass that closes the top item in section 10. The role check now covers every
action on `SettingsController`, not just the page.

### 9.1 What changed

**New: `BillColl_Main/Filters/AdminOnlyAttribute.cs`** — an `IAsyncActionFilter` placed on
the controller:

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AdminOnlyAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutingDelegate next)
    {
        var httpContext = context.HttpContext;

        var userContext = httpContext.RequestServices.GetService(typeof(IUserContext)) as IUserContext;

        if (userContext == null)
        {
            // No DI hook — fail closed rather than silently allow.
            context.Result = new ForbidResult();
            return;
        }

        var principal = httpContext.User;
        var currentUser = userContext.Resolve(principal);
        if (!currentUser.IsAdmin)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
```

**`SettingsController`** now carries:

```csharp
[Authorize]
[AdminOnly]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class SettingsController : Controller
```

and `Index` is back to being just `return View();` with a comment pointing at the filter.
The action-level check from section 8.2 is **gone** — it would be dead code behind the
filter, and two checks reading the same source can only ever disagree later.

`[AllowMultiple = false]` means a second `[AdminOnly]` is a compile error rather than a
silent double-evaluate.

### 9.2 Scope: what this now covers

All **22 routed data endpoints** plus the five un-routed actions on the controller:

`get_name_from_username`, `add_the_role`, `delete_role`, `view_role`, `update_role`,
`load_role`, `admin_logs`, `add_engagement_secretary`, `delete_engagement_secretary`,
`view_engagement_secretary`, `validate_engagement_secretary`,
`validate_adding_engagement_secretary`, `update_engagement_secretary`,
`load_engagement_secretary`, `engagement_secretary_logs`, `stafflist_executive`,
`stafflist_partner`, `stafflist`, `stafflist_for_engagement_users`, `view_reports_new`,
`download_admin_logs`, `download_secretary_logs`, plus `Index`, `GetData`, `GetClients`,
`RemoveException` and `Submit`.

`load_reason`, `load_deletion_reason`, `viewlogreport_clientengagement` and
`view_exception` are **not** on this controller — they live in `ClientsController`
(`ClientsController.cs:420-438`) and are deliberately **not** admin-gated, because the
Clients page uses them and regular users can reach `/Clients`. Do not move them.

### 9.3 ⚠️ The denial is a 302, not a 403

`ForbidResult` under the **cookie** authentication scheme does not produce a bare 403. The
cookie handler converts it into a redirect:

```
HTTP/1.1 302 Found
Location: http://localhost:5000/Account/AccessDenied?ReturnUrl=%2Fadd_the_role
```

Following it lands on `/Account/AccessDenied`, which returns **200**. So a non-admin typing
`/add_the_role` sees a 302 and then the Access Denied page.

Section 8.2 claimed 403; that was wrong and has been corrected here. The practical
consequences:

- A test asserting `403` will fail. Assert `302` with a `Location` of
  `/Account/AccessDenied`, or follow the redirect and assert the Access Denied page.
- An API client doing XHR will be transparently redirected rather than rejected. If a real
  403 is wanted for non-browser callers, either set
  `CookieAuthenticationEvents.OnRedirectToAccessDenied` to set the status and clear the
  header, or have `AdminOnlyAttribute` return `new StatusCodeResult(403)` instead of
  `ForbidResult`. **This is a deliberate decision, not an oversight** — the current
  behaviour gives a human-readable page rather than a bare status.

### 9.4 ⚠️ The file did not compile when it was written

The filter was committed with two compile errors, so `dotnet build` failed and the app
could not start. Both are now fixed:

| Error | Cause | Fix |
|---|---|---|
| `CS0246: The type or namespace name 'Task' could not be found` | implicit usings are disabled for this project | `using System.Threading.Tasks;` added |
| `CS1061: 'ActionExecutingContext' does not contain a definition for 'ControllerContext'` / `'ActionContext'` | both were assumed to exist on `FilterContext` | resolve from `context.HttpContext.RequestServices` and read the principal from `context.HttpContext.User` |

Build after the fix: **0 warnings, 0 errors**.

**Read this as a process lesson, not trivia.** The file looked complete, was committed, and
HANDOFF.md at the time claimed a clean build. A compile error is the cheapest possible
defect to catch and it survived anyway, because the build was not re-run after the edit.
**Run `dotnet build` after every change to this project**, and treat a clean build as the
only evidence that new code exists at all. The runtime verification in section 7 is what
confirms the filter is not merely compiling but actually blocking.

`context.HttpContext` is the portable way to reach both the request services and the
principal from a filter on this target framework (`net10.0`).

### 9.5 Verification performed

Full status-code matrix, both write paths, the post-write database check and the sidebar
comparison are in **section 7 — "Controller-wide admin enforcement verified"**. Summary:
admin 200 everywhere on the controller; every `SettingsController` endpoint redirects a
non-admin to `/Account/AccessDenied`; `User_Role` still holds exactly its original three
rows after the blocked write attempts; `/Home`, `/Clients` and `/` are unaffected.

### 9.6 Housekeeping in the same pass

- `.gitignore` had `Setup/` listed twice; the duplicate was removed (commit `17a8fff`,
  "remove duplicate").

---

## 10. Bottom line for whoever picks this up

**Deployers never need to touch code.** Every environment-specific value is in section 4.1.

Start it with `.\run-local.ps1`. There are no known runtime errors.

### Just finished

- **`[AdminOnly]` gates all of `SettingsController`**, not just the page. A non-admin can no
  longer POST to `/add_the_role` and grant themselves admin. Verified at runtime, including
  a database check proving the blocked writes did not land. See section 9.
- **Legacy directories and stale agent worktrees deleted** — `BCAutomation/`,
  `BCMailService/`, `Setup/`, `.kilo/worktrees/`. ~522 MB, no source lost, greps are now
  unambiguous. See section 3.

### Still requires code work

1. **Populate the edit-contact group field from the server.** The `client.js` dead branch is
   removed but `myInputEdit` is still never filled. See section 8.3. This is now the most
   important remaining item.
2. **`SharedSqlConnectionFactory` falls back to `myAppDBConnection`.** If that is ever set to
   the PostgreSQL connection string, the shared-SQL path will hand a PostgreSQL string to
   `SqlConnection`. It is empty today, so it is latent, not live.
3. **Decide whether non-browser callers need a real 403.** Today a non-admin hitting a
   `SettingsController` endpoint gets a 302 to `/Account/AccessDenied`. Fine for a human with
   a browser; if any XHR client or integration consumes these endpoints, return
   `StatusCodeResult(403)` instead. See section 9.3.
4. **DataProtection key ring** — see item 6 below; it is configuration plus a small
   `Startup.cs` registration.

### Still requires operational work, no code

5. **Rotate the leaked secrets** — the database password and `AzureAd:ClientSecret` were
   committed in plain text and must be considered compromised. See section 4.3 item 3.
6. **DataProtection key ring** — needed for restarts and multiple instances.
7. **Re-create the `DEVADMIN` `User_Role` row** in any new environment. Since `/Settings` is
   now server-enforced, without this row the Utilities page is denied for everyone locally.
   The row is inert outside Development, so it is safe to leave in place — see section 3.
