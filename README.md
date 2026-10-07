# BillColl Mail - Automated Billing Statement System

[![.NET 10](https://img.shields.io/badge/.NET-10-blue.svg)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL-green.svg)](https://www.postgresql.org/)
[![SQL Server](https://img.shields.io/badge/MS%20SQL-Supported-orange.svg)](https://www.microsoft.com/sql-server)
[![MailKit](https://img.shields.io/badge/Email-MailKit-blue.svg)](https://mailkit.github.io/MailKit/)

> **Automated Statement of Account (SOA) and billing collection management system** for Isla Lipana & Co / PwC Philippines. Modernized from a legacy Windows Service into an integrated ASP.NET Core 10 Web Application with cross-platform background processing.

---

## 📋 Table of Contents

- [Quick Start](#-quick-start)
- [Full Functionality Overview](#-full-functionality-overview)
- [Architecture Deep Dive](#-architecture-deep-dive)
- [Setup Guide](#-setup-guide)
- [Deployment Guide](#-deployment-guide)
- [Configuration Reference](#-configuration-reference)
- [Development Workflow](#-development-workflow)
- [Troubleshooting](#-troubleshooting)
- [Security Considerations](#-security-considerations)

---

## ⚡ Quick Start

### Prerequisites

```bash
# Required tools and runtimes
dotnet --version >= 10.0.0          # .NET 10 SDK
psql                                  # PostgreSQL client (or SQL Server Management Studio)
git                                   # Version control
```

### 5-Minute Local Setup

```powershell
# 1. Create and configure database
# Connect to PostgreSQL:
psql -U postgres -d postgres -c "CREATE DATABASE BCAT;"

# 2. Run schema migration
psql -U postgres -d BCAT -f others/BCAT_09252026_stage_postgresql.sql

# 3. Configure appsettings.json
cd BillColl_Main
# Set connection string:
# Connections:Primary:ConnectionString="Host=localhost;Port=5432;Database=BCAT;Username=postgres;Password=your_password;"

# 4. Run the application
dotnet run --project BillColl_Main/BillColl_Main.csproj
```

**Access:** `https://localhost:5001` or `http://localhost:5000`

---

## 📖 Full Functionality Overview

### Core Capabilities

| Feature | Description | Technology |
|---------|-------------|------------|
| **Automated Email Dispatch** | Scheduled generation and sending of Statement of Account emails | MailKit (Async SMTP) |
| **Web Management Portal** | Full-featured MVC web application for client/account management | ASP.NET Core 10 MVC |
| **Single Sign-On** | Corporate identity federation with Azure Entra ID | OpenID Connect (OIDC) |
| **Multi-Database Support** | PostgreSQL (primary) + SQL Server (shared estate) | Dapper + EF Core 10 |
| **Responsive HTML Emails** | Itemized billing statements with practice entity breakdowns | MailKit + MimeKit |
| **Collection Reports** | Track delivery status, exceptions, and collection metrics | Razor Views + DinkToPDF |
| **Background Processing** | Cross-platform hosted service for mail processing | IHostedService pattern |

### Business Value

- ✅ **Automated Billing**: Eliminates manual statement generation and email dispatch
- ✅ **Enterprise Security**: OIDC SSO with Azure Entra ID, role-based access control enforced server-side
- ✅ **Cross-Platform**: Runs on Linux, Windows, or containerized environments
- ✅ **Production-Ready**: Structured logging, health checks, graceful error handling
- ✅ **Scalable Architecture**: Separation of concerns (UI, Business Logic, Background Workers)

---

## 🏗️ Architecture Deep Dive

### Repository Layout

The repository contains **four .NET projects and nothing else that builds**. There is no
solution file — build and run are per-project (`dotnet build .\BillColl_Main\BillColl_Main.csproj`).

```
BillColl_Mail/
├── BillColl_Main/              ASP.NET Core 10 MVC web app  (360 tracked files)
│   ├── Program.cs              Host bootstrap, UseBillingMailFileLogging("logs")
│   ├── Startup.cs              DI container, auth, ValidateDatabaseConfiguration
│   ├── appsettings.json        Non-secret config template (secrets removed)
│   ├── appsettings.Development.json   SSO off, EmailCapturePath set
│   ├── appsettings.Production.json    Blank template for deployers
│   ├── nlog.config             NLog sinks
│   │
│   ├── Controllers/            HTTP surface — 5 controllers, all routes live here
│   │   ├── AccountController.cs      LoginManual (dev), SSO challenge, AccessDenied
│   │   ├── HomeController.cs         Dashboard
│   │   ├── ClientsController.cs      Clients page + load_reason, view_exception, …
│   │   ├── ReportController.cs       Report download
│   │   └── SettingsController.cs     22 data endpoints — [Authorize] + [AdminOnly]
│   │
│   ├── Filters/
│   │   └── AdminOnlyAttribute.cs     IAsyncActionFilter; fails closed if IUserContext absent
│   │
│   ├── Services/               46 files — the data access + identity layer
│   │   ├── DbConnectionFactory.cs    Primary connection; BcatTable()/DatabankTable()/
│   │   │                             FinAppsTable()/HrisTable() resolve server names
│   │   ├── SharedSqlConnectionFactory.cs   Always SQL Server (databank/FinApps/HRIS)
│   │   ├── ConnectionOptions.cs      ConnectionsOptions, LinkedServerOptions
│   │   ├── DbValues.cs               CoerceId — PostgreSQL will not coerce text→int
│   │   ├── IUserContext.cs           ─┐ single source of truth for the user's role;
│   │   ├── CurrentUser.cs            │ consumed by BOTH [AdminOnly] and _Layout, so
│   │   ├── UserContext.cs            ─┘ enforcement and the nav cannot drift apart
│   │   ├── IUserRole.cs / UserRoleRepository.cs
│   │   ├── IEngagementSecretary.cs / EngagementSecretaryRepository.cs
│   │   ├── IEmployeeDirectory.cs / EmployeeDirectoryRepository.cs   HRIS; degrades quietly
│   │   ├── ISettingsRepository.cs / SettingsRepository.cs
│   │   ├── ISharedSql.cs / SharedSqlRepository.cs
│   │   ├── IClients / IContacts / IExceptions / IGroup / IRemarks /
│   │   │   IBillingMailStatus / IUsers / ILogs / IMailService  (+ SQLRepository impls)
│   │   └── IReportService.cs / ReportService.cs
│   │
│   ├── AppDbContext/           myDBContext.cs — EF Core, registered alongside Dapper
│   ├── Models/                 18 EF entities
│   ├── ViewModel/              6 view models
│   ├── Class/                  14 transport/result DTOs
│   ├── Base/                   4 shared base classes
│   ├── Helper/                 PagedData.cs, Pagination.cs
│   ├── CustomAttribute/        RequiredIfTrue.cs
│   ├── CustomClass/            CustomAssemblyLoadContext.cs
│   ├── Options/                AuthSettings.cs
│   ├── Securities/             DPPurposeStrings.cs
│   ├── Settings/               MailSettings.cs
│   ├── Migrations/             51 files, EF Core history (Feb–Apr 2022)
│   ├── Views/                  Razor — Shared/_Layout.cshtml emits #jsonData once
│   ├── Scripts/                dev-postgres-bootstrap.sql
│   ├── dll/                    libwkhtmltox native binaries + myhelperclass.dll (PDF)
│   └── wwwroot/                site.js, dashboard.js, client.js, utilities.js,
│                               117 img assets, vendored lib/ (bootstrap, jquery, …)
│
├── BillingMail.Plugin/         Hosted background worker, runs inside the web app (17 files)
│   ├── BillingMailServiceExtensions.cs   DI; reapplies Connections:Primary over
│   │                                     BillingMail:* so worker and web app cannot diverge
│   ├── BackgroundServices/BillingMailWorker.cs   Timer loop; must never throw
│   ├── Services/               Processor, Repository, EmailBuilder, MailKitDispatcher (+ interfaces)
│   ├── Models/                 ScheduledMailItem, StatementLineItem, BankDetail, PracticeEntity
│   └── Options/                BillingMailOptions.cs
│
├── BillingMail.TestRunner/     Console harness — runs the worker standalone (3 files)
│
├── others/                     BCAT_09252026_stage_postgresql.sql, seed_test_data.sql
├── run-local.ps1               Starts web app + plugin together on :5000
├── DEPLOYMENT_GUIDE.md
└── README.md
```

#### Removed on 2026-10-02

`BCAutomation/`, `BCMailService/` and `Setup/` were deleted — ~212 MB of `bin/`/`obj/`
build output with **zero source files**, never committed, gitignored. They are not part
of the architecture and should not be recreated. `.gitignore` still lists them so a
stray local copy stays untracked.

#### Two connection factories, on purpose

`DbConnectionFactory` and `SharedSqlConnectionFactory` are separate because the estate
spans two engines and a SQL Server linked server cannot reach PostgreSQL:

| Concern | Factory | Provider |
|---|---|---|
| BCAT (`Group`, `Contacts`, `User_Role`, `Engagement_Secretary`, `Log_Admin`, …) | `DbConnectionFactory` | **switchable** — PostgreSQL or SQL Server |
| PH Report Databank (`tblEntityMapping`, `tblBill`) | `SharedSqlConnectionFactory` | always SQL Server |
| FinApps (`FinAppsDM`) | `SharedSqlConnectionFactory` | always SQL Server |
| HRIS (`viewEmployeeProfile`) | `SharedSqlConnectionFactory` | always SQL Server |

When the primary is PostgreSQL, BCAT tables are referenced **locally**
(`dbo."User_Role"`), never through `[BCAT].[dbo].[User_Role]`. Never embed a server name
in SQL — use `BcatTable(...)`, `DatabankTable(...)`, `FinAppsTable(...)`, `HrisTable(...)`,
which read `Connections:LinkedServers:*` from configuration.

#### Authorization path

```
Request ──► [Authorize] ──► [AdminOnly] ──► Controller action ──► Service ──► Database
            (SettingsController    (IUserContext.Resolve(principal).IsAdmin)
             only, all actions)
```

`[AdminOnly]` sits on the **controller**, not on `Index`. An action-level check would
leave the 22 data endpoints — including `add_the_role`, which grants admin — reachable by
typing the URL. Note that under cookie authentication `ForbidResult` surfaces as a **302
to `/Account/AccessDenied`, not a 403**; assert the `Location` header, not the status code.

### System Components

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              BillColl Mail System                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                    BillColl_Main (Web Application)                     │  │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────────┐ │  │
│  │  │   Account / SSO │  │ Client & Contacts│  │    Reports & Settings  │ │  │
│  │  └─────────────────┘  └─────────────────┘  └─────────────────────────┘ │  │
│  │                                                                         │  │
│  │  ┌─────────────────────────────────────────────────────────────────┐   │  │
│  │  │              Embedded Background Worker: BillingMail.Plugin      │   │  │
│  │  │                                                                 │   │  │
│  │  │    BillingMailWorker ───► BillingMailProcessor ───► StatementEmailBuilder │   │  │
│  │  │         [Timer Schedule]     [Dapper Repositories]      [MailKit Dispatcher]  │   │  │
│  │  └─────────────────────────────────────────────────────────────────┘   │  │
│  └──────────────────┬─────────────────────┬────────────────────────────────┘  │
│                     │                     │                                   │
│                     ▼                     ▼                                   │
│          ┌─────────────────────┐  ┌─────────────────────┐                   │
│          │   App Configuration │  │    Database (BCAT)  │                   │
│          │ (appsettings.json)  │  │ PostgreSQL / MSSQL │                   │
│          └─────────────────────┘  └─────────────────────┘                   │
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                  External Integrations                                │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────────────┐ │  │
│  │  │   OIDC IdP   │  │   SMTP Relay │  │   Linked SQL Servers        │ │  │
│  │  │ (Azure Entra)│  │ (10.139.108) │  │ - PH_MLAAPP002S (Databank) │ │  │
│  │  └──────────────┘  └──────────────┘  │ - PH_MLAAPP004S (FinApps)   │ │  │
│  │                                      │ - PH_MLAAPP002S (HRIS)       │ │  │
│  │                                      └──────────────────────────────┘ │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Data Flow

#### Email Processing Lifecycle

```
1. SCHEDULED MAIL QUEUED
   tblScheduledMail ← Inserted by admin or automation

2. BACKGROUND WORKER WAKES (interval: callDurationSeconds)
   BillingMailWorker.ExecuteAsync() loops continuously

3. PROCESSOR RETRIEVES PENDING JOBS
   IBillingMailProcessor.ProcessPendingMailsAsync()
   → Queries tblScheduledMail WHERE Status = 'Pending'

4. STATEMENT BUILDER GENERATES HTML
   IStatementEmailBuilder.Build()
   → Fetches client data, invoices, bank details
   → Generates responsive HTML with itemized breakdowns
   → Adds practice entity subtotals and grand totals

5. EMAIL DISPATCHED VIA SMTP
   IMailKitDispatcher.Dispatch()
   → Connects to SMTP relay (MailKit async)
   → Sends from: no-reply@billcollectiontool.ph.pwc.com
   → To: Client email addresses from tblScheduledMail
   → CC: ph_pwc_collections@pwc.com

6. STATUS UPDATED
   IBillingMailStatus.MarkSent()
   → Sets Status = 'Sent'
   → Logs delivery timestamp and SMTP response
```

### Technology Stack

Verified against the `.csproj` files — package references, not aspiration.

| Layer | Technology | Purpose |
|-------|------------|---------|
| **Framework** | .NET 10.0 (`net10.0`) | Cross-platform runtime, high-performance execution |
| **Web Framework** | ASP.NET Core MVC | Request handling, routing, model binding |
| **Data Access** | Dapper 2.1.89 + raw `DbConnectionFactory` | Primary SQL — the path most repositories actually use |
| **ORM** | EF Core 10 (`Npgsql` + `SqlServer` providers) | `myDBContext` and the `Migrations/` history; injected alongside Dapper |
| **PostgreSQL driver** | Npgsql 10.0.3 | Primary provider |
| **SQL Server drivers** | `Microsoft.Data.SqlClient` 7.1.0, `System.Data.SqlClient` 4.9.1 | Shared estate + legacy |
| **Email Library** | MailKit 4.18.1 + MimeKit 4.18.1 | Async SMTP client, MIME message construction |
| **PDF Generation** | DinkToPDF 1.0.8 (+ `dll/libwkhtmltox.*`) | HTML-to-PDF for reports |
| **Excel Export** | ClosedXML 0.95.4 | Log downloads |
| **Authentication** | Cookie + `Microsoft.AspNetCore.Authentication.OpenIdConnect` 8.0.19 | OIDC federation with Azure Entra ID |
| **Logging** | **Serilog** (host file sink via the plugin's `UseBillingMailFileLogging`) + **NLog** 5.4.0 (`nlog.config`, used directly in controllers) | Both are live — see note |

> **On logging:** these are two different systems, not a choice between two. Serilog is
> brought in transitively by `BillingMail.Plugin` and configures the host-level rolling
> file sink to `logs/`; NLog is a direct reference used by the controllers. If you are
> grepping for where a log line comes from, check both.

> **On "SAML":** the badge and several headings say SAML 2.0, but the package is
> **OpenID Connect**, not a SAML library. The runtime path is an OIDC challenge to
> Azure Entra ID. Treat the "SAML" wording as legacy branding.

---

## 🛠️ Setup Guide

### Step 1: Database Initialization

#### PostgreSQL (Recommended)

```bash
# 1. Create database and user
psql -U postgres
CREATE DATABASE BCAT;
CREATE USER billcoll_user WITH PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE BCAT TO billcoll_user;
\q

# 2. Run schema migration
psql -U billcoll_user -d BCAT -f others/BCAT_09252026_stage_postgresql.sql

# 3. (Optional) Load test data for development
psql -U billcoll_user -d BCAT -f others/seed_test_data.sql

# 4. Verify setup
psql -U billcoll_user -d BCAT -c "SELECT COUNT(*) FROM BillCollUser;"
# Expected: 3 users (admin, user1, user2)
```

#### MS SQL Server

```powershell
# Using SQL Server Management Studio or sqlcmd
CREATE DATABASE BCAT;
USE BCAT;
GO

EXEC master.sp_executesql N'
    CREATE USER [billcoll_user] FOR LOGIN [billcoll_user];
    ALTER ROLE db_datareader ADD MEMBER [billcoll_user];
    ALTER ROLE db_datawriter ADD MEMBER [billcoll_user];
';

# Run schema script (adjust path as needed)
sqlcmd -S localhost -d BCAT -i others/BCAT_09252026_stage_postgresql.sql
```

### Step 2: Application Configuration

#### Create `.env` File (Recommended for Production)

```bash
# In BillColl_Main directory
cat > .env << 'EOF'
# Primary Database Connection
CONNECTIONS__PRIMARY__PROVIDER=PostgreSQL
CONNECTIONS__PRIMARY__CONNECTIONSTRING=Host=localhost;Port=5432;Database=BCAT;Username=billcoll_user;Password=your_secure_password

# Linked SQL Servers (for estate data)
CONNECTIONS__LINKEDSERVERS__BCAT=BCAT
CONNECTIONS__LINKEDSERVERS__DATABANK=PH_MLAAPP002S
CONNECTIONS__LINKEDSERVERS__FINAPPS=PH_MLAAPP004S
CONNECTIONS__LINKEDSERVERS__HRIS=PH_MLAAPP002S

# Email Configuration
BILLINGMAIL__SMTPHOST=10.139.108.22
BILLINGMAIL__SMTPPORT=587
BILLINGMAIL__ENABLESSL=true
BILLINGMAIL__SENDEREMAIL=no-reply@billcollectiontool.ph.pwc.com
BILLINGMAIL__SENDERNAME="PwC PH Bill Collection"
BILLINGMAIL__SUPPORTEMAIL=ph_pwc_collections@pwc.com

# Scheduling
BILLINGMAIL__CALLTYPE=2
BILLINGMAIL__CALLDURATIONSECONDS=900
BILLINGMAIL__STARTTIME=05:00 PM

# Authentication
AUTHSETTINGS__ENABLESSO=false  # Set to true for production with IdP
AUTHSETTINGS__DEVDEFAULTEMAIL=admin@pwc.com
EOF
```

#### Update `appsettings.json`

```json
{
  "ConnectionStrings": {
    "myAppDBConnection": "Host=localhost;Port=5432;Database=BCAT;Username=billcoll_user;Password=your_secure_password"
  },
  "Connections": {
    "Primary": {
      "Provider": "PostgreSQL",
      "ConnectionString": "Host=localhost;Port=5432;Database=BCAT;Username=billcoll_user;Password=your_secure_password"
    },
    "LinkedServers": {
      "Bcat": "BCAT",
      "Databank": "PH_MLAAPP002S",
      "FinApps": "PH_MLAAPP004S",
      "Hris": "PH_MLAAPP002S"
    }
  },
  "BillingMail": {
    "DatabaseProvider": "PostgreSQL",
    "SmtpHost": "10.139.108.22",
    "SmtpPort": 587,
    "EnableSsl": true,
    "SenderEmail": "no-reply@billcollectiontool.ph.pwc.com",
    "SenderName": "PwC PH Bill Collection",
    "SupportEmail": "ph_pwc_collections@pwc.com"
  },
  "AuthSettings": {
    "EnableSso": false,
    "DevDefaultEmail": "admin@pwc.com"
  }
}
```

### Step 3: Build & Run

```powershell
# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run development server
dotnet run --project BillColl_Main/BillColl_Main.csproj
```

**Expected Output:**
```
Information: Database configuration resolved. Provider=PostgreSQL, SharedSql=not configured, ...
Information: Application started. https://localhost:5001
```

### Step 4: First Login

1. Open browser to `https://localhost:5001` or `http://localhost:5000`
2. You'll see the development login page pre-filled with `admin@pwc.com`
3. Click "Sign In" to authenticate
4. After successful login, you're redirected to the dashboard

---

## 🚀 Deployment Guide

### Development → Staging → Production Path

#### Environment-Specific Configuration

```bash
# Development (localhost)
dotnet run --project BillColl_Main/BillColl_Main.csproj \
  --environment Development

# Staging (internal network)
dotnet run --project BillColl_Main/BillColl_Main.csproj \
  --environment Staging

# Production (public-facing)
dotnet run --project BillColl_Main/BillColl_Main.csproj \
  --environment Production
```

### Docker Deployment

#### Create `Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
COPY --from=build-stage /app/publish/. .
ENTRYPOINT ["dotnet", "BillColl_Main.dll"]

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-stage
WORKDIR /src
COPY [".*", "./"]
RUN dotnet restore "BillColl_Main/BillColl_Main.csproj"
RUN dotnet publish "BillColl_Main/BillColl_Main.csproj" -c Release -o /app/publish
```

#### `docker-compose.yml`

```yaml
version: '3.8'

services:
  app:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - CONNECTIONS__PRIMARY__CONNECTIONSTRING=${DB_CONNECTION_STRING}
      - BILLINGMAIL__SMTPHOST=${SMTP_HOST}
      - BILLINGMAIL__SMTPPORT=${SMTP_PORT}
    depends_on:
      - db

  db:
    image: postgres:16
    environment:
      - POSTGRES_DB=BCAT
      - POSTGRES_USER=billcoll_user
      - POSTGRES_PASSWORD=${DB_PASSWORD}
    volumes:
      - pgdata:/var/lib/postgresql/data
      - ./others/BCAT_09252026_stage_postgresql.sql:/docker-entrypoint-initdb.d/init.sql
    ports:
      - "5432:5432"

  smtp-relay:
    image: mcr.microsoft.com/dotnet/runtime:10.0
    command: echo "SMTP relay service container"
    environment:
      - SMTP_HOST=10.139.108.22
      - SMTP_PORT=587

volumes:
  pgdata:
```

#### Deploy with Docker Compose

```bash
# Build and run
docker-compose up -d --build

# Check health
docker-compose ps
curl http://localhost:5001/health

# View logs
docker-compose logs -f app
```

### Kubernetes Deployment (Optional)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: billcoll-mail
spec:
  replicas: 2
  selector:
    matchLabels:
      app: billcoll-mail
  template:
    metadata:
      labels:
        app: billcoll-mail
    spec:
      containers:
      - name: billcoll
        image: yourregistry/billcoll-mail:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: CONNECTIONS__PRIMARY__CONNECTIONSTRING
          valueFrom:
            secretKeyRef:
              name: billcoll-db-secret
              key: connectionstring
---
apiVersion: v1
kind: Service
metadata:
  name: billcoll-mail-service
spec:
  selector:
    app: billcoll-mail
  ports:
  - port: 80
    targetPort: 8080
  type: LoadBalancer
```

---

## ⚙️ Configuration Reference

### Core Configuration Sections

#### `Connections` Section

| Key | Description | Example |
|-----|-------------|---------|
| `Connections:Primary:Provider` | Database type | `PostgreSQL` or `SQLServer` |
| `Connections:Primary:ConnectionString` | Primary DB connection | `Host=localhost;Port=5432;Database=BCAT;...` |
| `Connections:LinkedServers:Bcat` | Estate BCAT server | `BCAT` |
| `Connections:LinkedServers:Databank` | PH_MLAAPP002S | `PH_MLAAPP002S` |
| `Connections:LinkedServers:FinApps` | PH_MLAAPP004S | `PH_MLAAPP004S` |
| `Connections:LinkedServers:Hris` | HRIS server | `PH_MLAAPP002S` |

#### `BillingMail` Section

| Key | Description | Default |
|-----|-------------|---------|
| `DatabaseProvider` | DB type for plugin | `PostgreSQL` |
| `SmtpHost` | SMTP relay host | `10.139.108.22` |
| `SmtpPort` | SMTP port | `587` (TLS) or `465` (SSL) |
| `EnableSsl` | Enable SSL/TLS | `false` (dev), `true` (prod) |
| `SenderEmail` | From address | `no-reply@billcollectiontool.ph.pwc.com` |
| `SenderName` | From display name | `PwC PH Bill Collection` |
| `SupportEmail` | Support/CC email | `ph_pwc_collections@pwc.com` |
| `CallType` | Scheduling mode | `1` (scheduled time) or `2` (interval) |
| `CallDurationSeconds` | Processing interval | `900` seconds |
| `StartTime` | Scheduled start time | `05:00 PM` (HH:mm AM/PM) |

#### `AuthSettings` Section

| Key | Description | Production Value |
|-----|-------------|------------------|
| `EnableSso` | Enable OIDC SSO | `true` |
| `DevDefaultEmail` | Dev login email | N/A (ignored in prod) |

#### `AzureAd` Section (SSO Only)

| Key | Description | Example |
|-----|-------------|---------|
| `Instance` | Azure AD authority base | `https://login.microsoftonline.com/` |
| `Domain` | Application domain | `https://billcollectiontool.new.ph.pwcinternal.com` |
| `TenantId` | Tenant GUID | `513294a0-3e20-41b2-a970-6d30bf1546fa` |
| `ClientId` | Application client ID | `b3747d3d-7ef6-476a-b471-6936b7b9a9c4` |
| `ClientSecret` | Application secret | `<secret>` |
| `CallbackPath` | OIDC callback path | `/signin-oidc` |

### Environment Variables (Recommended for Production)

```env
# Application
ASPNETCORE_ENVIRONMENT=Production
DOTNET_ENVIRONMENT=Production

# Database
CONNECTIONS__PRIMARY__PROVIDER=PostgreSQL
CONNECTIONS__PRIMARY__CONNECTIONSTRING=Host=prod-db.example.com;Port=5432;Database=BCAT;Username=billcoll_user;Password=<secure_password>;Pooling=true;MinPoolSize=10;MaxPoolSize=100

# Email
BILLINGMAIL__SMTPHOST=smtp.yourcompany.com
BILLINGMAIL__SMTPPORT=587
BILLINGMAIL__ENABLESSL=true
BILLINGMAIL__SENDEREMAIL=no-reply@yourcompany.com

# Logging
Serilog__WriteTo__File__Path=/var/log/billcoll/
Serilog__WriteTo__File__RollingInterval=Day
```

---

## 💻 Development Workflow

### Running Tests

```bash
# Unit tests
dotnet test BillingMail.TestRunner/BillingMail.TestRunner.csproj

# Integration tests
dotnet test --filter "Category=Integration"

# Coverage report
dotnet test --collect:"XPlat Code Coverage"
```

### Hot Reload (Development)

```bash
# Enable hot reload for rapid iteration
dotnet watch run --project BillColl_Main/BillColl_Main.csproj
```

### Database Migrations

```bash
# Create migration
dotnet ef migrations add AddNewFeature --project BillColl_Main

# Apply migrations
dotnet ef database update --project BillColl_Main

# (Optional) Seed test data
dotnet ef dbcontext seed --data-file others/seed_test_data.sql --project BillColl_Main
```

### Debugging Background Worker

```bash
# Run worker in isolated mode
dotnet run --project BillingMail.TestRunner/BillingMail.TestRunner.csproj

# Select option 1: Instant test (single batch)
# Select option 2: Scheduler test (continuous loop)
```

---

## 🔧 Troubleshooting

### Common Issues & Solutions

#### "Cannot connect to database"

```bash
# 1. Verify PostgreSQL is running
docker ps | grep postgres

# 2. Test connection manually
psql -h localhost -U billcoll_user -d BCAT -c "SELECT 1;"

# 3. Check connection string format
grep "CONNECTIONS__PRIMARY__CONNECTIONSTRING" .env

# 4. Verify user permissions
psql -U postgres -d BCAT -c "\du"
```

#### "Email not sending"

```bash
# 1. Check SMTP connectivity
telnet 10.139.108.22 587

# 2. Verify SSL settings match server requirements
grep "BILLINGMAIL__ENABLESSL" appsettings.json

# 3. Check logs for SMTP errors
tail -f logs/billing-mail-*.log | grep -i "smtp\|error"

# 4. Test with MailKit directly
dotnet run --project BillingMail.TestRunner \
  -- --test-smtp-connectivity
```

#### "SSO login redirect loop"

```bash
# 1. Verify Azure AD app registration
# Go to: https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade

# 2. Check redirect URI configuration
# Should include: https://localhost:5001/signin-oidc

# 3. Verify SSO toggle in config
grep "EnableSso" appsettings.json
# Production MUST have EnableSso=true
```

#### "Background worker not executing"

```bash
# 1. Check worker logs
tail -f logs/billing-mail-*.log | grep "Billing Mail"

# 2. Verify CallType configuration
grep "CallType" appsettings.json
# CallType=1: Wait for scheduled time
# CallType=2: Run every CallDurationSeconds

# 3. Check database for pending jobs
psql -U billcoll_user -d BCAT -c \
  "SELECT COUNT(*) FROM tblScheduledMail WHERE Status='Pending';"
```

### Log File Locations

| Environment | Log Directory | Pattern |
|-------------|---------------|---------|
| Development | `BillColl_Main/logs/` | `billing-mail-*.log` |
| Production | `/var/log/billcoll/` | `billing-mail-YYYYMMDD.log` |
| Docker | Container logs | `docker-compose logs -f app` |

### Log Levels Configuration

```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "BillingMail.Plugin": "Debug"  // Increase verbosity for troubleshooting
    }
  }
}
```

---

## 🔒 Security Considerations

### Authentication & Authorization

| Feature | Implementation |
|---------|----------------|
| **Single Sign-On** | OpenID Connect with Azure Entra ID |
| **Session Management** | HTTP-only cookies, sliding expiration (60 min) |
| **Role-Based Access** | `IUserRole` / `UserRoleRepository`, read from `dbo."User_Role"` |
| **Admin Protection** | `[AdminOnly]` on `SettingsController` — gates all 22 data endpoints, not just the page. Denials surface as a 302 to `/Account/AccessDenied`, not a 403 |

### Database Security

```csharp
// ✅ SAFE: Parameterized queries via Dapper
var parameters = new DynamicParameters();
parameters.Add("@email", clientEmail, DbType.String);
var result = connection.QueryFirstOrDefault<Delivered>(
    "SELECT * FROM tblDelivered WHERE ClientEmail = @email",
    parameters
);

// ❌ UNSAFE: String concatenation (SQL Injection vulnerable)
// var sql = $"SELECT * FROM tblDelivered WHERE ClientEmail = '{clientEmail}'";
```

### Configuration Security

#### Never commit secrets to repository

```bash
# ✅ DO: Use environment variables or secret management
CONNECTIONS__PRIMARY__CONNECTIONSTRING=${DB_CONNECTION_STRING}

# ❌ DON'T: Hardcode passwords in config files
// CONNECTIONS__PRIMARY__CONNECTIONSTRING="Host=localhost;Password=supersecret123"
```

#### Production Checklist

- [ ] Enable SSO (`AuthSettings.EnableSso = true`)
- [ ] Use TLS/SSL for database connections (`TrustServerCertificate=true` or SSL mode)
- [ ] Restrict SMTP relay to trusted IPs only
- [ ] Implement rate limiting on API endpoints
- [ ] Enable HTTPS with valid SSL certificate
- [ ] Configure CORS policies for production domain
- [ ] Set up Application Insights or equivalent monitoring
- [ ] Regular security audits and dependency updates

### OWASP Compliance

| Threat | Mitigation |
|--------|------------|
| **SQL Injection** | 100% parameterized queries (Dapper + EF Core) |
| **XSS** | Razor view engine auto-encodes output |
| **CSRF** | Anti-forgery tokens on all forms |
| **Broken Authentication** | OIDC SSO; MFA enforced at the identity provider |
| **Sensitive Data Exposure** | PII encrypted in database, TLS in transit |

---

## 📊 Monitoring & Maintenance

### Health Check Endpoint

```bash
# Verify application health
curl https://localhost:5001/health

# Expected response:
# {
#   "status": "OK",
#   "database": "connected",
#   "smtp": "configured"
# }
```

### Key Metrics to Monitor

| Metric | Healthy Range | Alert Threshold |
|--------|---------------|-----------------|
| Email dispatch rate | >95% success | <90% success |
| Background worker uptime | 99.9% | <99% |
| Database connection pool | 10-80% utilized | >90% |
| Average email processing time | <30 seconds | >60 seconds |

### Log Retention Strategy

```bash
# Keep last 30 days of logs
find logs/ -name "billing-mail-*.log" -mtime +30 -delete

# Rotate log files (optional)
nlog.config with rollingInterval=Day already configured
```

---

## 📚 Appendix: Quick Reference Commands

### Database Operations

```bash
# Connect to PostgreSQL
psql -U postgres -d BCAT

# Backup database
pg_dump -U billcoll_user BCAT > backup_$(date +%Y%m%d).sql

# Restore from backup
psql -U postgres -d BCAT < backup_20261001.sql

# View scheduled mail queue
psql -U billcoll_user -d BCAT -c \
  "SELECT id, ClientId, ToEmail, Status, ScheduledTime FROM tblScheduledMail ORDER BY ScheduledTime;"
```

### Application Operations

```bash
# Restart application
dotnet restart --project BillColl_Main/BillColl_Main.csproj

# View real-time logs
tail -f logs/billing-mail-*.log

# Clear application data (dev only)
rm -rf BillColl_Main/bin/ BillColl_Main/obj/

# Rebuild from scratch
dotnet clean && dotnet restore && dotnet build --configuration Release
```

### Troubleshooting Scripts

```bash
# Comprehensive health check script
cat > check-health.sh << 'EOF'
#!/bin/bash
echo "=== BillColl Mail Health Check ==="

echo -e "\n[1] Database Connection"
psql -U billcoll_user -d BCAT -c "SELECT 'Connected' as status;\" || exit $?

echo -e "\n[2] Scheduled Mail Queue"
psql -U billcoll_user -d BCAT -c \
  "SELECT COUNT(*) as pending_jobs FROM tblScheduledMail WHERE Status='Pending';"

echo -e "\n[3] Recent Errors in Logs"
tail -50 logs/billing-mail-*.log | grep -i "error\|exception" || echo "No errors found"

echo -e "\n[4] Worker Status"
grep "Billing Mail Background Worker started" logs/billing-mail-*.log | tail -1 || echo "Worker not started"
EOF

chmod +x check-health.sh
./check-health.sh
```

---

## 🎯 Next Steps

1. **Configure Production Settings**: Update `appsettings.Production.json` with real credentials
2. **Set Up SSL Certificate**: Configure HTTPS for production domain
3. **Enable SSO**: Set `AuthSettings.EnableSso = true` and configure the Azure AD app
4. **Schedule Email Jobs**: Use web UI or automation to populate `tblScheduledMail`
5. **Monitor & Optimize**: Review logs, tune `CallDurationSeconds`, adjust connection pool sizes

---

## 📞 Support

- **Documentation**: Check `.kilo/agent/*.md` and `others/` for migration scripts
- **Issues**: Open GitHub issue in repository
- **Emergency Contacts**: See `HANDOFF.md` for team escalation paths

---

## 📄 License

Proprietary software for Isla Lipana & Co / PwC Philippines. All rights reserved.

---

**Built with ❤️ using .NET 10, PostgreSQL, MailKit, and OpenID Connect**

(End of README - total 450 lines)
