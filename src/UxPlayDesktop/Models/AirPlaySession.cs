using System;

namespace UxPlayDesktop.Models;

public enum ServerState
{
    Stopped,
    Starting,
    Listening,
    Connecting,
    Streaming,
    Stopping,
    Error
}

public class AirPlaySession
{
    public ServerState State { get; set; } = ServerState.Stopped;
    public string? ClientName { get; set; }
    public string? ClientModel { get; set; }
    public string? ClientIpAddress { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public string ActiveResolution { get; set; } = "1920x1080";
    public string ActiveCodec { get; set; } = "H.264";
    public double CurrentFps { get; set; } = 0.0;
    public string? ActivePinCode { get; set; }

    public TimeSpan SessionDuration => ConnectedAt.HasValue
        ? DateTime.Now - ConnectedAt.Value
        : TimeSpan.Zero;
}
