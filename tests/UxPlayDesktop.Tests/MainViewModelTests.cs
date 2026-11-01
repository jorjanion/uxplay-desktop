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
}
