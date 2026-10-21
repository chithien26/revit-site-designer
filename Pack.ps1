[CmdletBinding()]
param(
    [ValidateRange(2022,2027)][int]$Version = 2025,
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [string]$NativeLibrary,
    [string]$WhitelistFile,
    [string]$ContentRoot = "$env:ProgramData\Autodesk\Site Designer for Revit 2020",
    [string]$InnoCompiler,
    [switch]$SkipBuild,
    [string]$ProductVersion
)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$versionFile = Join-Path $root 'version.txt'
if (-not $ProductVersion -and (Test-Path -LiteralPath $versionFile)) {
    $ProductVersion = (Get-Content -LiteralPath $versionFile -Raw).Trim()
}
if (-not $ProductVersion) { throw 'Supply -ProductVersion (e.g. 1.0.1). Ask the user if they did not specify it.' }
if (-not $SkipBuild) { & (Join-Path $root 'Build.ps1') -Version $Version -Configuration $Configuration -ProductVersion $ProductVersion }

$framework = if ($Version -le 2024) { 'net48' } elseif ($Version -eq 2027) { 'net10.0-windows' } else { 'net8.0-windows' }
$buildRoot = Join-Path $root "bin\$Configuration\$Version\$framework"
$dll = Join-Path $buildRoot 'SiteDesigner.dll'
if (-not (Test-Path -LiteralPath $dll)) { throw "Build output missing: $dll" }

if (-not $NativeLibrary) {
    $nativeCandidates = @(
        (Join-Path $root 'Dependencies\Native\SitesDLLWrapper.dll'),
        (Join-Path $root 'bin\Release2025\2025\net8.0-windows\SitesDLLWrapper.dll'),
        (Join-Path $buildRoot 'SitesDLLWrapper.dll'),
        "$env:ProgramFiles\Autodesk\Revit Site Designer Extension 2020\SitesDLLWrapper.dll"
    )
    $NativeLibrary = $nativeCandidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
}
if (-not $NativeLibrary) { throw 'Supply -NativeLibrary with the licensed x64 SitesDLLWrapper.dll.' }
if (-not $WhitelistFile) {
    $companionWhitelist = Join-Path (Split-Path $NativeLibrary -Parent) 'whitelist.xml'
    if (Test-Path -LiteralPath $companionWhitelist -PathType Leaf) { $WhitelistFile = $companionWhitelist }
}
$contentXml = Join-Path $ContentRoot 'Data\Support\SITEDESIGNER_enu.xml'
if (-not (Test-Path -LiteralPath $contentXml)) { throw "Supply -ContentRoot with the licensed 2020 content folder containing Data\Support and Data\Library." }

$outRoot = Join-Path $root "artifacts\installer"
$staging = Join-Path $outRoot 'staging'
$payload = Join-Path $staging 'SiteDesigner'
if (Test-Path -LiteralPath $staging) { Remove-Item -LiteralPath $staging -Recurse -Force }
New-Item -ItemType Directory -Force -Path $payload | Out-Null

Get-ChildItem -LiteralPath $buildRoot | Where-Object { $_.Name -notin @('RevitAPI.dll','RevitAPIUI.dll','AdWindows.dll','UIFramework.dll','SitesDLLWrapper.dll','whitelist.xml') } | Copy-Item -Destination $payload -Recurse -Force
Copy-Item -LiteralPath $NativeLibrary -Destination (Join-Path $payload 'SitesDLLWrapper.dll') -Force
if ($WhitelistFile) { Copy-Item -LiteralPath $WhitelistFile -Destination (Join-Path $payload 'whitelist.xml') -Force }
Copy-Item -LiteralPath (Join-Path $ContentRoot 'Data') -Destination $payload -Recurse -Force
$stringsFile = Join-Path $payload 'Data\Support\SITEDESIGNER_enu.xml'
if (Test-Path -LiteralPath $stringsFile) {
    $xml = [System.IO.File]::ReadAllText($stringsFile)
    $xml = $xml -replace 'id="LandcaddSitesRibbonTab" value="Site Designer"', "id=`"LandcaddSitesRibbonTab`" value=`"Site Designer $Version`""
    [System.IO.File]::WriteAllText($stringsFile, $xml)
}

$addin = @"
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>Site Designer $Version</Name>
    <Assembly>SiteDesigner\SiteDesigner.dll</Assembly>
    <AddInId>BE8C3528-82C3-4A7C-8096-94A8DA602025</AddInId>
    <FullClassName>SITES.ExternalApplication</FullClassName>
    <VendorId>SDGN</VendorId>
    <VendorDescription>Site Designer multi-version migration</VendorDescription>
  </AddIn>
</RevitAddIns>
"@
Set-Content -LiteralPath (Join-Path $staging 'SiteDesigner.addin') -Value $addin -Encoding utf8

$zip = Join-Path $outRoot "SiteDesigner-$Version.zip"
if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
try {
    Compress-Archive -Path (Join-Path $staging '*') -DestinationPath $zip -Force
    Write-Output "Zip package: $zip"
} catch {
    Write-Output "Zip skipped: $($_.Exception.Message)"
}
Write-Output "Staged payload: $staging"

if (-not $InnoCompiler) {
    $InnoCompiler = @(
        "${env:ProgramFiles(x86)}\Inno Setup 7\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 7\ISCC.exe",
        "${env:LOCALAPPDATA}\Programs\Inno Setup 7\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
        "${env:LOCALAPPDATA}\Programs\Inno Setup 6\ISCC.exe"
    ) | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
}
if (-not $InnoCompiler) {
    Write-Output 'EXE not built: install Inno Setup 6 (https://jrsoftware.org/isinfo.php), then re-run .\Pack.ps1'
    Write-Output 'MSI: install WiX (`dotnet tool install --global wix`) if you need an MSI instead of an EXE.'
    return
}

$iss = Join-Path $root 'setup\SiteDesigner.iss'
Set-Content -LiteralPath $versionFile -Value $ProductVersion -Encoding ascii
$exeName = "SiteDesigner-$Version-Setup-$ProductVersion"
& $InnoCompiler $iss `
    "/DRevitVersion=$Version" `
    "/DAppVersion=$ProductVersion" `
    "/DStagingDir=$staging" `
    "/F$exeName"
if ($LASTEXITCODE -ne 0) { throw "Inno Setup compile failed." }
Write-Output "EXE installer: $(Join-Path $outRoot "$exeName.exe")"
Write-Output 'Close Revit before running the installer. First launch may prompt to load this unsigned add-in.'
