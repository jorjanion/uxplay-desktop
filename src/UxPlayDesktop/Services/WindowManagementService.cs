using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace UxPlayDesktop.Services;

public class DisplayInfo
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsPrimary { get; set; }

    public string DisplayLabel => $"{Name} ({Width}x{Height}){(IsPrimary ? " [Principal]" : "")}";
}

public interface IWindowManagementService
{
    IReadOnlyList<DisplayInfo> GetConnectedDisplays();
    void SetAlwaysOnTop(bool enable, IntPtr? targetHwnd = null);
    string? CaptureScreenshot(string? outputDirectory = null);
    void MoveVideoWindowToDisplay(int displayIndex);
}

public class WindowManagementService : IWindowManagementService
{
    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new(-2);
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const uint SRCCOPY = 0x00CC0020;

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

    private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MONITORINFOEX
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }

    public IReadOnlyList<DisplayInfo> GetConnectedDisplays()
    {
        var list = new List<DisplayInfo>();
        int index = 0;

        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr _, ref RECT _, IntPtr _) =>
        {
            var mi = new MONITORINFOEX { cbSize = Marshal.SizeOf(typeof(MONITORINFOEX)) };
            if (GetMonitorInfo(hMonitor, ref mi))
            {
                int width = Math.Abs(mi.rcMonitor.Right - mi.rcMonitor.Left);
                int height = Math.Abs(mi.rcMonitor.Bottom - mi.rcMonitor.Top);
                bool isPrimary = (mi.dwFlags & 1) != 0;

                list.Add(new DisplayInfo
                {
                    Index = index++,
                    Name = $"Pantalla {index}",
                    Width = width,
                    Height = height,
                    IsPrimary = isPrimary
                });
            }
            return true;
        }, IntPtr.Zero);

        if (list.Count == 0)
        {
            list.Add(new DisplayInfo
            {
                Index = 0,
                Name = "Pantalla Principal",
                Width = 1920,
                Height = 1080,
                IsPrimary = true
            });
        }

        return list;
    }

    public void SetAlwaysOnTop(bool enable, IntPtr? targetHwnd = null)
    {
        IntPtr hwnd = targetHwnd ?? FindAirPlayVideoHwnd();
        if (hwnd == IntPtr.Zero) return;

        SetWindowPos(hwnd, enable ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
    }

    public void MoveVideoWindowToDisplay(int displayIndex)
    {
        IntPtr hwnd = FindAirPlayVideoHwnd();
        if (hwnd == IntPtr.Zero) return;

        var displays = GetConnectedDisplays();
        if (displayIndex < 0 || displayIndex >= displays.Count) return;

        // Reposition window to target display coordinates
        int monitorCount = 0;
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr _, ref RECT _, IntPtr _) =>
        {
            if (monitorCount == displayIndex)
            {
                var mi = new MONITORINFOEX { cbSize = Marshal.SizeOf(typeof(MONITORINFOEX)) };
                if (GetMonitorInfo(hMonitor, ref mi))
                {
                    int x = mi.rcWork.Left + 50;
                    int y = mi.rcWork.Top + 50;
                    SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_SHOWWINDOW);
                }
                return false;
            }
            monitorCount++;
            return true;
        }, IntPtr.Zero);
    }

    public string? CaptureScreenshot(string? outputDirectory = null)
    {
        try
        {
            var dir = string.IsNullOrWhiteSpace(outputDirectory)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "UxPlay")
                : outputDirectory;

            Directory.CreateDirectory(dir);

            IntPtr targetHwnd = FindAirPlayVideoHwnd();
            if (targetHwnd == IntPtr.Zero) targetHwnd = GetDesktopWindow();

            GetWindowRect(targetHwnd, out var rect);
            int width = Math.Max(rect.Right - rect.Left, 640);
            int height = Math.Max(rect.Bottom - rect.Top, 480);

            IntPtr hdcSrc = GetWindowDC(targetHwnd);
            IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
            IntPtr hOld = SelectObject(hdcDest, hBitmap);

            BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, SRCCOPY);
            SelectObject(hdcDest, hOld);

            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            var filePath = Path.Combine(dir, $"UxPlay_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(fs);
            }

            DeleteObject(hBitmap);
            DeleteDC(hdcDest);
            ReleaseDC(targetHwnd, hdcSrc);

            return filePath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to capture screenshot: {ex.Message}");
            return null;
        }
    }

    public static IntPtr FindAirPlayVideoHwnd()
    {
        foreach (var proc in Process.GetProcessesByName("uxplay"))
        {
            if (proc.MainWindowHandle != IntPtr.Zero)
            {
                return proc.MainWindowHandle;
            }
        }

        IntPtr hwnd = FindWindow(null, "UxPlay");
        if (hwnd != IntPtr.Zero) return hwnd;

        hwnd = FindWindow(null, "GstD3D11VideoSink");
        if (hwnd != IntPtr.Zero) return hwnd;

        return IntPtr.Zero;
    }
}
