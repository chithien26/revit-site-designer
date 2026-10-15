using System;
using System.Windows;
using System.Windows.Controls;
using EPCommon;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace SiteDesigner.UI;

internal sealed class CellEditorWindow : Window
{
    public CellEditorWindow(QuantumListView source, Forms.ListViewItem item, int column)
    {
        Title = item.SubItems[0].Text;
        Width = 480;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var panel = new StackPanel { Margin = new Thickness(16) };
        panel.Children.Add(new TextBlock { Text = Title, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,0,0,10) });
        var choices = source.WpfCellChoices(item);
        var text = new TextBox { Text = item.SubItems[column].Text, MinHeight = 26 };
        var combo = new ComboBox { ItemsSource = choices, SelectedItem = item.SubItems[column].Text, MinHeight = 26 };
        panel.Children.Add(choices.Length > 0 ? (UIElement)combo : text);
        string key = item.SubItems[0].Text;
        if (key.IndexOf("template", StringComparison.OrdinalIgnoreCase) >= 0 || key.IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            var browse = new Button { Content = "Browse…", HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0,8,0,0), Padding = new Thickness(12,3,12,3) };
            browse.Click += (_, _) =>
            {
                if (key.IndexOf("template", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var dialog = new OpenFileDialog { Filter = "Revit family templates (*.rft)|*.rft|All files (*.*)|*.*", FileName = text.Text };
                    if (dialog.ShowDialog(this) == true) text.Text = dialog.FileName;
                }
                else
                {
                    using (var dialog = new Forms.FolderBrowserDialog { SelectedPath = text.Text })
                        if (dialog.ShowDialog(new EPCommon.WindowWrapper(new System.Windows.Interop.WindowInteropHelper(this).Handle)) == Forms.DialogResult.OK) text.Text = dialog.SelectedPath;
                }
            };
            panel.Children.Add(browse);
        }
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,16,0,0) };
        var ok = new Button { Content = "OK", IsDefault = true, MinWidth = 80, Padding = new Thickness(8,4,8,4) };
        var cancel = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80, Margin = new Thickness(8,0,0,0), Padding = new Thickness(8,4,8,4) };
        ok.Click += (_, _) => { source.SetWpfCellValue(item, column, choices.Length > 0 ? Convert.ToString(combo.SelectedItem) : text.Text); DialogResult = true; };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        panel.Children.Add(buttons);
        Content = panel;
        Loaded += (_, _) => { if (choices.Length == 0) { text.Focus(); text.SelectAll(); } else combo.Focus(); };
    }
}
