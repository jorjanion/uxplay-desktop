using System.Linq;
using UxPlayDesktop.Models;
using UxPlayDesktop.Services;
using Xunit;

namespace UxPlayDesktop.Tests;

public class NetworkAdapterTests
{
    private readonly ArgumentBuilder _builder = new();

    [Fact]
    public void BuildArguments_AutomaticAdapter_DoesNotPassInterfaceFlag()
    {
        var settings = new AppSettings { NetworkAdapterId = string.Empty };

        var args = _builder.BuildArguments(settings).ToList();

        Assert.DoesNotContain("-i", args);
    }

    [Fact]
    public void BuildArguments_UnknownAdapter_FallsBackToAutomatic()
    {
        var settings = new AppSettings { NetworkAdapterId = "{00000000-0000-0000-0000-000000000000}" };

        var args = _builder.BuildArguments(settings).ToList();

        Assert.DoesNotContain("-i", args);
    }

    [Fact]
    public void ResolveIpv4_NullOrEmptyId_ReturnsNull()
    {
        Assert.Null(NetworkAdapterService.ResolveIpv4(null));
        Assert.Null(NetworkAdapterService.ResolveIpv4(string.Empty));
        Assert.Null(NetworkAdapterService.ResolveIpv4("   "));
    }

    [Fact]
    public void GetAdapters_ReturnsOnlyAdaptersWithIpv4()
    {
        var adapters = NetworkAdapterService.GetAdapters();

        Assert.All(adapters, a => Assert.False(string.IsNullOrWhiteSpace(a.Ipv4)));
    }
}
