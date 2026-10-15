using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using EPCommon;
using SiteDesigner.UI;
using Forms = System.Windows.Forms;

class Program
{
    static int checks;
    [STAThread]
    static int Main()
    {
        try
        {
            new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            RuntimePaths.LoadNativeLibrary();
            typeof(ApplicationUtility).GetField("m_bIs64BitInstall", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, true);
            Check(EPSharedExternal.eplGeoE_ArePntsDuplicates2D(new Eps_Point(1,2,3), new Eps_Point(1,2,4)), "Native x64 geometry invocation");
            Check(!EPSharedExternal.eplGeoE_ArePntsDuplicates2D(new Eps_Point(1,2,3), new Eps_Point(2,2,3)), "Native geometry negative case");
            Check(Math.Abs(ValueConversion.ToDouble("1,5") - 1.5) < 1e-9, "Comma decimal input");
            Check(Math.Abs(ValueConversion.ToDouble("1.5") - 1.5) < 1e-9, "Dot decimal input");
            Check(EPShared.LocalRM.GetString("AMP_OKDialog") != null, "Embedded localization");
            foreach (var property in typeof(SITES.Sites_Core.Resources).GetProperties(BindingFlags.Public | BindingFlags.Static))
                if (property.PropertyType == typeof(System.Drawing.Bitmap) || property.PropertyType == typeof(System.Drawing.Icon))
                { var value = property.GetValue(null); Check(value != null, "Resource " + property.Name); (value as IDisposable)?.Dispose(); }
            CheckMemoryLoadedAssembly();
            using (var dialog = new WpfForm { Text = "Site Designer · WPF", ClientSize = new System.Drawing.Size(380,145) })
            {
                var label = new Forms.Label { Text = "Define a name for this site element.", Location = new System.Drawing.Point(12,12), Size = new System.Drawing.Size(350,24) };
                var input = new Forms.TextBox { Text = "Proposed terrain", Location = new System.Drawing.Point(12,48), Width = 350 };
                var accept = new Forms.Button { Text = "OK", DialogResult = Forms.DialogResult.OK, Location = new System.Drawing.Point(282,94), Size = new System.Drawing.Size(80,26) };
                dialog.Controls.AddRange(new Forms.Control[] { label, input, accept });
                dialog.AcceptButton = accept;
                Queue(dialog, window =>
                {
                    Check(!dialog.Visible, "Legacy controller is never visible");
                    Check(Descendants<TextBox>(window).Any(x => x.Text == "Proposed terrain"), "WPF text rendering");
                    var field = Descendants<TextBox>(window).Single(x => x.Text == "Proposed terrain");
                    field.Text = "WPF edited terrain";
                    Check(input.Text == "WPF edited terrain", "Text change reaches controller");
                    Capture(window, "input-dialog.png");
                    var ok = Descendants<Button>(window).First(x => x.Content?.ToString().Replace("_", "") == "OK");
                    ok.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                });
                Check(dialog.ShowDialog() == Forms.DialogResult.OK, "OK result and window close");
            }
            using (var dialog = new WpfForm { Text = "Site Designer · Control validation", ClientSize = new System.Drawing.Size(660,400) })
            {
                var list = new Forms.ListView { Location = new System.Drawing.Point(12,12), Size = new System.Drawing.Size(410,180), MultiSelect = true, View = Forms.View.Details };
                list.Columns.Add("Surface", 230); list.Columns.Add("Phase", 150);
                list.Items.Add(new Forms.ListViewItem(new[] { "Existing ground", "Existing" }));
                list.Items.Add(new Forms.ListViewItem(new[] { "Proposed terrain", "New construction" }));
                var check = new Forms.CheckBox { Text = "Hide original surface", Location = new System.Drawing.Point(12,210), Width = 200 };
                var combo = new Forms.ComboBox { Location = new System.Drawing.Point(12,250), Width = 250, DropDownStyle = Forms.ComboBoxStyle.DropDownList };
                combo.Items.AddRange(new object[] { "Meters", "Feet", "Millimeters" }); combo.SelectedIndex = 0;
                var tree = new Forms.TreeView { Location = new System.Drawing.Point(440,12), Size = new System.Drawing.Size(205,180) }; tree.Nodes.Add("Site").Nodes.Add("Streets"); tree.ExpandAll();
                var nativeText = new Forms.TextBox { Text = "Elevation", Location = new System.Drawing.Point(12,290), Width = 250 };
                int lostFocus = 0;
                nativeText.LostFocus += (_, _) => lostFocus++;
                var tabs = new Forms.TabControl { Location = new System.Drawing.Point(440,210), Size = new System.Drawing.Size(205,130) };
                tabs.TabPages.Add("General"); tabs.TabPages.Add("Details");
                dialog.Controls.AddRange(new Forms.Control[] { list, check, combo, tree, nativeText, tabs });
                Queue(dialog, window =>
                {
                    var grid = Descendants<DataGrid>(window).Single();
                    Check(grid.Items.Count == 2, "List rows render in WPF DataGrid");
                    grid.SelectedIndex = 1;
                    Check(list.SelectedItems.Count == 1 && list.SelectedItems[0].Index == 1, "WPF row selection reaches command logic");
                    Descendants<CheckBox>(window).Single().IsChecked = true;
                    Check(check.Checked, "Checkbox change reaches controller");
                    Descendants<ComboBox>(window).Single().SelectedIndex = 2;
                    Check(combo.SelectedIndex == 2, "Combo selection reaches controller");
                    WpfFocus.Focus(nativeText);
                    Check(WpfFocus.HasFocus(nativeText), "Controller focus request reaches WPF");
                    WpfFocus.Focus(combo);
                    Check(lostFocus == 1, "Lost-focus validation event reaches controller exactly once");
                    Descendants<TabControl>(window).Single().SelectedIndex = 1;
                    Check(tabs.SelectedIndex == 1, "WPF tab selection reaches controller");
                    nativeText.Hide(); dialog.Refresh();
                    Check(Descendants<TextBox>(window).Single(x => x.Text == "Elevation").Visibility == Visibility.Collapsed, "Dynamic field visibility");
                    Check(Descendants<TreeViewItem>(window).Count() >= 2, "Tree hierarchy renders");
                    list.Items.Add(new Forms.ListViewItem(new[] { "Updated surface", "Existing" }));
                    dialog.Refresh();
                    Check(grid.Items.Count == 3, "Dynamic list update");
                    Capture(window, "controls-dialog.png");
                    dialog.Close();
                });
                Check(dialog.ShowDialog() == Forms.DialogResult.Cancel, "Window close returns Cancel");
            }
            using (var dialog = new ProgressBarForm())
            {
                dialog.ShowForm(); dialog.MaxValue = 10; dialog.UpdateProgressBar(3); dialog.Message = "Preparing site geometry"; dialog.Refresh();
                Check(dialog.WpfWindow != null && !dialog.Visible, "Modeless progress uses WPF"); dialog.Close();
            }
            Console.WriteLine("PASS: " + checks + " checks");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    static void Check(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); checks++; }
    static void CheckMemoryLoadedAssembly()
    {
        var loaded = Assembly.Load(File.ReadAllBytes(typeof(RuntimePaths).Assembly.Location));
        Check(string.IsNullOrEmpty(loaded.Location), "Reproduce Add-in Manager byte-loaded assembly");
        var paths = loaded.GetType("EPCommon.RuntimePaths");
        var directory = (string)paths.GetProperty("AssemblyDirectory").GetValue(null);
        Check(Directory.Exists(directory) && File.Exists(Path.Combine(directory, "SitesDLLWrapper.dll")), "Memory-loaded assembly resolves native dependency folder");
        Check(File.Exists((string)paths.GetProperty("ManagedAssemblyPath").GetValue(null)), "Memory-loaded application resolves ribbon assembly file");
        var year = (loaded.GetReferencedAssemblies().Single(x => x.Name == "RevitAPI").Version.Major + 2000).ToString();
        var content = (string)paths.GetMethod("FindDataRoot").Invoke(null, new object[] { year });
        Check(File.Exists(Path.Combine(content, "Data", "Support", "SITEDESIGNER_enu.xml")), "Memory-loaded assembly resolves content");
        var settings = (string)paths.GetMethod("SettingsFile").Invoke(null, new object[] { year, "__MemoryLoadSmoke-" + Guid.NewGuid().ToString("N") + ".ini", content });
        try { Check(File.Exists(settings), "Memory-loaded assembly initializes settings without null paths"); }
        finally { File.Delete(settings); }
        paths.GetMethod("LoadNativeLibrary").Invoke(null, null);
        loaded.GetType("EPCommon.ApplicationUtility").GetField("m_bIs64BitInstall", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, true);
        var pointType = loaded.GetType("EPCommon.Eps_Point");
        var point1 = Activator.CreateInstance(pointType);
        var point2 = Activator.CreateInstance(pointType);
        pointType.GetProperty("z").SetValue(point2, 4.0);
        var geometry = loaded.GetType("EPCommon.EPSharedExternal").GetMethod("eplGeoE_ArePntsDuplicates2D", new[] { pointType, pointType });
        Check((bool)geometry.Invoke(null, new[] { point1,point2 }), "Byte-loaded assembly invokes native x64 geometry");
        var formType = loaded.GetType("SiteDesigner.UI.WpfForm");
        using (var dialog = (Forms.Form)Activator.CreateInstance(formType))
        {
            dialog.Text = "Site Designer · Add-in Manager regression";
            dialog.ClientSize = new System.Drawing.Size(380,100);
            dialog.Controls.Add(new Forms.Label { Text = "WPF from a byte-loaded assembly", Location = new System.Drawing.Point(12,12), Size = new System.Drawing.Size(350,30) });
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                try
                {
                    var window = (Window)formType.GetProperty("WpfWindow").GetValue(dialog);
                    Check(window != null && window.IsVisible && !dialog.Visible, "Byte-loaded assembly opens native WPF dialog");
                    Check(Descendants<TextBlock>(window).Any(x => x.Text == "WPF from a byte-loaded assembly"), "Byte-loaded WPF content renders");
                    Capture(window, "memory-loaded-dialog.png");
                    window.Close();
                }
                catch (Exception ex) { Console.Error.WriteLine(ex); Environment.Exit(1); }
            }));
            var result = formType.GetMethod("ShowDialog", Type.EmptyTypes).Invoke(dialog, null);
            Check((Forms.DialogResult)result == Forms.DialogResult.Cancel, "Byte-loaded WPF dialog closes normally");
        }
    }
    static void Queue(WpfForm form, Action<Window> action) => Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { try { action(form.WpfWindow); } catch (Exception ex) { Console.Error.WriteLine(ex); Environment.Exit(1); } }));
    static System.Collections.Generic.IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) yield break;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result) yield return result;
            foreach (var nested in Descendants<T>(child)) yield return nested;
        }
    }
    static void Capture(Window window, string name)
    {
        window.UpdateLayout();
        var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96,96,PixelFormats.Pbgra32);
        bitmap.Render(window);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        Directory.CreateDirectory("artifacts"); using (var stream = File.Create(Path.Combine("artifacts", name))) encoder.Save(stream);
    }
}
