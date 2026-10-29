using System;
using System.Diagnostics;
using System.IO;
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
    bool RepairOrInstallService();
}

public class BonjourService : IBonjourService
{
    private const string ServiceName = "Bonjour Service";

    public bool RepairOrInstallService()
    {
        try
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var batCandidates = new[]
            {
                Path.Combine(appDir, "fix_bonjour.bat"),
                Path.Combine(appDir, "resources", "fix_bonjour.bat"),
                Path.Combine(appDir, "..", "..", "fix_bonjour.bat"),
                Path.Combine(appDir, "..", "..", "resources", "fix_bonjour.bat")
            };

            string? batPath = null;
            foreach (var candidate in batCandidates)
            {
                if (File.Exists(candidate))
                {
                    batPath = Path.GetFullPath(candidate);
                    break;
                }
            }

            if (batPath != null)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"\"{batPath}\"\"",
                    WorkingDirectory = Path.GetDirectoryName(batPath),
                    Verb = "runas",
                    UseShellExecute = true
                };

                using var proc = Process.Start(psi);
                proc?.WaitForExit();

                return CheckStatus() == BonjourStatus.Running;
            }

            var mdnsExe = Path.Combine(appDir, "mDNSResponder.exe");
            if (!File.Exists(mdnsExe))
            {
                var cand = Path.Combine(appDir, "runtimes", "win-x64", "native", "mDNSResponder.exe");
                if (File.Exists(cand)) mdnsExe = cand;
            }

            if (File.Exists(mdnsExe))
            {
                var script = $"sc.exe stop \"{ServiceName}\"; sc.exe delete \"{ServiceName}\"; & '{mdnsExe}' -install; sc.exe config \"{ServiceName}\" start= auto; net start \"{ServiceName}\"";
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    Verb = "runas",
                    UseShellExecute = true
                };

                using var proc = Process.Start(psi);
                proc?.WaitForExit();

                return CheckStatus() == BonjourStatus.Running;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

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

                // If stopped, verify that the binary on disk actually exists
                if (IsServiceBinaryPresent())
                {
                    return BonjourStatus.InstalledNotRunning;
                }

                return BonjourStatus.NotInstalled;
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
                FileName = "net.exe",
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

    private static bool IsServiceBinaryPresent()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Bonjour Service");
            if (key != null)
            {
                var imagePath = key.GetValue("ImagePath") as string;
                if (!string.IsNullOrEmpty(imagePath))
                {
                    var cleanPath = imagePath.Trim('"');
                    return File.Exists(cleanPath);
                }
            }
        }
        catch { }

        return false;
    }

    private static BonjourStatus CheckRegistryFallback()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Bonjour Service");
            if (key != null)
            {
                if (IsServiceBinaryPresent())
                {
                    return BonjourStatus.InstalledNotRunning;
                }
            }
        }
        catch { }

        return BonjourStatus.NotInstalled;
    }
}
