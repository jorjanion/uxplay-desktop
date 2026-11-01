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
    public bool BorderlessWindow { get; set; } = true; // Clean borderless display mode
    public bool AlwaysOnTop { get; set; } = false; // Floating picture-in-picture mode
    public int TargetDisplayIndex { get; set; } = 0; // 0 for primary monitor
    public string VideoSink { get; set; } = "d3d11videosink"; // d3d11videosink, d3d12videosink, autovideosink

    // Audio Configuration
    public bool EnableAudio { get; set; } = true;
    public int AudioDelayMs { get; set; } = 0; // Audio/Video sync calibration
    public double AudioVolume { get; set; } = 1.0; // 0.0 to 1.0
    public bool IsMuted { get; set; } = false;
    public string AudioSink { get; set; } = "autoaudiosink"; // autoaudiosink, wasapisink

    // Recording & Captures
    public bool EnableRecording { get; set; } = false;
    public string RecordingDirectory { get; set; } = string.Empty;
    public string ScreenshotsDirectory { get; set; } = string.Empty;

    // Hotkeys & Productivity
    public bool EnableGlobalHotkeys { get; set; } = true;

    // Appearance & Personalization
    public string AccentColor { get; set; } = "#38BDF8"; // Sky Blue (default)
    public bool CompactMode { get; set; } = false;

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
