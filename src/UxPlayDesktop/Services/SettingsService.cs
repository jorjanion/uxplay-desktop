using System;
using System.IO;
using System.Text.Json;
using UxPlayDesktop.Models;

namespace UxPlayDesktop.Services;

public interface ISettingsService
{
    AppSettings CurrentSettings { get; }
    AppSettings Load();
    void Save(AppSettings settings);
}

public class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _settingsFilePath;
    private AppSettings _currentSettings;

    public SettingsService(string? customPath = null)
    {
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            _settingsFilePath = customPath;
        }
        else
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "UxPlayDesktop");
            _settingsFilePath = Path.Combine(folder, "settings.json");
        }

        _currentSettings = Load();
    }

    public AppSettings CurrentSettings => _currentSettings;

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                if (loaded != null)
                {
                    _currentSettings = loaded;
                    return _currentSettings;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings from {_settingsFilePath}: {ex.Message}");
        }

        _currentSettings = new AppSettings();
        Save(_currentSettings);
        return _currentSettings;
    }

    public void Save(AppSettings settings)
    {
        try
        {
            _currentSettings = settings ?? new AppSettings();
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_currentSettings, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings to {_settingsFilePath}: {ex.Message}");
        }
    }
}
