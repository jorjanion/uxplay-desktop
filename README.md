# UxPlay Desktop

**UxPlay Desktop** is a modern, high-performance, and intuitive Windows desktop application for AirPlay screen mirroring from iPad and iPhone devices to Windows PCs.

Specifically designed for **online teaching, Discord streaming, and interactive presentations**, UxPlay Desktop eliminates technical friction while delivering low latency, crisp 1080p/60fps video, and crystal-clear audio.

---

## Key Highlights & Solved Problems

UxPlay Desktop directly resolves the primary pain points found in previous Windows AirPlay implementations (such as `leapbtw/uxplay-windows`):

1. **Modern iPadOS & iOS Reliability (Handshake & Session Stability)**:
   - Integrates the latest upstream protocol improvements from `FDH2/UxPlay`.
   - Properly handles `TEARDOWN 110` packets on modern iPadOS / iOS versions.
   - Increases RAOP jitter buffer depth (from 32 to 256) to eliminate dropouts over home and school Wi-Fi.
   - Implements dead HTTP/RTSP connection recycling and non-blocking handshakes.
   - Fixes Windows network adapter binding conflicts when WSL2 / Hyper-V virtual adapters are active.

2. **First-Frame Freeze Bug Eliminated**:
   - Upgraded GStreamer Direct3D video sink pipeline lifecycle: codec initialization is deferred until stream negotiation completes and keyframes arrive.
   - Prevents the classic frozen video state on first connection.

3. **Modern Fluent UI & Human-Centric Design**:
   - Built with modern .NET / C# and WPF Fluent UI design principles.
   - Replaces technical jargon (e.g. `d3d11videosink vs d3d12videosink`, buffer sizes, socket ports) with simple presets: **Teaching / Presentations (Crisp Text)**, **Low Latency Gaming**, and **Audio Only**.
   - No more manual editing of raw text configuration files in Notepad.
   - Live stream HUD displaying connected device name, active resolution, FPS counter, and connection quality.
   - Discord & OBS friendly: borderless capture mode and dedicated window title for easy window capture.
   - System tray integration with quick actions (Start/Stop, Pin Pairing, Settings, Log viewer).

---

## Architecture Overview

```
                        ┌──────────────────────────────────────────┐
                        │        UxPlay Desktop (WPF / .NET)       │
                        │  Fluent UI • Tray • Settings • HUD • Log │
                        └────────────────────┬─────────────────────┘
                                             │ Process / IPC Management
                                             ▼
                        ┌──────────────────────────────────────────┐
                        │        UxPlay Core Native Engine         │
                        │     Upstream Protocol + Windows Fixes    │
                        └──────────┬────────────────────┬──────────┘
                                   │                    │
                        ┌──────────▼─────────┐ ┌────────▼──────────┐
                        │   Bonjour / mDNS   │ │  GStreamer D3D11  │
                        │  Device Discovery  │ │   Video Pipeline  │
                        └────────────────────┘ └───────────────────┘
```

- **Frontend (`src/UxPlayDesktop`)**: Modern C# WPF application utilizing Fluent design, reactive MVVM architecture, automatic network discovery diagnostics, and single-click controls.
- **Core Engine (`core/`)**: Native C++ UxPlay engine combining upstream protocol improvements with Windows power request integration (preventing screensaver/sleep during presentations).
- **Discovery (`sdk/` & `Bluetooth_LE_beacon/`)**: Apple Bonjour mDNS and Bluetooth Low Energy beacon support for instant AirPlay device discovery.

---

## Getting Started

### Prerequisites
- Windows 10 (version 1903+) or Windows 11 (x64 / ARM64).
- .NET 8.0 SDK or Desktop Runtime.
- Apple Bonjour service (or Bonjour SDK included).
- GStreamer runtime (for Direct3D hardware-accelerated video decode).

### Building from Source

```powershell
# Build WPF Desktop Application
dotnet build src/UxPlayDesktop/UxPlayDesktop.csproj -c Release

# Run tests
dotnet test tests/UxPlayDesktop.Tests/UxPlayDesktop.Tests.csproj
```

---

## License

GPL-3.0 License. See [LICENSE](core/LICENSE) for details.
Based on [UxPlay](https://github.com/FDH2/UxPlay) and [RPiPlay](https://github.com/FDH2/RPiPlay).
