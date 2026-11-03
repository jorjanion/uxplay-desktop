# ðŸ“º UxPlay Desktop (Deutsch)

<p align="center">
  <strong>Moderne AirPlay-Bildschirmspiegelung mit extrem geringer Latenz fÃ¼r Windows</strong><br>
  <em>Verwandelt jeden Windows-PC in einen leistungsstarken Apple AirPlay-EmpfÃ¤nger fÃ¼r iPad, iPhone und Mac.</em>
</p>

<p align="center">
  <a href="https://github.com/jorjanion/uxplay-desktop/releases/latest"><img src="https://img.shields.io/github/v/release/jorjanion/uxplay-desktop?color=blue&label=Neueste%20Version" alt="Release"></a>
  <a href="https://github.com/jorjanion/uxplay-desktop/blob/main/LICENSE"><img src="https://img.shields.io/badge/Lizenz-GPL%20v3.0-green.svg" alt="License"></a>
  <img src="https://img.shields.io/badge/Plattform-Windows%2010%20%7C%2011%20(x64%20%2F%20ARM64)-0078D6?logo=windows" alt="Platform">
  <img src="https://img.shields.io/badge/.NET-8.0%20Self--Contained-512BD4?logo=dotnet" alt=".NET 8">
  <img src="https://img.shields.io/badge/Direct3D-11%20%2F%2012%20Hardware-orange" alt="Direct3D">
  <img src="https://img.shields.io/badge/Kosten-0%20%E2%82%AC%20Frei%20%26%20Open%20Source-brightgreen" alt="Cost">
</p>

---

### ðŸŒ Diese Dokumentation in anderen Sprachen lesen:
[ðŸ‡ªðŸ‡¸ EspaÃ±ol](README.es.md) â€¢ [ðŸ‡ºðŸ‡¸ English](../../README.md) â€¢ [ðŸ‡«ðŸ‡· FranÃ§ais](README.fr.md) â€¢ [ðŸ‡©ðŸ‡ª Deutsch](README.de.md) â€¢ [ðŸ‡®ðŸ‡¹ Italiano](README.it.md) â€¢ [ðŸ‡§ðŸ‡· PortuguÃªs (Brasil)](README.pt.md) â€¢ [ðŸ‡·ðŸ‡º Ð ÑƒÑÑÐºÐ¸Ð¹](README.ru.md) â€¢ [ðŸ‡µðŸ‡± Polski](README.pl.md) â€¢ [ðŸ‡³ðŸ‡± Nederlands](README.nl.md) â€¢ [ðŸ‡ºðŸ‡¦ Ð£ÐºÑ€Ð°Ñ—Ð½ÑÑŒÐºÐ°](README.uk.md) â€¢ [ðŸ‡¨ðŸ‡³ ç®€ä½“ä¸­æ–‡](README.zh-CN.md) â€¢ [ðŸ‡¯ðŸ‡µ æ—¥æœ¬èªž](README.ja.md)

---

## âš¡ Warum UxPlay Desktop?

Lehrende, Dozenten und Profis, die den Bildschirm ihres iPads oder iPhones in Vorlesungen und Meetings spiegeln, stoÃŸen oft auf bekannte HÃ¼rden:
- **Kommerzielle AirPlay-Programme** verlangen teure Abonnements oder blenden stÃ¶rende Wasserzeichen ein.
- **Ã„ltere Open-Source-Tools** frieren unter aktuellen iPadOS-Versionen ein, erfordern umstÃ¤ndliche Konsolenbefehle, leiden unter Audio-AsynchronitÃ¤t oder scheitern an fehlenden DLL-Bibliotheken.

**UxPlay Desktop** wurde von Grund auf entwickelt, um diese Probleme dauerhaft zu lÃ¶sen:

**ðŸ“¦ Nativer 1-Klick-Installer (`.exe`):** Kein manuelles Entpacken von ZIP-Dateien mehr. Einfach die Setup-Datei ausfÃ¼hren und alles ist startbereit.
**ðŸš€ 100% Autark (Self-Contained):** Keine externe .NET-Installation nÃ¶tig; .NET 8 Runtime, C++ Bibliotheken und GStreamer-Plugins sind vollstÃ¤ndig integriert.
**âš¡ Direct3D 11/12 Hardwarebeschleunigung:** Kristallklare 60 FPS Darstellung, gestochen scharfe StiftfÃ¼hrung in Notizen-Apps und minimale Eingabelatenz.
**ðŸ–¼ï¸ Rahmenloser Fenstermodus:** StÃ¶rungsfreie Darstellung, ideal fÃ¼r PrÃ¤sentationen und Bildschirmfreigaben.
**ðŸ›¡ï¸ Automatische Bonjour- & Firewall-Reparatur:** Erkennt fehlerhafte mDNS-Dienste, richtet Apple Bonjour neu ein und Ã¶ffnet erforderliche Ports in der Windows-Firewall.
**ðŸŒ In Ã¼ber 15 Sprachen verfÃ¼gbar:** Deutsch, Englisch, Spanisch, FranzÃ¶sisch, Portugiesisch, Russisch, Chinesisch, Japanisch und mehr mit sofortigem Sprachwechsel.
**ðŸ”„ 1-Klick Auto-Updates:** Informiert Ã¼ber neue Versionen und aktualisiert die Anwendung vollautomatisch.

---

## ðŸ“– Schnellstartanleitung (3 einfache Schritte)

### Schritt 1: Programm installieren
1. Laden Sie **[`UxPlayDesktop-Setup.exe`](https://github.com/jorjanion/uxplay-desktop/releases/latest)** herunter.
2. FÃ¼hren Sie die Installation aus und folgen Sie den Bildschirmanweisungen.
3. Das Installationsprogramm richtet automatisch UxPlay Desktop, den Bonjour-Dienst und die Firewall-Regeln ein.

### Schritt 2: AirPlay-Server starten
1. Starten Sie **UxPlay Desktop** Ã¼ber den Desktop oder das StartmenÃ¼.
2. Klicken Sie auf die blaue SchaltflÃ¤che **"AirPlay Server Starten"**.
3. Der Status wechselt auf Blau: **"Bereit zum Verbinden"**.

### Schritt 3: iPad oder iPhone verbinden
1. Stellen Sie sicher, dass sich PC und Apple-GerÃ¤t im **selben WLAN-Netzwerk** befinden.
2. Ã–ffnen Sie auf dem iPad oder iPhone das **Kontrollzentrum**.
3. Tippen Sie auf **Bildschirmsynchronisierung** (zwei Ã¼berlappende Rechtecke).
4. WÃ¤hlen Sie **"UxPlay Windows"** aus der Liste aus.
5. Ihr Bildschirm erscheint sofort in flÃ¼ssigen 60 FPS auf Ihrem Windows-Monitor.

### Streaming-Profile & Voreinstellungen
| Profil | Empfohlen fÃ¼r | Funktionsweise |
| :--- | :--- | :--- |
| **ðŸŽ“ Unterricht & PrÃ¤sentationen** *(Standard)* | Vorlesungen, Folien, digitale Tafel | Maximale TextschÃ¤rfe, Direct3D-Beschleunigung und Standby-Sperre gegen Bildschirmabschaltung. |
| **ðŸŽ® Geringe Latenz & Gaming** | Zeichnen mit Stift, mobile Spiele | Minimaler Puffer fÃ¼r unverzÃ¶gerte 60 FPS Reaktion. |
| **ðŸŽ¬ Kino & HD-Video** | Filme, YouTube, Videowiedergabe | Adaptiver Jitter-Puffer gegen Ruckler und Bildartefakte. |
| **ðŸŽµ Nur Audio** | Musik, Podcasts, Apple Music | Schaltet die Videoanzeige ab und Ã¼bertrÃ¤gt verlustfreies ALAC-Audio ressourcenschonend. |

---

## ðŸ› ï¸ Fehlerbehebung & HÃ¤ufige Fragen

**Das iPad findet "UxPlay Windows" nicht in der Bildschirmsynchronisierung?**
  1. Vergewissern Sie sich, dass beide GerÃ¤te im **selben WLAN** sind (GÃ¤ste-Netzwerke mit Client-Isolation vermeiden).
  2. PrÃ¼fen Sie in UxPlay Desktop auf der Hauptseite die Karte **Netzwerkerkennung (Apple Bonjour)**. Klicken Sie bei Bedarf auf **"Bonjour Reparieren / Starten"**.
**Audio und Video laufen leicht asynchron?**
  - Ã–ffnen Sie **Einstellungen > Audio-Einstellungen** und justieren Sie den Schieberegler **Audio-SynchronisationsverzÃ¶gerung** (z. B. `+100 ms` oder `-100 ms`).

---

## ðŸ”¬ Technische Architektur & Funktionsweise

FÃ¼r Entwickler und Mitwirkende, die das technische Fundament verstehen mÃ¶chten:

```mermaid
graph TD
    subgraph Apple-GerÃ¤t
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
* **FairPlay-Krypto & Handshake:** BewÃ¤ltigt den kryptografischen SchlÃ¼sselaustausch moderner iOS/iPadOS-Versionen Ã¼ber RTSP (Ports 7000/7100).
* **Kein Linux-D-Bus unter Windows:** Nutzt anstelle von D-Bus die native Win32-Kernel-API `SetThreadExecutionState` zur Standby-Verhinderung.

### 2. GStreamer Direct3D Video-Pipeline
* Hardwarebeschleunigte Dekodierung Ã¼ber **Direct3D 11 (`d3d11videosink`)** und Direct3D 12.
* Pipeline: `appsrc -> h264parse / h265parse -> d3d11h264dec / d3d11h265dec -> d3d11videosink`.
* Zero-Copy GPU-SpeicherÃ¼bertragung mit minimaler CPU-Last (< 3%).

### 3. Prozesssicherheit durch Windows Job Objects
* Native Subprozesse sind an ein Win32 **Job Object** mit `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE` gebunden.
* Bei Beendigung der BenutzeroberflÃ¤che schlieÃŸt der Windows-Kernel automatisch alle Kindprozesse atomar.

### 4. Apple Bonjour Auto-Reparatur
* Direkte Anbindung an Apples `dnssd.dll`.
* Repariert verwaiste RegistrierungseintrÃ¤ge von `mDNSResponder.exe` in `C:\Program Files\Bonjour` und Ã¶ffnet Port UDP 5353 sowie TCP 7000-7100 in der Firewall.

---

## ðŸ› Fehler Melden & Issues Erstellen

Haben Sie einen Fehler entdeckt? Wir freuen uns Ã¼ber Ihren Bericht!

Erstellen Sie bitte ein Issue unter **[GitHub Issues](https://github.com/jorjanion/uxplay-desktop/issues)** mit folgenden Angaben:
1. **Betriebssystem:** Windows 10 oder 11 (Build-Nummer).
2. **Apple-GerÃ¤t:** iPad/iPhone-Modell und iOS/iPadOS-Version.
3. **Logs:** Wechseln Sie in den Reiter **Echtzeit-Logs**, klicken Sie auf **"Kopieren"** und fÃ¼gen Sie den Auszug ein.
4. **Netzwerk:** WLAN 2.4/5GHz, LAN oder Mesh-Netzwerk.

---

## ðŸ¤ Mitwirken & Pull Requests (PRs)

Pull Requests sind **herzlich willkommen**!

Leitlinien fÃ¼r BeitrÃ¤ge:
1. **Echter Mehrwert:** PRs sollten messbare Verbesserungen (Leistung, StabilitÃ¤t, ZugÃ¤nglichkeit) bringen.
2. **Null-Kosten-Grenze (â‚¬0):** Streng 100% frei und Open Source. Keine proprietÃ¤ren SDKs oder kostenpflichtigen Cloud-Dienste.
3. **VerstÃ¤ndlichkeit:** Bitte Ã„nderungen in der Beschreibung nachvollziehbar erklÃ¤ren, damit der Maintainer den Code prÃ¼fen kann.
4. **Testabdeckung:** Alle Unit-Tests in `tests/UxPlayDesktop.Tests` mÃ¼ssen erfolgreich durchlaufen.

---

## ðŸ“œ Lizenz & Danksagung

* **UxPlay Desktop** steht unter der **[GNU General Public License v3.0 (GPL-3.0)](LICENSE)**.
* Baut auf dem Open-Source-Projekt von **[FDH2/UxPlay](https://github.com/FDH2/UxPlay)** auf.
* Entwickelt und gepflegt mit â¤ï¸ von **jorjanion**.

