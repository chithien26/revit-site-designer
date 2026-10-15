[CmdletBinding()]
param(
    [ValidateRange(2022,2027)][int]$Version = 2025,
    [ValidateRange(60,900)][int]$TimeoutSeconds = 300
)
$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
$revitExe = Join-Path $env:ProgramFiles "Autodesk\Revit $Version\Revit.exe"
if (-not (Test-Path -LiteralPath $revitExe)) { throw "Revit $Version is not installed." }
if (Get-Process Revit -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $revitExe }) { throw "Close Revit $Version before running this isolated integration test." }
& (Join-Path $repository 'Install.ps1') -Version $Version
& dotnet build (Join-Path $PSScriptRoot 'RevitSmoke\RevitSmoke.csproj') -c Release "-p:RevitVersion=$Version" --nologo -v:minimal
if ($LASTEXITCODE -ne 0) { throw 'Integration harness build failed.' }
$framework = if ($Version -le 2024) { 'net48' } elseif ($Version -eq 2027) { 'net10.0-windows' } else { 'net8.0-windows' }
$smokeAssembly = [System.Security.SecurityElement]::Escape((Join-Path $PSScriptRoot "RevitSmoke\bin\Release\$framework\RevitSmoke.dll"))
$manifestPath = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$Version\zz.SiteDesigner.Smoke.addin"
if (Test-Path -LiteralPath $manifestPath) { throw "An existing smoke manifest must be removed first: $manifestPath" }
$resultFolder = Join-Path $env:LOCALAPPDATA "SiteDesigner\RevitSmoke\$($Version - 2000)"
$resultPath = Join-Path $resultFolder 'result.txt'
if (Test-Path -LiteralPath $resultPath) { Move-Item -LiteralPath $resultPath -Destination "$resultPath.$([DateTime]::Now.ToString('yyyyMMdd-HHmmss')).bak" }
$manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns><AddIn Type="Application"><Name>Site Designer Smoke Test</Name><Assembly>$smokeAssembly</Assembly><AddInId>9224EB56-15B0-49A6-A029-F6151D862345</AddInId><FullClassName>RevitSmoke.SmokeApplication</FullClassName><VendorId>SDGN</VendorId><VendorDescription>Temporary development verification</VendorDescription></AddIn></RevitAddIns>
"@
try {
    $manifest | Set-Content -LiteralPath $manifestPath -Encoding utf8
    Write-Output 'Revit may ask you to load these locally built unsigned assemblies. The harness uses its own generated project and exits its test session.'
    $testProcess = Start-Process -FilePath $revitExe -ArgumentList '/nosplash','/language','ENU' -WindowStyle Hidden -PassThru
    $watch = [Diagnostics.Stopwatch]::StartNew()
    while (-not $testProcess.HasExited -and $watch.Elapsed.TotalSeconds -lt $TimeoutSeconds) { Start-Sleep -Seconds 1 }
    if (-not $testProcess.HasExited) { throw "Test timed out. Check this test session for a Revit dialog. Process ID: $($testProcess.Id)" }
    if (-not (Test-Path -LiteralPath $resultPath)) { throw 'Revit exited without a test result.' }
    $result = Get-Content -LiteralPath $resultPath -Raw
    $result
    $artifacts = Join-Path $repository 'artifacts'
    New-Item -ItemType Directory -Force -Path $artifacts | Out-Null
    Copy-Item -LiteralPath $resultPath -Destination (Join-Path $artifacts "revit-$Version-smoke.txt") -Force
    $screenPath = Join-Path $resultFolder 'settings.png'
    if (Test-Path -LiteralPath $screenPath) { Copy-Item -LiteralPath $screenPath -Destination (Join-Path $artifacts "settings-revit-$Version.png") -Force }
    if ($result -notmatch '(?m)^SUCCESS:') { throw 'Revit-hosted checks failed; see the result above.' }
}
finally {
    if (Test-Path -LiteralPath $manifestPath) { Remove-Item -LiteralPath $manifestPath }
}
