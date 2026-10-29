using System;
using System.Windows;
using UxPlayDesktop.Services;
using UxPlayDesktop.ViewModels;

namespace UxPlayDesktop;

public partial class App : Application
{
    private ICoreProcessManager? _processManager;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settingsService = new SettingsService();
        var argumentBuilder = new ArgumentBuilder();
        var logParser = new LogParser();
        var processManager = new CoreProcessManager(argumentBuilder, logParser);
        var bonjourService = new BonjourService();
        var autostartService = new AutostartService();

        _processManager = processManager;

        var mainVm = new MainViewModel(settingsService, processManager, bonjourService, autostartService);

        var mainWindow = new MainWindow
        {
            DataContext = mainVm
        };

        bool startMinimized = settingsService.CurrentSettings.StartMinimized;
        foreach (var arg in e.Args)
        {
            if (arg.Equals("--minimized", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("-min", StringComparison.OrdinalIgnoreCase))
            {
                startMinimized = true;
                break;
            }
        }

        if (startMinimized)
        {
            mainWindow.WindowState = WindowState.Minimized;
            mainWindow.Show();
        }
        else
        {
            mainWindow.Show();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_processManager != null)
        {
            await _processManager.StopAsync();
            if (_processManager is IDisposable disp)
            {
                disp.Dispose();
            }
        }
        base.OnExit(e);
    }
}
