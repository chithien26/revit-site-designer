[CmdletBinding()]
param(
    [ValidateRange(2022,2027)][int]$Version = 2025,
    [string]$ProductVersion = '0.1.0',
    [string]$StagingDir
)
$ErrorActionPreference = 'Stop'
if (-not $PSScriptRoot) { $PSScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path }
if (-not $StagingDir) { $StagingDir = Join-Path $PSScriptRoot '..\artifacts\installer\staging' }
$StagingDir = [System.IO.Path]::GetFullPath($StagingDir)
$payload = Join-Path $StagingDir 'SiteDesigner'
$addin = Join-Path $StagingDir 'SiteDesigner.addin'
if (-not (Test-Path -LiteralPath $payload)) { throw "Staging payload missing: $payload. Run .\Pack.ps1 first." }
if (-not (Test-Path -LiteralPath $addin)) { throw "Staging add-in missing: $addin. Run .\Pack.ps1 first." }

function New-WixId([string]$prefix, [string]$value) {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($value)
    $hash = [System.BitConverter]::ToString([System.Security.Cryptography.SHA1]::Create().ComputeHash($bytes)).Replace('-', '')
    return ($prefix + $hash.Substring(0, 16))
}

$dirs = New-Object System.Collections.Generic.HashSet[string]
$files = Get-ChildItem -LiteralPath $payload -Recurse -File
foreach ($file in $files) {
    $rel = $file.DirectoryName.Substring($payload.Length).TrimStart('\')
    if ($rel) {
        $parts = $rel -split '\\'
        $acc = ''
        foreach ($part in $parts) {
            $acc = if ($acc) { "$acc\$part" } else { $part }
            [void]$dirs.Add($acc)
        }
    }
}

$dirXml = New-Object System.Text.StringBuilder
[void]$dirXml.AppendLine('            <Directory Id="INSTALLFOLDER" Name="SiteDesigner">')
foreach ($rel in ($dirs | Sort-Object { $_.Length }, { $_ })) {
    $id = New-WixId 'd' $rel
    $name = Split-Path $rel -Leaf
    $parent = Split-Path $rel -Parent
    $parentId = if ($parent) { New-WixId 'd' $parent } else { 'INSTALLFOLDER' }
    [void]$dirXml.AppendLine("              <Directory Id=`"$id`" Name=`"$name`" />")
    # Nested later via DirectoryRef
}
# Better: emit nested tree
$dirXml = New-Object System.Text.StringBuilder
function Write-DirTree([string]$parentRel, [int]$indent) {
    $pad = ' ' * $indent
    $children = $dirs | Where-Object {
        $parent = Split-Path $_ -Parent
        if (-not $parentRel) { return -not $parent }
        return $parent -eq $parentRel
    } | Sort-Object
    foreach ($child in $children) {
        $id = New-WixId 'd' $child
        $name = Split-Path $child -Leaf
        $hasKids = [bool]($dirs | Where-Object { (Split-Path $_ -Parent) -eq $child })
        $filesHere = @($files | Where-Object { $_.DirectoryName.Substring($payload.Length).TrimStart('\') -eq $child })
        [void]$dirXml.AppendLine("$pad<Directory Id=`"$id`" Name=`"$name`">")
        foreach ($file in $filesHere) {
            $cid = New-WixId 'c' $file.FullName
            $fid = New-WixId 'f' $file.FullName
            $src = $file.FullName
            [void]$dirXml.AppendLine("$pad  <Component Id=`"$cid`" Guid=`"*`">")
            [void]$dirXml.AppendLine("$pad    <File Id=`"$fid`" Source=`"$src`" KeyPath=`"yes`" />")
            [void]$dirXml.AppendLine("$pad  </Component>")
        }
        Write-DirTree $child ($indent + 2)
        [void]$dirXml.AppendLine("$pad</Directory>")
    }
}

$rootFiles = @($files | Where-Object { $_.DirectoryName -eq $payload })
[void]$dirXml.AppendLine('            <Directory Id="INSTALLFOLDER" Name="SiteDesigner">')
foreach ($file in $rootFiles) {
    $cid = New-WixId 'c' $file.FullName
    $fid = New-WixId 'f' $file.FullName
    [void]$dirXml.AppendLine("              <Component Id=`"$cid`" Guid=`"*`">")
    [void]$dirXml.AppendLine("                <File Id=`"$fid`" Source=`"$($file.FullName)`" KeyPath=`"yes`" />")
    [void]$dirXml.AppendLine('              </Component>')
}
Write-DirTree '' 14
[void]$dirXml.AppendLine('            </Directory>')

$wxsPath = Join-Path $PSScriptRoot 'SiteDesigner.generated.wxs'
$wxs = @"
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
  <Package Name="Site Designer $Version"
           Manufacturer="SDGN"
           Version="$ProductVersion"
           UpgradeCode="8f3c2a91-4d6e-4b17-9c80-1e5a7b2d9f10"
           Scope="perUser"
           Compressed="yes">
    <MajorUpgrade DowngradeErrorMessage="A newer version is already installed." />
    <MediaTemplate EmbedCab="yes" CompressionLevel="high" />
    <StandardDirectory Id="AppDataFolder">
      <Directory Id="AutodeskFolder" Name="Autodesk">
        <Directory Id="RevitFolder" Name="Revit">
          <Directory Id="AddinsFolder" Name="Addins">
            <Directory Id="YearFolder" Name="$Version">
              <Component Id="AddinManifest" Guid="*">
                <File Id="AddinFile" Source="$addin" Name="SiteDesigner.addin" KeyPath="yes" />
              </Component>
$($dirXml.ToString().TrimEnd())
            </Directory>
          </Directory>
        </Directory>
      </Directory>
    </StandardDirectory>
    <Feature Id="ProductFeature" Title="Site Designer $Version" Level="1">
      <ComponentRef Id="AddinManifest" />
      <ComponentGroupRef Id="ProductComponents" />
    </Feature>
    <ComponentGroup Id="ProductComponents" />
  </Package>
</Wix>
"@

# ComponentGroup empty is wrong - Feature should auto-include directory components if we use ComponentGroup from harvest
# Use Feature with ComponentGroup that lists all, OR just rely on Feature harvesting all components under Package.
# In WiX v4, Feature must reference components. Collect all component IDs.

$componentIds = [regex]::Matches($dirXml.ToString(), 'Component Id="([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
$refs = ($componentIds | ForEach-Object { "      <ComponentRef Id=`"$_`" />" }) -join "`n"

$wxs = @"
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
  <Package Name="Site Designer $Version"
           Manufacturer="SDGN"
           Version="$ProductVersion"
           UpgradeCode="8f3c2a91-4d6e-4b17-9c80-1e5a7b2d9f10"
           Scope="perUser"
           Compressed="yes">
    <MajorUpgrade DowngradeErrorMessage="A newer version is already installed." />
    <MediaTemplate EmbedCab="yes" CompressionLevel="high" />
    <StandardDirectory Id="AppDataFolder">
      <Directory Id="AutodeskFolder" Name="Autodesk">
        <Directory Id="RevitFolder" Name="Revit">
          <Directory Id="AddinsFolder" Name="Addins">
            <Directory Id="YearFolder" Name="$Version">
              <Component Id="AddinManifest" Guid="*">
                <File Id="AddinFile" Source="$addin" Name="SiteDesigner.addin" KeyPath="yes" />
              </Component>
$($dirXml.ToString().TrimEnd())
            </Directory>
          </Directory>
        </Directory>
      </Directory>
    </StandardDirectory>
    <Feature Id="ProductFeature" Title="Site Designer $Version" Level="1">
      <ComponentRef Id="AddinManifest" />
$refs
    </Feature>
  </Package>
</Wix>
"@
[System.IO.File]::WriteAllText($wxsPath, $wxs)

$outDir = Join-Path $PSScriptRoot '..\artifacts\installer'
$msiName = "SiteDesigner-Revit$Version-$ProductVersion-x64.msi"
$msi = Join-Path $outDir $msiName
& wix build $wxsPath -o $msi --nologo
if ($LASTEXITCODE -ne 0) { throw "WiX build failed." }
Write-Output $([System.IO.Path]::GetFullPath($msi))
