using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace SiteDesigner.UI;

// The original forms remain controllers for validation and Revit transactions.
// Their HWNDs are never displayed or hosted. Every visible control is native WPF.
public class WpfForm : Forms.Form
{
    private DialogWindow window;
    private WpfDialogPresenter presenter;
    private bool initialized;
    private bool closing;
    private bool modal;
    public Window WpfWindow => window;
    public bool IsWpfOpen => window != null;

    public new Forms.DialogResult ShowDialog() => ShowDialog(ApplicationOwner());
    public new Forms.DialogResult ShowDialog(Forms.IWin32Window owner)
    {
        base.DialogResult = Forms.DialogResult.None;
        modal = true;
        Open(owner);
        window.ShowDialog();
        return base.DialogResult;
    }
    public new void Show() => Show(ApplicationOwner());
    public new void Show(Forms.IWin32Window owner)
    {
        modal = false;
        Open(owner);
        window.Show();
    }
    private static Forms.IWin32Window ApplicationOwner() => EPCommon.ApplicationUtility.RevitWindowWrapper;

    private void Open(Forms.IWin32Window owner)
    {
        if (window != null) throw new InvalidOperationException("This dialog is already open.");
        EPCommon.RuntimeDiagnostics.Write("Preparing WPF dialog " + GetType().FullName);
        // ListView.SelectedItems and data-bound ComboBoxes need their controller handles.
        CreateControllerHandles(this);
        if (!initialized)
        {
            initialized = true;
            OnLoad(EventArgs.Empty);
        }
        window = new DialogWindow { Title = Text, ResizeMode = IsResizable ? ResizeMode.CanResize : ResizeMode.NoResize };
        if (owner != null && owner.Handle != IntPtr.Zero) new WindowInteropHelper(window).Owner = owner.Handle;
        var size = ClientSize;
        window.Width = Math.Max(200, size.Width + 18);
        window.Height = Math.Max(100, size.Height + 40);
        window.MinWidth = Math.Max(150, MinimumSize.Width);
        window.MinHeight = Math.Max(80, MinimumSize.Height);
        window.MaxHeight = SystemParameters.WorkArea.Height;
        window.MaxWidth = SystemParameters.WorkArea.Width;
        if (StartPosition == Forms.FormStartPosition.Manual && SystemParameters.WorkArea.Contains(new System.Windows.Point(Location.X, Location.Y)))
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = Location.X;
            window.Top = Location.Y;
        }
        presenter = new WpfDialogPresenter(this, window);
        window.SetContent(presenter.Content);
        window.Closing += WindowClosing;
        window.Closed += WindowClosed;
        window.PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                if (CancelButton is Forms.Control cancel) presenter.Click(cancel);
                else { base.DialogResult = Forms.DialogResult.Cancel; Close(); }
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && !(e.OriginalSource is System.Windows.Controls.TextBox box && box.AcceptsReturn))
            {
                if (AcceptButton is Forms.Control accept) { presenter.Click(accept); e.Handled = true; }
            }
            else if (e.Key == Key.F1)
            {
                OnHelpRequested(new Forms.HelpEventArgs(new System.Drawing.Point()));
                e.Handled = true;
            }
        };
        window.ContentRendered += (_, _) =>
        {
            EPCommon.RuntimeDiagnostics.Write("Displayed WPF dialog " + GetType().FullName + "; owner=" + (owner?.Handle ?? IntPtr.Zero));
            OnShown(EventArgs.Empty);
            presenter.Refresh();
            window?.Activate();
        };
    }
    private bool IsResizable => FormBorderStyle == Forms.FormBorderStyle.Sizable || FormBorderStyle == Forms.FormBorderStyle.SizableToolWindow;
    private static void CreateControllerHandles(Forms.Control control)
    {
        var handle = control.Handle;
        foreach (Forms.Control child in control.Controls) CreateControllerHandles(child);
    }
    public new void Close()
    {
        if (window != null) { if (!closing) window.Close(); }
        else base.Close();
    }
    public new void Hide() { if (window != null) window.Hide(); }
    public new void Refresh()
    {
        if (window == null) return;
        presenter.Refresh();
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => window?.UpdateLayout()));
    }
    public new bool Focus() { return window?.Activate() ?? false; }
    private void WindowClosing(object sender, CancelEventArgs e)
    {
        if (closing) return;
        closing = true;
        bool accepted = false;
        try
        {
            Location = new System.Drawing.Point((int)window.Left, (int)window.Top);
            if (IsResizable) ClientSize = new System.Drawing.Size(Math.Max(1, (int)presenter.Content.ActualWidth), Math.Max(1, (int)presenter.Content.ActualHeight));
            var args = new Forms.FormClosingEventArgs(Forms.CloseReason.UserClosing, false);
            OnFormClosing(args);
            e.Cancel = args.Cancel;
            if (!e.Cancel && base.DialogResult == Forms.DialogResult.None) base.DialogResult = Forms.DialogResult.Cancel;
            accepted = !e.Cancel;
        }
        // Losing keyboard focus can run controller handlers before WPF raises Closed.
        finally { closing = accepted; }
    }
    private void WindowClosed(object sender, EventArgs e)
    {
        EPCommon.RuntimeDiagnostics.Write("Closed WPF dialog " + GetType().FullName);
        presenter?.Dispose();
        presenter = null;
        window = null;
        closing = false;
        OnFormClosed(new Forms.FormClosedEventArgs(Forms.CloseReason.UserClosing));
    }
    internal void FinishInteraction()
    {
        presenter?.Refresh();
        if (!closing && modal && base.DialogResult != Forms.DialogResult.None) window?.Close();
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { window?.Close(); presenter?.Dispose(); }
        base.Dispose(disposing);
    }
}

