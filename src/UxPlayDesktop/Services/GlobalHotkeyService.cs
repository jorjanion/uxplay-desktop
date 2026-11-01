using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace UxPlayDesktop.Services;

public interface IGlobalHotkeyService : IDisposable
{
    void Initialize(Window window);
    event Action? OnToggleFullscreen;
    event Action? OnToggleMute;
    event Action? OnToggleRecording;
    event Action? OnToggleServer;
    event Action? OnToggleAlwaysOnTop;
    event Action? OnCaptureScreenshot;
}

public class GlobalHotkeyService : IGlobalHotkeyService
{
    private HwndSource? _source;
    private IntPtr _hwnd = IntPtr.Zero;

    private const int WM_HOTKEY = 0x0312;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_NOREPEAT = 0x4000;

    private const int HOTKEY_ID_FULLSCREEN = 9001; // Ctrl+Shift+F
    private const int HOTKEY_ID_MUTE = 9002;       // Ctrl+Shift+M
    private const int HOTKEY_ID_RECORDING = 9003;  // Ctrl+Shift+R
    private const int HOTKEY_ID_SERVER = 9004;     // Ctrl+Shift+S
    private const int HOTKEY_ID_TOPMOST = 9005;    // Ctrl+Shift+T
    private const int HOTKEY_ID_SCREENSHOT = 9006; // Ctrl+Shift+C

    private const uint VK_F = 0x46;
    private const uint VK_M = 0x4D;
    private const uint VK_R = 0x52;
    private const uint VK_S = 0x53;
    private const uint VK_T = 0x54;
    private const uint VK_C = 0x43;

    public event Action? OnToggleFullscreen;
    public event Action? OnToggleMute;
    public event Action? OnToggleRecording;
    public event Action? OnToggleServer;
    public event Action? OnToggleAlwaysOnTop;
    public event Action? OnCaptureScreenshot;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public void Initialize(Window window)
    {
        var helper = new WindowInteropHelper(window);
        _hwnd = helper.EnsureHandle();
        _source = HwndSource.FromHwnd(_hwnd);
        _source?.AddHook(HwndHook);

        RegisterAllHotkeys();
    }

    private void RegisterAllHotkeys()
    {
        if (_hwnd == IntPtr.Zero) return;

        uint mods = MOD_CONTROL | MOD_SHIFT | MOD_NOREPEAT;
        RegisterHotKey(_hwnd, HOTKEY_ID_FULLSCREEN, mods, VK_F);
        RegisterHotKey(_hwnd, HOTKEY_ID_MUTE, mods, VK_M);
        RegisterHotKey(_hwnd, HOTKEY_ID_RECORDING, mods, VK_R);
        RegisterHotKey(_hwnd, HOTKEY_ID_SERVER, mods, VK_S);
        RegisterHotKey(_hwnd, HOTKEY_ID_TOPMOST, mods, VK_T);
        RegisterHotKey(_hwnd, HOTKEY_ID_SCREENSHOT, mods, VK_C);
    }

    private void UnregisterAllHotkeys()
    {
        if (_hwnd == IntPtr.Zero) return;

        UnregisterHotKey(_hwnd, HOTKEY_ID_FULLSCREEN);
        UnregisterHotKey(_hwnd, HOTKEY_ID_MUTE);
        UnregisterHotKey(_hwnd, HOTKEY_ID_RECORDING);
        UnregisterHotKey(_hwnd, HOTKEY_ID_SERVER);
        UnregisterHotKey(_hwnd, HOTKEY_ID_TOPMOST);
        UnregisterHotKey(_hwnd, HOTKEY_ID_SCREENSHOT);
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            switch (id)
            {
                case HOTKEY_ID_FULLSCREEN:
                    OnToggleFullscreen?.Invoke();
                    handled = true;
                    break;
                case HOTKEY_ID_MUTE:
                    OnToggleMute?.Invoke();
                    handled = true;
                    break;
                case HOTKEY_ID_RECORDING:
                    OnToggleRecording?.Invoke();
                    handled = true;
                    break;
                case HOTKEY_ID_SERVER:
                    OnToggleServer?.Invoke();
                    handled = true;
                    break;
                case HOTKEY_ID_TOPMOST:
                    OnToggleAlwaysOnTop?.Invoke();
                    handled = true;
                    break;
                case HOTKEY_ID_SCREENSHOT:
                    OnCaptureScreenshot?.Invoke();
                    handled = true;
                    break;
            }
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        UnregisterAllHotkeys();
        _source?.RemoveHook(HwndHook);
        _source = null;
    }
}
