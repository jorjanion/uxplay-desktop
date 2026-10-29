@echo off
title Reparar Servicio de Descubrimiento Bonjour (AirPlay)
echo ========================================================
echo   Reparando y Configurando Bonjour Service...
echo ========================================================
echo.
cd /d "%~dp0"

if exist "mDNSResponder.exe" (
    "%~dp0mDNSResponder.exe" -install
) else if exist "out\publish\mDNSResponder.exe" (
    "%~dp0out\publish\mDNSResponder.exe" -install
) else (
    echo [ERROR] No se encontro mDNSResponder.exe.
    pause
    exit /b 1
)

net start "Bonjour Service" > nul 2>&1
echo [OK] Servicio de descubrimiento Bonjour configurado y activo.
echo Ya puedes conectar tu iPad y iPhone por AirPlay.
echo.
pause
