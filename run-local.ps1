<#
    Local demo launcher for BillColl_Main + the BillingMail plugin.

    The plugin runs as a hosted background service inside the web application, so
    starting the web app starts both. Use -Plugin to run the plugin standalone instead.

    Requires: PostgreSQL listening on localhost:5432 with the BCAT database, and the
    connection string in user-secrets (set once, see HANDOFF.md section 4.1).
#>
[CmdletBinding()]
param(
    [switch]$Plugin
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$port = 5000

if ($Plugin) {
    Write-Host 'BillingMail plugin only (single batch).' -ForegroundColor Cyan
    dotnet run --project "$root\BillingMail.TestRunner\BillingMail.TestRunner.csproj" -p:UseAppHost=false -- 1
    return
}

# A second instance dies with a bare "address already in use" and a wall of stack traces.
# Check first and say what to do instead.
$inUse = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue
if ($inUse) {
    $owner = $inUse | Select-Object -First 1
    $proc = Get-Process -Id $owner.OwningProcess -ErrorAction SilentlyContinue
    Write-Host ''
    Write-Host "Port $port is already in use by process $($owner.OwningProcess) ($($proc.ProcessName))." -ForegroundColor Red
    Write-Host 'BillColl_Main is probably already running. Open http://localhost:5000 instead.'
    Write-Host ''
    Write-Host 'To stop it and start a fresh instance:' -ForegroundColor Yellow
    Write-Host "  Stop-Process -Id $($owner.OwningProcess) -Force" -ForegroundColor Yellow
    Write-Host ''
    exit 1
}

# Development is what loads user-secrets. --no-launch-profile is used so the port
# stays http://localhost:5000 rather than the profile's https://localhost:5001.
$env:ASPNETCORE_ENVIRONMENT = 'Development'

Write-Host ''
Write-Host 'BillColl_Main + BillingMail plugin' -ForegroundColor Green
Write-Host '  Web      http://localhost:5000' -ForegroundColor Green
Write-Host '  Login    admin@pwc.com (local login, no password)' -ForegroundColor Green
Write-Host '  Emails   written to captured_emails\ (SMTP relay is unreachable locally)' -ForegroundColor Green
Write-Host '  Logs     BillColl_Main\logs\' -ForegroundColor Green
Write-Host ''

dotnet run --project "$root\BillColl_Main\BillColl_Main.csproj" --no-launch-profile
