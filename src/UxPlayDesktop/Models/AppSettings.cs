using System.Text.Json.Serialization;

namespace UxPlayDesktop.Models;

public class AppSettings
{
    // General
    public string ServerName { get; set; } = "UxPlay Windows";
    public string Language { get; set; } = "auto";
    public bool StartOnBoot { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public bool StartMinimized { get; set; } = false;
    public bool AutoStartServer { get; set; } = true;

    // Preset & Streaming
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PerformancePreset Preset { get; set; } = PerformancePreset.TeachingAndPresentation;

    // Video Configuration
    public int DisplayWidth { get; set; } = 1920;
    public int DisplayHeight { get; set; } = 1080;
    public int TargetFps { get; set; } = 60;
    public bool EnableH265 { get; set; } = true;
    public bool ForceFullscreen { get; set; } = false;
    public bool BorderlessWindow { get; set; } = true; // Preferred for Discord/OBS window capture
    public string VideoSink { get; set; } = "d3d11videosink"; // d3d11videosink, d3d12videosink, autovideosink

    // Audio Configuration
    public bool EnableAudio { get; set; } = true;
    public int AudioDelayMs { get; set; } = 0; // Audio/Video sync calibration
    public string AudioSink { get; set; } = "autoaudiosink"; // autoaudiosink, wasapisink

    // Security & Pairing
    public bool RequirePin { get; set; } = false;
    public string? FixedPin { get; set; } = null; // 4-digit PIN code or null for random
    public string? Password { get; set; } = null;

    // Discovery & Networking
    public bool EnableBluetoothBeacon { get; set; } = true;
    public bool InhibitSleepAndScreensaver { get; set; } = true;
    public int NetworkPort { get; set; } = 0; // 0 for automatic port allocation

    // Advanced / Diagnostics
    public bool VerboseLogging { get; set; } = false;
    public string AdditionalArguments { get; set; } = string.Empty;
}
