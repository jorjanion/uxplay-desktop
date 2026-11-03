<#
.SYNOPSIS
    UxPlay Desktop unified build, test, and package automation script.
.DESCRIPTION
    Builds the native C++ core engine (when MSYS2 is available), compiles the
    modern WPF desktop application for x64 or ARM64, runs unit test suites,
    and creates publishable release artifacts.
.PARAMETER Action
    Target action: build, test, package, or clean. Default is 'build'.
.PARAMETER Configuration
    Build configuration: Debug or Release. Default is 'Release'.
.PARAMETER Architecture
    Target CPU architecture: x64 or arm64. Default is 'x64'.
.PARAMETER MsysRoot
    Path to MSYS2 installation. Defaults to $env:MSYS2_ROOT or 'C:\msys64'.
#>
[CmdletBinding()]
param(
    [ValidateSet("build", "test", "package", "clean")]
    [string]$Action = "build",

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [ValidateSet("x64", "arm64")]
    [string]$Architecture = "x64",

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

$rid = if ($Architecture -eq "arm64") { "win-arm64" } else { "win-x64" }
$msysEnv = if ($Architecture -eq "arm64") { "clangarm64" } else { "ucrt64" }

function Build-NativeCore {
    $cmake = Join-Path $MsysRoot "$msysEnv\bin\cmake.exe"
    if (Test-Path $cmake) {
        Write-Host "MSYS2 $msysEnv toolchain detected. Compiling native core engine ($Architecture)..." -ForegroundColor Yellow
        $coreBuildDir = Join-Path $ProjectRoot "core\build-$Architecture"
        if (-not (Test-Path $coreBuildDir)) { New-Item -ItemType Directory -Force -Path $coreBuildDir | Out-Null }

        & $cmake -S (Join-Path $ProjectRoot "core") -B $coreBuildDir -G "Ninja" -DCMAKE_BUILD_TYPE="Release"
        & $cmake --build $coreBuildDir
        Write-Host "Native core build completed successfully." -ForegroundColor Green
    } else {
        Write-Host "MSYS2 toolchain not detected at '$MsysRoot\$msysEnv'. Skipping native core compilation." -ForegroundColor Gray
    }
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " UxPlay Desktop Build System [$Action - $Configuration - $Architecture]" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

switch ($Action) {
    "clean" {
        Write-Host "Cleaning output directories and build caches..." -ForegroundColor Yellow
        & $DotnetExe clean $SolutionPath -c $Configuration
        if (Test-Path $OutDir) {
            Remove-Item -Recurse -Force $OutDir
        }
        $coreBuildDirs = @(
            (Join-Path $ProjectRoot "core\build"),
            (Join-Path $ProjectRoot "core\build-x64"),
            (Join-Path $ProjectRoot "core\build-arm64")
        )
        foreach ($d in $coreBuildDirs) {
            if (Test-Path $d) { Remove-Item -Recurse -Force $d }
        }
        Write-Host "Clean completed successfully." -ForegroundColor Green
    }

    "build" {
        Build-NativeCore

        Write-Host "Building UxPlay Desktop WPF Solution ($Configuration - $Architecture)..." -ForegroundColor Yellow
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

        Write-Host "Publishing WPF Desktop Application ($rid)..." -ForegroundColor Yellow
        & $DotnetExe publish (Join-Path $ProjectRoot "src\UxPlayDesktop\UxPlayDesktop.csproj") `
            -c "Release" `
            -r $rid `
            --self-contained true `
            -o $publishDir `
            --nologo

        # Copy resources and scripts to package
        $resourceDest = Join-Path $publishDir "resources"
        if (-not (Test-Path $resourceDest)) { New-Item -ItemType Directory -Force -Path $resourceDest | Out-Null }
        Copy-Item -Path (Join-Path $ProjectRoot "resources\*") -Destination $resourceDest -Force -Recurse
        if (Test-Path (Join-Path $ProjectRoot "fix_bonjour.bat")) {
            Copy-Item -Path (Join-Path $ProjectRoot "fix_bonjour.bat") -Destination $publishDir -Force
        }

        $nativeCandidates = @(
            (Join-Path $ProjectRoot "core\build-$Architecture\uxplay.exe"),
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

        # Copy runtime dependencies
        $runtimeNative = Join-Path $ProjectRoot "runtimes\$rid\native"
        if (Test-Path $runtimeNative) {
            Write-Host "Bundling runtime dependencies from $runtimeNative..." -ForegroundColor Green
            Copy-Item -Path (Join-Path $runtimeNative "*") -Destination $publishDir -Recurse -Force
        } elseif (Test-Path (Join-Path $MsysRoot "$msysEnv\bin")) {
            Write-Host "Staging MSYS2 runtime dependencies from $msysEnv..." -ForegroundColor Yellow
            $gstPluginSrc = Join-Path $MsysRoot "$msysEnv\lib\gstreamer-1.0"
            $gstPluginDest = Join-Path $publishDir "lib\gstreamer-1.0"
            if (Test-Path $gstPluginSrc) {
                if (-not (Test-Path $gstPluginDest)) { New-Item -ItemType Directory -Force -Path $gstPluginDest | Out-Null }
                Copy-Item -Path (Join-Path $gstPluginSrc "*.dll") -Destination $gstPluginDest -Force
            }
            $gstScanner = Join-Path $MsysRoot "$msysEnv\libexec\gstreamer-1.0\gst-plugin-scanner.exe"
            if (Test-Path $gstScanner) {
                $scannerDest = Join-Path $publishDir "libexec\gstreamer-1.0"
                if (-not (Test-Path $scannerDest)) { New-Item -ItemType Directory -Force -Path $scannerDest | Out-Null }
                Copy-Item -LiteralPath $gstScanner -Destination $scannerDest -Force
            }
            $collectScript = Join-Path $ProjectRoot "scripts\collect-runtime-dependencies.ps1"
            if (Test-Path $collectScript) {
                & $collectScript -StageDir $publishDir -MsysRoot $MsysRoot -EnvironmentName $msysEnv
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

        # Compile native Windows Setup Installer if Inno Setup is installed
        $isccCandidates = @(
            "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
            "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
            "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
        )
        $isccExe = $null
        foreach ($cand in $isccCandidates) {
            if (Test-Path $cand) { $isccExe = $cand; break }
        }

        $setupExeName = "UxPlayDesktop-$Architecture-Setup.exe"
        if ($isccExe) {
            Write-Host "Compiling native Windows Setup Installer ($Architecture) (Inno Setup)..." -ForegroundColor Yellow
            $issPath = Join-Path $ProjectRoot "installer\UxPlayDesktop.iss"
            & $isccExe "/DAppArch=$Architecture" $issPath
            if ($LASTEXITCODE -ne 0) {
                throw "Inno Setup compilation failed with exit code $LASTEXITCODE"
            }
            Write-Host "Native Setup Installer created successfully: $setupExeName" -ForegroundColor Green
        }

        $zipPath = Join-Path $artifactDir "UxPlayDesktop-$rid.zip"
        if (Test-Path $zipPath) { Remove-Item -Force $zipPath }

        Write-Host "Creating portable package zip: $zipPath..." -ForegroundColor Yellow
        Compress-Archive -Path "$publishDir\*" -DestinationPath $zipPath

        Write-Host "==========================================================" -ForegroundColor Green
        Write-Host " Packaging complete! Artifacts available at:" -ForegroundColor Green
        if ($isccExe) {
            Write-Host " - Installer: $(Join-Path $artifactDir $setupExeName)" -ForegroundColor White
        }
        Write-Host " - Portable:  $zipPath" -ForegroundColor White
        Write-Host "==========================================================" -ForegroundColor Green
    }
}
