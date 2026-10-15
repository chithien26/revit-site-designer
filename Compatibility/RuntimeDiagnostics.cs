using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace EPCommon;

public static class RuntimeDiagnostics
{
    internal static bool IsRevitHost { get; set; }
    public static string LogPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SiteDesigner", "logs", "SiteDesigner.log");
    public static void Write(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath));
            File.AppendAllText(LogPath, DateTimeOffset.Now.ToString("O") + " " + message + Environment.NewLine);
        }
        catch (Exception exception) { System.Diagnostics.Trace.WriteLine(message + Environment.NewLine + exception); }
    }
    public static void Report(string operation, Exception exception)
    {
        Write(operation + ": " + exception);
        ShowMessage(operation + " failed", exception.Message + "\n\nLog: " + LogPath, exception.ToString(), true);
    }
    public static void ShowMessage(string instruction, string message, string details = null, bool error = false)
    {
        if (IsRevitHost)
            ShowRevitMessage(instruction, message, details, error);
        else
            System.Windows.MessageBox.Show(instruction + "\n\n" + message, "Site Designer", System.Windows.MessageBoxButton.OK,
                error ? System.Windows.MessageBoxImage.Error : System.Windows.MessageBoxImage.Information);
    }
    // Keep the standalone UI harness independent of Revit's native API modules.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ShowRevitMessage(string instruction, string message, string details, bool error)
    {
        using (var dialog = new Autodesk.Revit.UI.TaskDialog("Site Designer"))
        {
            dialog.TitleAutoPrefix = false;
            dialog.MainInstruction = instruction;
            dialog.MainContent = message;
            dialog.ExpandedContent = details;
            dialog.MainIcon = error ? Autodesk.Revit.UI.TaskDialogIcon.TaskDialogIconError : Autodesk.Revit.UI.TaskDialogIcon.TaskDialogIconInformation;
            dialog.CommonButtons = Autodesk.Revit.UI.TaskDialogCommonButtons.Ok;
            dialog.Show();
        }
    }
}
