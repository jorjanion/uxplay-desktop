# 📺 UxPlay Desktop (Deutsch)

<p align="center">
  <strong>Moderne AirPlay-Bildschirmspiegelung mit extrem geringer Latenz für Windows</strong><br>
  <em>Verwandelt jeden Windows-PC in einen leistungsstarken Apple AirPlay-Empfänger für iPad, iPhone und Mac.</em>
</p>

<p align="center">
  <a href="https://github.com/jorjanion/uxplay-desktop/releases/latest"><img src="https://img.shields.io/github/v/release/jorjanion/uxplay-desktop?color=blue&label=Neueste%20Version" alt="Release"></a>
  <a href="https://github.com/jorjanion/uxplay-desktop/blob/main/LICENSE"><img src="https://img.shields.io/badge/Lizenz-GPL%20v3.0-green.svg" alt="License"></a>
  <img src="https://img.shields.io/badge/Plattform-Windows%2010%20%7C%2011%20(x64)-0078D6?logo=windows" alt="Platform">
  <img src="https://img.shields.io/badge/.NET-8.0%20Self--Contained-512BD4?logo=dotnet" alt=".NET 8">
  <img src="https://img.shields.io/badge/Direct3D-11%20%2F%2012%20Hardware-orange" alt="Direct3D">
  <img src="https://img.shields.io/badge/Kosten-0%20%E2%82%AC%20Frei%20%26%20Open%20Source-brightgreen" alt="Cost">
</p>

---

### 🌐 Diese Dokumentation in anderen Sprachen lesen:
[🇪🇸 Español](README.es.md) • [🇺🇸 English](../../README.md) • [🇫🇷 Français](README.fr.md) • [🇩🇪 Deutsch](README.de.md) • [🇮🇹 Italiano](README.it.md) • [🇧🇷 Português (Brasil)](README.pt.md) • [🇷🇺 Русский](README.ru.md) • [🇵🇱 Polski](README.pl.md) • [🇳🇱 Nederlands](README.nl.md) • [🇺🇦 Українська](README.uk.md) • [🇨🇳 简体中文](README.zh-CN.md) • [🇯🇵 日本語](README.ja.md)

---

## ⚡ Warum UxPlay Desktop?

Lehrende, Dozenten und Profis, die den Bildschirm ihres iPads oder iPhones in Vorlesungen und Meetings spiegeln, stoßen oft auf bekannte Hürden:
- **Kommerzielle AirPlay-Programme** verlangen teure Abonnements oder blenden störende Wasserzeichen ein.
- **Ältere Open-Source-Tools** frieren unter aktuellen iPadOS-Versionen ein, erfordern umständliche Konsolenbefehle, leiden unter Audio-Asynchronität oder scheitern an fehlenden DLL-Bibliotheken.

**UxPlay Desktop** wurde von Grund auf entwickelt, um diese Probleme dauerhaft zu lösen:

**📦 Nativer 1-Klick-Installer (`.exe`):** Kein manuelles Entpacken von ZIP-Dateien mehr. Einfach die Setup-Datei ausführen und alles ist startbereit.
**🚀 100% Autark (Self-Contained):** Keine externe .NET-Installation nötig; .NET 8 Runtime, C++ Bibliotheken und GStreamer-Plugins sind vollständig integriert.
**⚡ Direct3D 11/12 Hardwarebeschleunigung:** Kristallklare 60 FPS Darstellung, gestochen scharfe Stiftführung in Notizen-Apps und minimale Eingabelatenz.
**🖼️ Rahmenloser Fenstermodus:** Störungsfreie Darstellung, ideal für Präsentationen und Bildschirmfreigaben.
**🛡️ Automatische Bonjour- & Firewall-Reparatur:** Erkennt fehlerhafte mDNS-Dienste, richtet Apple Bonjour neu ein und öffnet erforderliche Ports in der Windows-Firewall.
**🌍 In über 15 Sprachen verfügbar:** Deutsch, Englisch, Spanisch, Französisch, Portugiesisch, Russisch, Chinesisch, Japanisch und mehr mit sofortigem Sprachwechsel.
**🔄 1-Klick Auto-Updates:** Informiert über neue Versionen und aktualisiert die Anwendung vollautomatisch.

---

## 📖 Schnellstartanleitung (3 einfache Schritte)

### Schritt 1: Programm installieren
1. Laden Sie **[`UxPlayDesktop-Setup.exe`](https://github.com/jorjanion/uxplay-desktop/releases/latest)** herunter.
2. Führen Sie die Installation aus und folgen Sie den Bildschirmanweisungen.
3. Das Installationsprogramm richtet automatisch UxPlay Desktop, den Bonjour-Dienst und die Firewall-Regeln ein.

### Schritt 2: AirPlay-Server starten
1. Starten Sie **UxPlay Desktop** über den Desktop oder das Startmenü.
2. Klicken Sie auf die blaue Schaltfläche **"AirPlay Server Starten"**.
3. Der Status wechselt auf Blau: **"Bereit zum Verbinden"**.

### Schritt 3: iPad oder iPhone verbinden
1. Stellen Sie sicher, dass sich PC und Apple-Gerät im **selben WLAN-Netzwerk** befinden.
2. Öffnen Sie auf dem iPad oder iPhone das **Kontrollzentrum**.
3. Tippen Sie auf **Bildschirmsynchronisierung** (zwei überlappende Rechtecke).
4. Wählen Sie **"UxPlay Windows"** aus der Liste aus.
5. Ihr Bildschirm erscheint sofort in flüssigen 60 FPS auf Ihrem Windows-Monitor.

### Streaming-Profile & Voreinstellungen
| Profil | Empfohlen für | Funktionsweise |
| :--- | :--- | :--- |
| **🎓 Unterricht & Präsentationen** *(Standard)* | Vorlesungen, Folien, digitale Tafel | Maximale Textschärfe, Direct3D-Beschleunigung und Standby-Sperre gegen Bildschirmabschaltung. |
| **🎮 Geringe Latenz & Gaming** | Zeichnen mit Stift, mobile Spiele | Minimaler Puffer für unverzögerte 60 FPS Reaktion. |
| **🎬 Kino & HD-Video** | Filme, YouTube, Videowiedergabe | Adaptiver Jitter-Puffer gegen Ruckler und Bildartefakte. |
| **🎵 Nur Audio** | Musik, Podcasts, Apple Music | Schaltet die Videoanzeige ab und überträgt verlustfreies ALAC-Audio ressourcenschonend. |

---

## 🛠️ Fehlerbehebung & Häufige Fragen

**Das iPad findet "UxPlay Windows" nicht in der Bildschirmsynchronisierung?**
  1. Vergewissern Sie sich, dass beide Geräte im **selben WLAN** sind (Gäste-Netzwerke mit Client-Isolation vermeiden).
  2. Prüfen Sie in UxPlay Desktop auf der Hauptseite die Karte **Netzwerkerkennung (Apple Bonjour)**. Klicken Sie bei Bedarf auf **"Bonjour Reparieren / Starten"**.
**Audio und Video laufen leicht asynchron?**
  - Öffnen Sie **Einstellungen > Audio-Einstellungen** und justieren Sie den Schieberegler **Audio-Synchronisationsverzögerung** (z. B. `+100 ms` oder `-100 ms`).

---

## 🔬 Technische Architektur & Funktionsweise

Für Entwickler und Mitwirkende, die das technische Fundament verstehen möchten:

```mermaid
graph TD
    subgraph Apple-Gerät
        A[iPad / iPhone / Mac] -->|mDNS DNS-SD _airplay._tcp| B(Bonjour Daemon port 5353)
        A -->|RTSP Handshake port 7000/7100| C[Nativer UxPlay Core]
        A -->|H.264 / H.265 Stream| D[GStreamer Media Pipeline]
        A -->|ALAC / AAC Audio| E[WASAPI Audio Sink]
    end

    subgraph Windows-PC Host
        C -->|d3d11videosink / d3d12videosink| F[Direct3D Hardware Swapchain]
        F --> G[DirectX Borderless Surface]
        H[UxPlayDesktop.exe C# WPF UI] -->|Job Object Process Safety| C
        H -->|Stdout / Stderr Regex Parser| I[Echtzeit-HUD Metriken & Logs]
        H -->|Win32 SetThreadExecutionState| J[Windows Energieverwaltung]
    end
```

### 1. AirPlay-Protokollstack & Nativer Core
* **Basis:** Basierend auf dem Upstream-Master von [`FDH2/UxPlay`](https://github.com/FDH2/UxPlay) (Version 1.74).
* **FairPlay-Krypto & Handshake:** Bewältigt den kryptografischen Schlüsselaustausch moderner iOS/iPadOS-Versionen über RTSP (Ports 7000/7100).
* **Kein Linux-D-Bus unter Windows:** Nutzt anstelle von D-Bus die native Win32-Kernel-API `SetThreadExecutionState` zur Standby-Verhinderung.

### 2. GStreamer Direct3D Video-Pipeline
* Hardwarebeschleunigte Dekodierung über **Direct3D 11 (`d3d11videosink`)** und Direct3D 12.
* Pipeline: `appsrc -> h264parse / h265parse -> d3d11h264dec / d3d11h265dec -> d3d11videosink`.
* Zero-Copy GPU-Speicherübertragung mit minimaler CPU-Last (< 3%).

### 3. Prozesssicherheit durch Windows Job Objects
* Native Subprozesse sind an ein Win32 **Job Object** mit `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE` gebunden.
* Bei Beendigung der Benutzeroberfläche schließt der Windows-Kernel automatisch alle Kindprozesse atomar.

### 4. Apple Bonjour Auto-Reparatur
* Direkte Anbindung an Apples `dnssd.dll`.
* Repariert verwaiste Registrierungseinträge von `mDNSResponder.exe` in `C:\Program Files\Bonjour` und öffnet Port UDP 5353 sowie TCP 7000-7100 in der Firewall.

---

## 🐛 Fehler Melden & Issues Erstellen

Haben Sie einen Fehler entdeckt? Wir freuen uns über Ihren Bericht!

Erstellen Sie bitte ein Issue unter **[GitHub Issues](https://github.com/jorjanion/uxplay-desktop/issues)** mit folgenden Angaben:
1. **Betriebssystem:** Windows 10 oder 11 (Build-Nummer).
2. **Apple-Gerät:** iPad/iPhone-Modell und iOS/iPadOS-Version.
3. **Logs:** Wechseln Sie in den Reiter **Echtzeit-Logs**, klicken Sie auf **"Kopieren"** und fügen Sie den Auszug ein.
4. **Netzwerk:** WLAN 2.4/5GHz, LAN oder Mesh-Netzwerk.

---

## 🤝 Mitwirken & Pull Requests (PRs)

Pull Requests sind **herzlich willkommen**!

Leitlinien für Beiträge:
1. **Echter Mehrwert:** PRs sollten messbare Verbesserungen (Leistung, Stabilität, Zugänglichkeit) bringen.
2. **Null-Kosten-Grenze (€0):** Streng 100% frei und Open Source. Keine proprietären SDKs oder kostenpflichtigen Cloud-Dienste.
3. **Verständlichkeit:** Bitte Änderungen in der Beschreibung nachvollziehbar erklären, damit der Maintainer den Code prüfen kann.
4. **Testabdeckung:** Alle Unit-Tests in `tests/UxPlayDesktop.Tests` müssen erfolgreich durchlaufen.

---

## 📜 Lizenz & Danksagung

* **UxPlay Desktop** steht unter der **[GNU General Public License v3.0 (GPL-3.0)](LICENSE)**.
* Baut auf dem Open-Source-Projekt von **[FDH2/UxPlay](https://github.com/FDH2/UxPlay)** auf.
* Entwickelt und gepflegt mit ❤️ von **jorjanion**.
