using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace SiteDesigner.UI;

public partial class DialogWindow : Window
{
    private readonly ContentControl dialogContent;
    public DialogWindow()
    {
        // A command loader may hold several byte-loaded assemblies with the same name.
        // Read this assembly's XAML directly instead of WPF's application-wide pack-URI cache.
        using (var stream = typeof(DialogWindow).Assembly.GetManifestResourceStream("SiteDesigner.UI.DialogWindow.xaml"))
        {
            if (stream == null) throw new InvalidOperationException("The Site Designer WPF dialog resources are missing.");
            Resources = (ResourceDictionary)XamlReader.Load(stream);
        }
        Style = (Style)Resources["SiteDesignerDialogStyle"];
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dialogContent = new ContentControl { HorizontalContentAlignment = HorizontalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Stretch };
        Content = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = dialogContent
        };
    }
    public void SetContent(FrameworkElement content) => dialogContent.Content = content;
}
