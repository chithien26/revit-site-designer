[Setup]
AppName=SiteDesigner2025
AppVersion=1.0
DefaultDirName={commonappdata}\Autodesk\Revit\Addins\2025
DefaultGroupName=SiteDesigner2025
OutputDir=D:\Thien\projects\tobic\SiteDesigner\artifacts\installer
OutputBaseFilename=SiteDesigner2025_Setup
Compression=lzma
SolidCompression=yes

[Files]
Source: "D:\Thien\projects\tobic\SiteDesigner\SiteDesigner2025.addin"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Thien\projects\tobic\SiteDesigner\bin\Release2025\2025\net8.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs