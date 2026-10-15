using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using EPCommon;
using Forms = System.Windows.Forms;

namespace SiteDesigner.UI;

public sealed class WpfDialogPresenter : IDisposable
{
    private readonly WpfForm form;
    private readonly Window window;
    private readonly List<Action> updates = new List<Action>();
    private readonly Dictionary<Forms.Control, FrameworkElement> controls = new Dictionary<Forms.Control, FrameworkElement>();
    private readonly DispatcherTimer timer;
    private bool syncing;
    private bool disposed;
    public Canvas Content { get; }

    public WpfDialogPresenter(WpfForm form, Window window)
    {
        this.form = form;
        this.window = window;
        Content = Panel(form);
        Content.Width = form.ClientSize.Width;
        Content.Height = form.ClientSize.Height;
        window.SizeChanged += Resize;
        timer = new DispatcherTimer(DispatcherPriority.Background, window.Dispatcher) { Interval = TimeSpan.FromMilliseconds(150) };
        timer.Tick += Tick;
        timer.Start();
        Refresh();
    }
    private void Resize(object sender, SizeChangedEventArgs args)
    {
        if (!window.IsLoaded || form.FormBorderStyle != Forms.FormBorderStyle.Sizable) return;
        var width = Math.Max(form.MinimumSize.Width, window.ActualWidth - 18);
        var height = Math.Max(form.MinimumSize.Height, window.ActualHeight - 40);
        form.ClientSize = new System.Drawing.Size((int)width, (int)height);
        Content.Width = width;
        Content.Height = height;
        Refresh();
    }
    private void Tick(object sender, EventArgs e) => Refresh();
    public void Refresh()
    {
        if (disposed || syncing) return;
        syncing = true;
        try
        {
            window.Title = form.Text;
            foreach (var update in updates.ToArray()) update();
        }
        finally { syncing = false; }
    }
    private void Run(Action action)
    {
        if (syncing || disposed) return;
        try { action(); form.FinishInteraction(); }
        catch (Exception ex) { RuntimeDiagnostics.Report("Dialog action", ex); }
    }
    internal static void Raise(Forms.Control control, string method, EventArgs args)
    {
        var info = control.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { args.GetType() }, null)
            ?? typeof(Forms.Control).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(EventArgs) }, null);
        if (info == null) return;
        try { info.Invoke(control, new object[] { args }); }
        catch (TargetInvocationException ex) when (ex.InnerException != null) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); }
    }
    internal void Click(Forms.Control control) => Run(() =>
    {
        // Commit a focused editor before OK, matching dialog validation order.
        Keyboard.ClearFocus();
        var validation = new CancelEventArgs();
        foreach (var pair in controls)
        {
            if (!pair.Value.IsEnabled || !pair.Value.IsVisible) continue;
            Raise(pair.Key, "OnValidating", validation);
            if (validation.Cancel) { pair.Value.Focus(); return; }
            Raise(pair.Key, "OnValidated", EventArgs.Empty);
        }
        Raise(control, "OnClick", EventArgs.Empty);
        if (control is Forms.Button button && button.DialogResult != Forms.DialogResult.None && form.DialogResult == Forms.DialogResult.None)
            form.DialogResult = button.DialogResult;
    });
    private static bool LocallyVisible(Forms.Control control)
    {
        var method = typeof(Forms.Control).GetMethod("GetState", BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null) return control.Visible;
        var type = method.GetParameters()[0].ParameterType;
        return (bool)method.Invoke(control, new[] { type.IsEnum ? Enum.ToObject(type, 2) : (object)2 });
    }
    private static string Label(string value) => (value ?? "").Replace("&&", "\u0001").Replace("&", "_").Replace("\u0001", "&");
    private Canvas Panel(Forms.Control source)
    {
        var canvas = new Canvas { ClipToBounds = true };
        int childCount = -1;
        Action rebuild = () =>
        {
            if (childCount == source.Controls.Count) return;
            childCount = source.Controls.Count;
            canvas.Children.Clear();
            foreach (Forms.Control child in source.Controls.Cast<Forms.Control>().Reverse()) canvas.Children.Add(Build(child));
        };
        rebuild();
        updates.Add(rebuild);
        return canvas;
    }
    private FrameworkElement Build(Forms.Control source)
    {
        if (controls.TryGetValue(source, out var existing)) return existing;
        FrameworkElement target;
        if (source is Forms.TabControl tabs) target = Tabs(tabs);
        else if (source is Forms.GroupBox group)
        {
            var content = Panel(group);
            var box = new GroupBox { Content = content, Padding = new Thickness(0), VerticalContentAlignment = VerticalAlignment.Stretch };
            // WinForms child coordinates include the group title inset.
            content.RenderTransform = new TranslateTransform(-6, -17);
            updates.Add(() => box.Header = Label(group.Text));
            target = box;
        }
        else if (source is Forms.ListView list) target = Table(list);
        else if (source is Forms.TreeView tree) target = Tree(tree);
        else if (source is Forms.ComboBox combo) target = Combo(combo);
        else if (source is Forms.ListBox listBox) target = List(listBox);
        else if (source is Forms.CheckBox check)
        {
            var box = new CheckBox { VerticalContentAlignment = VerticalAlignment.Center };
            box.Checked += (_, _) => Run(() => check.Checked = true);
            box.Unchecked += (_, _) => Run(() => check.Checked = false);
            updates.Add(() => { box.Content = Label(check.Text); box.IsChecked = check.Checked; });
            target = box;
        }
        else if (source is Forms.RadioButton radio)
        {
            var box = new RadioButton { GroupName = source.Parent?.Name ?? "Options", VerticalContentAlignment = VerticalAlignment.Center };
            box.Checked += (_, _) => Run(() => radio.Checked = true);
            updates.Add(() => { box.Content = Label(radio.Text); box.IsChecked = radio.Checked; });
            target = box;
        }
        else if (source is Forms.Button button)
        {
            var box = new Button { IsDefault = ReferenceEquals(form.AcceptButton, button), IsCancel = false };
            box.Click += (_, _) => Click(button);
            System.Drawing.Image previousImage = null;
            string previousText = null;
            updates.Add(() =>
            {
                if (previousText == button.Text && ReferenceEquals(previousImage, button.Image)) return;
                previousText = button.Text;
                previousImage = button.Image;
                if (button.Image == null) box.Content = Label(button.Text);
                else
                {
                    var row = new StackPanel { Orientation = Orientation.Horizontal };
                    row.Children.Add(new System.Windows.Controls.Image { Source = SafeResources.ToImageSource(button.Image), Width = button.Image.Width, Height = button.Image.Height });
                    if (!string.IsNullOrWhiteSpace(button.Text)) row.Children.Add(new AccessText { Text = Label(button.Text), Margin = new Thickness(4,0,0,0) });
                    box.Content = row;
                }
            });
            target = box;
        }
        else if (source is Forms.TextBoxBase text)
        {
            var box = new TextBox { AcceptsReturn = text is Forms.TextBox tb && tb.Multiline, TextWrapping = TextWrapping.Wrap, IsReadOnly = text.ReadOnly, MaxLength = text.MaxLength };
            box.TextChanged += (_, _) => Run(() => text.Text = box.Text);
            updates.Add(() => { if (box.Text != text.Text) box.Text = text.Text; });
            target = box;
        }
        else if (source is Forms.Label label)
        {
            var box = new TextBlock { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
            updates.Add(() => box.Text = (label.Text ?? "").Replace("&", ""));
            target = box;
        }
        else if (source is EPCommon.ProgressBar progress)
        {
            var box = new System.Windows.Controls.ProgressBar { Foreground = Brushes.ForestGreen };
            updates.Add(() => { box.Minimum = progress.MinValue; box.Maximum = Math.Max(progress.MinValue + 1, progress.MaxValue); box.Value = progress.Value; });
            target = box;
        }
        else if (source is Forms.NumericUpDown number)
        {
            var box = new TextBox();
            box.LostKeyboardFocus += (_, _) => Run(() => { if (decimal.TryParse(box.Text, out var value)) number.Value = Math.Max(number.Minimum, Math.Min(number.Maximum, value)); });
            updates.Add(() => { if (!box.IsKeyboardFocusWithin) box.Text = number.Value.ToString(); });
            target = box;
        }
        else if (source is Forms.PictureBox picture)
        {
            var box = new System.Windows.Controls.Image { Stretch = Stretch.Uniform };
            System.Drawing.Image previous = null;
            updates.Add(() =>
            {
                if (picture.Image == null && picture.Width > 0 && picture.Height > 0)
                {
                    using (var image = new System.Drawing.Bitmap(picture.Width, picture.Height))
                    using (var graphics = System.Drawing.Graphics.FromImage(image))
                    {
                        graphics.Clear(picture.BackColor);
                        Raise(picture, "OnPaint", new Forms.PaintEventArgs(graphics, new System.Drawing.Rectangle(0,0,picture.Width,picture.Height)));
                        box.Source = SafeResources.ToImageSource(image);
                    }
                }
                else if (!ReferenceEquals(previous, picture.Image)) { previous = picture.Image; box.Source = SafeResources.ToImageSource(previous); }
            });
            target = box;
        }
        else if (source.Controls.Count > 0 || source is Forms.Panel) target = Panel(source);
        else target = new Border();
        controls[source] = target;
        WpfFocus.Register(source, target);
        target.Tag = source;
        target.Focusable = source.TabStop;
        KeyboardNavigation.SetTabIndex(target, source.TabIndex);
        target.GotKeyboardFocus += (_, e) =>
        {
            if (!ReferenceEquals(e.OriginalSource, target) && !(target is ComboBox)) return;
            Run(() => { Raise(source, "OnEnter", EventArgs.Empty); Raise(source, "OnGotFocus", EventArgs.Empty); });
        };
        target.LostKeyboardFocus += (_, e) =>
        {
            if (!ReferenceEquals(e.OriginalSource, target) && !(target is ComboBox)) return;
            Run(() => { Raise(source, "OnLeave", EventArgs.Empty); Raise(source, "OnLostFocus", EventArgs.Empty); });
        };
        target.PreviewTextInput += (_, e) =>
        {
            if (e.Text.Length != 1) return;
            var args = new Forms.KeyPressEventArgs(e.Text[0]);
            Run(() => Raise(source, "OnKeyPress", args));
            e.Handled = args.Handled;
        };
        target.PreviewKeyDown += (_, e) =>
        {
            var args = new Forms.KeyEventArgs((Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key));
            Run(() => Raise(source, "OnKeyDown", args));
            if (args.Handled) e.Handled = true;
        };
        updates.Add(() =>
        {
            Canvas.SetLeft(target, source.Left);
            Canvas.SetTop(target, source.Top);
            target.Width = Math.Max(0, source.Width);
            target.Height = Math.Max(0, source.Height);
            target.IsEnabled = source.Enabled;
            target.Visibility = LocallyVisible(source) ? Visibility.Visible : Visibility.Collapsed;
            if (form is EPForm ep)
            {
                var tip = ep.EPToolTip.GetToolTip(source);
                if (!string.IsNullOrEmpty(tip)) target.ToolTip = tip;
            }
        });
        return target;
    }
    private FrameworkElement Combo(Forms.ComboBox source)
    {
        var combo = new ComboBox { IsEditable = source.DropDownStyle != Forms.ComboBoxStyle.DropDownList, IsTextSearchEnabled = true, DisplayMemberPath = source.DisplayMember };
        object[] previous = null;
        combo.SelectionChanged += (_, _) => Run(() => source.SelectedIndex = combo.SelectedIndex);
        combo.LostKeyboardFocus += (_, _) => Run(() => { if (combo.IsEditable) source.Text = combo.Text; });
        updates.Add(() =>
        {
            var values = source.Items.Cast<object>().ToArray();
            if (previous == null || !previous.SequenceEqual(values)) { previous = values; combo.ItemsSource = values; }
            combo.DisplayMemberPath = source.DisplayMember;
            if (combo.SelectedIndex != source.SelectedIndex) combo.SelectedIndex = source.SelectedIndex;
            if (combo.IsEditable && !combo.IsKeyboardFocusWithin && combo.Text != source.Text) combo.Text = source.Text;
        });
        return combo;
    }
    private FrameworkElement Tabs(Forms.TabControl source)
    {
        var tabs = new TabControl();
        Forms.TabPage[] previous = null;
        tabs.SelectionChanged += (_, e) => { if (ReferenceEquals(e.Source, tabs)) Run(() => source.SelectedIndex = tabs.SelectedIndex); };
        updates.Add(() =>
        {
            var pages = source.TabPages.Cast<Forms.TabPage>().ToArray();
            if (previous == null || !previous.SequenceEqual(pages))
            {
                previous = pages;
                tabs.Items.Clear();
                foreach (var page in pages) tabs.Items.Add(new TabItem { Header = Label(page.Text), Content = Panel(page) });
            }
            if (tabs.SelectedIndex != source.SelectedIndex) tabs.SelectedIndex = source.SelectedIndex;
        });
        return tabs;
    }
    private FrameworkElement List(Forms.ListBox source)
    {
        var list = new ListBox { SelectionMode = source.SelectionMode == Forms.SelectionMode.One ? SelectionMode.Single : SelectionMode.Extended, DisplayMemberPath = source.DisplayMember };
        object[] previous = null;
        list.SelectionChanged += (_, _) => Run(() =>
        {
            source.ClearSelected();
            foreach (int index in list.SelectedItems.Cast<object>().Select(x => source.Items.IndexOf(x))) source.SetSelected(index, true);
        });
        list.MouseDoubleClick += (_, _) => Run(() => Raise(source, "OnDoubleClick", EventArgs.Empty));
        updates.Add(() =>
        {
            var items = source.Items.Cast<object>().ToArray();
            if (previous == null || !previous.SequenceEqual(items)) { previous = items; list.ItemsSource = items; }
            if (list.SelectedIndex != source.SelectedIndex) list.SelectedIndex = source.SelectedIndex;
        });
        return list;
    }
    public sealed class Row
    {
        public Forms.ListViewItem Source { get; }
        public string[] Cells => Source.SubItems.Cast<Forms.ListViewItem.ListViewSubItem>().Select(x => x.Text).ToArray();
        public Row(Forms.ListViewItem source) { Source = source; }
    }
    private FrameworkElement Table(Forms.ListView source)
    {
        var grid = new DataGrid { AutoGenerateColumns = false, IsReadOnly = true, CanUserAddRows = false, CanUserDeleteRows = false, CanUserSortColumns = true, SelectionMode = source.MultiSelect ? DataGridSelectionMode.Extended : DataGridSelectionMode.Single, SelectionUnit = DataGridSelectionUnit.FullRow };
        grid.Sorting += (_, e) => { e.Handled = true; Run(() => Raise(source, "OnColumnClick", new Forms.ColumnClickEventArgs(e.Column.DisplayIndex))); };
        string fingerprint = null;
        grid.SelectionChanged += (_, _) => Run(() =>
        {
            var selected = new HashSet<Forms.ListViewItem>(grid.SelectedItems.Cast<Row>().Select(x => x.Source));
            foreach (Forms.ListViewItem item in source.Items) item.Selected = selected.Contains(item);
        });
        grid.MouseDoubleClick += (_, e) => Run(() =>
        {
            if (source is QuantumListView quantum && grid.CurrentItem is Row row && grid.CurrentColumn != null)
            {
                int column = grid.CurrentColumn.DisplayIndex;
                if (column > 0) EditCell(quantum, row.Source, column);
            }
            else Raise(source, "OnDoubleClick", EventArgs.Empty);
            e.Handled = true;
        });
        updates.Add(() =>
        {
            var items = source.Items.Cast<Forms.ListViewItem>().ToArray();
            var next = string.Join("\u001f", source.Columns.Cast<Forms.ColumnHeader>().Select(x => x.Text + x.Width)) + "|" + string.Join("\u001e", items.Select(x => x.GetHashCode() + ":" + string.Join("\u001f", x.SubItems.Cast<Forms.ListViewItem.ListViewSubItem>().Select(s => s.Text))));
            if (next != fingerprint)
            {
                fingerprint = next;
                grid.Columns.Clear();
                if (source.Columns.Count == 0) grid.Columns.Add(new DataGridTextColumn { Header = "", Binding = new Binding("Cells[0]"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
                for (int i = 0; i < source.Columns.Count; i++) grid.Columns.Add(new DataGridTextColumn { Header = Label(source.Columns[i].Text), Binding = new Binding("Cells[" + i + "]"), Width = Math.Max(30, source.Columns[i].Width) });
                grid.ItemsSource = items.Select(x => new Row(x)).ToArray();
            }
            if (grid.ItemsSource != null)
            {
                var desired = ((IEnumerable<Row>)grid.ItemsSource).Where(x => x.Source.Selected).ToArray();
                if (!grid.SelectedItems.Cast<Row>().SequenceEqual(desired))
                {
                    grid.SelectedItems.Clear();
                    foreach (var item in desired) grid.SelectedItems.Add(item);
                }
            }
        });
        return grid;
    }
    private void EditCell(QuantumListView source, Forms.ListViewItem item, int column)
    {
        // Reuse the controller's row-specific choices and browse/validation logic.
        var editor = new CellEditorWindow(source, item, column) { Owner = window };
        editor.ShowDialog();
    }
    private FrameworkElement Tree(Forms.TreeView source)
    {
        var tree = new TreeView();
        string fingerprint = null;
        Func<Forms.TreeNode, TreeViewItem> build = null;
        build = node =>
        {
            object header = node.Text;
            if (source.CheckBoxes)
            {
                var check = new CheckBox { Content = node.Text, IsChecked = node.Checked };
                check.Checked += (_, _) => Run(() => node.Checked = true);
                check.Unchecked += (_, _) => Run(() => node.Checked = false);
                header = check;
            }
            var item = new TreeViewItem { Header = header, Tag = node, IsExpanded = node.IsExpanded };
            foreach (Forms.TreeNode child in node.Nodes) item.Items.Add(build(child));
            item.Selected += (_, e) => { Run(() => source.SelectedNode = node); e.Handled = true; };
            return item;
        };
        updates.Add(() =>
        {
            var next = string.Join("|", source.Nodes.Cast<Forms.TreeNode>().Select(x => x.Text + x.GetNodeCount(true)));
            if (fingerprint == next) return;
            fingerprint = next;
            tree.Items.Clear();
            foreach (Forms.TreeNode node in source.Nodes) tree.Items.Add(build(node));
        });
        return tree;
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        timer.Stop();
        window.SizeChanged -= Resize;
        updates.Clear();
        foreach (var control in controls.Keys) WpfFocus.Unregister(control);
        controls.Clear();
    }
}
