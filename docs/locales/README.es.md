# ðŸ“º UxPlay Desktop (EspaÃ±ol)

<p align="center">
  <strong>Cliente de duplicado de pantalla AirPlay moderno y de ultra-baja latencia para Windows</strong><br>
  <em>Convierte cualquier ordenador con Windows en un receptor AirPlay de alto rendimiento para iPad, iPhone y Mac.</em>
</p>

<p align="center">
  <a href="https://github.com/jorjanion/uxplay-desktop/releases/latest"><img src="https://img.shields.io/github/v/release/jorjanion/uxplay-desktop?color=blue&label=Ãšltima VersiÃ³n" alt="Release"></a>
  <a href="https://github.com/jorjanion/uxplay-desktop/blob/main/LICENSE"><img src="https://img.shields.io/badge/Licencia-GPL%20v3.0-green.svg" alt="License"></a>
  <img src="https://img.shields.io/badge/Plataforma-Windows%2010%20%7C%2011%20(x64%20%2F%20ARM64)-0078D6?logo=windows" alt="Platform">
  <img src="https://img.shields.io/badge/.NET-8.0%20Self--Contained-512BD4?logo=dotnet" alt=".NET 8">
  <img src="https://img.shields.io/badge/Direct3D-11%20%2F%2012%20Hardware-orange" alt="Direct3D">
  <img src="https://img.shields.io/badge/Coste-0%20%E2%82%AC%20Libre%20y%20Gratuito-brightgreen" alt="Cost">
</p>

---

### ðŸŒ Leer esta documentaciÃ³n en otros idiomas:
[ðŸ‡ªðŸ‡¸ EspaÃ±ol](README.es.md) â€¢ [ðŸ‡ºðŸ‡¸ English](../../README.md) â€¢ [ðŸ‡«ðŸ‡· FranÃ§ais](README.fr.md) â€¢ [ðŸ‡©ðŸ‡ª Deutsch](README.de.md) â€¢ [ðŸ‡®ðŸ‡¹ Italiano](README.it.md) â€¢ [ðŸ‡§ðŸ‡· PortuguÃªs (Brasil)](README.pt.md) â€¢ [ðŸ‡·ðŸ‡º Ð ÑƒÑÑÐºÐ¸Ð¹](README.ru.md) â€¢ [ðŸ‡µðŸ‡± Polski](README.pl.md) â€¢ [ðŸ‡³ðŸ‡± Nederlands](README.nl.md) â€¢ [ðŸ‡ºðŸ‡¦ Ð£ÐºÑ€Ð°Ñ—Ð½ÑÑŒÐºÐ°](README.uk.md) â€¢ [ðŸ‡¨ðŸ‡³ ç®€ä½“ä¸­æ–‡](README.zh-CN.md) â€¢ [ðŸ‡¯ðŸ‡µ æ—¥æœ¬èªž](README.ja.md)

---

## âš¡ Â¿Por quÃ© UxPlay Desktop?

Muchos profesionales, docentes y usuarios que comparten la pantalla de su iPad o iPhone en presentaciones y reuniones se topan con problemas continuos:
- **Las aplicaciones comerciales de AirPlay** exigen suscripciones mensuales caras, cuotas de renovaciÃ³n o estampan marcas de agua molestas en mitad de la pantalla.
- **Otras herramientas libres antiguas** se congelan en el primer fotograma con las Ãºltimas versiones de iPadOS, requieren comandos tÃ©cnicos raros por consola, sufren desincronizaciÃ³n de audio o fallan por falta de librerÃ­as DLL y servicios de Linux.

**UxPlay Desktop** ha sido diseÃ±ado desde cero para solucionar todo esto de forma definitiva:

- **ðŸ“¦ Instalador Nativo Directo (`.exe`):** OlvÃ­date de descomprimir zips ni lidiar con carpetas. Descargas el instalador, pulsas Siguiente y te lo deja todo listo.
- **ðŸš€ 100% AutÃ³nomo (Self-Contained):** No te pedirÃ¡ descargar .NET ni componentes externos; incluye el motor de .NET 8, librerÃ­as C++ y plugins de GStreamer en su propio ejecutable.
- **âš¡ AceleraciÃ³n Direct3D 11/12 por Hardware:** 60 FPS ultra fluidos, nitidez absoluta en los trazos de GoodNotes y Notability y latencia imperceptible.
- **ðŸ–¼ï¸ Modo Ventana sin Bordes:** VisualizaciÃ³n limpia y optimizada para proyecciones y captura fluida de pantalla sin distracciones ni marcos molestos.
- **ðŸ›¡ï¸ ReparaciÃ³n y ConfiguraciÃ³n AutomÃ¡tica de Bonjour y Firewall:** Detecta servicios rotos de versiones antiguas, reconfigura Apple Bonjour en un clic y abre los puertos de red en el Firewall de Windows para que el iPad lo detecte al instante.
- **ðŸŒ Traducido a mÃ¡s de 15 idiomas:** EspaÃ±ol, inglÃ©s, francÃ©s, alemÃ¡n, portuguÃ©s, ruso, chino, japonÃ©s y mÃ¡s, con cambio dinÃ¡mico al instante.
- **ðŸ”„ Actualizaciones en 1 Clic:** La app te avisa si hay una nueva versiÃ³n y te la actualiza automÃ¡ticamente con un solo clic.

---

## ðŸ“– GuÃ­a RÃ¡pida para Usuarios (Puesta en Marcha en 3 Pasos)

### Paso 1: Instalar el Programa
1. Descarga el instalador **[`UxPlayDesktop-Setup.exe`](https://github.com/jorjanion/uxplay-desktop/releases/latest)** de la Ãºltima versiÃ³n.
2. Abre el instalador y sigue las sencillas instrucciones en pantalla.
3. El instalador se encargarÃ¡ automÃ¡ticamente de instalar la aplicaciÃ³n, registrar el servicio Bonjour y habilitar las excepciones en el Firewall de Windows.

### Paso 2: Iniciar el Servidor AirPlay
1. Abre **UxPlay Desktop** desde tu Escritorio o el menÃº Inicio.
2. Pulsa el botÃ³n azul **"Iniciar Servidor AirPlay"**.
3. El indicador cambiarÃ¡ a color azul con el mensaje: **"Listo para Conectar"**.

### Paso 3: Conectar tu iPad o iPhone
1. Comprueba que tu iPad/iPhone y tu PC con Windows estÃ¡n conectados a la **misma red Wi-Fi**.
2. En tu iPad o iPhone, desliza el dedo desde la esquina superior derecha para abrir el **Centro de Control**.
3. Toca en **Duplicar pantalla** (el icono de los dos rectÃ¡ngulos superpuestos).
4. Elige **"UxPlay Windows"** en la lista.
5. Â¡Listo! La pantalla de tu iPad aparecerÃ¡ al instante en tu monitor a 60 FPS sin retraso.

### Perfiles de Rendimiento (Presets Explicados)
| Perfil | Recomendado para | Â¿QuÃ© hace? |
| :--- | :--- | :--- |
| **ðŸŽ“ Clases y Presentaciones** *(Por defecto)* | Docencia, presentaciones, pizarra digital y diapositivas | MÃ¡xima nitidez de texto, aceleraciÃ³n Direct3D y bloqueo automÃ¡tico del salvapantallas para que la pantalla no se apague en clase. |
| **ðŸŽ® Baja Latencia y Juegos** | Dibujo rÃ¡pido, juegos de iPad | BÃºfer reducido al mÃ­nimo para conseguir respuesta inmediata en cada trazo a 60 FPS. |
| **ðŸŽ¬ Cine y VÃ­deo HD** | PelÃ­culas, YouTube, multimedia | BÃºfer adaptativo contra tirones para reproducir vÃ­deo sin artefactos. |
| **ðŸŽµ Solo Audio** | MÃºsica, podcasts, Apple Music | Apaga el vÃ­deo para ahorrar recursos y transmite audio ALAC de alta fidelidad. |

---

## ðŸ› ï¸ Preguntas Frecuentes y SoluciÃ³n de Problemas

- **Â¿El iPad o iPhone no detecta "UxPlay Windows" en Duplicar Pantalla?**
  1. AsegÃºrate de que ambos dispositivos estÃ¡n conectados a la **misma red Wi-Fi** (ojo con redes de invitados con aislamiento de clientes activado).
  2. En UxPlay Desktop, ve al panel principal y mira la tarjeta **Descubrimiento en Red (Apple Bonjour)**. Si dice *Detenido* o *Roto*, pulsa el botÃ³n **"Reparar / Iniciar Bonjour"** para que lo configure automÃ¡ticamente como Administrador.
- **Â¿El audio va ligeramente desfasado con el vÃ­deo?**
  - Ve a la pestaÃ±a **Ajustes > Ajustes de Audio** y mueve el control deslizante **CalibraciÃ³n de SincronizaciÃ³n** (por ejemplo `+100 ms` o `-100 ms`) para compensar el retardo de auriculares Bluetooth.

---

## ðŸ”¬ Arquitectura TÃ©cnica y Funcionamiento Interno

Para desarrolladores y colaboradores interesados en la arquitectura tÃ©cnica y el diseÃ±o interno:

```mermaid
graph TD
    subgraph Dispositivo Apple
        A[iPad / iPhone / Mac] -->|mDNS DNS-SD _airplay._tcp| B(Bonjour Daemon port 5353)
        A -->|RTSP Handshake port 7000/7100| C[Motor Nativo UxPlay Core]
        A -->|H.264 / H.265 Stream| D[GStreamer Media Pipeline]
        A -->|ALAC / AAC Audio| E[WASAPI Audio Sink]
    end

    subgraph Host PC Windows
        C -->|d3d11videosink / d3d12videosink| F[Direct3D Hardware Swapchain]
        F --> G[DirectX Borderless Surface]
        H[UxPlayDesktop.exe C# WPF UI] -->|Job Object Process Safety| C
        H -->|Stdout / Stderr Regex Parser| I[MÃ©tricas de HUD en Vivo y Logs]
        H -->|Win32 SetThreadExecutionState| J[Subsistema de EnergÃ­a de Windows]
    end
```

### 1. Protocolo AirPlay y Motor Nativo
* **NÃºcleo:** Basado en la rama activa master de [`FDH2/UxPlay`](https://github.com/FDH2/UxPlay) (versiÃ³n 1.74).
* **CriptografÃ­a FairPlay y Handshake:** Resuelve los intercambios de claves de iOS/iPadOS 17/18/19 mediante negociaciÃ³n RTSP (puertos TCP 7000/7100).
* **EliminaciÃ³n de D-Bus en Windows:** La versiÃ³n original de Linux usa D-Bus para el salvapantallas. UxPlay Desktop intercepta la gestiÃ³n de energÃ­a mediante la API nativa de Windows `SetThreadExecutionState`.

### 2. Pipeline de VÃ­deo Direct3D en GStreamer
* DecodificaciÃ³n acelerada por GPU mediante **Direct3D 11 (`d3d11videosink`)** y Direct3D 12.
* Pipeline: `appsrc -> h264parse / h265parse -> d3d11h264dec / d3d11h265dec -> d3d11videosink`.
* Renderizado zero-copy de memoria GPU con consumo de CPU inferior al 3% en procesadores multinÃºcleo.

### 3. Seguridad de Procesos con Job Objects de Windows
* Los subprocesos nativos (`uxplay.exe` y balizas Bluetooth LE) estÃ¡n asignados a un **Job Object** de Windows con el flag `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`.
* Si la interfaz WPF se cierra o cae, el kernel de Windows liquida todos los procesos hijos de forma atÃ³mica, evitando procesos zombie o puertos bloqueados.

### 4. RecuperaciÃ³n y ReparaciÃ³n AutomÃ¡tica de Apple Bonjour
* UxPlay se comunica con la biblioteca `dnssd.dll` de Apple.
* Si instalaciones previas de otros programas dejaron un servicio corrupto o huÃ©rfano, el motor de diagnÃ³stico reinstala y registra limpiamente `mDNSResponder.exe` en `C:\Program Files\Bonjour`, abriendo automÃ¡ticamente los puertos UDP 5353 y TCP 7000-7100 en el Firewall de Windows.

---

## ðŸ› Reportar Errores y Abrir Issues

Â¿Has encontrado un fallo o problema de conexiÃ³n? Â¡Tu reporte nos ayuda a mejorar!

Por favor, abre una incidencia en **[GitHub Issues](https://github.com/jorjanion/uxplay-desktop/issues)** incluyendo:
1. **Sistema Operativo:** Windows 10 u 11 (nÃºmero de compilaciÃ³n).
2. **Dispositivo Apple:** Modelo de iPad/iPhone y versiÃ³n de iOS/iPadOS (ej. iPad Pro M2, iPadOS 18.1).
3. **Registro de Logs:** Ve a la pestaÃ±a **Logs en Tiempo Real** dentro de UxPlay Desktop, pulsa **"Copiar"** y pega las lÃ­neas relevantes.
4. **Tipo de ConexiÃ³n:** Si usas Wi-Fi 2.4GHz, 5GHz, cable de red o router mesh.

---

## ðŸ¤ CÃ³mo Colaborar y Enviar Pull Requests (PRs)

Â¡Las contribuciones y Pull Requests son **muy bien recibidas**!

Antes de abrir una PR, ten en cuenta nuestra filosofÃ­a:
1. **Aporte Real:** La PR debe aportar una mejora medible (rendimiento, correcciÃ³n de bugs, estabilidad o accesibilidad para profesores y usuarios).
2. **Presupuesto Cero (â‚¬0):** Este proyecto es estrictamente **100% Libre y de CÃ³digo Abierto**. JamÃ¡s se aceptarÃ¡n dependencias de pago, servicios cloud de suscripciÃ³n o librerÃ­as privativas.
3. **ComprensiÃ³n del Creador:** Explica claramente en la descripciÃ³n de la PR *quÃ©* has cambiado y *por quÃ©*. Es fundamental que el creador del repositorio entienda cada lÃ­nea de cÃ³digo aportada.
4. **Calidad y Tests:** Todo el conjunto de pruebas unitarias en `tests/UxPlayDesktop.Tests` debe compilar y pasar limpiamente (`powershell .\build.ps1 -Action test`).

---

## ðŸ“œ Licencia y Agradecimientos

* **UxPlay Desktop** estÃ¡ publicado bajo la licencia **[GNU General Public License v3.0 (GPL-3.0)](LICENSE)**.
* Basado en el excelente trabajo original de ingenierÃ­a inversa de **[FDH2/UxPlay](https://github.com/FDH2/UxPlay)**.
* DiseÃ±ado y mantenido con â¤ï¸ por **jorjanion**.

