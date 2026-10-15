[CmdletBinding()]
param(
    [ValidateRange(2022,2027)][int[]]$Version = @(2025),
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [switch]$All
)
$ErrorActionPreference = 'Stop'
if ($All) { $Version = 2022..2027 }
foreach ($year in $Version) {
    & dotnet build (Join-Path $PSScriptRoot 'SiteDesigner.csproj') -c $Configuration "-p:RevitVersion=$year" --nologo -v:minimal
    if ($LASTEXITCODE -ne 0) { throw "Revit $year build failed." }
}
