#define AppName "Site Designer"
#ifndef RevitVersion
  #define RevitVersion "2025"
#endif
#ifndef AppVersion
  #define AppVersion "2025.0.0"
#endif
#ifndef StagingDir
  #define StagingDir "..\artifacts\installer\staging"
#endif

[Setup]
AppId={{BE8C3528-82C3-4A7C-8096-94A8DA602025}-{#RevitVersion}
AppName={#AppName} {#RevitVersion}
AppVersion={#AppVersion}
AppPublisher=Site Designer
DefaultDirName={userappdata}\Autodesk\Revit\Addins\{#RevitVersion}\SiteDesigner
DisableDirPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
OutputDir=..\artifacts\installer
OutputBaseFilename=SiteDesigner-{#RevitVersion}-Setup-{#AppVersion}
UninstallDisplayName={#AppName} {#RevitVersion}
SetupLogging=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#StagingDir}\SiteDesigner\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#StagingDir}\SiteDesigner.addin"; DestDir: "{userappdata}\Autodesk\Revit\Addins\{#RevitVersion}"; Flags: ignoreversion

[InstallDelete]
Type: filesandordirs; Name: "{app}"

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
Type: files; Name: "{userappdata}\Autodesk\Revit\Addins\{#RevitVersion}\SiteDesigner.addin"
Type: files; Name: "{commonappdata}\Autodesk\Revit\Addins\{#RevitVersion}\SiteDesigner2025.addin"
