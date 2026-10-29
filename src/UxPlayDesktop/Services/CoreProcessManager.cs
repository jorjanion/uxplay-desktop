using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UxPlayDesktop.Models;

namespace UxPlayDesktop.Services;

public interface ICoreProcessManager
{
    AirPlaySession CurrentSession { get; }
    bool IsRunning { get; }
    event Action<ServerState>? StateChanged;
    event Action<AirPlaySession>? SessionUpdated;
    event Action<LogEntry>? LogReceived;
    event Action<string>? PinCodePrompt;

    Task<bool> StartAsync(AppSettings settings, CancellationToken cancellationToken = default);
    Task StopAsync();
    string? FindCoreExecutablePath();
}

public class CoreProcessManager : ICoreProcessManager, IDisposable
{
    private readonly IArgumentBuilder _argumentBuilder;
    private readonly ILogParser _logParser;

    private Process? _process;
    private Process? _beaconProcess;
    private IntPtr _jobHandle = IntPtr.Zero;
    private readonly object _lock = new();

    public AirPlaySession CurrentSession { get; private set; } = new();
    public bool IsRunning => _process != null && !_process.HasExited;

    public event Action<ServerState>? StateChanged;
    public event Action<AirPlaySession>? SessionUpdated;
    public event Action<LogEntry>? LogReceived;
    public event Action<string>? PinCodePrompt;

    public CoreProcessManager(IArgumentBuilder argumentBuilder, ILogParser logParser)
    {
        _argumentBuilder = argumentBuilder;
        _logParser = logParser;
        InitJobObject();
    }

    public string? FindCoreExecutablePath()
    {
        var appDir = AppDomain.CurrentDomain.BaseDirectory;

        string[] searchPaths =
        [
            Path.Combine(appDir, "uxplay.exe"),
            Path.Combine(appDir, "core", "uxplay.exe"),
            Path.Combine(appDir, "bin", "uxplay.exe"),
            Path.Combine(appDir, "resources", "uxplay.exe"),
            Path.Combine(appDir, "runtimes", "win-x64", "native", "uxplay.exe"),
            Path.Combine(appDir, "..", "..", "core", "uxplay.exe"),
            Path.Combine(appDir, "..", "..", "core", "build", "uxplay.exe"),
            Path.Combine(appDir, "..", "..", "..", "core", "uxplay.exe"),
            Path.Combine(appDir, "..", "..", "..", "core", "build", "uxplay.exe"),
            Path.Combine(appDir, "..", "..", "..", "..", "..", "core", "uxplay.exe"),
            Path.Combine(appDir, "..", "..", "..", "..", "..", "core", "build", "uxplay.exe"),
            Path.Combine(appDir, "..", "..", "..", "..", "..", "leapbtw-uxplay", "uxplay.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "uxplay-windows", "uxplay.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "UxPlayDesktop", "uxplay.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UxPlayDesktop", "uxplay.exe")
        ];

        foreach (var path in searchPaths)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        // Check system PATH
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var folder in pathEnv.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var candidate = Path.Combine(folder.Trim(), "uxplay.exe");
                if (File.Exists(candidate)) return candidate;
            }
            catch
            {
                // Ignore invalid path format in PATH
            }
        }

        return null;
    }

    public async Task<bool> StartAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        lock (_lock)
        {
            if (IsRunning) return true;
        }

        SetState(ServerState.Starting);

        var exePath = FindCoreExecutablePath();
        if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
        {
            // If the native binary isn't built on the machine yet, log clear diagnostic message
            Log(LogLevel.Warning, "UxPlay native engine binary ('uxplay.exe') not found in deployment directory. Running in diagnostic ready mode.");
            SetState(ServerState.Listening);
            return false;
        }

        string? bleFilePath = null;
        if (settings.EnableBluetoothBeacon)
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), "UxPlayDesktop");
            Directory.CreateDirectory(tempFolder);
            bleFilePath = Path.Combine(tempFolder, "uxplay_status.ble");
        }

        var arguments = _argumentBuilder.BuildCommandLineString(settings, bleFilePath);

        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8
        };

        ConfigureEnvironmentVariables(startInfo);

        try
        {
            var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

            process.OutputDataReceived += (_, e) => HandleProcessOutput(e.Data);
            process.ErrorDataReceived += (_, e) => HandleProcessOutput(e.Data);
            process.Exited += (_, _) => OnProcessExited();

            if (!process.Start())
            {
                Log(LogLevel.Error, "Failed to start uxplay core process.");
                SetState(ServerState.Error);
                return false;
            }

            // Assign process to Job Object to guarantee atomic cleanup
            AssignProcessToJob(process);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            lock (_lock)
            {
                _process = process;
            }

            Log(LogLevel.Info, $"Started AirPlay Core Engine [PID {process.Id}]: {exePath} {arguments}");
            SetState(ServerState.Listening);

            // If BLE beacon is enabled, start beacon script
            if (settings.EnableBluetoothBeacon && !string.IsNullOrEmpty(bleFilePath))
            {
                StartBleBeacon(bleFilePath);
            }

            return true;
        }
        catch (Exception ex)
        {
            Log(LogLevel.Error, $"Exception launching engine: {ex.Message}");
            SetState(ServerState.Error);
            return false;
        }
    }

    public async Task StopAsync()
    {
        Process? proc;
        lock (_lock)
        {
            proc = _process;
            _process = null;
        }

        StopBleBeacon();

        if (proc != null && !proc.HasExited)
        {
            SetState(ServerState.Stopping);
            try
            {
                Log(LogLevel.Info, "Stopping AirPlay Core Engine...");
                proc.Kill(entireProcessTree: true);
                await proc.WaitForExitAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log(LogLevel.Warning, $"Error while stopping process: {ex.Message}");
            }
            finally
            {
                proc.Dispose();
            }
        }

        SetState(ServerState.Stopped);
        ResetSession();
    }

    private void StartBleBeacon(string bleFilePath)
    {
        try
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] beaconScriptCandidates =
            [
                Path.Combine(appDir, "core", "Bluetooth_LE_beacon", "uxplay-beacon-windows.py"),
                Path.Combine(appDir, "scripts", "uxplay-beacon-windows.py"),
                Path.Combine(appDir, "..", "..", "..", "..", "..", "core", "Bluetooth_LE_beacon", "uxplay-beacon-windows.py")
            ];

            string? scriptPath = null;
            foreach (var candidate in beaconScriptCandidates)
            {
                var full = Path.GetFullPath(candidate);
                if (File.Exists(full))
                {
                    scriptPath = full;
                    break;
                }
            }

            if (scriptPath != null)
            {
                var pyStartInfo = new ProcessStartInfo
                {
                    FileName = "python.exe",
                    Arguments = $"\"{scriptPath}\" \"{bleFilePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var beacon = Process.Start(pyStartInfo);
                if (beacon != null)
                {
                    AssignProcessToJob(beacon);
                    _beaconProcess = beacon;
                    Log(LogLevel.Info, $"Bluetooth LE Beacon broadcaster started [PID {beacon.Id}].");
                }
            }
        }
        catch (Exception ex)
        {
            Log(LogLevel.Debug, $"Bluetooth LE beacon could not be started: {ex.Message}");
        }
    }

    private void StopBleBeacon()
    {
        try
        {
            if (_beaconProcess != null && !_beaconProcess.HasExited)
            {
                _beaconProcess.Kill(entireProcessTree: true);
                _beaconProcess.Dispose();
            }
        }
        catch { }
        finally
        {
            _beaconProcess = null;
        }
    }

    private void HandleProcessOutput(string? data)
    {
        if (string.IsNullOrWhiteSpace(data)) return;

        var result = _logParser.ParseLine(data);
        LogReceived?.Invoke(result.LogEntry);

        bool sessionChanged = false;

        if (result.NewState.HasValue && result.NewState.Value != CurrentSession.State)
        {
            SetState(result.NewState.Value);
            sessionChanged = true;
        }

        if (result.DetectedClientIp != null && CurrentSession.ClientIpAddress != result.DetectedClientIp)
        {
            CurrentSession.ClientIpAddress = result.DetectedClientIp;
            CurrentSession.ConnectedAt = DateTime.Now;
            sessionChanged = true;
        }

        if (result.DetectedClientName != null && CurrentSession.ClientName != result.DetectedClientName)
        {
            CurrentSession.ClientName = result.DetectedClientName;
            sessionChanged = true;
        }

        if (result.DetectedResolution != null && CurrentSession.ActiveResolution != result.DetectedResolution)
        {
            CurrentSession.ActiveResolution = result.DetectedResolution;
            sessionChanged = true;
        }

        if (result.DetectedCodec != null && CurrentSession.ActiveCodec != result.DetectedCodec)
        {
            CurrentSession.ActiveCodec = result.DetectedCodec;
            sessionChanged = true;
        }

        if (result.DetectedFps.HasValue)
        {
            CurrentSession.CurrentFps = result.DetectedFps.Value;
            sessionChanged = true;
        }

        if (result.DetectedPin != null)
        {
            CurrentSession.ActivePinCode = result.DetectedPin;
            PinCodePrompt?.Invoke(result.DetectedPin);
            sessionChanged = true;
        }

        if (result.ClientDisconnected)
        {
            ResetSession();
            sessionChanged = true;
        }

        if (sessionChanged)
        {
            SessionUpdated?.Invoke(CurrentSession);
        }
    }

    private void OnProcessExited()
    {
        Log(LogLevel.Info, "UxPlay engine process terminated.");
        SetState(ServerState.Stopped);
        ResetSession();
    }

    private void SetState(ServerState state)
    {
        CurrentSession.State = state;
        StateChanged?.Invoke(state);
        SessionUpdated?.Invoke(CurrentSession);
    }

    private void ResetSession()
    {
        CurrentSession.ClientName = null;
        CurrentSession.ClientModel = null;
        CurrentSession.ClientIpAddress = null;
        CurrentSession.ConnectedAt = null;
        CurrentSession.CurrentFps = 0;
        CurrentSession.ActivePinCode = null;
    }

    private static void ConfigureEnvironmentVariables(ProcessStartInfo startInfo)
    {
        try
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var localPluginDir = Path.Combine(appDir, "lib", "gstreamer-1.0");

            string[] gstCandidates =
            [
                appDir,
                @"C:\msys64\ucrt64\bin",
                @"C:\msys64\mingw64\bin",
                @"C:\gstreamer\1.0\msvc_x86_64\bin",
                @"C:\gstreamer\1.0\mingw_x86_64\bin"
            ];

            var pathVar = startInfo.EnvironmentVariables["PATH"] ?? Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            var paths = new System.Collections.Generic.List<string>(pathVar.Split(';', StringSplitOptions.RemoveEmptyEntries));

            foreach (var candidate in gstCandidates)
            {
                if (Directory.Exists(candidate) && !paths.Contains(candidate, StringComparer.OrdinalIgnoreCase))
                {
                    paths.Insert(0, candidate);
                }
            }

            startInfo.EnvironmentVariables["PATH"] = string.Join(";", paths);

            if (Directory.Exists(localPluginDir))
            {
                startInfo.EnvironmentVariables["GST_PLUGIN_PATH"] = localPluginDir;
                startInfo.EnvironmentVariables["GST_PLUGIN_PATH_1_0"] = localPluginDir;
                startInfo.EnvironmentVariables["GST_PLUGIN_SYSTEM_PATH"] = localPluginDir;
                startInfo.EnvironmentVariables["GST_PLUGIN_SYSTEM_PATH_1_0"] = localPluginDir;
            }
        }
        catch { }
    }

    private void Log(LogLevel level, string message)
    {
        var entry = new LogEntry { Level = level, Message = message };
        LogReceived?.Invoke(entry);
    }

    #region Windows Job Object (Process Tree Cleanup)

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string? lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetInformationJobObject(IntPtr hJob, int JobObjectInfoClass, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public uint LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public UIntPtr Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IO_COUNTERS
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryLimit;
        public UIntPtr PeakJobMemoryLimit;
    }

    private const int JobObjectExtendedLimitInformation = 9;
    private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;

    private void InitJobObject()
    {
        try
        {
            _jobHandle = CreateJobObject(IntPtr.Zero, null);
            if (_jobHandle != IntPtr.Zero)
            {
                var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
                info.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;

                var length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
                var infoPtr = Marshal.AllocHGlobal(length);
                try
                {
                    Marshal.StructureToPtr(info, infoPtr, false);
                    SetInformationJobObject(_jobHandle, JobObjectExtendedLimitInformation, infoPtr, (uint)length);
                }
                finally
                {
                    Marshal.FreeHGlobal(infoPtr);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to initialize Job Object: {ex.Message}");
        }
    }

    private void AssignProcessToJob(Process process)
    {
        if (_jobHandle != IntPtr.Zero && process.Handle != IntPtr.Zero)
        {
            try
            {
                AssignProcessToJobObject(_jobHandle, process.Handle);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to assign process to Job Object: {ex.Message}");
            }
        }
    }

    #endregion

    public void Dispose()
    {
        StopBleBeacon();
        if (_jobHandle != IntPtr.Zero)
        {
            CloseHandle(_jobHandle);
            _jobHandle = IntPtr.Zero;
        }
        GC.SuppressFinalize(this);
    }
}
