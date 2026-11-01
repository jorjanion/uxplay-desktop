using System;
using System.ComponentModel;
using System.Windows;
using UxPlayDesktop.Services;
using UxPlayDesktop.ViewModels;

namespace UxPlayDesktop;

public partial class MainWindow : Window
{
    private ITrayIconService? _trayService;
    private bool _isExplicitExit = false;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public void InitializeTray()
    {
        if (_trayService == null && DataContext is MainViewModel vm)
        {
            _trayService = new TrayIconService();
            _trayService.Initialize(this, vm);
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitializeTray();
        if (DataContext is MainViewModel vm)
        {
            vm.InitializeHotkeys(this);
        }
    }

    public void RequestCloseAndExit()
    {
        _isExplicitExit = true;
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is MainViewModel vmExit && !_isExplicitExit && vmExit.Settings.MinimizeToTray)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            Hide();
            _trayService?.ShowNotification("UxPlay Desktop", "UxPlay is running in the background. Click this tray icon to restore.");
            return;
        }

        if (DataContext is MainViewModel vmCleanup)
        {
            vmCleanup.DisposeHotkeys();
        }

        _trayService?.Dispose();
        base.OnClosing(e);
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState != WindowState.Minimized)
        {
            ShowInTaskbar = true;
        }
    }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
        }
        catch { }
        e.Handled = true;
    }
}