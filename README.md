# Site Designer for Revit 2022–2027

This migrates the supplied decompiled Revit 2020 add-in to version-specific x64 builds. Revit **2025 is the default**.

| Revit | Framework | Revit_All_Main_Versions_API_x64 |
|---|---|---|
| 2022 | .NET Framework 4.8 | 2022.0.0 |
| 2023 | .NET Framework 4.8 | 2023.0.0 |
| 2024 | .NET Framework 4.8 | 2024.0.0 |
| 2025 | .NET 8 Windows | 2025.0.0 |
| 2026 | .NET 8 Windows | 2026.0.0 |
| 2027 | .NET 10 Windows | 2027.0.0 |

The NuGet API assemblies are compile references. Revit supplies them at runtime; do not deploy RevitAPI.dll, RevitAPIUI.dll, AdWindows.dll, or UIFramework.dll with this add-in.

## Build

Install the .NET 10 SDK and .NET Framework 4.8 targeting pack. The SDK builds all three framework generations; Revit itself is needed for integration tests, not compilation.

```powershell
# Default: Release, Revit 2025
.\Build.ps1

# Every supported version
.\Build.ps1 -All

# One version
.\Build.ps1 -Version 2027
dotnet build SiteDesigner.csproj -c Release -p:RevitVersion=2025
```

Outputs are separate, for example `bin/Release/2025/net8.0-windows/SiteDesigner.dll`. Intermediate files are isolated by Revit version as well. Open `SiteDesigner.sln` in a Visual Studio version supporting .NET 10; choose Debug/Release for 2025 or a configuration such as Debug2022/Release2027.

## Install

```powershell
# Builds and installs to the current user's Revit 2025 Addins folder
.\Install.ps1

# Select another version and/or supply your licensed legacy dependencies
.\Install.ps1 -Version 2027 `
  -NativeLibrary 'C:\path\SitesDLLWrapper.dll' `
  -ContentRoot 'C:\path\Site Designer for Revit 2020'
```

Your `SitesDLLWrapper.dll` and `whitelist.xml` from `bin/Release2025/2025/net8.0-windows` are copied unchanged to `Dependencies/Native`, the default dependency source. Every version's build copies these files beside its output assembly. Keeping the source outside `bin` preserves it across clean builds. Override the build source with `-p:NativeDependencyDirectory='C:\path\native'`. The installer prefers `Dependencies/Native`, copies the DLL's companion `whitelist.xml` unchanged, and falls back to the supplied Release2025 folder, selected build output, or existing 2020 installation. Use `-NativeLibrary` and optionally `-WhitelistFile` to override installation sources.

Content still defaults to the existing 2020 installation. `ContentRoot` must contain `Data/Support/SITEDESIGNER_enu.xml` and `Data/Library`. The original native binary and family assets remain required dependencies; the source export does not contain their implementations.

The installer writes `%APPDATA%/Autodesk/Revit/Addins/<year>/SiteDesigner.addin`, with an absolute assembly path, and copies the matching build, native DLL, companion whitelist when present, defaults, and Data folder to its SiteDesigner subfolder. It does not require administrator rights. Restart the selected Revit version, and choose **Load Once** or **Always Load** if Revit prompts about this locally built unsigned assembly.

Settings are writable per user and version under `%LOCALAPPDATA%/SiteDesigner/<year>/SitesRevit.ini`. The installer does not overwrite these settings or import machine-specific 2020 template paths. Runtime errors are logged to `%LOCALAPPDATA%/SiteDesigner/logs/SiteDesigner.log`.

If a command opens no window, check the log for `Command`, `Preparing WPF dialog`, and `Displayed WPF dialog`. Each command entry records the loaded DLL path, build ID, host version, and whether a document is open. Commands initialize required services when invoked through a command loader without application startup. Missing documents produce an explanation; exceptions display a Revit TaskDialog with the log location and expandable error details. A build loaded into the wrong Revit year also produces an explicit error. About works without an open document; Settings and family managers require one.

Add-in Manager can load the DLL from bytes, leaving `Assembly.Location` empty. Runtime paths support that mode: they probe the actual assembly folder when available, an optional `SITE_DESIGNER_HOME` environment variable, the build-output directory embedded in the DLL, and the matching Revit Addins installation. Native DLL and content probing skip empty paths. Set `SITE_DESIGNER_HOME` to the folder containing `SiteDesigner.dll`, `SitesDLLWrapper.dll`, and `Defaults` if a relocated memory-loaded build has neither its original build output nor an installed copy available. Restart Revit after changing an environment variable. After rebuilding, reload the updated DLL in Add-in Manager; an already loaded copy retains its previous code.

To uninstall, close that Revit version and remove its `SiteDesigner.addin` manifest and SiteDesigner subfolder. User settings can be retained.

## UI migration

Active dialog classes route through `UI/WpfForm.cs`. The visible windows, fields, tables, trees, tabs, checkboxes, buttons, editors, and progress displays are native WPF. Styling is in `UI/DialogWindow.xaml`. No WindowsFormsHost is used.

`DialogWindow.xaml` is an embedded WPF resource dictionary loaded directly from the executing assembly. It avoids WPF's global pack-URI assembly lookup, which can confuse multiple in-memory copies loaded by a command manager. Disk-loaded and byte-loaded copies can display dialogs in the same process. Dialog closing also guards against reentry from focus-loss events.

This is a WPF presentation migration with a compatibility layer: existing WinForms control objects remain as hidden controllers for the extensive validation and command logic. This project consequently still references WinForms. It is not a complete MVVM rewrite or removal of the WinForms dependency. `UI/WpfDialogPresenter.cs` synchronizes controller state, selection, events, and dynamically added controls. Double-click a parameter value to open its WPF editor. Standard operating-system file/folder pickers are retained.

Unused UI cleanup removed 24 legacy dialog/control types, their 10 unreachable workflow callers, 50 unused per-form resource files, and the unused generated `WinFormStrings.cs` accessor. Three uncalled plant-search/shared-parameter launcher methods were removed from `ManagePlants`; its shared helpers remain. The `WinFormStrings.resx` localization data remains in use by `EPShared.LocalRM`. Required dialog controllers and resource entries read at runtime are retained. The smoke harness now uses the active WPF bridge directly instead of retaining an otherwise unused input dialog. `artifacts/ui-cleanup.json` records every removed file and its backup ZIP; `artifacts/ui-cleanup-before.json` and `artifacts/ui-cleanup-launcher-analysis.json` record the reference analysis. The backup preserves the production source before this cleanup.

`tools/UiAudit` performs a read-only Roslyn reference audit using evaluated build inputs. Its roots include every external application/command and production types referenced by the smoke harnesses. It retains whole classes and their dependencies, then identifies unused WinForms types and the unreachable callers that must be removed with them. It also reports unreferenced methods that create UI controls; those need individual review because dynamic callbacks and public APIs can have callers outside the source. The cleanup removes the three identified launchers rather than trimming other methods from shared classes.

Serialized image objects in the old resx files were converted to raw PNG/ICO bytes. The add-in does not enable BinaryFormatter, including for .NET 10. `Compatibility/SafeResources.cs` decodes these assets and creates ribbon images without leaking GDI bitmap handles.

## Revit compatibility and scope

The repaired source restores command attributes, parameter access, operators, resource names, and the original family-load behavior. `Compatibility/LegacyUnits.cs` converts the old persisted unit/parameter enums to ForgeTypeId APIs. `Compatibility/RevitIds.cs` preserves 64-bit element IDs from Revit 2024 onward. Startup resolves the native library and content explicitly, and settings no longer depend on obsolete Code Access Security checks or writing to protected installation folders.

**Terrain operations retain the original TopographySurface and TopographyEditScope workflow. They do not edit native Toposolid elements.** Revit 2024–2027 retain the legacy API, although it is deprecated. Use legacy surfaces, including those produced by LandXML import, for these commands. A complete Toposolid redesign would also need to address subdivisions, building pads, grading, and the original geometry/placement associations; it is outside this compatibility migration.

Keep licensed 2020 content and the x64 native geometry library available. Family upgrades can take time on their first load. The original native license check is preserved. No licensing checks were bypassed.

## Validation

Builds for all six versions have been checked. The UI/native smoke harness runs outside Revit on each framework generation:

```powershell
dotnet run --project tools/SmokeTests/SmokeTests.csproj -p:RevitVersion=2022
dotnet run --project tools/SmokeTests/SmokeTests.csproj -p:RevitVersion=2025
dotnet run --project tools/SmokeTests/SmokeTests.csproj -p:RevitVersion=2027
```

The expanded harness runs **98 checks** on the .NET Framework 4.8, .NET 8, and .NET 10 builds. It covers x64 native geometry calls, localized resources, comma/dot decimal input, WPF edits and selections, focus and lost-focus validation, tabs, dynamic field visibility, table updates, dialog results, and progress windows. Nine regression checks load the add-in from bytes with an empty assembly location, resolve native/content/settings paths, call native geometry, and display WPF before opening disk-loaded dialogs in the same process. Screenshots and Revit-hosted results are saved in `artifacts`; a test log ending in `PASS: 98 checks` confirms that runtime's current result.

`tools/RevitSmoke` is an opt-in integration harness. Its application performs API operations in a new test project, then saves that generated project under `%LOCALAPPDATA%/SiteDesigner/RevitSmoke/<API-major>` to activate it and exercise command entry points. It exits its Revit test session. Do not register it for normal work sessions. It is not included in the production add-in.

Run `./tools/Test-InRevit.ps1 -Version 2025` to install a temporary harness manifest, launch an isolated test session, collect the report, and remove the harness manifest afterward. Close that Revit version first. Revit may require a normal unsigned-publisher load decision for the locally built assemblies.

The Revit 2025 hosted test passed initialization, the original license check, unit conversion, terrain creation/editing, mesh extraction, upgrading/loading a 2020 curb family, Forge family parameter creation, and the WPF Settings dialog. This is representative integration coverage, not proof of every command on every possible project. Remaining compiler warnings include unused fields/locals in the recovered code and Autodesk's legacy topography deprecations.

The expanded harness additionally exercises the About, Settings, Street Family Manager, and Soft Terrain Family Manager command `Execute` methods, verifies visible WPF windows owned by Revit, missing-document feedback, error TaskDialogs, and initialization without application startup. Results from that expanded run are only confirmed when its report ends with `SUCCESS`.

The memory-load fix was also checked in the live Revit 2025 Add-in Manager: `SitesAboutCommand` and `ConvertObjectToToposurfaceCommand` opened their WPF dialogs from a byte-loaded Release2025 assembly. The conversion dialog was cancelled before conversion. `artifacts/memory-load-validation.json` records this coverage separately from the earlier hosted automation.

The supplied source is preserved in `tools/original-2020-source.zip`. The installed 2020 DLL was used with its matching API references to recover valid C# where the initial decompiler export had lost attributes and operators; the ZIP preserves the starting files.

References: [NuGet package](https://www.nuget.org/packages/Revit_All_Main_Versions_API_x64), [Autodesk's Revit 2027 .NET 10 and deployment changes](https://blog.autodesk.io/revit-2027-sdk-net-10-api-changes-and-additions/).
