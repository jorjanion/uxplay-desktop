using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace UxPlayDesktop.Services;

public class UpdateInfo
{
    public bool IsUpdateAvailable { get; set; }
    public string LatestVersion { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}

public interface IUpdateService
{
    Task<UpdateInfo?> CheckForUpdatesAsync();
    Task<bool> DownloadAndApplyUpdateAsync(string downloadUrl, Action<int>? progressCallback = null);
}

public class UpdateService : IUpdateService
{
    private const string GitHubApiUrl = "https://api.github.com/repos/jorjanion/uxplay-desktop/releases/latest";
    private readonly HttpClient _httpClient;

    public UpdateService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "UxPlayDesktop-AutoUpdater");
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
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
            if (root.TryGetProperty("assets", out var assetsElement) && assetsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assetsElement.EnumerateArray())
                {
                    var name = asset.GetProperty("name").GetString();
                    if (name != null && name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? string.Empty;
                        break;
                    }
                }
            }

            var notes = root.TryGetProperty("body", out var bodyEl) ? bodyEl.GetString() ?? "" : "";

            return new UpdateInfo
            {
                IsUpdateAvailable = true,
                LatestVersion = latestTag,
                ReleaseNotes = notes,
                DownloadUrl = downloadUrl
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DownloadAndApplyUpdateAsync(string downloadUrl, Action<int>? progressCallback = null)
    {
        if (string.IsNullOrEmpty(downloadUrl)) return false;

        try
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "UxPlayDesktopUpdate");
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            var zipPath = Path.Combine(tempDir, "update.zip");
            var extractPath = Path.Combine(tempDir, "extracted");

            // Download
            using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                using var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);

                var buffer = new byte[81920];
                long totalRead = 0;
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

            // Extract
            ZipFile.ExtractToDirectory(zipPath, extractPath, true);

            // Create batch updater script
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
        catch
        {
            return false;
        }
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
