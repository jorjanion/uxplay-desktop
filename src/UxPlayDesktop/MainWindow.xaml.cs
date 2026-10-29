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

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            _trayService = new TrayIconService();
            _trayService.Initialize(this, vm);
        }
    }

    public void RequestCloseAndExit()
    {
        _isExplicitExit = true;
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isExplicitExit && DataContext is MainViewModel vm && vm.Settings.MinimizeToTray)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            _trayService?.ShowNotification("UxPlay Desktop", "UxPlay is running in the background. Click this tray icon to restore.");
            return;
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