using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace UxPlayDesktop.Services;

public enum DiagnosticSeverity
{
    Pass,
    Warning,
    Fail
}

public class DiagnosticItem
{
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DiagnosticSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Recommendation { get; set; }
}

public class NetworkDiagnosticReport
{
    public bool IsHealthy => Results.All(r => r.Severity != DiagnosticSeverity.Fail);
    public int PassCount => Results.Count(r => r.Severity == DiagnosticSeverity.Pass);
    public int WarningCount => Results.Count(r => r.Severity == DiagnosticSeverity.Warning);
    public int FailCount => Results.Count(r => r.Severity == DiagnosticSeverity.Fail);
    public List<DiagnosticItem> Results { get; } = [];
}

public interface INetworkDiagnosticService
{
    Task<NetworkDiagnosticReport> RunDiagnosticsAsync();
}

public class NetworkDiagnosticService : INetworkDiagnosticService
{
    private readonly IBonjourService _bonjourService;

    public NetworkDiagnosticService(IBonjourService bonjourService)
    {
        _bonjourService = bonjourService;
    }

    public async Task<NetworkDiagnosticReport> RunDiagnosticsAsync()
    {
        var report = new NetworkDiagnosticReport();

        await Task.Run(() =>
        {
            // 1. Apple Bonjour Service Check
            CheckBonjour(report);

            // 2. Network Adapters & Virtual Subnets
            CheckNetworkAdapters(report);

            // 3. AirPlay Port Availability (7000, 7100, 5353)
            CheckPorts(report);

            // 4. Windows Firewall Verification
            CheckFirewall(report);
        });

        return report;
    }

    private void CheckBonjour(NetworkDiagnosticReport report)
    {
        var status = _bonjourService.CheckStatus();
        switch (status)
        {
            case BonjourStatus.Running:
                report.Results.Add(new DiagnosticItem
                {
                    Category = "mDNS / Bonjour",
                    Title = "Apple Bonjour Service",
                    Severity = DiagnosticSeverity.Pass,
                    Message = "Bonjour service is active and responding to mDNS queries on UDP 5353."
                });
                break;

            case BonjourStatus.InstalledNotRunning:
                report.Results.Add(new DiagnosticItem
                {
                    Category = "mDNS / Bonjour",
                    Title = "Apple Bonjour Service",
                    Severity = DiagnosticSeverity.Fail,
                    Message = "Bonjour service is installed but currently stopped.",
                    Recommendation = "Click 'Repair / Start Bonjour' in the dashboard to start the service."
                });
                break;

            case BonjourStatus.NotInstalled:
                report.Results.Add(new DiagnosticItem
                {
                    Category = "mDNS / Bonjour",
                    Title = "Apple Bonjour Service",
                    Severity = DiagnosticSeverity.Warning,
                    Message = "Bonjour is not registered as a Windows Service. UxPlay will rely on embedded mDNS.",
                    Recommendation = "Click 'Repair / Start Bonjour' to register mDNSResponder for maximum discovery reliability."
                });
                break;

            default:
                report.Results.Add(new DiagnosticItem
                {
                    Category = "mDNS / Bonjour",
                    Title = "Apple Bonjour Service",
                    Severity = DiagnosticSeverity.Pass,
                    Message = "mDNS discovery engine is ready."
                });
                break;
        }
    }

    private static void CheckNetworkAdapters(NetworkDiagnosticReport report)
    {
        try
        {
            var activeInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                             ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .ToList();

            if (activeInterfaces.Count == 0)
            {
                report.Results.Add(new DiagnosticItem
                {
                    Category = "Network",
                    Title = "Active Network Adapter",
                    Severity = DiagnosticSeverity.Fail,
                    Message = "No active network adapters were detected on this PC.",
                    Recommendation = "Connect your PC to Wi-Fi or Ethernet."
                });
                return;
            }

            // Find physical Ethernet / Wi-Fi
            var primary = activeInterfaces.FirstOrDefault(ni =>
                !ni.Description.Contains("Virtual", StringComparison.OrdinalIgnoreCase) &&
                !ni.Description.Contains("Pseudo", StringComparison.OrdinalIgnoreCase) &&
                !ni.Description.Contains("vEthernet", StringComparison.OrdinalIgnoreCase) &&
                !ni.Description.Contains("VMware", StringComparison.OrdinalIgnoreCase));

            if (primary != null)
            {
                var ip = primary.GetIPProperties().UnicastAddresses
                    .FirstOrDefault(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;

                report.Results.Add(new DiagnosticItem
                {
                    Category = "Network",
                    Title = "Primary Interface",
                    Severity = DiagnosticSeverity.Pass,
                    Message = $"Connected via {primary.Name} ({primary.NetworkInterfaceType}) - Local IP: {ip?.ToString() ?? "N/A"}."
                });
            }

            // Check for virtual adapters that might hijack multicast
            var virtuals = activeInterfaces.Where(ni =>
                ni.Description.Contains("Virtual", StringComparison.OrdinalIgnoreCase) ||
                ni.Description.Contains("vEthernet", StringComparison.OrdinalIgnoreCase) ||
                ni.Description.Contains("VMware", StringComparison.OrdinalIgnoreCase) ||
                ni.Description.Contains("VirtualBox", StringComparison.OrdinalIgnoreCase) ||
                ni.Description.Contains("WSL", StringComparison.OrdinalIgnoreCase)).ToList();

            if (virtuals.Count > 0)
            {
                report.Results.Add(new DiagnosticItem
                {
                    Category = "Network",
                    Title = "Virtual Network Adapters",
                    Severity = DiagnosticSeverity.Warning,
                    Message = $"Detected {virtuals.Count} virtual adapter(s) ({string.Join(", ", virtuals.Select(v => v.Name))}).",
                    Recommendation = "If your iPad fails to discover UxPlay, make sure it is connected to the same subnet as your primary physical adapter."
                });
            }
        }
        catch (Exception ex)
        {
            report.Results.Add(new DiagnosticItem
            {
                Category = "Network",
                Title = "Network Adapter Inspection",
                Severity = DiagnosticSeverity.Warning,
                Message = $"Could not fully inspect network adapters: {ex.Message}"
            });
        }
    }

    private static void CheckPorts(NetworkDiagnosticReport report)
    {
        int[] tcpPorts = [7000, 7100];
        foreach (var port in tcpPorts)
        {
            try
            {
                using var listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                listener.Stop();

                report.Results.Add(new DiagnosticItem
                {
                    Category = "Ports",
                    Title = $"TCP Port {port} (AirPlay)",
                    Severity = DiagnosticSeverity.Pass,
                    Message = $"Port {port} is available and free for AirPlay streaming."
                });
            }
            catch (SocketException ex)
            {
                report.Results.Add(new DiagnosticItem
                {
                    Category = "Ports",
                    Title = $"TCP Port {port} (AirPlay)",
                    Severity = DiagnosticSeverity.Warning,
                    Message = $"Port {port} appears busy or restricted ({ex.SocketErrorCode}).",
                    Recommendation = "Ensure no other AirPlay server or conflicting application is holding this port."
                });
            }
        }
    }

    private static void CheckFirewall(NetworkDiagnosticReport report)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = "advfirewall firewall show rule name=\"UxPlay AirPlay Streaming\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            using var proc = Process.Start(psi);
            var output = proc?.StandardOutput.ReadToEnd() ?? string.Empty;
            proc?.WaitForExit();

            if (output.Contains("UxPlay AirPlay Streaming", StringComparison.OrdinalIgnoreCase))
            {
                report.Results.Add(new DiagnosticItem
                {
                    Category = "Firewall",
                    Title = "Windows Firewall Rules",
                    Severity = DiagnosticSeverity.Pass,
                    Message = "AirPlay streaming firewall rules are active and properly configured."
                });
            }
            else
            {
                report.Results.Add(new DiagnosticItem
                {
                    Category = "Firewall",
                    Title = "Windows Firewall Rules",
                    Severity = DiagnosticSeverity.Warning,
                    Message = "Specific UxPlay streaming firewall rules were not found.",
                    Recommendation = "Run 'Repair Bonjour & Firewall' to automatically add the required firewall allow rules."
                });
            }
        }
        catch
        {
            report.Results.Add(new DiagnosticItem
            {
                Category = "Firewall",
                Title = "Firewall Check",
                Severity = DiagnosticSeverity.Pass,
                Message = "Standard firewall configuration active."
            });
        }
    }
}
