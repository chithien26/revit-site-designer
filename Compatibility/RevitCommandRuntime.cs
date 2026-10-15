using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace EPCommon;

public static class RevitCommandRuntime
{
    public static void Begin(UIApplication application, string command)
    {
        RuntimeDiagnostics.IsRevitHost = true;
        var assembly = typeof(RevitCommandRuntime).Assembly;
        RuntimeDiagnostics.Write("Command " + command + "; Revit=" + application.Application.VersionNumber
            + "; document=" + (application.ActiveUIDocument == null ? "none" : "open")
            + "; assembly=" + (string.IsNullOrEmpty(assembly.Location) ? "<memory>" : assembly.Location) + "; build=" + assembly.ManifestModule.ModuleVersionId);
        int targetYear = assembly.GetReferencedAssemblies().First(x => x.Name == "RevitAPI").Version.Major + 2000;
        if (application.Application.VersionNumber != targetYear.ToString())
            throw new InvalidOperationException("This Site Designer build targets Revit " + targetYear
                + ". Install the matching build for Revit " + application.Application.VersionNumber + ".");
        if (ApplicationUtility.Settings == null || ApplicationUtility.StringTable == null)
        {
            if (!ApplicationUtility.Initialize(application))
                throw new InvalidOperationException("Site Designer supports Revit 2022 through 2027.");
            new RunCommands().epl_SetRevitVersion(RevitVersion.Revit2011, ApplicationUtility.Is64BitInstall);
        }
        if (!ApplicationUtility.IsRevitVersionValid)
            throw new InvalidOperationException("Site Designer could not initialize for this Revit version.");
    }

    public static Document RequireDocument(UIApplication application, string command)
    {
        var document = application.ActiveUIDocument?.Document;
        if (document == null)
        {
            RuntimeDiagnostics.Write(command + ": no active document; command cancelled.");
            RuntimeDiagnostics.ShowMessage("Open a Revit document first",
                "This command needs an open document. Open or create a project, then run the Site Designer command again.");
        }
        return document;
    }
}
