@echo off
setlocal EnableDelayedExpansion
title Reparar y Activar Servicio Bonjour (AirPlay)

set "IS_SILENT=0"
if /i "%~1"=="/silent" set "IS_SILENT=1"
if /i "%~1"=="-silent" set "IS_SILENT=1"
if /i "%~2"=="/silent" set "IS_SILENT=1"
if /i "%~2"=="-silent" set "IS_SILENT=1"

:: Comprobar si se ejecuta como Administrador; si no, auto-elevar con ventana UAC
net session >nul 2>&1
if %errorlevel% neq 0 (
    if "%IS_SILENT%"=="1" (
        powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process cmd -ArgumentList '/c `\"%~f0`\" /silent' -Verb RunAs"
    ) else (
        echo [INFO] Solicitando permisos de Administrador para configurar el servicio de red Bonjour...
        powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process cmd -ArgumentList '/c `\"%~f0`\"' -Verb RunAs"
    )
    exit /b
)

if "%IS_SILENT%"=="0" (
    echo ======================================================================
    echo    CONFIGURANDO Y REPARANDO APPLE BONJOUR Y FIREWALL PARA AIRPLAY
    echo ======================================================================
    echo.
)

cd /d "%~dp0"

set "MDNS_SRC="
if exist "%~dp0mDNSResponder.exe" set "MDNS_SRC=%~dp0mDNSResponder.exe"
if not defined MDNS_SRC if exist "%~dp0resources\mDNSResponder.exe" set "MDNS_SRC=%~dp0resources\mDNSResponder.exe"
if not defined MDNS_SRC if exist "%~dp0..\mDNSResponder.exe" set "MDNS_SRC=%~dp0..\mDNSResponder.exe"
if not defined MDNS_SRC if exist "%~dp0out\publish\mDNSResponder.exe" set "MDNS_SRC=%~dp0out\publish\mDNSResponder.exe"
if not defined MDNS_SRC if exist "%~dp0runtimes\win-x64\native\mDNSResponder.exe" set "MDNS_SRC=%~dp0runtimes\win-x64\native\mDNSResponder.exe"

if not defined MDNS_SRC (
    if "%IS_SILENT%"=="0" (
        echo [ERROR] No se encontro mDNSResponder.exe en la carpeta actual.
        echo Asegurate de ejecutar este script desde la carpeta de UxPlay.
        pause
    )
    exit /b 1
)

if "%IS_SILENT%"=="0" echo [1/5] Deteniendo y limpiando registros huerfanos de Bonjour...
sc.exe stop "Bonjour Service" >nul 2>&1
timeout /t 1 /nobreak >nul
sc.exe delete "Bonjour Service" >nul 2>&1

if "%IS_SILENT%"=="0" echo [2/5] Copiando mDNSResponder a C:\Program Files\Bonjour...
if not exist "%ProgramFiles%\Bonjour" mkdir "%ProgramFiles%\Bonjour" >nul 2>&1
copy /y "%MDNS_SRC%" "%ProgramFiles%\Bonjour\mDNSResponder.exe" >nul
if exist "%~dp0dnssd.dll" copy /y "%~dp0dnssd.dll" "%ProgramFiles%\Bonjour\dnssd.dll" >nul
if exist "%~dp0resources\dnssd.dll" copy /y "%~dp0resources\dnssd.dll" "%ProgramFiles%\Bonjour\dnssd.dll" >nul
if exist "%~dp0..\dnssd.dll" copy /y "%~dp0..\dnssd.dll" "%ProgramFiles%\Bonjour\dnssd.dll" >nul
if exist "%~dp0out\publish\dnssd.dll" copy /y "%~dp0out\publish\dnssd.dll" "%ProgramFiles%\Bonjour\dnssd.dll" >nul
if exist "%~dp0runtimes\win-x64\native\dnssd.dll" copy /y "%~dp0runtimes\win-x64\native\dnssd.dll" "%ProgramFiles%\Bonjour\dnssd.dll" >nul

if "%IS_SILENT%"=="0" echo [3/5] Registrando servicio Bonjour en Windows Service Control Manager...
"%ProgramFiles%\Bonjour\mDNSResponder.exe" -install >nul 2>&1
sc.exe config "Bonjour Service" start= auto >nul 2>&1

if "%IS_SILENT%"=="0" echo [4/5] Configurando Reglas de Firewall para AirPlay y Bonjour en todas las redes...
netsh advfirewall firewall delete rule name="Bonjour Service (AirPlay)" >nul 2>&1
netsh advfirewall firewall add rule name="Bonjour Service (AirPlay)" dir=in action=allow program="%ProgramFiles%\Bonjour\mDNSResponder.exe" enable=yes profile=any >nul 2>&1
netsh advfirewall firewall add rule name="Bonjour Service (AirPlay)" dir=out action=allow program="%ProgramFiles%\Bonjour\mDNSResponder.exe" enable=yes profile=any >nul 2>&1

netsh advfirewall firewall delete rule name="UxPlay AirPlay Streaming" >nul 2>&1
netsh advfirewall firewall add rule name="UxPlay AirPlay Streaming" dir=in action=allow protocol=TCP localport=7000,7001,7100 enable=yes profile=any >nul 2>&1
netsh advfirewall firewall add rule name="UxPlay AirPlay Streaming UDP" dir=in action=allow protocol=UDP localport=6000-6009,7010,7011,5353 enable=yes profile=any >nul 2>&1

:: Limpiar reglas residuales o bloqueos accidentales
netsh advfirewall firewall delete rule name="uxplay" >nul 2>&1
netsh advfirewall firewall delete rule name="uxplay-windows" >nul 2>&1
netsh advfirewall firewall delete rule name="UxPlay Core Engine" >nul 2>&1
netsh advfirewall firewall delete rule name="UxPlay Desktop UI" >nul 2>&1

if exist "%ProgramFiles%\UxPlayDesktop\uxplay.exe" (
    netsh advfirewall firewall add rule name="UxPlay Core Engine" dir=in action=allow program="%ProgramFiles%\UxPlayDesktop\uxplay.exe" enable=yes profile=any >nul 2>&1
)
if exist "%ProgramFiles%\UxPlayDesktop\UxPlayDesktop.exe" (
    netsh advfirewall firewall add rule name="UxPlay Desktop UI" dir=in action=allow program="%ProgramFiles%\UxPlayDesktop\UxPlayDesktop.exe" enable=yes profile=any >nul 2>&1
)
if exist "%~dp0uxplay.exe" (
    netsh advfirewall firewall add rule name="UxPlay Core Engine" dir=in action=allow program="%~dp0uxplay.exe" enable=yes profile=any >nul 2>&1
)
if exist "%~dp0UxPlayDesktop.exe" (
    netsh advfirewall firewall add rule name="UxPlay Desktop UI" dir=in action=allow program="%~dp0UxPlayDesktop.exe" enable=yes profile=any >nul 2>&1
)

if "%IS_SILENT%"=="0" echo [5/5] Iniciando Bonjour Service...
net start "Bonjour Service" >nul 2>&1

if "%IS_SILENT%"=="0" (
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
)
exit /b 0
