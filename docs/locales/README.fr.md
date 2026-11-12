</p>

<p align="center">
  <img src="../screenshots/dashboard.png" width="95%" alt="UxPlay Desktop Dashboard" />
</p>

<p align="center">
  <img src="../screenshots/mirroring_demo.jpg" width="95%" alt="UxPlay Direct3D 11 AirPlay Mirroring" />
</p>

--- ðŸ“º UxPlay Desktop (FranÃ§ais)

<p align="center">
  <strong>Client de recopie d'Ã©cran AirPlay moderne et Ã  ultra-faible latence pour Windows</strong><br>
  <em>Transformez n'importe quel PC Windows en un rÃ©cepteur AirPlay haute performance pour iPad, iPhone et Mac.</em>
</p>

<p align="center">
  <a href="https://github.com/jorjanion/uxplay-desktop/releases/latest"><img src="https://img.shields.io/github/v/release/jorjanion/uxplay-desktop?color=blue&label=Derni%C3%A8re%20Version" alt="Release"></a>
  <a href="https://github.com/jorjanion/uxplay-desktop/blob/main/LICENSE"><img src="https://img.shields.io/badge/Licence-GPL%20v3.0-green.svg" alt="License"></a>
  <img src="https://img.shields.io/badge/Plateforme-Windows%2010%20%7C%2011%20(x64%20%2F%20ARM64)-0078D6?logo=windows" alt="Platform">
  <img src="https://img.shields.io/badge/.NET-8.0%20Self--Contained-512BD4?logo=dotnet" alt=".NET 8">
  <img src="https://img.shields.io/badge/Direct3D-11%20%2F%2012%20Hardware-orange" alt="Direct3D">
  <img src="https://img.shields.io/badge/Co%C3%BBt-0%20%E2%82%AC%20Libre%20%26%20Gratuit-brightgreen" alt="Cost">
</p>

---

### ðŸŒ Lire cette documentation dans une autre langue:
[ðŸ‡ªðŸ‡¸ EspaÃ±ol](README.es.md) â€¢ [ðŸ‡ºðŸ‡¸ English](../../README.md) â€¢ [ðŸ‡«ðŸ‡· FranÃ§ais](README.fr.md) â€¢ [ðŸ‡©ðŸ‡ª Deutsch](README.de.md) â€¢ [ðŸ‡®ðŸ‡¹ Italiano](README.it.md) â€¢ [ðŸ‡§ðŸ‡· PortuguÃªs (Brasil)](README.pt.md) â€¢ [ðŸ‡·ðŸ‡º Ð ÑƒÑÑÐºÐ¸Ð¹](README.ru.md) â€¢ [ðŸ‡µðŸ‡± Polski](README.pl.md) â€¢ [ðŸ‡³ðŸ‡± Nederlands](README.nl.md) â€¢ [ðŸ‡ºðŸ‡¦ Ð£ÐºÑ€Ð°Ñ—Ð½ÑÑŒÐºÐ°](README.uk.md) â€¢ [ðŸ‡¨ðŸ‡³ ç®€ä½“ä¸­æ–‡](README.zh-CN.md) â€¢ [ðŸ‡¯ðŸ‡µ æ—¥æœ¬èªž](README.ja.md)

---

## âš¡ Pourquoi UxPlay Desktop ?

De nombreux enseignants, formateurs et professionnels qui partagent l'Ã©cran de leur iPad ou iPhone sont confrontÃ©s Ã  des blocages rÃ©currents :
- **Les applications AirPlay commerciales** imposent des abonnements mensuels onÃ©reux ou apposent des filigranes intrusifs.
- **Les anciens outils open source** se figent dÃ¨s la premiÃ¨re image avec les versions rÃ©centes d'iPadOS, requiÃ¨rent des commandes complexes dans le terminal, souffrent de dÃ©synchronisation audio ou Ã©chouent par manque de bibliothÃ¨ques DLL.

**UxPlay Desktop** a Ã©tÃ© conÃ§u pour rÃ©soudre dÃ©finitivement ces dÃ©sagrÃ©ments :

- **ðŸ“¦ Installateur Natif (.exe) en 1 Clic :** Fini les archives ZIP Ã  dÃ©compresser manuellement. TÃ©lÃ©chargez l'exÃ©cutable, suivez l'assistant et tout est prÃªt.
- **ðŸš€ 100% Autonome (Self-Contained) :** N'exige aucun tÃ©lÃ©chargement externe ; intÃ¨gre le runtime .NET 8, les DLL C++ et les plugins GStreamer.
- **âš¡ AccÃ©lÃ©ration MatÃ©rielle Direct3D 11/12 :** DÃ©bit fluide de 60 FPS, nettetÃ© parfaite des notes manuscrites et latence imperceptible.
- **ðŸ–¼ï¸ Mode FenÃªtre sans Bordure :** Affichage Ã©purÃ© idÃ©al pour la vidÃ©oprojection et le partage d'Ã©cran sans distractions.
- **ðŸ›¡ï¸ RÃ©paration Automatique Bonjour & Pare-feu :** DÃ©tecte les services mDNS corrompus, reconfigure Apple Bonjour et ouvre les ports rÃ©seau dans le Pare-feu Windows.
- **ðŸŒ Disponible en plus de 15 langues :** FranÃ§ais, anglais, espagnol, allemand, portugais, russe, chinois, japonais et bien d'autres.
- **ðŸ”„ Mises Ã  Jour en 1 Clic :** DÃ©tection automatique des nouvelles versions GitHub et mise Ã  niveau transparente.

---

## ðŸ“– Guide de DÃ©marrage Rapide (3 Ã‰tapes)

### Ã‰tape 1 : Installer l'Application
1. TÃ©lÃ©chargez l'installateur **[`UxPlayDesktop-Setup.exe`](https://github.com/jorjanion/uxplay-desktop/releases/latest)**.
2. Lancez l'installation et suivez l'assistant.
3. L'installateur installe automatiquement le programme, configure le service Bonjour et ouvre les ports de sÃ©curitÃ© Windows.

### Ã‰tape 2 : DÃ©marrer le Serveur AirPlay
1. Ouvrez **UxPlay Desktop** depuis votre Bureau ou le menu DÃ©marrer.
2. Cliquez sur le bouton bleu **Â« DÃ©marrer le Serveur AirPlay Â»**.
3. L'indicateur s'allume en bleu avec le message : **Â« PrÃªt Ã  connecter Â»**.

### Ã‰tape 3 : Connecter votre iPad ou iPhone
1. Assurez-vous que votre appareil Apple et votre PC sont sur le **mÃªme rÃ©seau Wi-Fi**.
2. Sur votre iPad ou iPhone, ouvrez le **Centre de contrÃ´le**.
3. Touchez **Recopie de l'Ã©cran** (l'icÃ´ne avec deux rectangles imbriquÃ©s).
4. SÃ©lectionnez **Â« UxPlay Windows Â»** dans la liste.
5. Votre Ã©cran s'affiche immÃ©diatement sur votre moniteur en haute dÃ©finition 60 FPS sans dÃ©calage.

### Profils de Diffusion (PrÃ©rÃ©glages ExpliquÃ©s)
| Profil | RecommandÃ© pour | Description |
| :--- | :--- | :--- |
| **ðŸŽ“ Enseignement & PrÃ©sentations** *(DÃ©faut)* | Cours, diapositives, prise de notes numÃ©rique | ClartÃ© de texte maximale, Direct3D et inhibition automatique de mise en veille d'Ã©cran. |
| **ðŸŽ® Faible Latence & Jeux** | Dessin au stylet, jeux mobiles rÃ©actifs | MÃ©moire tampon minimale pour un retour tactile instantanÃ© Ã  60 FPS. |
| **ðŸŽ¬ CinÃ©ma & VidÃ©o HD** | Films, YouTube, multimÃ©dia | Tampon adaptatif Ã©vitant les saccades et garantissant une lecture fluide. |
| **ðŸŽµ Audio Uniquement** | Musique, podcasts, Apple Music | DÃ©sactive le flux vidÃ©o pour Ã©conomiser les ressources et diffuser en audio ALAC sans perte. |

---

## ðŸ› ï¸ DÃ©pannage & Questions FrÃ©quentes

- **L'iPad ne dÃ©tecte pas "UxPlay Windows" dans la recopie d'Ã©cran ?**
  1. VÃ©rifiez que les deux appareils sont sur le **mÃªme rÃ©seau Wi-Fi** (attention aux rÃ©seaux invitÃ©s avec isolation des clients).
  2. Dans UxPlay Desktop, vÃ©rifiez la carte **DÃ©couverte RÃ©seau (Apple Bonjour)**. Si l'Ã©tat indique *ArrÃªtÃ©*, cliquez sur **Â« RÃ©parer / DÃ©marrer Bonjour Â»**.
- **Le son a un lÃ©ger dÃ©calage par rapport Ã  la vidÃ©o ?**
  - Rendez-vous dans **ParamÃ¨tres > ParamÃ¨tres Audio** et ajustez le curseur **DÃ©lai de Synchronisation Audio** (par exemple `+100 ms` ou `-100 ms`).

---

## ðŸ”¬ Architecture Technique & Fonctionnement Interne

Pour les dÃ©veloppeurs et contributeurs souhaitant explorer les rouages techniques :

```mermaid
graph TD
    subgraph Appareil Apple
        A[iPad / iPhone / Mac] -->|mDNS DNS-SD _airplay._tcp| B(Bonjour Daemon port 5353)
        A -->|RTSP Handshake port 7000/7100| C[Moteur Natif UxPlay Core]
        A -->|H.264 / H.265 Stream| D[GStreamer Media Pipeline]
        A -->|ALAC / AAC Audio| E[WASAPI Audio Sink]
    end

    subgraph HÃ´te PC Windows
        C -->|d3d11videosink / d3d12videosink| F[Direct3D Hardware Swapchain]
        F --> G[DirectX Borderless Surface]
        H[UxPlayDesktop.exe C# WPF UI] -->|Job Object Process Safety| C
        H -->|Stdout / Stderr Regex Parser| I[MÃ©triques HUD & Journaux]
        H -->|Win32 SetThreadExecutionState| J[Sous-systÃ¨me d'Alimentation Windows]
    end
```

### 1. Pile Protocolaire AirPlay & Moteur Natif
* **Base :** DÃ©rivÃ© de la branche master de [`FDH2/UxPlay`](https://github.com/FDH2/UxPlay) (v1.74).
* **Cryptographie FairPlay :** Prise en charge des Ã©changes de clÃ©s iOS/iPadOS 17/18 via nÃ©gociation RTSP (ports 7000/7100).
* **ZÃ©ro D-Bus sous Windows :** Remplacement de D-Bus par l'API Win32 `SetThreadExecutionState` pour empÃªcher la mise en veille pendant les prÃ©sentations.

### 2. Pipeline VidÃ©o Direct3D GStreamer
* DÃ©codage GPU accÃ©lÃ©rÃ© par **Direct3D 11 (`d3d11videosink`)** et Direct3D 12.
* Pipeline : `appsrc -> h264parse / h265parse -> d3d11h264dec / d3d11h265dec -> d3d11videosink`.
* Rendu sans copie en mÃ©moire GPU avec consommation CPU infÃ©rieure Ã  3%.

### 3. SÃ©curitÃ© des Processus via Windows Job Objects
* Les sous-processus natifs sont rattachÃ©s Ã  un **Job Object** Win32 avec `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`.
* En cas de fermeture imprÃ©vue de l'interface, le systÃ¨me Windows termine automatiquement tous les processus enfants pour Ã©viter les ports orphelins.

### 4. Auto-RÃ©paration du Service Apple Bonjour
* Liaison directe avec `dnssd.dll`.
* En cas de service mDNS corrompu, le moteur rÃ©inscrit proprement `mDNSResponder.exe` dans `C:\Program Files\Bonjour` et configure les autorisations de pare-feu correspondantes.

---

## ðŸ› Signaler un Bug & Ouvrir une Issue

Vous avez repÃ©rÃ© un bogue ? Vos retours sont prÃ©cieux !

Ouvrez une issue sur **[GitHub Issues](https://github.com/jorjanion/uxplay-desktop/issues)** en mentionnant :
1. **SystÃ¨me d'exploitation :** Windows 10 ou 11 (version exacte).
2. **Appareil Apple :** ModÃ¨le d'iPad/iPhone et version iPadOS/iOS.
3. **Journaux :** Allez dans l'onglet **Journaux en Temps RÃ©el**, cliquez sur **Â« Copier Â»** et collez les lignes pertinentes.
4. **Type de rÃ©seau :** Wi-Fi 2.4/5GHz, cÃ¢ble ou rÃ©pÃ©teur mesh.

---

## ðŸ¤ Contribuer & Proposer des Pull Requests (PR)

Les contributions sont **chaleureusement accueillies** !

Principes directeurs :
1. **Apport Mesurable :** Les PR doivent apporter une amÃ©lioration tangible (performances, stabilitÃ©, ergonomie).
2. **Budget ZÃ©ro (â‚¬0) :** Projet 100% libre et gratuit. Aucun service payant ou composant propriÃ©taire n'est acceptÃ©.
3. **ClartÃ© :** DÃ©taillez clairement les modifications apportÃ©es afin que le mainteneur puisse aisÃ©ment comprendre le code soumis.
4. **Tests Unitaires :** L'ensemble des 39 tests de `tests/UxPlayDesktop.Tests` doit rÃ©ussir sans avertissement.

---

## ðŸ“œ Licence & Remerciements

* **UxPlay Desktop** est distribuÃ© sous licence **[GNU General Public License v3.0 (GPL-3.0)](LICENSE)**.
* ConÃ§u sur les bases du moteur open source de **[FDH2/UxPlay](https://github.com/FDH2/UxPlay)**.
* DÃ©veloppÃ© et maintenu avec â¤ï¸ par **jorjanion**.

