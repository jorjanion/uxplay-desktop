using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace UxPlayDesktop.Services;

public interface IAutostartService
{
    bool IsAutostartEnabled();
    void SetAutostart(bool enable, bool startMinimized = false);
}

public class AutostartService : IAutostartService
{
    private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "UxPlayDesktop";

    public bool IsAutostartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
            var value = key?.GetValue(AppName) as string;
            return !string.IsNullOrEmpty(value);
        }
        catch
        {
            return false;
        }
    }

    public void SetAutostart(bool enable, bool startMinimized = false)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
            if (key == null) return;

            if (enable)
            {
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath))
                {
                    var args = startMinimized ? " --minimized" : "";
                    key.SetValue(AppName, $"\"{exePath}\"{args}");
                }
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to configure autostart in registry: {ex.Message}");
        }
    }
}
