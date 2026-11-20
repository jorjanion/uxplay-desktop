using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UxPlayDesktop.Models;

namespace UxPlayDesktop.Services;

/// <summary>
/// Enumerates the machine's network adapters that have an IPv4 address (for example
/// Ethernet, Wi-Fi, or a Tailscale tunnel) and resolves a saved adapter choice back
/// to its current IPv4 address, so the core can advertise on that adapter.
/// </summary>
public static class NetworkAdapterService
{
    /// <summary>Adapters that are up and have a usable (non-loopback) IPv4 address.</summary>
    public static IReadOnlyList<NetworkAdapterInfo> GetAdapters()
    {
        var result = new List<NetworkAdapterInfo>();
        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                var ipv4 = GetIpv4(ni);
                if (ipv4 == null)
                {
                    continue;
                }

                result.Add(new NetworkAdapterInfo(ni.Id, ni.Name, ni.Description, ipv4.ToString()));
            }
        }
        catch
        {
            // Adapter enumeration can fail on locked-down machines; the UI then offers only "Automatic".
        }

        return result.OrderBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
    }

    /// <summary>
    /// Returns the current IPv4 address of the adapter with the given id, or null when
    /// the id is empty (automatic) or the adapter is missing or down.
    /// </summary>
    public static string? ResolveIpv4(string? adapterId)
    {
        if (string.IsNullOrWhiteSpace(adapterId)) return null;

        try
        {
            var ni = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n => string.Equals(n.Id, adapterId, StringComparison.OrdinalIgnoreCase));
            if (ni == null || ni.OperationalStatus != OperationalStatus.Up) return null;

            return GetIpv4(ni)?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static IPAddress? GetIpv4(NetworkInterface ni)
    {
        return ni.GetIPProperties().UnicastAddresses
            .Select(ua => ua.Address)
            .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a));
    }
}

/// <summary>A network adapter that can be selected for AirPlay advertising.</summary>
public sealed record NetworkAdapterInfo(string Id, string Name, string Description, string Ipv4);

/// <summary>One entry in the adapter picker. An empty <see cref="AdapterId"/> means automatic.</summary>
public sealed record NetworkAdapterOption(string AdapterId, string Label);
