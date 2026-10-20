[CmdletBinding()]
param(
    [ValidateRange(2022,2027)][int]$Version = 2025,
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [string]$NativeLibrary,
    [string]$WhitelistFile,
    [string]$ContentRoot = "$env:ProgramData\Autodesk\Site Designer for Revit 2020",
    [switch]$SkipBuild
)
$ErrorActionPreference = 'Stop'
if (-not $SkipBuild) { & (Join-Path $PSScriptRoot 'Build.ps1') -Version $Version -Configuration $Configuration }
$framework = if ($Version -le 2024) { 'net48' } elseif ($Version -eq 2027) { 'net10.0-windows' } else { 'net8.0-windows' }
$buildRoot = Join-Path $PSScriptRoot "bin\$Configuration\$Version\$framework"
if (-not (Test-Path -LiteralPath (Join-Path $buildRoot 'SiteDesigner.dll'))) { throw "Build output missing: $buildRoot" }
if (-not $NativeLibrary) {
    $nativeCandidates = @(
        (Join-Path $PSScriptRoot 'Dependencies\Native\SitesDLLWrapper.dll'),
        (Join-Path $PSScriptRoot 'bin\Release2025\2025\net8.0-windows\SitesDLLWrapper.dll'),
        (Join-Path $buildRoot 'SitesDLLWrapper.dll'),
        "$env:ProgramFiles\Autodesk\Revit Site Designer Extension 2020\SitesDLLWrapper.dll"
    )
    $NativeLibrary = $nativeCandidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
}
if (-not $NativeLibrary) { throw 'Supply -NativeLibrary with the licensed x64 SitesDLLWrapper.dll.' }
if (-not (Test-Path -LiteralPath $NativeLibrary)) { throw "Supply -NativeLibrary with the licensed x64 SitesDLLWrapper.dll." }
if (-not $WhitelistFile) {
    $companionWhitelist = Join-Path (Split-Path $NativeLibrary -Parent) 'whitelist.xml'
    if (Test-Path -LiteralPath $companionWhitelist -PathType Leaf) { $WhitelistFile = $companionWhitelist }
}
if ($WhitelistFile -and -not (Test-Path -LiteralPath $WhitelistFile -PathType Leaf)) { throw "Whitelist file not found: $WhitelistFile" }
if (-not (Test-Path -LiteralPath (Join-Path $ContentRoot 'Data\Support\SITEDESIGNER_enu.xml'))) { throw "Supply -ContentRoot with the licensed 2020 content folder containing Data\Support and Data\Library." }
$addinRoot = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$Version"
$destination = Join-Path $addinRoot 'SiteDesigner'
New-Item -ItemType Directory -Force -Path $destination | Out-Null
Get-ChildItem -LiteralPath $buildRoot | Where-Object { $_.Name -notin @('RevitAPI.dll','RevitAPIUI.dll','AdWindows.dll','UIFramework.dll','SitesDLLWrapper.dll','whitelist.xml') } | Copy-Item -Destination $destination -Recurse -Force
Copy-Item -LiteralPath $NativeLibrary -Destination (Join-Path $destination 'SitesDLLWrapper.dll') -Force
if ($WhitelistFile) { Copy-Item -LiteralPath $WhitelistFile -Destination (Join-Path $destination 'whitelist.xml') -Force }
Copy-Item -LiteralPath (Join-Path $ContentRoot 'Data') -Destination $destination -Recurse -Force
$stringsFile = Join-Path $destination 'Data\Support\SITEDESIGNER_enu.xml'
if (Test-Path -LiteralPath $stringsFile) {
    $xml = [System.IO.File]::ReadAllText($stringsFile)
    $xml = $xml -replace 'id="LandcaddSitesRibbonTab" value="Site Designer"', "id=`"LandcaddSitesRibbonTab`" value=`"Site Designer $Version`""
    [System.IO.File]::WriteAllText($stringsFile, $xml)
}
$assemblyPath = [System.Security.SecurityElement]::Escape((Join-Path $destination 'SiteDesigner.dll'))
$manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>Site Designer $Version</Name>
    <Assembly>$assemblyPath</Assembly>
    <AddInId>BE8C3528-82C3-4A7C-8096-94A8DA602025</AddInId>
    <FullClassName>SITES.ExternalApplication</FullClassName>
    <VendorId>SDGN</VendorId>
    <VendorDescription>Site Designer multi-version migration</VendorDescription>
  </AddIn>
</RevitAddIns>
"@
$manifestPath = Join-Path $addinRoot 'SiteDesigner.addin'
$manifest | Set-Content -LiteralPath $manifestPath -Encoding utf8
Write-Output "Installed Revit $Version add-in: $manifestPath"
Write-Output "Native library source: $NativeLibrary"
if ($WhitelistFile) { Write-Output "Whitelist source: $WhitelistFile" }
Write-Output 'Restart the corresponding Revit version to load the add-in.'
