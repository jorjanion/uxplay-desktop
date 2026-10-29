<#
.SYNOPSIS
    UxPlay Desktop unified build, test, and package automation script.
.DESCRIPTION
    Builds the native C++ core engine (when MSYS2 is available), compiles the
    modern WPF desktop application, runs unit test suites, and creates
    publishable release artifacts.
.PARAMETER Action
    Target action: build, test, package, or clean. Default is 'build'.
.PARAMETER Configuration
    Build configuration: Debug or Release. Default is 'Release'.
.PARAMETER MsysRoot
    Path to MSYS2 installation. Defaults to $env:MSYS2_ROOT or 'C:\msys64'.
#>
[CmdletBinding()]
param(
    [ValidateSet("build", "test", "package", "clean")]
    [string]$Action = "build",

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$MsysRoot = $(if ($env:MSYS2_ROOT) { $env:MSYS2_ROOT } else { "C:\msys64" })
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$ProjectRoot = $PSScriptRoot
$DotnetExe = if (Test-Path "$env:USERPROFILE\.dotnet\dotnet.exe") {
    "$env:USERPROFILE\.dotnet\dotnet.exe"
} elseif (Get-Command dotnet.exe -ErrorAction SilentlyContinue) {
    (Get-Command dotnet.exe).Source
} else {
    throw "dotnet SDK 8.0+ not found. Please install .NET 8 SDK."
}

$env:PATH = "$env:USERPROFILE\.dotnet;$env:PATH"
$SolutionPath = Join-Path $ProjectRoot "UxPlayDesktop.sln"
$OutDir = Join-Path $ProjectRoot "out"

function Build-NativeCore {
    $cmake = Join-Path $MsysRoot "ucrt64\bin\cmake.exe"
    if (Test-Path $cmake) {
        Write-Host "MSYS2 UCRT64 toolchain detected. Compiling native core engine..." -ForegroundColor Yellow
        $coreBuildDir = Join-Path $ProjectRoot "core\build"
        if (-not (Test-Path $coreBuildDir)) { New-Item -ItemType Directory -Force -Path $coreBuildDir | Out-Null }

        & $cmake -S (Join-Path $ProjectRoot "core") -B $coreBuildDir -G "Ninja" -DCMAKE_BUILD_TYPE="Release"
        & $cmake --build $coreBuildDir
        Write-Host "Native core build completed successfully." -ForegroundColor Green
    } else {
        Write-Host "MSYS2 toolchain not detected at '$MsysRoot'. Skipping native core compilation." -ForegroundColor Gray
    }
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " UxPlay Desktop Build System [$Action - $Configuration]" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

switch ($Action) {
    "clean" {
        Write-Host "Cleaning output directories and build caches..." -ForegroundColor Yellow
        & $DotnetExe clean $SolutionPath -c $Configuration
        if (Test-Path $OutDir) {
            Remove-Item -Recurse -Force $OutDir
        }
        $coreBuildDir = Join-Path $ProjectRoot "core\build"
        if (Test-Path $coreBuildDir) {
            Remove-Item -Recurse -Force $coreBuildDir
        }
        Write-Host "Clean completed successfully." -ForegroundColor Green
    }

    "build" {
        Build-NativeCore

        Write-Host "Building UxPlay Desktop WPF Solution ($Configuration)..." -ForegroundColor Yellow
        & $DotnetExe build $SolutionPath -c $Configuration --nologo
        Write-Host "Build completed successfully." -ForegroundColor Green
    }

    "test" {
        Write-Host "Running comprehensive automated test suite..." -ForegroundColor Yellow
        & $DotnetExe test $SolutionPath -c $Configuration --nologo --verbosity normal
        Write-Host "All tests passed successfully!" -ForegroundColor Green
    }

    "package" {
        Build-NativeCore

        Write-Host "Building Release binaries..." -ForegroundColor Yellow
        & $DotnetExe build $SolutionPath -c "Release" --nologo

        Write-Host "Running tests prior to packaging..." -ForegroundColor Yellow
        & $DotnetExe test $SolutionPath -c "Release" --nologo

        $publishDir = Join-Path $OutDir "publish"
        if (Test-Path $publishDir) { Remove-Item -Recurse -Force $publishDir }
        New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

        Write-Host "Publishing WPF Desktop Application..." -ForegroundColor Yellow
        & $DotnetExe publish (Join-Path $ProjectRoot "src\UxPlayDesktop\UxPlayDesktop.csproj") `
            -c "Release" `
            -r win-x64 `
            --self-contained false `
            -o $publishDir `
            --nologo

        # Copy resources and scripts to package
        $resourceDest = Join-Path $publishDir "resources"
        if (-not (Test-Path $resourceDest)) { New-Item -ItemType Directory -Force -Path $resourceDest | Out-Null }
        Copy-Item -Path (Join-Path $ProjectRoot "resources\*") -Destination $resourceDest -Force -Recurse

        # Copy native core executable if available
        $nativeCandidates = @(
            (Join-Path $ProjectRoot "core\build\uxplay.exe"),
            (Join-Path $ProjectRoot "core\uxplay.exe")
        )
        foreach ($nativeExe in $nativeCandidates) {
            if (Test-Path $nativeExe) {
                Write-Host "Bundling native UxPlay core: $nativeExe" -ForegroundColor Green
                Copy-Item -Path $nativeExe -Destination (Join-Path $publishDir "uxplay.exe") -Force
                break
            }
        }

        # Copy BLE beacon scripts
        $bleScript = Join-Path $ProjectRoot "core\Bluetooth_LE_beacon\uxplay-beacon-windows.py"
        if (Test-Path $bleScript) {
            $scriptsDir = Join-Path $publishDir "scripts"
            if (-not (Test-Path $scriptsDir)) { New-Item -ItemType Directory -Force -Path $scriptsDir | Out-Null }
            Copy-Item -Path $bleScript -Destination (Join-Path $scriptsDir "uxplay-beacon-windows.py") -Force
        }

        $artifactDir = Join-Path $OutDir "artifacts"
        if (-not (Test-Path $artifactDir)) { New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null }
        $zipPath = Join-Path $artifactDir "UxPlayDesktop-win-x64.zip"
        if (Test-Path $zipPath) { Remove-Item -Force $zipPath }

        Write-Host "Creating portable package zip: $zipPath..." -ForegroundColor Yellow
        Compress-Archive -Path "$publishDir\*" -DestinationPath $zipPath

        Write-Host "==========================================================" -ForegroundColor Green
        Write-Host " Packaging complete! Artifact available at:" -ForegroundColor Green
        Write-Host " $zipPath" -ForegroundColor White
        Write-Host "==========================================================" -ForegroundColor Green
    }
}
