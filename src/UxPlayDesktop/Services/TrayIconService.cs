using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using UxPlayDesktop.ViewModels;

namespace UxPlayDesktop.Services;

public interface ITrayIconService : IDisposable
{
    void Initialize(Window window, MainViewModel viewModel);
    void UpdateTooltip(string text);
    void ShowNotification(string title, string message);
    void RestoreWindow();
}

public class TrayIconService : ITrayIconService
{
    private Window? _window;
    private MainViewModel? _viewModel;
    private HwndSource? _hwndSource;
    private IntPtr _hIcon = IntPtr.Zero;
    private bool _isCreated = false;
    private NOTIFYICONDATA _nid;

    private const int WM_USER = 0x0400;
    private const int WM_TRAY = WM_USER + 100;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_LBUTTONDBLCLK = 0x0203;
    private const int WM_RBUTTONUP = 0x0205;

    private const int NIM_ADD = 0x00000000;
    private const int NIM_MODIFY = 0x00000001;
    private const int NIM_DELETE = 0x00000002;

    private const int NIF_MESSAGE = 0x00000001;
    private const int NIF_ICON = 0x00000002;
    private const int NIF_TIP = 0x00000004;
    private const int NIF_INFO = 0x00000010;

    private const int NIIF_INFO = 0x00000001;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public int dwState;
        public int dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public int uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public int dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public void Initialize(Window window, MainViewModel viewModel)
    {
        _window = window;
        _viewModel = viewModel;

        var helper = new WindowInteropHelper(window);
        var hwnd = helper.EnsureHandle();
        _hwndSource = HwndSource.FromHwnd(hwnd);
        _hwndSource?.AddHook(WndProc);

        _hIcon = LoadAppIcon();

        _nid = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA)),
            hWnd = hwnd,
            uID = 1001,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = WM_TRAY,
            hIcon = _hIcon,
            szTip = "UxPlay Desktop - AirPlay Mirroring"
        };

        _isCreated = Shell_NotifyIcon(NIM_ADD, ref _nid);
    }

    private static IntPtr LoadAppIcon()
    {
        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] iconCandidates =
            [
                Path.Combine(baseDir, "resources", "newicon.ico"),
                Path.Combine(baseDir, "newicon.ico"),
                Path.Combine(baseDir, "..", "..", "..", "..", "..", "resources", "newicon.ico")
            ];

            foreach (var candidate in iconCandidates)
            {
                var full = Path.GetFullPath(candidate);
                if (File.Exists(full))
                {
                    var h = LoadImage(IntPtr.Zero, full, 1 /* IMAGE_ICON */, 16, 16, 0x00000010 /* LR_LOADFROMFILE */);
                    if (h != IntPtr.Zero) return h;
                }
            }
        }
        catch { }

        return IntPtr.Zero;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_TRAY)
        {
            int action = lParam.ToInt32();
            if (action == WM_LBUTTONDBLCLK || action == WM_LBUTTONUP)
            {
                RestoreWindow();
                handled = true;
            }
            else if (action == WM_RBUTTONUP)
            {
                ShowContextMenu();
                handled = true;
            }
        }
        return IntPtr.Zero;
    }

    public void RestoreWindow()
    {
        if (_window == null) return;
        _window.Show();
        _window.ShowInTaskbar = true;
        _window.WindowState = WindowState.Normal;
        _window.Activate();
        _window.Focus();
        if (_hwndSource != null)
        {
            SetForegroundWindow(_hwndSource.Handle);
        }
    }

    private void ShowContextMenu()
    {
        if (_window == null || _viewModel == null) return;

        var menu = new ContextMenu();

        var headerItem = new MenuItem
        {
            Header = "UxPlay Desktop",
            FontWeight = FontWeights.Bold
        };
        headerItem.Click += (_, _) => RestoreWindow();
        menu.Items.Add(headerItem);

        menu.Items.Add(new Separator());

        var toggleItem = new MenuItem
        {
            Header = _viewModel.IsServerRunning ? "Stop AirPlay Server" : "Start AirPlay Server"
        };
        toggleItem.Click += (_, _) =>
        {
            if (_viewModel.ToggleServerCommand.CanExecute(null))
            {
                _viewModel.ToggleServerCommand.Execute(null);
            }
        };
        menu.Items.Add(toggleItem);

        menu.Items.Add(new Separator());

        var exitItem = new MenuItem
        {
            Header = "Exit"
        };
        exitItem.Click += (_, _) =>
        {
            if (_window is MainWindow mw)
            {
                mw.RequestCloseAndExit();
            }
            else
            {
                Application.Current.Shutdown();
            }
        };
        menu.Items.Add(exitItem);

        menu.IsOpen = true;
    }

    public void UpdateTooltip(string text)
    {
        if (!_isCreated) return;
        _nid.uFlags = NIF_TIP;
        _nid.szTip = text.Length > 127 ? text[..127] : text;
        Shell_NotifyIcon(NIM_MODIFY, ref _nid);
    }

    public void ShowNotification(string title, string message)
    {
        if (!_isCreated) return;
        _nid.uFlags = NIF_INFO;
        _nid.szInfoTitle = title.Length > 63 ? title[..63] : title;
        _nid.szInfo = message.Length > 255 ? message[..255] : message;
        _nid.dwInfoFlags = NIIF_INFO;
        Shell_NotifyIcon(NIM_MODIFY, ref _nid);
    }

    public void Dispose()
    {
        if (_isCreated)
        {
            Shell_NotifyIcon(NIM_DELETE, ref _nid);
            _isCreated = false;
        }

        if (_hIcon != IntPtr.Zero)
        {
            DestroyIcon(_hIcon);
            _hIcon = IntPtr.Zero;
        }

        _hwndSource?.RemoveHook(WndProc);
    }
}
