; UxPlay Desktop - Inno Setup Script
; High-performance, self-contained Windows AirPlay installer

#define MyAppName "UxPlay Desktop"
#define MyAppVersion "1.1.1"
#define MyAppPublisher "jorjanion"
#define MyAppURL "https://github.com/jorjanion/uxplay-desktop"
#define MyAppExeName "UxPlayDesktop.exe"

[Setup]
AppId={{D37F61CA-73E1-4B9F-841A-93C3806C9B01}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\UxPlayDesktop
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
OutputDir=..\out\artifacts
OutputBaseFilename=UxPlayDesktop-Setup
SetupIconFile=..\resources\newicon.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[CustomMessages]
english.AutoStartDesc=Launch UxPlay Desktop automatically on Windows startup
spanish.AutoStartDesc=Iniciar UxPlay Desktop automáticamente al arrancar Windows
french.AutoStartDesc=Lancer UxPlay Desktop automatiquement au démarrage de Windows
german.AutoStartDesc=UxPlay Desktop automatisch beim Windows-Start ausführen
italian.AutoStartDesc=Avvia automaticamente UxPlay Desktop all'avvio di Windows
portuguese.AutoStartDesc=Iniciar o UxPlay Desktop automaticamente ao iniciar o Windows
brazilianportuguese.AutoStartDesc=Iniciar o UxPlay Desktop automaticamente ao iniciar o Windows
russian.AutoStartDesc=Автоматически запускать UxPlay Desktop при загрузке Windows
polish.AutoStartDesc=Uruchamiaj UxPlay Desktop automatycznie podczas uruchamiania systemu Windows
dutch.AutoStartDesc=UxPlay Desktop automatisch starten bij het opstarten van Windows
japanese.AutoStartDesc=Windows起動時にUxPlay Desktopを自動起動する
ukrainian.AutoStartDesc=Автоматично запускати UxPlay Desktop під час завантаження Windows

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "autostart"; Description: "{cm:AutoStartDesc}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\out\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "UxPlayDesktop"; ValueData: """{app}\{#MyAppExeName}"" -minimized"; Flags: uninsdeletevalue; Tasks: autostart

[Run]
; Automatically configure Bonjour Service and open Windows Firewall ports for AirPlay
Filename: "{app}\fix_bonjour.bat"; Parameters: "/silent"; Flags: runhidden waituntilterminated
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "sc.exe"; Parameters: "stop ""Bonjour Service"""; Flags: runhidden
