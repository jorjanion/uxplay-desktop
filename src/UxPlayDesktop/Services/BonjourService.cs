using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace UxPlayDesktop.Services;

public enum BonjourStatus
{
    Running,
    InstalledNotRunning,
    NotInstalled,
    EmbeddedAvailable
}

public interface IBonjourService
{
    BonjourStatus CheckStatus();
    bool TryStartService();
}

public class BonjourService : IBonjourService
{
    private const string ServiceName = "Bonjour Service";

    public BonjourStatus CheckStatus()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"query \"{ServiceName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi);
            if (process == null) return CheckRegistryFallback();

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(2000);

            if (process.ExitCode == 0)
            {
                if (output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase))
                {
                    return BonjourStatus.Running;
                }
                return BonjourStatus.InstalledNotRunning;
            }

            return CheckRegistryFallback();
        }
        catch
        {
            return CheckRegistryFallback();
        }
    }

    public bool TryStartService()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"start \"{ServiceName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            process.WaitForExit(4000);
            return CheckStatus() == BonjourStatus.Running;
        }
        catch
        {
            return false;
        }
    }

    private static BonjourStatus CheckRegistryFallback()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Bonjour Service");
            if (key != null)
            {
                return BonjourStatus.InstalledNotRunning;
            }
        }
        catch { }

        return BonjourStatus.NotInstalled;
    }
}
