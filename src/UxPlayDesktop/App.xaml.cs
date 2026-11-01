using System;
using System.Threading;
using System.Windows;
using UxPlayDesktop.Services;
using UxPlayDesktop.ViewModels;

namespace UxPlayDesktop;

public partial class App : Application
{
    private ICoreProcessManager? _processManager;
    private static Mutex? _singleInstanceMutex;

    public App()
    {
        DispatcherUnhandledException += (s, e) =>
        {
            try
            {
                var dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UxPlayDesktop");
                System.IO.Directory.CreateDirectory(dir);
                System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "crash.log"), e.Exception.ToString());
                MessageBox.Show($"Ha ocurrido un error inesperado en UxPlay Desktop:\n\n{e.Exception.Message}\n\nLos detalles se han guardado en crash.log.", "UxPlay Desktop - Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch { }
            e.Handled = true;
            Shutdown(1);
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        const string mutexName = "Local\\UxPlayDesktop_SingleInstance_Mutex";
        _singleInstanceMutex = new Mutex(true, mutexName, out bool isOnlyInstance);

        if (!isOnlyInstance)
        {
            // Another instance is already running: signal it to restore and bring to foreground
            int wmShow = TrayIconService.RegisterWindowMessage("UxPlayDesktop_ShowMainWindow_Message");
            TrayIconService.PostMessage((IntPtr)0xffff /* HWND_BROADCAST */, wmShow, IntPtr.Zero, IntPtr.Zero);
            Shutdown();
            return;
        }

        base.OnStartup(e);

        var settingsService = new SettingsService();
        var argumentBuilder = new ArgumentBuilder();
        var logParser = new LogParser();
        var processManager = new CoreProcessManager(argumentBuilder, logParser);
        var bonjourService = new BonjourService();
        var autostartService = new AutostartService();
        var updateService = new UpdateService();

        _processManager = processManager;

        var mainVm = new MainViewModel(settingsService, processManager, bonjourService, autostartService, updateService);

        var mainWindow = new MainWindow
        {
            DataContext = mainVm
        };

        // Initialize tray icon early before window state transitions
        mainWindow.InitializeTray();

        bool startMinimized = settingsService.CurrentSettings.StartMinimized;
        foreach (var arg in e.Args)
        {
            if (arg.Equals("--minimized", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("-minimized", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("/minimized", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("-min", StringComparison.OrdinalIgnoreCase))
            {
                startMinimized = true;
                break;
            }
            if (arg.Equals("--normal", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("-normal", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("/normal", StringComparison.OrdinalIgnoreCase))
            {
                startMinimized = false;
                break;
            }
        }

        if (startMinimized)
        {
            if (settingsService.CurrentSettings.MinimizeToTray)
            {
                mainWindow.WindowState = WindowState.Minimized;
                mainWindow.ShowInTaskbar = false;
                mainWindow.Hide();
            }
            else
            {
                mainWindow.WindowState = WindowState.Minimized;
                mainWindow.Show();
            }
        }
        else
        {
            mainWindow.Show();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_singleInstanceMutex != null)
        {
            try { _singleInstanceMutex.ReleaseMutex(); } catch { }
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
        }

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
