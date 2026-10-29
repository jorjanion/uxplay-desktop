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
    public static string GetDisplayName(this PerformancePreset preset) => preset switch
    {
        PerformancePreset.TeachingAndPresentation => "Teaching & Presentations (Crisp Text, Stable)",
        PerformancePreset.LowLatencyGaming => "Low Latency & Gaming (Fastest Response)",
        PerformancePreset.HighQualityCinema => "High Definition Cinema (Maximum Quality, 4K)",
        PerformancePreset.AudioOnly => "Audio Only (AirPlay Music / Podcast)",
        PerformancePreset.Custom => "Custom Advanced Configuration",
        _ => preset.ToString()
    };

    public static string GetDescription(this PerformancePreset preset) => preset switch
    {
        PerformancePreset.TeachingAndPresentation =>
            "Optimized for online classes, Discord streaming, and slides. Direct3D hardware acceleration with enhanced text sharpness and automatic screensaver inhibition.",
        PerformancePreset.LowLatencyGaming =>
            "Prioritizes minimal buffering latency and fluid 60 FPS response for fast interactions.",
        PerformancePreset.HighQualityCinema =>
            "Enables H.265 / HEVC decode for ultra-sharp high-bitrate streaming.",
        PerformancePreset.AudioOnly =>
            "Disables video decoding to save resources; streams high-fidelity lossless ALAC audio with album cover display.",
        PerformancePreset.Custom =>
            "Manually customize resolution, framerate, video sink, and port parameters.",
        _ => string.Empty
    };
}
