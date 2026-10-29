using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using UxPlayDesktop.Models;
using UxPlayDesktop.Services;

namespace UxPlayDesktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly ICoreProcessManager _processManager;
    private readonly IBonjourService _bonjourService;
    private readonly IAutostartService _autostartService;
    private readonly IUpdateService _updateService;
    private readonly DispatcherTimer _sessionTimer;

    private AppSettings _settings;
    private ServerState _serverState = ServerState.Stopped;
    private string? _activePinCode;
    private string _formattedDuration = "00:00:00";
    private BonjourStatus _bonjourStatus;
    private string _bonjourStatusText = "Checking...";

    private UpdateInfo? _latestUpdate;
    private bool _isUpdateAvailable;
    private string _updateVersionText = "";
    private bool _isUpdating;
    private int _updateProgress;

    public AppSettings Settings
    {
        get => _settings;
        set => SetField(ref _settings, value);
    }

    public ServerState ServerState
    {
        get => _serverState;
        private set
        {
            if (SetField(ref _serverState, value))
            {
                OnPropertyChanged(nameof(StatusTitle));
                OnPropertyChanged(nameof(StatusSubtitle));
                OnPropertyChanged(nameof(StatusDescription));
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(IsServerRunning));
                OnPropertyChanged(nameof(StartStopButtonText));
                OnPropertyChanged(nameof(StartStopButtonIcon));
                OnPropertyChanged(nameof(DisplayClientName));
                OnPropertyChanged(nameof(DisplayClientIp));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public AirPlaySession Session => _processManager.CurrentSession;
    public LogsViewModel Logs { get; }
    public ILocalizationService Loc => LocalizationService.Instance;

    public LanguageItem SelectedLanguage
    {
        get
        {
            var code = Settings.Language ?? "auto";
            foreach (var lang in Loc.SupportedLanguages)
            {
                if (lang.Code.Equals(code, StringComparison.OrdinalIgnoreCase)) return lang;
            }
            return Loc.SupportedLanguages[0];
        }
        set
        {
            if (value != null && Settings.Language != value.Code)
            {
                Settings.Language = value.Code;
                Loc.SetLanguage(value.Code);
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusTitle));
                OnPropertyChanged(nameof(StatusSubtitle));
                OnPropertyChanged(nameof(StatusDescription));
                OnPropertyChanged(nameof(StartStopButtonText));
                OnPropertyChanged(nameof(PresetDescription));
                OnPropertyChanged(nameof(DisplayClientName));
                OnPropertyChanged(nameof(DisplayClientIp));
                OnPropertyChanged(nameof(DisplayTargetFps));
                OnPropertyChanged(nameof(DisplayAudioDelay));
                OnPropertyChanged(nameof(DisplayStep3Desc));
                _settingsService.Save(Settings);
            }
        }
    }

    public bool IsServerRunning => ServerState is ServerState.Listening or ServerState.Connecting or ServerState.Streaming;

    public string StartStopButtonText => IsServerRunning ? Loc["StopServer"] : Loc["StartServer"];
    public string StartStopButtonIcon => IsServerRunning ? "⏹" : "▶";

    public string StatusTitle => ServerState switch
    {
        ServerState.Stopped => Loc["ServerStopped"],
        ServerState.Starting => Loc["StartingEngine"],
        ServerState.Listening => Loc["ReadyToConnect"],
        ServerState.Connecting => Loc["StartingEngine"],
        ServerState.Streaming => string.Format(Loc["StreamingActiveDesc"], Session.ClientName ?? "Apple Device"),
        ServerState.Stopping => Loc["StoppingServer"],
        ServerState.Error => Loc["ServerError"],
        _ => "Idle"
    };

    public string StatusSubtitle => ServerState switch
    {
        ServerState.Stopped => Loc["ServerStoppedDesc"],
        ServerState.Starting => Loc["StartingEngineDesc"],
        ServerState.Listening => Loc["ReadyToConnectSummary"],
        ServerState.Connecting => Loc["StartingEngineDesc"],
        ServerState.Streaming => $"{Session.ActiveResolution} • {FormattedDuration}",
        ServerState.Stopping => Loc["StoppingServerDesc"],
        ServerState.Error => Loc["ServerErrorDesc"],
        _ => string.Empty
    };

    public string StatusDescription => ServerState switch
    {
        ServerState.Stopped => Loc["ServerStoppedDesc"],
        ServerState.Starting => Loc["StartingEngineDesc"],
        ServerState.Listening => string.Format(Loc["ReadyToConnectDesc"], Settings.ServerName),
        ServerState.Connecting => $"Handshake with {Session.ClientIpAddress ?? "device"}...",
        ServerState.Streaming => $"{Session.ActiveResolution} @ {Session.ActiveCodec} • {FormattedDuration}",
        ServerState.Stopping => Loc["StoppingServerDesc"],
        ServerState.Error => Loc["ServerErrorDesc"],
        _ => string.Empty
    };

    public string DisplayClientName => string.IsNullOrWhiteSpace(Session.ClientName) ? Loc["NoDevice"] : Session.ClientName;
    public string DisplayClientIp => string.IsNullOrWhiteSpace(Session.ClientIpAddress) ? Loc["WaitingConnection"] : Session.ClientIpAddress;
    public string DisplayTargetFps => string.Format(Loc["TargetFpsFormat"], Settings.TargetFps);
    public string DisplayAudioDelay => string.Format(Loc["AudioDelayFormat"], Settings.AudioDelayMs);
    public string DisplayStep3Desc => string.Format(Loc["Step3Desc"], Settings.ServerName);
    public string AppVersionDisplay => "UxPlay Desktop v1.1.5";

    public string StatusColor => ServerState switch
    {
        ServerState.Streaming => "#198754",   // Green
        ServerState.Listening => "#0D6EFD",   // Blue
        ServerState.Connecting => "#FFC107",  // Amber
        ServerState.Starting => "#0DCAF0",    // Cyan
        ServerState.Error => "#DC3545",       // Red
        _ => "#6C757D"                        // Gray
    };

    public string FormattedDuration
    {
        get => _formattedDuration;
        private set => SetField(ref _formattedDuration, value);
    }

    public string? ActivePinCode
    {
        get => _activePinCode;
        set => SetField(ref _activePinCode, value);
    }

    public BonjourStatus BonjourStatus
    {
        get => _bonjourStatus;
        private set => SetField(ref _bonjourStatus, value);
    }

    public string BonjourStatusText
    {
        get => _bonjourStatusText;
        private set => SetField(ref _bonjourStatusText, value);
    }

    public PerformancePreset SelectedPreset
    {
        get => Settings.Preset;
        set
        {
            if (Settings.Preset != value)
            {
                Settings.Preset = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PresetDescription));
                _settingsService.Save(Settings);
            }
        }
    }

    public string PresetDescription => Settings.Preset.GetDescription();

    public IReadOnlyList<PerformancePreset> AvailablePresets { get; } =
    [
        PerformancePreset.TeachingAndPresentation,
        PerformancePreset.LowLatencyGaming,
        PerformancePreset.HighQualityCinema,
        PerformancePreset.AudioOnly,
        PerformancePreset.Custom
    ];

    public bool IsUpdateAvailable
    {
        get => _isUpdateAvailable;
        set => SetField(ref _isUpdateAvailable, value);
    }

    public string UpdateVersionText
    {
        get => _updateVersionText;
        set => SetField(ref _updateVersionText, value);
    }

    public bool IsUpdating
    {
        get => _isUpdating;
        set => SetField(ref _isUpdating, value);
    }

    public int UpdateProgress
    {
        get => _updateProgress;
        set => SetField(ref _updateProgress, value);
    }

    public ICommand ToggleServerCommand { get; }
    public ICommand SaveSettingsCommand { get; }
    public ICommand FixBonjourCommand { get; }
    public ICommand DismissPinCommand { get; }
    public ICommand ApplyUpdateCommand { get; }
    public ICommand DismissUpdateCommand { get; }

    public MainViewModel(
        ISettingsService settingsService,
        ICoreProcessManager processManager,
        IBonjourService bonjourService,
        IAutostartService autostartService,
        IUpdateService? updateService = null)
    {
        _settingsService = settingsService;
        _processManager = processManager;
        _bonjourService = bonjourService;
        _autostartService = autostartService;
        _updateService = updateService ?? new UpdateService();

        _settings = _settingsService.CurrentSettings;
        Logs = new LogsViewModel();

        Loc.SetLanguage(_settings.Language);
        Loc.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(StatusTitle));
            OnPropertyChanged(nameof(StatusSubtitle));
            OnPropertyChanged(nameof(StatusDescription));
            OnPropertyChanged(nameof(StartStopButtonText));
            OnPropertyChanged(nameof(PresetDescription));
            OnPropertyChanged(nameof(DisplayClientName));
            OnPropertyChanged(nameof(DisplayClientIp));
            OnPropertyChanged(nameof(DisplayTargetFps));
            OnPropertyChanged(nameof(DisplayAudioDelay));
            OnPropertyChanged(nameof(DisplayStep3Desc));
            OnPropertyChanged(nameof(SelectedLanguage));
        };

        _processManager.StateChanged += state => RunOnUi(() => ServerState = state);
        _processManager.SessionUpdated += _ => RunOnUi(OnSessionUpdated);
        _processManager.LogReceived += entry => RunOnUi(() => Logs.AddEntry(entry));
        _processManager.PinCodePrompt += pin => RunOnUi(() => ActivePinCode = pin);

        ToggleServerCommand = new RelayCommand(async () => await ToggleServerAsync());
        SaveSettingsCommand = new RelayCommand(SaveSettings);
        FixBonjourCommand = new RelayCommand(FixBonjour);
        DismissPinCommand = new RelayCommand(() => ActivePinCode = null);
        ApplyUpdateCommand = new RelayCommand(async () => await ApplyUpdateAsync());
        DismissUpdateCommand = new RelayCommand(() => IsUpdateAvailable = false);

        _sessionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _sessionTimer.Tick += (_, _) => UpdateDuration();
        _sessionTimer.Start();

        CheckBonjourStatus();

        if (Settings.AutoStartServer)
        {
            _ = StartServerAsync();
        }

        _ = CheckForUpdatesAsync();
    }

    private async Task ApplyUpdateAsync()
    {
        if (IsUpdating) return;
        if (_latestUpdate == null || string.IsNullOrEmpty(_latestUpdate.DownloadUrl)) return;
        IsUpdating = true;
        UpdateProgress = 0;
        bool ok = await _updateService.DownloadAndApplyUpdateAsync(_latestUpdate.DownloadUrl, p => RunOnUi(() => UpdateProgress = p));
        if (!ok)
        {
            IsUpdating = false;
            var details = !string.IsNullOrEmpty(_updateService.LastError) ? $"\n\nDetails: {_updateService.LastError}" : "";
            MessageBox.Show($"Failed to download or apply update automatically.{details}\n\nPlease download the latest installer from GitHub Releases.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var info = await _updateService.CheckForUpdatesAsync();
            if (info != null && info.IsUpdateAvailable)
            {
                RunOnUi(() =>
                {
                    _latestUpdate = info;
                    UpdateVersionText = info.LatestVersion;
                    IsUpdateAvailable = true;
                });
            }
        }
        catch { }
    }

    private void FixBonjour()
    {
        if (_bonjourService.CheckStatus() == BonjourStatus.Running)
        {
            MessageBox.Show("Bonjour Service is already active and running!", "Discovery", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (_bonjourService.RepairOrInstallService() || _bonjourService.TryStartService())
        {
            CheckBonjourStatus();
            MessageBox.Show("Bonjour Service configured and started successfully!", "Discovery", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show(
                "Could not automatically configure Apple Bonjour service. Please run 'fix_bonjour.bat' as Administrator if Apple devices cannot find the PC.",
                "Discovery",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private async Task ToggleServerAsync()
    {
        if (IsServerRunning)
        {
            await _processManager.StopAsync();
        }
        else
        {
            await StartServerAsync();
        }
    }

    private async Task StartServerAsync()
    {
        _settingsService.Save(Settings);
        await _processManager.StartAsync(Settings);
    }

    private void SaveSettings()
    {
        _settingsService.Save(Settings);
        _autostartService.SetAutostart(Settings.StartOnBoot);
        OnPropertyChanged(nameof(DisplayTargetFps));
        OnPropertyChanged(nameof(DisplayAudioDelay));
        OnPropertyChanged(nameof(DisplayStep3Desc));
        OnPropertyChanged(nameof(StatusDescription));
        OnPropertyChanged(nameof(StatusSubtitle));
    }

    private void CheckBonjourStatus()
    {
        BonjourStatus = _bonjourService.CheckStatus();
        BonjourStatusText = BonjourStatus switch
        {
            BonjourStatus.Running => "Bonjour Service active",
            BonjourStatus.InstalledNotRunning => "Bonjour stopped (click to start)",
            BonjourStatus.NotInstalled => "Using embedded mDNS discovery",
            _ => "Discovery Ready"
        };
    }

    private void OnSessionUpdated()
    {
        OnPropertyChanged(nameof(Session));
        OnPropertyChanged(nameof(StatusTitle));
        OnPropertyChanged(nameof(StatusSubtitle));
        OnPropertyChanged(nameof(StatusDescription));
        OnPropertyChanged(nameof(DisplayClientName));
        OnPropertyChanged(nameof(DisplayClientIp));
    }

    private void UpdateDuration()
    {
        if (Session.ConnectedAt.HasValue && ServerState == ServerState.Streaming)
        {
            var span = DateTime.Now - Session.ConnectedAt.Value;
            FormattedDuration = span.ToString(@"hh\:mm\:ss");
        }
        else
        {
            FormattedDuration = "00:00:00";
        }
    }

    private static void RunOnUi(Action action)
    {
        var app = Application.Current;
        if (app?.Dispatcher != null && !app.Dispatcher.CheckAccess())
        {
            app.Dispatcher.BeginInvoke(action);
        }
        else
        {
            action();
        }
    }
}
