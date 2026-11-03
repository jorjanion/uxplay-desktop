using System.Collections.Generic;
using UxPlayDesktop.Services;
using Xunit;

namespace UxPlayDesktop.Tests;

public class UpdateServiceTests
{
    [Fact]
    public void SelectBestAsset_PrefersInstallerForTargetArchitecture_WhenAvailable()
    {
        var assets = new List<(string Name, string Url)>
        {
            ("UxPlayDesktop-arm64-Setup.exe", "https://example.com/arm64-setup.exe"),
            ("UxPlayDesktop-x64-Setup.exe", "https://example.com/x64-setup.exe"),
            ("UxPlayDesktop-win-arm64.zip", "https://example.com/arm64.zip"),
            ("UxPlayDesktop-win-x64.zip", "https://example.com/x64.zip")
        };

        var (x64Url, isX64Installer) = UpdateService.SelectBestAsset(assets, "x64");
        Assert.True(isX64Installer);
        Assert.Equal("https://example.com/x64-setup.exe", x64Url);

        var (armUrl, isArmInstaller) = UpdateService.SelectBestAsset(assets, "arm64");
        Assert.True(isArmInstaller);
        Assert.Equal("https://example.com/arm64-setup.exe", armUrl);
    }

    [Fact]
    public void SelectBestAsset_FallsBackToZipForTargetArchitecture_WhenInstallerMissing()
    {
        var assets = new List<(string Name, string Url)>
        {
            ("UxPlayDesktop-win-arm64.zip", "https://example.com/arm64.zip"),
            ("UxPlayDesktop-win-x64.zip", "https://example.com/x64.zip")
        };

        var (armUrl, isArmInstaller) = UpdateService.SelectBestAsset(assets, "arm64");
        Assert.False(isArmInstaller);
        Assert.Equal("https://example.com/arm64.zip", armUrl);

        var (x64Url, isX64Installer) = UpdateService.SelectBestAsset(assets, "x64");
        Assert.False(isX64Installer);
        Assert.Equal("https://example.com/x64.zip", x64Url);
    }

    [Fact]
    public void SelectBestAsset_FallsBackToGeneric_WhenArchitectureSpecificMissing()
    {
        var assets = new List<(string Name, string Url)>
        {
            ("UxPlayDesktop-Setup.exe", "https://example.com/generic-setup.exe")
        };

        var (url, isInstaller) = UpdateService.SelectBestAsset(assets, "arm64");
        Assert.True(isInstaller);
        Assert.Equal("https://example.com/generic-setup.exe", url);
    }
}
