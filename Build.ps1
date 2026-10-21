[CmdletBinding()]
param(
    [ValidateRange(2022,2027)][int[]]$Version = @(2025),
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [switch]$All,
    [string]$ProductVersion
)
$ErrorActionPreference = 'Stop'
if ($All) { $Version = 2022..2027 }
foreach ($year in $Version) {
    $args = @((Join-Path $PSScriptRoot 'SiteDesigner.csproj'), '-c', $Configuration, "-p:RevitVersion=$year", '--nologo', '-v:minimal')
    if ($ProductVersion) { $args += "-p:ProductVersion=$ProductVersion" }
    & dotnet build @args
    if ($LASTEXITCODE -ne 0) { throw "Revit $year build failed." }
}
