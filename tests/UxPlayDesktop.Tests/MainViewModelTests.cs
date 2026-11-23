using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UxPlayDesktop.Models;
using UxPlayDesktop.Services;
using UxPlayDesktop.ViewModels;
using Xunit;

namespace UxPlayDesktop.Tests;

public class MockProcessManager : ICoreProcessManager
{
    public AirPlaySession CurrentSession { get; set; } = new();
    public bool IsRunning { get; set; } = false;

    public event Action<ServerState>? StateChanged;
    public event Action<AirPlaySession>? SessionUpdated;
    public event Action<LogEntry>? LogReceived;
    public event Action<string>? PinCodePrompt;

    public Task<bool> StartAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        IsRunning = true;
        CurrentSession.State = ServerState.Listening;
        StateChanged?.Invoke(ServerState.Listening);
        SessionUpdated?.Invoke(CurrentSession);
        return Task.FromResult(true);
    }

    public Task StopAsync()
    {
        IsRunning = false;
        CurrentSession.State = ServerState.Stopped;
        StateChanged?.Invoke(ServerState.Stopped);
        SessionUpdated?.Invoke(CurrentSession);
        return Task.CompletedTask;
    }

    public string? FindCoreExecutablePath() => "mock/uxplay.exe";

    public void TriggerPinPrompt(string pin) => PinCodePrompt?.Invoke(pin);
    public void TriggerLog(LogEntry entry) => LogReceived?.Invoke(entry);
    public void TriggerState(ServerState state) => StateChanged?.Invoke(state);
    public void TriggerSessionUpdated(AirPlaySession session) => SessionUpdated?.Invoke(session);
}

public class MockBonjourService : IBonjourService
{
    public BonjourStatus Status { get; set; } = BonjourStatus.Running;
    public BonjourStatus CheckStatus() => Status;
    public bool TryStartService() => true;
    public bool RepairOrInstallService() => true;
}

public class MockAutostartService : IAutostartService
{
    public bool AutostartEnabled { get; set; } = false;
    public bool StartMinimized { get; set; } = false;
    public bool IsAutostartEnabled() => AutostartEnabled;
    public void SetAutostart(bool enable, bool startMinimized = false)
    {
        AutostartEnabled = enable;
        StartMinimized = startMinimized;
    }
}

public class MockWindowManagementService : IWindowManagementService
{
    public List<DisplayInfo> Displays { get; set; } = new()
    {
        new DisplayInfo { Index = 0, Name = "Display 1", Width = 1920, Height = 1080, IsPrimary = true },
        new DisplayInfo { Index = 1, Name = "Display 2", Width = 2560, Height = 1440, IsPrimary = false }
    };
    public int? LastMovedDisplayIndex { get; private set; }
    public bool? LastForceFullscreen { get; private set; }
    public bool? LastAlwaysOnTop { get; private set; }

    public IReadOnlyList<DisplayInfo> GetConnectedDisplays() => Displays.AsReadOnly();
    public void SetAlwaysOnTop(bool enable, IntPtr? targetHwnd = null) => LastAlwaysOnTop = enable;
    public string? CaptureScreenshot(string? outputDirectory = null) => null;
    public void MoveVideoWindowToDisplay(int displayIndex, bool forceFullscreen = false)
    {
        LastMovedDisplayIndex = displayIndex;
        LastForceFullscreen = forceFullscreen;
    }
}

public class MockUpdateService : IUpdateService
{
    public string? LastError { get; set; }
    public UpdateInfo? UpdateInfoToReturn { get; set; }
    public string? DownloadedPackagePathToReturn { get; set; } = "C:\\mock\\update.exe";
    public bool DownloadPackageSuccess { get; set; } = true;
    public bool ApplyUpdateCalled { get; private set; }
    public bool LastAppliedVerySilent { get; private set; }
    public string? LastAppliedPackagePath { get; private set; }

    public Task<UpdateInfo?> CheckForUpdatesAsync() => Task.FromResult(UpdateInfoToReturn);

    public Task<string?> DownloadUpdatePackageAsync(string downloadUrl, Action<int>? progressCallback = null)
    {
        progressCallback?.Invoke(100);
        return Task.FromResult(DownloadPackageSuccess ? DownloadedPackagePathToReturn : null);
    }

    public bool ApplyDownloadedUpdate(string downloadedFilePath, bool verySilent = false)
    {
        ApplyUpdateCalled = true;
        LastAppliedPackagePath = downloadedFilePath;
        LastAppliedVerySilent = verySilent;
        return true;
    }

    public Task<bool> DownloadAndApplyUpdateAsync(string downloadUrl, Action<int>? progressCallback = null)
    {
        ApplyUpdateCalled = true;
        LastAppliedVerySilent = false;
        return Task.FromResult(true);
    }
}

public class MainViewModelTests
{
    private readonly ISettingsService _settingsService;
    private readonly MockProcessManager _processManager;
    private readonly MockBonjourService _bonjourService;
    private readonly MockAutostartService _autostartService;

    public MainViewModelTests()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"settings_{Guid.NewGuid()}.json");
        _settingsService = new SettingsService(tempFile);
        _settingsService.CurrentSettings.Language = "en";
        LocalizationService.Instance.SetLanguage("en");
        _processManager = new MockProcessManager();
        _bonjourService = new MockBonjourService();
        _autostartService = new MockAutostartService();
    }

    [Fact]
    public void MainViewModel_InitialState_WithAutoStartDisabled_IsStopped()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        var vm = new MainViewModel(_settingsService, _processManager, _bonjourService, _autostartService);

        Assert.Equal(ServerState.Stopped, vm.ServerState);
        Assert.False(vm.IsServerRunning);
        Assert.Equal("Start AirPlay Server", vm.StartStopButtonText);
        Assert.Equal("▶", vm.StartStopButtonIcon);
        Assert.Equal("Server Stopped", vm.StatusTitle);
    }

    [Fact]
    public void MainViewModel_InitialState_WithAutoStartEnabled_StartsServer()
    {
        _settingsService.CurrentSettings.AutoStartServer = true;
        var vm = new MainViewModel(_settingsService, _processManager, _bonjourService, _autostartService);

        Assert.Equal(ServerState.Listening, vm.ServerState);
        Assert.True(vm.IsServerRunning);
        Assert.Equal("Stop AirPlay Server", vm.StartStopButtonText);
        Assert.Equal("Ready to Connect", vm.StatusTitle);
    }

    [Fact]
    public void MainViewModel_ToggleServer_StartsAndStops()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        var vm = new MainViewModel(_settingsService, _processManager, _bonjourService, _autostartService);

        Assert.Equal(ServerState.Stopped, vm.ServerState);

        vm.ToggleServerCommand.Execute(null);
        Assert.Equal(ServerState.Listening, vm.ServerState);
        Assert.True(vm.IsServerRunning);
        Assert.Equal("Stop AirPlay Server", vm.StartStopButtonText);

        vm.ToggleServerCommand.Execute(null);
        Assert.Equal(ServerState.Stopped, vm.ServerState);
        Assert.False(vm.IsServerRunning);
        Assert.Equal("Start AirPlay Server", vm.StartStopButtonText);
    }

    [Fact]
    public void MainViewModel_PresetChange_UpdatesSettingsAndDescription()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        var vm = new MainViewModel(_settingsService, _processManager, _bonjourService, _autostartService);

        vm.SelectedPreset = PerformancePreset.LowLatencyGaming;

        Assert.Equal(PerformancePreset.LowLatencyGaming, vm.Settings.Preset);
        Assert.Contains("buffering latency", vm.PresetDescription, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MainViewModel_PinPrompt_SetsActivePinCode_AndDismissClears()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        var vm = new MainViewModel(_settingsService, _processManager, _bonjourService, _autostartService);

        _processManager.TriggerPinPrompt("9341");
        Assert.Equal("9341", vm.ActivePinCode);

        vm.DismissPinCommand.Execute(null);
        Assert.Null(vm.ActivePinCode);
    }

    [Fact]
    public void MainViewModel_LanguageChange_UpdatesStrings()
    {
        var vm = new MainViewModel(_settingsService, _processManager, _bonjourService, _autostartService);
        vm.SelectedLanguage = vm.Loc.SupportedLanguages.First(l => l.Code == "es");

        Assert.Equal("Detener Servidor AirPlay", vm.Loc["StopServer"]);
        Assert.Equal("Panel Principal", vm.Loc["TabDashboard"]);

        vm.SelectedLanguage = vm.Loc.SupportedLanguages.First(l => l.Code == "fr");
        Assert.Equal("Arrêter le Serveur AirPlay", vm.Loc["StopServer"]);
    }

    [Fact]
    public void MainViewModel_PresetOption_UpdatesDynamicallyOnLanguageChange()
    {
        var vm = new MainViewModel(_settingsService, _processManager, _bonjourService, _autostartService);
        vm.SelectedLanguage = vm.Loc.SupportedLanguages.First(l => l.Code == "es");
        vm.SelectedPreset = PerformancePreset.TeachingAndPresentation;

        Assert.Contains("Clases y Presentaciones", vm.SelectedPresetOption.DisplayName);
        Assert.Contains("120-150 ms", vm.SelectedPresetOption.DisplayName);

        // Switch to English
        vm.SelectedLanguage = vm.Loc.SupportedLanguages.First(l => l.Code == "en");

        Assert.Contains("Teaching & Presentations", vm.SelectedPresetOption.DisplayName);
        Assert.Contains("120-150 ms", vm.SelectedPresetOption.DisplayName);
        Assert.Contains("120-150 ms", vm.SelectedPresetOption.ExpectedDelay);
    }

    [Fact]
    public void MainViewModel_SelectedPresetOption_BidirectionalSync()
    {
        var vm = new MainViewModel(_settingsService, _processManager, _bonjourService, _autostartService);
        
        var gamingOption = vm.PresetOptions.First(p => p.Preset == PerformancePreset.LowLatencyGaming);
        vm.SelectedPresetOption = gamingOption;

        Assert.Equal(PerformancePreset.LowLatencyGaming, vm.SelectedPreset);
        Assert.Equal(PerformancePreset.LowLatencyGaming, vm.Settings.Preset);

        vm.SelectedPreset = PerformancePreset.HighQualityCinema;
        Assert.Equal(PerformancePreset.HighQualityCinema, vm.SelectedPresetOption.Preset);
        Assert.Contains("500-1000 ms", vm.SelectedPresetOption.ExpectedDelay);
    }

    [Fact]
    public void MainViewModel_LogReceived_AddsToLogsViewModel()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        var vm = new MainViewModel(_settingsService, _processManager, _bonjourService, _autostartService);

        _processManager.TriggerLog(new LogEntry
        {
            Level = LogLevel.Info,
            Message = "Test log message from engine"
        });

        Assert.Contains(vm.Logs.Logs, entry => entry.Message == "Test log message from engine");
    }

    [Fact]
    public void MainViewModel_StartOnBootAndStartMinimized_UpdatesSettingsAndAutostart()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        var vm = new MainViewModel(_settingsService, _processManager, _bonjourService, _autostartService);

        vm.StartOnBoot = true;
        vm.StartMinimized = true;

        Assert.True(_settingsService.CurrentSettings.StartOnBoot);
        Assert.True(_settingsService.CurrentSettings.StartMinimized);
        Assert.True(_autostartService.AutostartEnabled);
        Assert.True(_autostartService.StartMinimized);

        vm.StartMinimized = false;
        Assert.False(_settingsService.CurrentSettings.StartMinimized);
        Assert.False(_autostartService.StartMinimized);
    }

    [Fact]
    public void MainViewModel_AccentColor_UpdatesSettingsAndProperty()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        var vm = new MainViewModel(_settingsService, _processManager, _bonjourService, _autostartService);

        vm.SetAccentColorCommand.Execute("#A855F7");
        Assert.Equal("#A855F7", vm.AccentColor);
        Assert.Equal("#A855F7", _settingsService.CurrentSettings.AccentColor);
    }

    [Fact]
    public void MainViewModel_VolumeAndMute_TogglesMuteState()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        var vm = new MainViewModel(_settingsService, _processManager, _bonjourService, _autostartService);

        vm.IsMuted = false;
        vm.AudioVolume = 0.8;
        Assert.False(vm.IsMuted);
        Assert.Equal(0.8, vm.AudioVolume);

        vm.ToggleMuteCommand.Execute(null);
        Assert.True(vm.IsMuted);
        Assert.True(_settingsService.CurrentSettings.IsMuted);

        vm.ToggleMuteCommand.Execute(null);
        Assert.False(vm.IsMuted);
    }

    [Fact]
    public void MainViewModel_CompactMode_TogglesDimensions()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        var vm = new MainViewModel(_settingsService, _processManager, _bonjourService, _autostartService);

        Assert.False(vm.IsCompactMode);
        Assert.Equal(740, vm.WindowHeight);

        vm.ToggleCompactModeCommand.Execute(null);
        Assert.True(vm.IsCompactMode);
        Assert.Equal(140, vm.WindowHeight);

        vm.ToggleCompactModeCommand.Execute(null);
        Assert.False(vm.IsCompactMode);
        Assert.Equal(740, vm.WindowHeight);
    }

    [Fact]
    public void DisplayInfo_Equality_ChecksIndex()
    {
        var d1 = new DisplayInfo { Index = 1, Name = "Monitor 2", Width = 1920, Height = 1080 };
        var d2 = new DisplayInfo { Index = 1, Name = "Different Name", Width = 2560, Height = 1440 };
        var d3 = new DisplayInfo { Index = 0, Name = "Monitor 1", Width = 1920, Height = 1080 };

        Assert.Equal(d1, d2);
        Assert.True(d1.Equals(d2));
        Assert.Equal(d1.GetHashCode(), d2.GetHashCode());
        Assert.NotEqual(d1, d3);
    }

    [Fact]
    public void MainViewModel_SelectedDisplay_PreservesSavedTargetDisplayIndex()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        _settingsService.CurrentSettings.TargetDisplayIndex = 1;
        var mockWindowService = new MockWindowManagementService();

        var vm = new MainViewModel(
            _settingsService,
            _processManager,
            _bonjourService,
            _autostartService,
            windowService: mockWindowService);

        Assert.NotNull(vm.SelectedDisplay);
        Assert.Equal(1, vm.SelectedDisplay.Index);
        Assert.Equal("Display 2", vm.SelectedDisplay.Name);
    }

    [Fact]
    public void MainViewModel_SelectedDisplay_ChangesTargetDisplayIndexAndSaves()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        _settingsService.CurrentSettings.TargetDisplayIndex = 0;
        var mockWindowService = new MockWindowManagementService();

        var vm = new MainViewModel(
            _settingsService,
            _processManager,
            _bonjourService,
            _autostartService,
            windowService: mockWindowService);

        Assert.Equal(0, vm.SelectedDisplay?.Index);

        var targetDisplay = vm.AvailableDisplays.First(d => d.Index == 1);
        vm.SelectedDisplay = targetDisplay;

        Assert.Equal(1, vm.Settings.TargetDisplayIndex);
        Assert.Equal(1, _settingsService.CurrentSettings.TargetDisplayIndex);
        Assert.Equal(1, mockWindowService.LastMovedDisplayIndex);
    }

    [Fact]
    public void MainViewModel_RefreshDisplays_UpdatesAvailableDisplays()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        var mockWindowService = new MockWindowManagementService();

        var vm = new MainViewModel(
            _settingsService,
            _processManager,
            _bonjourService,
            _autostartService,
            windowService: mockWindowService);

        Assert.Equal(2, vm.AvailableDisplays.Count);

        mockWindowService.Displays.Add(new DisplayInfo { Index = 2, Name = "Display 3", Width = 3840, Height = 2160 });
        vm.RefreshDisplaysCommand.Execute(null);

        Assert.Equal(3, vm.AvailableDisplays.Count);
    }

    [Fact]
    public void MainViewModel_SessionActiveResolution_RepositionsWindowToTargetDisplay()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        _settingsService.CurrentSettings.TargetDisplayIndex = 1;
        var mockWindowService = new MockWindowManagementService();

        var vm = new MainViewModel(
            _settingsService,
            _processManager,
            _bonjourService,
            _autostartService,
            windowService: mockWindowService);

        // Put server in streaming state
        _processManager.TriggerState(ServerState.Streaming);

        // Update session with active resolution
        _processManager.CurrentSession.ActiveResolution = "1920x1080";
        _processManager.CurrentSession.State = ServerState.Streaming;

        // Trigger session updated
        _processManager.TriggerSessionUpdated(_processManager.CurrentSession);

        Assert.Equal(1, mockWindowService.LastMovedDisplayIndex);
    }

    [Fact]
    public async Task AutoUpdate_WhenIdle_DownloadsAndAppliesSilently()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        _settingsService.CurrentSettings.AutoUpdate = true;
        var mockUpdateService = new MockUpdateService
        {
            UpdateInfoToReturn = new UpdateInfo
            {
                IsUpdateAvailable = true,
                LatestVersion = "1.3.0",
                DownloadUrl = "https://example.com/UxPlayDesktop-Setup.exe"
            }
        };

        var vm = new MainViewModel(
            _settingsService,
            _processManager,
            _bonjourService,
            _autostartService,
            updateService: mockUpdateService);

        // Run check
        await vm.CheckForUpdatesAsync();

        // Banner is displayed
        Assert.True(vm.IsUpdateAvailable);
        Assert.Equal("1.3.0", vm.UpdateVersionText);

        // Update package was downloaded and prepared
        Assert.True(vm.IsUpdateDownloadedAndPending);
        Assert.Equal("C:\\mock\\update.exe", vm.PendingUpdatePackagePath);

        // Because no streaming session was active, it applied update silently
        Assert.True(mockUpdateService.ApplyUpdateCalled);
        Assert.True(mockUpdateService.LastAppliedVerySilent);
        Assert.Equal("C:\\mock\\update.exe", mockUpdateService.LastAppliedPackagePath);
    }

    [Fact]
    public async Task AutoUpdate_WhenStreaming_DelaysApplyUntilStreamingEnds()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        _settingsService.CurrentSettings.AutoUpdate = true;
        var mockUpdateService = new MockUpdateService();

        var vm = new MainViewModel(
            _settingsService,
            _processManager,
            _bonjourService,
            _autostartService,
            updateService: mockUpdateService);

        // Active streaming session in progress
        _processManager.TriggerState(ServerState.Streaming);
        _processManager.CurrentSession.State = ServerState.Streaming;
        _processManager.CurrentSession.ClientName = "iPad User";
        _processManager.CurrentSession.ClientIpAddress = "192.168.1.50";

        mockUpdateService.UpdateInfoToReturn = new UpdateInfo
        {
            IsUpdateAvailable = true,
            LatestVersion = "1.3.0",
            DownloadUrl = "https://example.com/UxPlayDesktop-Setup.exe"
        };

        // Check for updates
        await vm.CheckForUpdatesAsync();

        // Update is downloaded and pending
        Assert.True(vm.IsUpdateAvailable);
        Assert.True(vm.IsUpdateDownloadedAndPending);

        // BUT apply must NOT be called while streaming is active!
        Assert.False(mockUpdateService.ApplyUpdateCalled);

        // Client stops mirroring and disconnects (state transitions to Listening)
        _processManager.TriggerState(ServerState.Listening);
        _processManager.CurrentSession.State = ServerState.Listening;
        _processManager.CurrentSession.ClientName = null;
        _processManager.CurrentSession.ClientIpAddress = null;

        // Trigger session updated / state change to process pending update
        vm.TryApplyPendingUpdate();

        // NOW update must be applied silently!
        Assert.True(mockUpdateService.ApplyUpdateCalled);
        Assert.True(mockUpdateService.LastAppliedVerySilent);
    }

    [Fact]
    public async Task AutoUpdate_WhenDisabled_DoesNotAutoDownload()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        _settingsService.CurrentSettings.AutoUpdate = false;
        var mockUpdateService = new MockUpdateService
        {
            UpdateInfoToReturn = new UpdateInfo
            {
                IsUpdateAvailable = true,
                LatestVersion = "1.3.0",
                DownloadUrl = "https://example.com/UxPlayDesktop-Setup.exe"
            }
        };

        var vm = new MainViewModel(
            _settingsService,
            _processManager,
            _bonjourService,
            _autostartService,
            updateService: mockUpdateService);

        await vm.CheckForUpdatesAsync();

        // Banner shows for user awareness
        Assert.True(vm.IsUpdateAvailable);
        Assert.Equal("1.3.0", vm.UpdateVersionText);

        // Auto download and auto apply must NOT have run
        Assert.False(vm.IsUpdateDownloadedAndPending);
        Assert.False(mockUpdateService.ApplyUpdateCalled);
    }

    [Fact]
    public async Task ApplyUpdateCommand_WhenPackagePreDownloaded_AppliesImmediately()
    {
        _settingsService.CurrentSettings.AutoStartServer = false;
        _settingsService.CurrentSettings.AutoUpdate = true;
        var mockUpdateService = new MockUpdateService();

        var vm = new MainViewModel(
            _settingsService,
            _processManager,
            _bonjourService,
            _autostartService,
            updateService: mockUpdateService);

        // Simulate active streaming session while downloading
        _processManager.TriggerState(ServerState.Streaming);
        _processManager.CurrentSession.State = ServerState.Streaming;
        _processManager.CurrentSession.ClientName = "iPad User";
        _processManager.CurrentSession.ClientIpAddress = "192.168.1.50";

        mockUpdateService.UpdateInfoToReturn = new UpdateInfo
        {
            IsUpdateAvailable = true,
            LatestVersion = "1.3.0",
            DownloadUrl = "https://example.com/UxPlayDesktop-Setup.exe"
        };

        await vm.CheckForUpdatesAsync();

        Assert.True(vm.IsUpdateDownloadedAndPending);
        Assert.False(mockUpdateService.ApplyUpdateCalled);

        // User clicks manual "Actualizar ahora"
        vm.ApplyUpdateCommand.Execute(null);

        Assert.True(mockUpdateService.ApplyUpdateCalled);
        Assert.False(mockUpdateService.LastAppliedVerySilent); // manual execution shows progress UI
        Assert.Equal("C:\\mock\\update.exe", mockUpdateService.LastAppliedPackagePath);
    }
}
