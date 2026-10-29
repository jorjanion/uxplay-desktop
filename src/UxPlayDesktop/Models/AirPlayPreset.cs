using UxPlayDesktop.Services;

namespace UxPlayDesktop.Models;

public enum PerformancePreset
{
    TeachingAndPresentation,
    LowLatencyGaming,
    HighQualityCinema,
    AudioOnly,
    Custom
}

public static class PresetExtensions
{
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
