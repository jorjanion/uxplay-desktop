using UxPlayDesktop.Services;
using UxPlayDesktop.ViewModels;

namespace UxPlayDesktop.Models;

public enum PerformancePreset
{
    TeachingAndPresentation,
    LowLatencyGaming,
    HighQualityCinema,
    AudioOnly,
    Custom
}

public class PresetOption : ViewModelBase
{
    public PerformancePreset Preset { get; }

    public PresetOption(PerformancePreset preset)
    {
        Preset = preset;
    }

    public string DisplayName => Preset.GetDisplayName();
    public string Description => Preset.GetDescription();
    public string ExpectedDelay => Preset.GetExpectedDelay();

    public void Refresh()
    {
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(ExpectedDelay));
    }

    public override string ToString() => DisplayName;
}

public static class PresetExtensions
{
    public static string GetExpectedDelay(this PerformancePreset preset)
    {
        return preset switch
        {
            PerformancePreset.LowLatencyGaming => "~30-50 ms",
            PerformancePreset.TeachingAndPresentation => "~120-150 ms",
            PerformancePreset.HighQualityCinema => "~500-1000 ms",
            PerformancePreset.AudioOnly => "~200-250 ms",
            PerformancePreset.Custom => "Configurable",
            _ => string.Empty
        };
    }

    public static string GetDisplayName(this PerformancePreset preset)
    {
        var loc = LocalizationService.Instance;
        return preset switch
        {
            PerformancePreset.TeachingAndPresentation => loc["PresetTeaching"],
            PerformancePreset.LowLatencyGaming => loc["PresetGaming"],
            PerformancePreset.HighQualityCinema => loc["PresetCinema"],
            PerformancePreset.AudioOnly => loc["PresetAudio"],
            PerformancePreset.Custom => loc["PresetCustom"],
            _ => preset.ToString()
        };
    }

    public static string GetDescription(this PerformancePreset preset)
    {
        var loc = LocalizationService.Instance;
        return preset switch
        {
            PerformancePreset.TeachingAndPresentation => loc["PresetTeachingDesc"],
            PerformancePreset.LowLatencyGaming => loc["PresetGamingDesc"],
            PerformancePreset.HighQualityCinema => loc["PresetCinemaDesc"],
            PerformancePreset.AudioOnly => loc["PresetAudioDesc"],
            PerformancePreset.Custom => loc["PresetCustomDesc"],
            _ => string.Empty
        };
    }
}
