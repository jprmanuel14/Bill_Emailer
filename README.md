# BillColl System (.NET 10 Web Application & Billing Mail Plugin)

Automated Statement of Account (SOA) and billing collection management system built for Isla Lipana & Co. / PwC Philippines. Modernized from a legacy **.NET Framework 4.7.2** Windows Service into an integrated **ASP.NET Core 10 Web Application** (`BillColl_Main`) and cross-platform **.NET 10 Background Plugin** (`BillingMail.Plugin`) supporting both **PostgreSQL** and **MS SQL Server**.

---

## 📌 Architecture & System Overview

```
+-----------------------------------------------------------------------------------+
|                                 BillColl_Main                                     |
|                       (ASP.NET Core 10 MVC Web Application)                       |
|                                                                                   |
|  +--------------------+   +-----------------------+   +------------------------+  |
|  | Account / SAML SSO |   | Client & Contacts UI  |   | Reports & Settings UI  |  |
|  +--------------------+   +-----------------------+   +------------------------+  |
|                                                                                   |
|  +-----------------------------------------------------------------------------+  |
|  |                 Embedded Hosted Service: BillingMail.Plugin                 |  |
|  |                                                                             |  |
|  |   BillingMailWorker ----> BillingMailProcessor ----> StatementEmailBuilder  |  |
|  |           |                        |                           |            |  |
|  |           v                        v                           v            |  |
|  |   [Timer Schedule]         [Dapper Repositories]      [MailKit Dispatcher]  |  |
|  +-----------+-------------------------+---------------------------+------------+  |
+--------------|-------------------------|---------------------------|--------------+
               |                         |                           |
               v                         v                           v
     +-------------------+    +--------------------+       +--------------------+
     | App Configuration |    | Database (BCAT)    |       | SMTP Relay Server  |
     | (appsettings.json)|    | PostgreSQL / MSSQL |       | (MailKit Async)    |
     +-------------------+    +--------------------+       +--------------------+
```

The system automates client billing management and statement dispatches:
1. **Web Portal (`BillColl_Main`):** Enterprise web interface for account management, SAML 2.0 Single Sign-On (SSO), client contact management, engagement team assignments, bank/entity settings, and collection reporting.
2. **Background Engine (`BillingMail.Plugin`):** Runs as an in-process `IHostedService` background worker that queries pending scheduled statement jobs (`tblScheduledMail`), generates responsive HTML email statements with itemized invoices, practice entity subtotals, grand totals, and bank remittance details, and dispatches emails via MailKit SMTP.

---

## 📁 Repository Structure

```
BillColl_Mail/
├── BillColl_Main/                        # Main ASP.NET Core 10 MVC Web Application
│   ├── BillColl_Main.csproj              # Web App Project (.NET 10.0)
│   ├── Program.cs                        # WebHost Builder & Daily File Logging Setup
│   ├── Startup.cs                        # DI Registration, SAML 2.0, EF Core & Plugin Setup
│   ├── Controllers/                      # MVC Controllers (Account, Clients, Settings, Report, Home)
│   ├── Services/                         # Dapper Repositories (Clients, Contacts, Remarks, Mail, etc.)
│   ├── AppDbContext/                     # EF Core DbContext (myDBContext)
│   ├── Views/                            # Razor Views & Shared Layouts
│   └── appsettings.json                  # Application & Database Configuration
│
├── BillingMail.Plugin/                   # Core .NET 10 Background Plugin Library
│   ├── BillingMail.Plugin.csproj         # Plugin Project (.NET 10.0)
│   ├── BillingMailServiceExtensions.cs   # AddBillingMailPlugin() & File Logging DI Extensions
│   ├── BackgroundServices/               # BillingMailWorker (IHostedService / BackgroundService)
│   ├── Services/                         # Core Logic, Dapper Repositories, Statement Builder, MailKit
│   └── Options/                          # Configuration DTOs (BillingMailOptions)
│
├── BillingMail.TestRunner/               # .NET 10 CLI Utility for Isolated Plugin Testing
│   ├── Program.cs                        # Interactive CLI Menu (Instant Test & Continuous Worker)
│   └── appsettings.json                  # Local Test Credentials & Settings
│
├── others/                               # Database Migration & Seed Scripts
│   ├── BCAT_09252026_stage_postgresql.sql # PostgreSQL DDL Schema Script
│   └── seed_test_data.sql                # Sample Seed Data for Testing
│
├── BCMailService/                        # Legacy Windows Service (.NET Framework 4.7.2 - Reference Only)
└── Setup/                                # Legacy Installer Setup (.vdproj - Reference Only)
```

---

## 🔌 Developer Guide: Main Web App & Plugin Integration

`BillColl_Main` references `BillingMail.Plugin` directly to execute background mail processing in the same application host.

### 1. Service Registration (`Startup.cs` / `Program.cs`)

In `Program.cs`:
```csharp
using BillingMail.Plugin;

public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseBillingMailFileLogging("logs") // Saves rolling daily logs to logs/billing-mail-YYYYMMDD.log
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
```

In `Startup.cs`:
```csharp
using BillingMail.Plugin;

public void ConfigureServices(IServiceCollection services)
{
    // Dual Database EF Core Setup
    var dbProvider = Configuration["BillingMail:DatabaseProvider"] ?? "PostgreSQL";
    var connString = Configuration.GetConnectionString("myAppDBConnection") ?? Configuration["BillingMail:ConnectionString"];

    services.AddDbContext<myDBContext>(options =>
    {
        if (string.Equals(dbProvider, "PostgreSQL", StringComparison.OrdinalIgnoreCase))
            options.UseNpgsql(connString);
        else
            options.UseSqlServer(connString);
    });

    // Register Plugin Services & Background Worker
    services.AddBillingMailPlugin(Configuration);
    
    // MVC Controllers & Razor Pages
    services.AddControllersWithViews();
}
```

### 2. Application Configuration (`BillColl_Main/appsettings.json`)

```json
{
  "ConnectionStrings": {
    "myAppDBConnection": "Host=localhost;Port=5432;Database=BCAT;Username=postgres;Password=your_password;"
  },
  "BillingMail": {
    "DatabaseProvider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Port=5432;Database=BCAT;Username=postgres;Password=your_password;",
    "CallType": 2,
    "CallDurationSeconds": 900,
    "StartTime": "05:00 PM",
    "SmtpHost": "10.139.108.22",
    "SmtpPort": 25,
    "EnableSsl": false,
    "SenderEmail": "no-reply@billcollectiontool.ph.pwc.com",
    "SenderName": "PwC Billing Collections",
    "SupportEmail": "ph_pwc_collections@pwc.com"
  },
  "Saml2": {
    "IdPMetadata": "https://login.pwc.com/openam/saml2/jsp/exportmetadata.jsp?entityid=urn:pwc:cert:uid:p_2031&realm=/pwc",
    "Issuer": "https://billcollectiontool.ph.pwcinternal.com",
    "SignatureAlgorithm": "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"
  }
}
```

> ⚠️ **Note:** Ensure `"DatabaseProvider"` and connection strings match across both EF Core (`myAppDBConnection`) and Dapper (`BillingMail:ConnectionString`).

---

## 🔓 Authentication Modes (SSO Toggle)

The application supports two authentication modes controlled by the `AuthSettings` configuration section. SAML 2.0 registration and middleware are only wired up when SSO is enabled, so the app can be started without IdP metadata, signing certificates, or network access to the corporate IdP.

```json
"AuthSettings": {
  "EnableSso": true,
  "DevDefaultEmail": "admin@pwc.com"
}
```

| `EnableSso` | Behaviour |
| :--- | :--- |
| `true` | `/Account/Index` issues a SAML 2.0 redirect to the corporate IdP. `/Account/Login` consumes the SAML response and validates the email against `tblUsers` (unknown users are redirected to `AccessDenied`). |
| `false` | `/Account/Index` renders a local login form (`Views/Account/Index.cshtml`). Posting to `/Account/LoginManual` issues a cookie session for `DevDefaultEmail` with **no database lookup**, so login succeeds even before the user tables are created. |

### Running locally without SSO

`appsettings.Development.json` already ships with:

```json
"AuthSettings": {
  "EnableSso": false,
  "DevDefaultEmail": "admin@pwc.com"
}
```

```powershell
dotnet run --project BillColl_Main/BillColl_Main.csproj
```

Open `https://localhost:5001` — you will land on the **Development Login** page, pre-filled with `admin@pwc.com`.

**Claims issued on manual login** (`NameIdentifier`, `Name`, `Email`, `GivenName`, `Role = Administrator`).

> ⚠️ **Production Safeguard:** `appsettings.json` keeps `"EnableSso": true`. Never override it to `false` in a deployed environment — manual login bypasses all IdP and `tblUsers` validation by design.

---

## 🐘 Database Setup Guide (PostgreSQL & SQL Server)

### PostgreSQL (Recommended for Container/Cloud Deployments)
1. Install PostgreSQL and create a database named **`BCAT`**.
2. Run the DDL schema script in pgAdmin / `psql`:
   ```bash
   psql -U postgres -d BCAT -f others/BCAT_09252026_stage_postgresql.sql
   ```
3. Seed test data:
   ```bash
   psql -U postgres -d BCAT -f others/seed_test_data.sql
   ```

### MS SQL Server
Set `"DatabaseProvider": "SqlServer"` and provide a valid SQL Server connection string (e.g. `Server=localhost;Database=BCAT;Trusted_Connection=True;TrustServerCertificate=True;`).

---

## 🧪 Local Running & Testing

### Option A: Run Full Web Application (`BillColl_Main`)
```powershell
dotnet run --project BillColl_Main/BillColl_Main.csproj
```
Access the application at `https://localhost:5001` or `http://localhost:5000`.

### Option B: Run Standalone CLI Test Runner (`BillingMail.TestRunner`)
To test the email processing engine without launching the full web UI:
1. Update `BillingMail.TestRunner/appsettings.json` with local database and SMTP configuration.
2. Execute the CLI runner:
   ```powershell
   dotnet run --project BillingMail.TestRunner/BillingMail.TestRunner.csproj
   ```
3. Choose mode:
   - **Option `1` (Instant Test):** Executes a single batch query, compiles email templates, and outputs execution details.
   - **Option `2` (Scheduler Test):** Runs continuous background worker loop simulating production.

---

## 📊 Feature & Technology Modernization Matrix

| Feature | Legacy System (`BCMailService`) | Modernized System (`BillColl_Main` & `BillingMail.Plugin`) |
| :--- | :--- | :--- |
| **Framework** | .NET Framework 4.7.2 | **.NET 10.0** |
| **Hosting Model** | Windows Service (`ServiceBase`) | **ASP.NET Core 10 Web App / `IHostedService`** |
| **Database** | SQL Server Only | **PostgreSQL & MS SQL Server (Dual Support)** |
| **Data Access** | String Concatenation SQL | **EF Core 10 + Dapper (100% Parameterized)** |
| **Mail Dispatch** | `System.Net.Mail` (Obsolete) | **MailKit / MimeKit (Async)** |
| **Authentication** | Legacy AD / Local | **SAML 2.0 Single Sign-On (SSO)** |
| **Configuration** | `App.config` | **`appsettings.json` / Environment Variables** |
| **Logging** | Flat file logging | **Structured Daily Rolling Logs (`logs/`)** |

---

## 🛡️ Engineering Standards & Security
- **OWASP Guidelines:** 100% parameterized queries via Dapper and EF Core prevent SQL Injection.
- **Clean Architecture:** Loose coupling between UI Controllers, Repositories, and Background Plugin Services.
- **Logging:** Daily rolling logs written automatically to `logs/billing-mail-YYYYMMDD.log`.
