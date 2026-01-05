# Run Web and API Projects
# This script starts the Web application and API simultaneously
# The Angular SPA is served from the Web application

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting CommonArchitecture Projects" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get the script directory (which is the solution root)
$solutionRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

# Project paths
$apiPath = Join-Path $solutionRoot "src\CommonArchitecture.API"
$webPath = Join-Path $solutionRoot "src\CommonArchitecture.Web"

# Check if projects exist
if (-not (Test-Path $apiPath)) {
    Write-Host "ERROR: API project not found at $apiPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $webPath)) {
    Write-Host "ERROR: Web project not found at $webPath" -ForegroundColor Red
    exit 1
}

Write-Host "API Project: $apiPath" -ForegroundColor Green
Write-Host "Web Project: $webPath" -ForegroundColor Green
Write-Host ""

# Start API in a new window
Write-Host "Starting API (https://localhost:7220)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$apiPath'; Write-Host 'API Server Starting...' -ForegroundColor Cyan; dotnet run --launch-profile https"

# Wait a bit for API to start
Write-Host "Waiting 5 seconds for API to initialize..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Start Web in a new window
Write-Host "Starting Web Application (https://localhost:7296)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$webPath'; Write-Host 'Web Application Starting...' -ForegroundColor Cyan; dotnet run --launch-profile https"

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Projects are starting!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "API:         https://localhost:7220" -ForegroundColor Cyan
Write-Host "Web + Angular: https://localhost:7296" -ForegroundColor Cyan
Write-Host "Angular SPA:  https://localhost:7296/angular" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to stop all projects..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Stop all dotnet processes (optional - user can close windows manually)
Write-Host "To stop the servers, close the PowerShell windows." -ForegroundColor Yellow
