<#
Run API, Web, and Angular dev server in separate PowerShell windows.
Usage:
 Open PowerShell in repository root and run:
 .\run-all.ps1
Notes:
 - Requires dotnet, npm and PowerShell available on PATH.
 - Angular uses `npm start` (reads package.json). If you don't have global @angular/cli, npm start will use local CLI.
 - If execution policy prevents running scripts, run PowerShell as admin and: Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
#>

param(
 [switch] $ReinstallNodeModules
)

$root = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
Set-Location $root

$apiPath = Join-Path $root 'src\CommonArchitecture.API'
$webPath = Join-Path $root 'src\CommonArchitecture.Web'
$ngPath = Join-Path $root 'ecommerce-angular'

$logsDir = Join-Path $root 'run-logs'
if (!(Test-Path $logsDir)) { New-Item -ItemType Directory -Path $logsDir | Out-Null }

$pidsFile = Join-Path $root 'run-logs\run-all.pids.json'

Write-Host "Starting services (background). Logs in: $logsDir"

# Helper to start a process and redirect output to files
function Start-BackgroundProcess($filePath, $arguments, $workingDirectory, $outLog, $errLog) {
 $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
 $startInfo.FileName = $filePath
 $startInfo.Arguments = $arguments
 $startInfo.WorkingDirectory = $workingDirectory
 $startInfo.RedirectStandardOutput = $true
 $startInfo.RedirectStandardError = $true
 $startInfo.UseShellExecute = $false
 $startInfo.CreateNoWindow = $true

 $process = [System.Diagnostics.Process]::Start($startInfo)

 # Async copy output to files
 $outStream = [System.IO.File]::Open($outLog, [System.IO.FileMode]::Append, [System.IO.FileAccess]::Write, [System.IO.FileShare]::ReadWrite)
 $errStream = [System.IO.File]::Open($errLog, [System.IO.FileMode]::Append, [System.IO.FileAccess]::Write, [System.IO.FileShare]::ReadWrite)

 $stdOutTask = $process.StandardOutput.BaseStream.BeginRead((New-Object byte[]4096),0,0, $null, $null)

 Start-Job -ScriptBlock {
 param($procId, $outPath, $errPath)
 $proc = [System.Diagnostics.Process]::GetProcessById($procId)
 $outWriter = New-Object System.IO.StreamWriter($outPath, $true)
 $errWriter = New-Object System.IO.StreamWriter($errPath, $true)
 $outWriter.AutoFlush = $true
 $errWriter.AutoFlush = $true
 try {
 while (-not $proc.HasExited) {
 $line = $proc.StandardOutput.ReadLine()
 if ($line -ne $null) { $outWriter.WriteLine($line) }
 $err = $proc.StandardError.ReadLine()
 if ($err -ne $null) { $errWriter.WriteLine($err) }
 Start-Sleep -Milliseconds50
 }
 # drain remaining
 while (($line = $proc.StandardOutput.ReadLine()) -ne $null) { $outWriter.WriteLine($line) }
 while (($err = $proc.StandardError.ReadLine()) -ne $null) { $errWriter.WriteLine($err) }
 }
 catch {
 # ignore
 }
 finally {
 $outWriter.Close(); $errWriter.Close();
 }
 } -ArgumentList $process.Id, $outLog, $errLog | Out-Null

 return $process.Id
}

# Start API
$apiOut = Join-Path $logsDir 'api.out.log'
$apiErr = Join-Path $logsDir 'api.err.log'
Write-Host "Starting API: dotnet run --project $apiPath"
$apiPid = Start-BackgroundProcess 'dotnet' "run --project `"$apiPath`"" $apiPath $apiOut $apiErr
Write-Host "API PID: $apiPid"

Start-Sleep -Milliseconds500

# Start Web
$webOut = Join-Path $logsDir 'web.out.log'
$webErr = Join-Path $logsDir 'web.err.log'
Write-Host "Starting Web: dotnet run --project $webPath"
$webPid = Start-BackgroundProcess 'dotnet' "run --project `"$webPath`"" $webPath $webOut $webErr
Write-Host "Web PID: $webPid"

Start-Sleep -Milliseconds500

# Optional: reinstall node modules
if ($ReinstallNodeModules) {
 Write-Host "Running npm install in $ngPath"
 Push-Location $ngPath
 npm install
 Pop-Location
}

# Start Angular
$ngOut = Join-Path $logsDir 'ng.out.log'
$ngErr = Join-Path $logsDir 'ng.err.log'
Write-Host "Starting Angular: npm start (in $ngPath)"
$ngPid = Start-BackgroundProcess 'npm' "start" $ngPath $ngOut $ngErr
Write-Host "Angular PID: $ngPid"

# Save PIDs
$pids = @{ Api = $apiPid; Web = $webPid; Angular = $ngPid }
$pids | ConvertTo-Json | Out-File -FilePath $pidsFile -Encoding utf8

Write-Host "Started all processes. Use the PIDs file at: $pidsFile"
Write-Host "Tails:"
Write-Host " API logs: $apiOut"
Write-Host " Web logs: $webOut"
Write-Host " NG logs: $ngOut"

Write-Host "To stop these processes run:"
Write-Host " .\run-all.ps1 -Stop"

# Support stopping
if ($PSBoundParameters.ContainsKey('Stop')) {
 # not used here; placeholder
}
