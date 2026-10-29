@echo off
setlocal EnableDelayedExpansion
title Reparar y Activar Servicio Bonjour (AirPlay)

:: Comprobar si se ejecuta como Administrador; si no, auto-elevar con ventana UAC
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [INFO] Solicitando permisos de Administrador para configurar el servicio de red Bonjour...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process cmd -ArgumentList '/c `\"%~f0`\"' -Verb RunAs"
    exit /b
)

echo ======================================================================
echo    CONFIGURANDO Y REPARANDO APPLE BONJOUR Y FIREWALL PARA AIRPLAY
echo ======================================================================
echo.

cd /d "%~dp0"

set "MDNS_SRC="
if exist "%~dp0mDNSResponder.exe" set "MDNS_SRC=%~dp0mDNSResponder.exe"
if not defined MDNS_SRC if exist "%~dp0..\mDNSResponder.exe" set "MDNS_SRC=%~dp0..\mDNSResponder.exe"
if not defined MDNS_SRC if exist "%~dp0..\out\publish\mDNSResponder.exe" set "MDNS_SRC=%~dp0..\out\publish\mDNSResponder.exe"
if not defined MDNS_SRC if exist "%~dp0..\runtimes\win-x64\native\mDNSResponder.exe" set "MDNS_SRC=%~dp0..\runtimes\win-x64\native\mDNSResponder.exe"

if not defined MDNS_SRC (
    echo [ERROR] No se encontro mDNSResponder.exe en la carpeta actual.
    echo Asegurate de ejecutar este script desde la carpeta de UxPlay.
    pause
    exit /b 1
)

echo [1/5] Deteniendo y limpiando registros huerfanos de Bonjour...
sc.exe stop "Bonjour Service" >nul 2>&1
timeout /t 1 /nobreak >nul
sc.exe delete "Bonjour Service" >nul 2>&1

echo [2/5] Copiando mDNSResponder a C:\Program Files\Bonjour...
if not exist "%ProgramFiles%\Bonjour" mkdir "%ProgramFiles%\Bonjour" >nul 2>&1
copy /y "%MDNS_SRC%" "%ProgramFiles%\Bonjour\mDNSResponder.exe" >nul
if exist "%~dp0dnssd.dll" copy /y "%~dp0dnssd.dll" "%ProgramFiles%\Bonjour\dnssd.dll" >nul
if exist "%~dp0..\dnssd.dll" copy /y "%~dp0..\dnssd.dll" "%ProgramFiles%\Bonjour\dnssd.dll" >nul
if exist "%~dp0..\out\publish\dnssd.dll" copy /y "%~dp0..\out\publish\dnssd.dll" "%ProgramFiles%\Bonjour\dnssd.dll" >nul
if exist "%~dp0..\runtimes\win-x64\native\dnssd.dll" copy /y "%~dp0..\runtimes\win-x64\native\dnssd.dll" "%ProgramFiles%\Bonjour\dnssd.dll" >nul

echo [3/5] Registrando servicio Bonjour en Windows Service Control Manager...
"%ProgramFiles%\Bonjour\mDNSResponder.exe" -install
sc.exe config "Bonjour Service" start= auto >nul 2>&1

echo [4/5] Configurando Reglas de Firewall para AirPlay y Bonjour...
netsh advfirewall firewall delete rule name="Bonjour Service (AirPlay)" >nul 2>&1
netsh advfirewall firewall add rule name="Bonjour Service (AirPlay)" dir=in action=allow program="%ProgramFiles%\Bonjour\mDNSResponder.exe" enable=yes >nul 2>&1
netsh advfirewall firewall add rule name="Bonjour Service (AirPlay)" dir=out action=allow program="%ProgramFiles%\Bonjour\mDNSResponder.exe" enable=yes >nul 2>&1

netsh advfirewall firewall delete rule name="UxPlay AirPlay Streaming" >nul 2>&1
netsh advfirewall firewall add rule name="UxPlay AirPlay Streaming" dir=in action=allow protocol=TCP localport=7000,7001,7100 enable=yes >nul 2>&1
netsh advfirewall firewall add rule name="UxPlay AirPlay Streaming UDP" dir=in action=allow protocol=UDP localport=6000-6009,7010,7011,5353 enable=yes >nul 2>&1

echo [5/5] Iniciando Bonjour Service...
net start "Bonjour Service"

echo.
sc.exe query "Bonjour Service" | findstr /i "STATE"
echo.
echo ======================================================================
echo  [OK] CONFIGURACION COMPLETADA CON EXITO!
echo  Bonjour esta activo y el Firewall esta configurado para AirPlay.
echo  Tu iPad y iPhone ya detectaran 'UxPlay Windows' en Duplicar Pantalla.
echo ======================================================================
echo.
pause
