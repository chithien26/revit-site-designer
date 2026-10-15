using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Forms = System.Windows.Forms;

namespace SiteDesigner.UI;

public static class WpfFocus
{
    private static readonly Dictionary<Forms.Control, FrameworkElement> Targets = new Dictionary<Forms.Control, FrameworkElement>();
    internal static void Register(Forms.Control source, FrameworkElement target) => Targets[source] = target;
    internal static void Unregister(Forms.Control source) => Targets.Remove(source);
    public static bool Focus(Forms.Control control) => Targets.TryGetValue(control, out var target) ? target.Focus() : control.Focus();
    public static bool HasFocus(Forms.Control control) => Targets.TryGetValue(control, out var target) ? target.IsKeyboardFocusWithin : control.Focused;
    public static void SelectAll(Forms.TextBoxBase control)
    {
        if (Targets.TryGetValue(control, out var target) && target is TextBox text) text.SelectAll();
        else control.SelectAll();
    }
}
