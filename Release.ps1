[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ProductVersion,
    [ValidateRange(2022, 2027)][int]$Version = 2025,
    [switch]$SkipPack,
    [switch]$Draft
)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
Set-Location $root

$ProductVersion = $ProductVersion.Trim().TrimStart('v', 'V')
if ($ProductVersion -notmatch '^\d+(\.\d+){1,3}$') {
    throw "ProductVersion must look like 1.0.2 (got '$ProductVersion')."
}

function Get-GhExe {
    $cmd = Get-Command gh -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    @(
        "$env:ProgramFiles\GitHub CLI\gh.exe",
        "${env:ProgramFiles(x86)}\GitHub CLI\gh.exe",
        "$env:LOCALAPPDATA\GitHub CLI\gh.exe"
    ) | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
}

$gh = Get-GhExe
if (-not $gh) {
    throw 'GitHub CLI (gh) is not installed. Install from https://cli.github.com/ then run: gh auth login'
}

$exeName = "SiteDesigner-$Version-Setup-$ProductVersion.exe"
$exe = Join-Path $root "artifacts\installer\$exeName"
if (-not $SkipPack -or -not (Test-Path -LiteralPath $exe)) {
    & (Join-Path $root 'Pack.ps1') -Version $Version -ProductVersion $ProductVersion
    if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { throw "Pack.ps1 failed ($LASTEXITCODE)." }
}
if (-not (Test-Path -LiteralPath $exe)) { throw "Installer missing: $exe" }

$tag = "v$ProductVersion"
$notes = @"
Site Designer $Version $ProductVersion

Installer: $exeName
Close Revit before installing.
"@

$ghArgs = @(
    'release', 'create', $tag, $exe,
    '--repo', 'chithien26/revit-site-designer',
    '--title', "Site Designer $ProductVersion",
    '--notes', $notes
)
if ($Draft) { $ghArgs += '--draft' }

& $gh @ghArgs
if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { throw "gh release create failed ($LASTEXITCODE)." }

$url = & $gh release view $tag --repo chithien26/revit-site-designer --json url --jq .url
Write-Output "RELEASE $url"
Write-Output "ASSET $exeName"
