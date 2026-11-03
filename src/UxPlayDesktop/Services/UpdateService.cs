using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace UxPlayDesktop.Services;

public class UpdateInfo
{
    public bool IsUpdateAvailable { get; set; }
    public string LatestVersion { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public bool IsInstaller { get; set; }
}

public interface IUpdateService
{
    string? LastError { get; }
    Task<UpdateInfo?> CheckForUpdatesAsync();
    Task<bool> DownloadAndApplyUpdateAsync(string downloadUrl, Action<int>? progressCallback = null);
}

public class UpdateService : IUpdateService
{
    private const string GitHubApiUrl = "https://api.github.com/repos/jorjanion/uxplay-desktop/releases/latest";
    private readonly HttpClient _httpClient;

    public string? LastError { get; private set; }

    public UpdateService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(10)
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "UxPlayDesktop-AutoUpdater");
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            LastError = null;
            var response = await _httpClient.GetStringAsync(GitHubApiUrl).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (!root.TryGetProperty("tag_name", out var tagElement)) return null;

            var latestTag = tagElement.GetString()?.Trim().TrimStart('v') ?? string.Empty;
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

            if (string.IsNullOrEmpty(latestTag)) return null;

            var isNewer = IsVersionNewer(latestTag, currentVersion);
            if (!isNewer) return new UpdateInfo { IsUpdateAvailable = false, LatestVersion = latestTag };

            string downloadUrl = string.Empty;
            bool isInstaller = false;

            if (root.TryGetProperty("assets", out var assetsElement) && assetsElement.ValueKind == JsonValueKind.Array)
            {
                var isArm64 = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ||
                              RuntimeInformation.OSArchitecture == Architecture.Arm64;
                var currentArch = isArm64 ? "arm64" : "x64";

                var parsedAssets = new List<(string Name, string Url)>();
                foreach (var asset in assetsElement.EnumerateArray())
                {
                    var name = asset.GetProperty("name").GetString();
                    var url = asset.GetProperty("browser_download_url").GetString();
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(url))
                    {
                        parsedAssets.Add((name, url));
                    }
                }

                (downloadUrl, isInstaller) = SelectBestAsset(parsedAssets, currentArch);
            }

            var notes = root.TryGetProperty("body", out var bodyEl) ? bodyEl.GetString() ?? "" : "";

            return new UpdateInfo
            {
                IsUpdateAvailable = true,
                LatestVersion = latestTag,
                ReleaseNotes = notes,
                DownloadUrl = downloadUrl,
                IsInstaller = isInstaller
            };
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }

    public async Task<bool> DownloadAndApplyUpdateAsync(string downloadUrl, Action<int>? progressCallback = null)
    {
        if (string.IsNullOrEmpty(downloadUrl))
        {
            LastError = "Download URL is empty.";
            return false;
        }

        try
        {
            LastError = null;
            var tempDir = Path.Combine(Path.GetTempPath(), $"UxPlayUpdate_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            bool isExe = downloadUrl.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
            var downloadedFile = Path.Combine(tempDir, isExe ? "UxPlayDesktop-Setup.exe" : "update.zip");

            // Download stream
            long totalBytes = -1L;
            long totalRead = 0;
            using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                totalBytes = response.Content.Headers.ContentLength ?? -1L;
                using var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var fileStream = new FileStream(downloadedFile, FileMode.Create, FileAccess.Write, FileShare.None);

                var buffer = new byte[81920];
                int read;
                while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read).ConfigureAwait(false);
                    totalRead += read;
                    if (totalBytes > 0 && progressCallback != null)
                    {
                        var progress = (int)((totalRead * 100) / totalBytes);
                        progressCallback(progress);
                    }
                }
            }

            if (totalBytes > 0 && totalRead < totalBytes)
            {
                LastError = $"Download was incomplete ({totalRead}/{totalBytes} bytes). Please try again.";
                return false;
            }

            if (isExe)
            {
                var fi = new FileInfo(downloadedFile);
                if (!fi.Exists || fi.Length < 50_000_000)
                {
                    LastError = $"Downloaded update package is incomplete ({fi.Length / 1024 / 1024} MB). Please try again or download directly from GitHub Releases.";
                    return false;
                }

                // Cleanly kill native engine process prior to running update
                try
                {
                    foreach (var proc in Process.GetProcessesByName("uxplay"))
                    {
                        proc.Kill(true);
                    }
                }
                catch { }

                // Run installer with /CLOSEAPPLICATIONS and /SILENT for automated upgrade
                var psi = new ProcessStartInfo
                {
                    FileName = downloadedFile,
                    Arguments = "/CLOSEAPPLICATIONS /SILENT",
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(psi);
                Environment.Exit(0);
                return true;
            }
            else
            {
                // Portable ZIP fallback update
                var extractPath = Path.Combine(tempDir, "extracted");
                ZipFile.ExtractToDirectory(downloadedFile, extractPath, true);

                var currentExe = Process.GetCurrentProcess().MainModule?.FileName;
                var installDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
                var pid = Process.GetCurrentProcess().Id;

                var updaterScript = Path.Combine(tempDir, "apply_update.bat");
                var scriptContent = $@"@echo off
timeout /t 1 /nobreak > nul
:waitpid
tasklist /fi ""PID eq {pid}"" | find ""{pid}"" > nul
if %ERRORLEVEL% equ 0 (
    timeout /t 1 /nobreak > nul
    goto waitpid
)
xcopy ""{extractPath}\*"" ""{installDir}\"" /s /e /y /q > nul
start """" ""{currentExe}""
exit
";
                await File.WriteAllTextAsync(updaterScript, scriptContent).ConfigureAwait(false);

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{updaterScript}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(psi);

                Environment.Exit(0);
                return true;
            }
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return false;
        }
    }

    public static (string DownloadUrl, bool IsInstaller) SelectBestAsset(
        IEnumerable<(string Name, string Url)> assets,
        string targetArch)
    {
        var assetList = new List<(string Name, string Url)>(assets);

        // 1st priority: Native Windows Setup Installer (.exe) for target architecture
        foreach (var (name, url) in assetList)
        {
            if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) &&
                name.Contains(targetArch, StringComparison.OrdinalIgnoreCase))
            {
                return (url, true);
            }
        }

        // 2nd priority: Any installer (.exe) fallback
        foreach (var (name, url) in assetList)
        {
            if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                return (url, true);
            }
        }

        // 3rd priority: Portable ZIP archive for target architecture
        foreach (var (name, url) in assetList)
        {
            if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
                name.Contains(targetArch, StringComparison.OrdinalIgnoreCase))
            {
                return (url, false);
            }
        }

        // 4th priority: Any portable ZIP fallback
        foreach (var (name, url) in assetList)
        {
            if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return (url, false);
            }
        }

        return (string.Empty, false);
    }

    private static bool IsVersionNewer(string latest, string current)
    {
        if (Version.TryParse(latest, out var vLatest) && Version.TryParse(current, out var vCurrent))
        {
            return vLatest > vCurrent;
        }

        return string.Compare(latest, current, StringComparison.OrdinalIgnoreCase) > 0;
    }
}
