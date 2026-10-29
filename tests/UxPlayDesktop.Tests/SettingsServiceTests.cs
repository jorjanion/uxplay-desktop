using System;
using System.IO;
using UxPlayDesktop.Models;
using UxPlayDesktop.Services;
using Xunit;

namespace UxPlayDesktop.Tests;

public class SettingsServiceTests : IDisposable
{
    private readonly string _tempSettingsPath;

    public SettingsServiceTests()
    {
        _tempSettingsPath = Path.Combine(Path.GetTempPath(), $"uxplay_test_{Guid.NewGuid():N}.json");
    }

    public void Dispose()
    {
        if (File.Exists(_tempSettingsPath))
        {
            try { File.Delete(_tempSettingsPath); } catch { }
        }
    }

    [Fact]
    public void Load_ReturnsDefaultSettings_WhenFileDoesNotExist()
    {
        var service = new SettingsService(_tempSettingsPath);
        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.Equal("UxPlay Windows", settings.ServerName);
        Assert.Equal(PerformancePreset.TeachingAndPresentation, settings.Preset);
        Assert.Equal(1920, settings.DisplayWidth);
        Assert.Equal(1080, settings.DisplayHeight);
        Assert.Equal(60, settings.TargetFps);
        Assert.True(settings.EnableH265);
        Assert.True(settings.InhibitSleepAndScreensaver);
    }

    [Fact]
    public void SaveAndLoad_PersistsValuesCorrectly()
    {
        var service = new SettingsService(_tempSettingsPath);
        var original = new AppSettings
        {
            ServerName = "Classroom iPad Mirror",
            Preset = PerformancePreset.LowLatencyGaming,
            DisplayWidth = 2560,
            DisplayHeight = 1440,
            TargetFps = 60,
            RequirePin = true,
            FixedPin = "1234",
            AudioDelayMs = 45,
            InhibitSleepAndScreensaver = true
        };

        service.Save(original);
        var loaded = service.Load();

        Assert.NotNull(loaded);
        Assert.Equal("Classroom iPad Mirror", loaded.ServerName);
        Assert.Equal(PerformancePreset.LowLatencyGaming, loaded.Preset);
        Assert.Equal(2560, loaded.DisplayWidth);
        Assert.Equal(1440, loaded.DisplayHeight);
        Assert.True(loaded.RequirePin);
        Assert.Equal("1234", loaded.FixedPin);
        Assert.Equal(45, loaded.AudioDelayMs);
    }

    [Fact]
    public void Load_HandlesCorruptedJson_FallbackToDefaults()
    {
        File.WriteAllText(_tempSettingsPath, "{ corrupted json !??! }");

        var service = new SettingsService(_tempSettingsPath);
        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.Equal("UxPlay Windows", settings.ServerName);
    }
}
