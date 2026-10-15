using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using EPCommon;
using SiteDesigner.UI;

namespace RevitSmoke;

public class SmokeApplication : IExternalApplication
{
    private static readonly string Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SiteDesigner", "RevitSmoke", typeof(SmokeApplication).Assembly.GetReferencedAssemblies().First(x => x.Name == "RevitAPI").Version.Major.ToString());
    private readonly List<string> checks = new List<string>();
    private bool started;
    public Result OnStartup(UIControlledApplication application)
    {
        application.Idling += Run;
        Directory.CreateDirectory(Folder);
        File.WriteAllText(Path.Combine(Folder, "started.txt"), DateTimeOffset.Now.ToString());
        return Result.Succeeded;
    }
    private void Check(bool value, string name)
    {
        if (!value) throw new Exception("FAIL: " + name);
        checks.Add("PASS: " + name);
        File.WriteAllLines(Path.Combine(Folder, "progress.txt"), checks);
    }
    private void Run(object sender, IdlingEventArgs args)
    {
        if (started) return;
        started = true;
        var ui = (UIApplication)sender;
        ui.Idling -= Run;
        Document document = null;
        try
        {
            Check(ApplicationUtility.Settings != null, "Site Designer OnStartup initialized");
            Check(EPShared.eplLicM_GetLicense(), "Original native license check");
            var data = (ExternalCommandData)Activator.CreateInstance(typeof(ExternalCommandData), true);
            data.Application = ui;
            string message = "";
            Result commandResult = Result.Succeeded;
            var notifications = CaptureTaskDialogs(ui, () => commandResult = new SITES.SitesSettingsCommand().Execute(data, ref message, new ElementSet()));
            Check(commandResult == Result.Cancelled && notifications.Any(x => x.Contains("open document")), "Settings command explains missing active document");
            notifications = CaptureTaskDialogs(ui, () => RuntimeDiagnostics.Report("Smoke diagnostic", new InvalidOperationException("Expected smoke-test failure")));
            Check(notifications.Any(x => x.Contains("Expected smoke-test failure")), "Runtime errors display a Revit TaskDialog");
            // Exercise the command-loader path, where ExternalApplication.OnStartup was not called.
            typeof(ApplicationUtility).GetField("m_Settings", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
            typeof(ApplicationUtility).GetField("m_StringTableModule", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
            RunWindowCommand(ui, data, new SITES.SitesAboutCommand(), "about-command.png");
            Check(ApplicationUtility.Settings != null && ApplicationUtility.StringTable != null, "Command initializes missing startup services");
            document = ui.Application.NewProjectDocument(UnitSystem.Metric);
            var units = document.GetUnits();
            var options = LegacyUnits.GetFormatOptions(units, EPCommon.UnitType.UT_Length);
            Check(options.LegacyDisplayUnits() != EPCommon.DisplayUnitType.DUT_UNDEFINED, "Modern project units mapping");
            Check(LegacyUnits.TryParse(units, EPCommon.UnitType.UT_Length, "1000 mm", out double length) && Math.Abs(length - 1000/304.8) < 1e-8, "Units parse preserves internal feet");
            string formatted = "";
            RevitGeometry.Units_ConvertDoubleToString(length, EPCommon.DisplayUnitType.DUT_MILLIMETERS, 0.1, EPCommon.UnitSymbolType.UST_NONE, units, ref formatted);
            Check(formatted.Contains("1000") || formatted.Contains("1,000"), "Units format millimeters");
            TopographySurface surface;
            using (var transaction = new Transaction(document, "Site Designer smoke terrain"))
            {
                transaction.Start();
                surface = TopographySurface.Create(document, new List<XYZ> { new XYZ(0,0,0), new XYZ(100,0,0), new XYZ(100,100,1), new XYZ(0,100,0), new XYZ(50,50,0.5) });
                transaction.Commit();
            }
            Check(surface.GetPoints().Count == 5, "Legacy topography creation in Revit " + ui.Application.VersionNumber);
            var container = new TopographySurfaceContainer(document.Application, surface);
            container.FindMinMaxPointsOfTopographySurface();
            Check(container.Triangles != null && container.Triangles.Count > 0, "Terrain mesh and geometry container");
            using (var scope = new TopographyEditScope(document, "Smoke topography edit"))
            {
                scope.Start(surface.Id);
                using (var transaction = new Transaction(document, "Add point"))
                { transaction.Start(); surface.AddPoints(new List<XYZ> { new XYZ(25,25,0.2) }); transaction.Commit(); }
                scope.Commit(new TopographyEditFailuresPreprocessor());
            }
            Check(surface.GetPoints().Count == 6, "Topography edit scope");
            var familyPath = Path.Combine(ApplicationUtility.RegistryPaths.ProgramPath, "Data", "Library", "Curbs - Metric.rfa");
            using (var transaction = new Transaction(document, "Load Site Designer family"))
            {
                transaction.Start();
                Check(document.LoadFamily(familyPath, new FamilyLoadOptions(true), out var family) && family != null, "Upgrade/load 2020 curb family");
                transaction.Commit();
            }
            var massTemplate = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Autodesk", "RVT " + ui.Application.VersionNumber, "Family Templates", "English", "Conceptual Mass", "Metric Mass.rft");
            using (var familyDocument = ui.Application.NewFamilyDocument(massTemplate))
            {
                using (var transaction = new Transaction(familyDocument, "Test parameter adapter"))
                {
                    transaction.Start();
                    var parameter = LegacyUnits.AddParameter(familyDocument.FamilyManager, "SmokeLength", EPCommon.BuiltInParameterGroup.PG_DATA, EPCommon.ParameterType.Length, true);
                    Check(parameter.Definition.GetDataType() == SpecTypeId.Length, "Forge family parameter creation");
                    transaction.RollBack();
                }
                familyDocument.Close(false);
            }
            using (var dialog = new SettingsForm(document, RevitProduct.SITES))
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                {
                    try
                    {
                        var window = dialog.WpfWindow;
                        Check(window != null && !dialog.Visible, "Settings rendered by WPF in Revit");
                        window.UpdateLayout();
                        var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96,96,PixelFormats.Pbgra32);
                        bitmap.Render(window);
                        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        using (var file = File.Create(Path.Combine(Folder, "settings.png"))) encoder.Save(file);
                        dialog.Close();
                    }
                    catch (Exception ex) { File.WriteAllText(Path.Combine(Folder,"dialog-error.txt"), ex.ToString()); dialog.Close(); }
                }));
                dialog.ShowDialog();
            }
            var commandProject = Path.Combine(Folder, "command-test-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".rvt");
            document.SaveAs(commandProject);
            document.Close(false);
            document = ui.OpenAndActivateDocument(commandProject).Document;
            data.View = document.ActiveView;
            RunWindowCommand(ui, data, new SITES.SitesSettingsCommand(), "settings-command.png");
            RunWindowCommand(ui, data, new SITES.StreetsFamilyManagerCommand(), "street-family-command.png");
            RunWindowCommand(ui, data, new SITES.SoftTerrainFamilyManagerCommand(), "terrain-family-command.png");
            // This is the harness's own generated project; save it so ExitRevit needs no save prompt.
            document.Save();
            checks.Add("SUCCESS: " + checks.Count + " Revit-hosted checks");
            File.WriteAllLines(Path.Combine(Folder, "result.txt"), checks);
        }
        catch (Exception ex) { File.WriteAllText(Path.Combine(Folder,"result.txt"), string.Join(Environment.NewLine, checks) + Environment.NewLine + ex); }
        finally
        {
            if (document != null && document.IsValidObject && ui.ActiveUIDocument?.Document != document) document.Close(false);
            ui.PostCommand(RevitCommandId.LookupPostableCommandId(PostableCommand.ExitRevit));
        }
    }
    private List<string> CaptureTaskDialogs(UIApplication ui, Action action)
    {
        var messages = new List<string>();
        EventHandler<DialogBoxShowingEventArgs> handler = (_, args) =>
        {
            if (args is TaskDialogShowingEventArgs task)
            {
                messages.Add(task.Message);
                task.OverrideResult((int)TaskDialogResult.Ok);
            }
        };
        ui.DialogBoxShowing += handler;
        try { action(); }
        finally { ui.DialogBoxShowing -= handler; }
        return messages;
    }
    private void RunWindowCommand(UIApplication ui, ExternalCommandData data, IExternalCommand command, string screenshot)
    {
        bool shown = false;
        Exception callbackError = null;
        var callback = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
        {
            var window = System.Windows.Application.Current.Windows.OfType<DialogWindow>().FirstOrDefault(x => x.IsVisible);
            try
            {
                Check(window != null, command.GetType().Name + " opens a visible WPF window");
                Check(new WindowInteropHelper(window).Owner == ui.MainWindowHandle, command.GetType().Name + " window belongs to Revit");
                shown = true;
                window.UpdateLayout();
                var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96,96,PixelFormats.Pbgra32);
                bitmap.Render(window);
                var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using (var file = File.Create(Path.Combine(Folder, screenshot))) encoder.Save(file);
            }
            catch (Exception ex) { callbackError = ex; }
            finally { window?.Close(); }
        }));
        string message = "";
        Result result = Result.Failed;
        var notifications = CaptureTaskDialogs(ui, () => result = command.Execute(data, ref message, new ElementSet()));
        if (callback.Status == DispatcherOperationStatus.Pending) callback.Abort();
        if (callbackError != null) throw callbackError;
        Check(shown && result == Result.Succeeded && notifications.Count == 0,
            command.GetType().Name + " completes without errors: " + message + string.Join("; ", notifications));
    }
    public Result OnShutdown(UIControlledApplication application) { application.Idling -= Run; return Result.Succeeded; }
}

