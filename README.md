# 🦅 Cóndor Táctico 3D - Combate Aéreo en Tiempo Real

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0--windows-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Three.js WebGL 2.0](https://img.shields.io/badge/3D%20Engine-Three.js%20WebGL%202.0-00dcff?logo=threedotjs&logoColor=white)](#-mapa-ágil-de-arquitectura-y-memoria-del-código)
[![WebView2 GPU](https://img.shields.io/badge/Host-WebView2%20GPU%20Accelerated-0078D6?logo=microsoftedge&logoColor=white)](#-mapa-ágil-de-arquitectura-y-memoria-del-código)
[![Platform: Windows](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D6?logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![Network: UDP LAN + Internet](https://img.shields.io/badge/Network-UDP%20LAN%20%2B%20Internet-blue)](#subsistema-de-red-cliente-servidor-udp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE.md)

**Cóndor Táctico 3D** es un videojuego moderno de combate aéreo en **3D (Three.js WebGL 2.0 acelerado por GPU sobre .NET 8.0 + Microsoft WebView2)** desarrollado por estudiantes de la **Universidad Nacional de Juliaca (UNAJ)**.

Combina un **motor gráfico 3D en tiempo real** (aeronaves esculpidas en 3D con materiales PBR metálicos, hangar 3D orbital interactivo, 5 mundos 3D con terreno dinámico y Jefes Finales colosales), el **emblema oficial del Cóndor Dorado (`logo.png`)**, una **interfaz moderna e intuitiva diseñada para cualquier jugador sin tecnicismos**, y un **backend nativo C#** para persistencia JSON y multijugador UDP en Red Local (Wi-Fi / LAN) e Internet.

![Flota de Cóndor Táctico renderizada en Blender Cycles](./assets/renders/Flota_Showcase.png)

---

## 🗺️ Mapa Ágil de Arquitectura y Memoria del Código

> *Este índice sirve como memoria de navegación rápida para localizar y entender las responsabilidades de cada archivo del proyecto sin necesidad de releer el código fuente completo.*

| Directorio | Archivo | Responsabilidad Principal |
| :--- | :--- | :--- |
| **Raíz** | [`CondorTactico.slnx`](./CondorTactico.slnx) | Solución XML moderna de Visual Studio / .NET 8. |
| **Raíz** | [`logo.png`](./logo.png) | Emblema oficial del Cóndor Dorado de combate aéreo (procesado con transparencia alfa en `wwwroot/assets/logo.png`). |
| **Proyecto** | [`CondorTactico/Program.cs`](./CondorTactico/Program.cs) | Punto de entrada principal: inicializa `GestorEstadoGlobal` y lanza `VentanaJuego3D`. |
| **UI 3D** | [`CondorTactico/UI/VentanaJuego3D.cs`](./CondorTactico/UI/VentanaJuego3D.cs) | **Host nativo acelerado por GPU (`WebView2`)**: sirve `https://condor.local/index.html`, aplica el icono oficial `logo.png` y conecta el motor 3D con `GestorEstadoGlobal` y `GestorRedClienteServidor`. |
| **Web 3D** | [`CondorTactico/wwwroot/index.html`](./CondorTactico/wwwroot/index.html) | **Estructura visual de la Edición 3D**: Pantalla de Carga con `logo.png`, Menú Principal de 9 opciones sobre Hangar 3D en vivo, Modales y HUD de combate limpio. |
| **Web 3D** | [`CondorTactico/wwwroot/css/estilos.css`](./CondorTactico/wwwroot/css/estilos.css) | **Diseño Glassmorphism AAA**: Estilo visual dorado y cian inspirado en `logo.png`, tarjetas modernas, barras de energía animadas y diseño adaptable. |
| **Motor 3D** | [`CondorTactico/wwwroot/js/motor3d.js`](./CondorTactico/wwwroot/js/motor3d.js) | **Motor Gráfico 3D (Three.js WebGL 2.0)**: Esculpe en 3D el *Cóndor Dorado X*, *Caza F-35 Relámpago* y *Bombardero B-2 Espíritu*, Hangar 3D orbital, 5 biomas 3D, Jefes Finales 3D, ondas expansivas y audio Web Audio API. |
| **UI JS** | [`CondorTactico/wwwroot/js/interfaz.js`](./CondorTactico/wwwroot/js/interfaz.js) | **Gestor de Interfaz Amigable y Puente C#**: Lenguaje 100% claro sin tecnicismos para Campaña Casual, Multijugador (En Casa Wi-Fi vs. Internet), Tienda, Inventario, Progresión, Logros y Ajustes. |
| **Web Lib** | [`CondorTactico/wwwroot/lib/three.min.js`](./CondorTactico/wwwroot/lib/three.min.js) | Librería gráfica 3D Three.js empaquetada localmente para ejecución 100% offline. |
| **Core** | [`CondorTactico/Core/GestorEstadoGlobal.cs`](./CondorTactico/Core/GestorEstadoGlobal.cs) | **Estado global y persistencia JSON (`%LocalAppData%\CondorTacticoUNAJ`)**: Perfil del piloto, monedas, experiencia, catálogo de tienda, niveles desbloqueados y logros. |
| **Networking**| [`CondorTactico/Networking/RedClienteServidor.cs`](./CondorTactico/Networking/RedClienteServidor.cs) | **Motor de Red UDP LAN + Internet**: Auto-descubrimiento LAN Broadcast (`:15051`), Codificador de Invitación (`CodificadorSalaTactica` `CT-...`), modos `1v1`/`2v2`/`Coop`. |
| **Tools** | [`tools/GeneradorModelos/renderizar_estudio_cycles.py`](./tools/GeneradorModelos/renderizar_estudio_cycles.py) | **Estudio de render Blender Cycles**: genera todos los renders de `assets/renders/` desde los GLB reales del juego (vista cenital 2048 px, sprite 512 px, plano heroico 3/4 y alineación de la flota). |

---

## 🎮 Controles de Combate

| Acción | Control / Tecla | Detalle de Operación |
| :--- | :--- | :--- |
| **Volar / Mover tu Avión 3D** | `🠔 🠖 🠕 🠗` o `W A S D`, `Ratón` o stick del mando | Vuelo con inercia: cada avión acelera, frena e inclina las alas a su manera. Configurable en *Configuración*. |
| **Disparar Cañones Láser** | `Espacio`, `Clic Izquierdo` o `A` / `RT` del mando | Dispara ráfagas dobles o triples según el avión 3D equipado. |
| **Tonel Evasivo** | `Shift` o `F`, `X` / `LB` del mando | Giro de 360° con impulso lateral: los disparos y meteoros te atraviesan durante 0.42 s. Rozar proyectiles recarga el superpoder. |
| **Activar Superpoder** | `Q`, `E`, `Clic Derecho` o `B` / `RB` del mando | Activa el superpoder equipado (*Onda Paralizante*, *Escudo Invencible*, *Fuego Triple Rápido* o *Explosión Solar*). |
| **Menú de Pausa** | `P`, `Escape` o `Start` del mando | Pausa la partida para continuar, reintentar desde el Jefe Final o volver al menú. El juego también se pausa solo al cambiar de ventana. |

---

## 🚀 Compilación y Ejecución

```powershell
# Ubicarse en el directorio raíz del proyecto
cd JuegoAviones

# Compilar la solución (.NET 8.0 Windows + WebView2 3D)
dotnet build CondorTactico.slnx

# Ejecutar Cóndor Táctico 3D
dotnet run --project CondorTactico/CondorTactico.csproj
```

---

## 📝 Registro de Estado y Avances del Proyecto

### Versión 5.1.0 — Pulido de Jugabilidad Profesional (Versión Actual)
- **Física de vuelo con inercia y perfiles por avión (`motor3d.js` → `PERFILES_VUELO`)**: aceleración/frenado exponenciales, contravolante más rápido, límites blandos y alabeo/cabeceo/guiñada derivados de la velocidad real. El *F-35* es el más ágil, el *F-22* el más rápido y el *B-2* el más pesado (antes los cuatro se pilotaban igual).
- **Tonel evasivo `Shift`**: maniobra de 360° con fotogramas de intangibilidad, recarga visible en el HUD y bonificación por roce de proyectiles.
- **Cámara de persecución**: resorte críticamente amortiguado, anticipación en virajes, inclinación sutil, FOV dinámico y sacudida por *trauma* con ruido continuo (sustituye al temblor aleatorio por fotograma). Intensidad ajustable o desactivable.
- **Gráficos**: sombras reales del sol sobre terreno y río (antes el mapa de sombras estaba activo pero ninguna luz proyectaba), estelas de condensación en las puntas de ala, líneas de velocidad, corriente animada del río, explosiones por capas (destello, bola de fuego, chispas balísticas y humo que se expande), humo en aeronaves dañadas y destello emisivo al recibir impacto.
- **IA y justicia del combate**: los enemigos telegrafían cada disparo con un brillo de carga de 0.4 s y usan puntería predictiva parcial según dificultad y misión.
- **Dificultad Fácil / Normal / Difícil**, **calidad gráfica Alta / Media / Baja** y **vibración de cámara** en *Configuración*.
- **Audio**: bus maestro con compresor, capas de ruido filtrado para explosiones e impactos, zumbido continuo de turbinas que responde a la velocidad, alarma de casco crítico y sonido de tonel.
- **HUD**: viñeta roja de daño y pulso de casco crítico, indicador de recarga del tonel.
- **Soporte de mando** (Gamepad API, zona muerta analógica) y **pausa automática** al perder el foco.
- **Rendimiento**: pool fijo de 600 partículas reutilizables (antes se creaba un material por partícula en cada explosión, estela de meteoro y destello), caché de nodos de efectos (antes `getObjectByName` recorría todo el modelo GLB varias veces por fotograma) y sin `preserveDrawingBuffer`. Medido: ~3.5 ms por fotograma en calidad Alta sobre una Intel UHD 630 integrada.
- **Renders oficiales rehechos en Blender 5.2 Cycles** (`tools/GeneradorModelos/renderizar_estudio_cycles.py`): antes eran recortes de imágenes conceptuales con el fondo eliminado por umbral, y las variantes de cada modelo eran copias idénticas. Ahora se renderizan los modelos GLB reales del juego con iluminación de estudio (clave, relleno, dos contornos de color según el bando y cenital), HDRI de estudio, líneas de panel y desgaste procedurales, pintura satinada, sombra de contacto, resplandor en el plasma y fondo transparente:
  - `*_Render_UltraHD.png`: vista cenital ortográfica, proa arriba, 2048 px (sprite maestro del modo 2D).
  - `*_Render_Game.png` y `F35_Render_Rival.png`: sprite de juego de 512 px (el rival con pintura carmesí).
  - `*_Render_3D_Top.png`: plano heroico en 3/4.
  - `Flota_Showcase.png`: alineación de la flota del jugador.
  - Para regenerarlos: `blender -b --factory-startup --python tools/GeneradorModelos/renderizar_estudio_cycles.py -- [modelos] [--calidad final|prueba]`.
- **Correcciones**:
  - El choque con meteoro causaba derrota instantánea incluso con *Escudo Invencible* o nave de reserva; ahora causa daño masivo coherente (arrasa el escudo y gran parte del casco) y respeta la invulnerabilidad.
  - Al reintentar desde el checkpoint del Jefe Final, derrotarlo reanudaba las oleadas en lugar de completar la misión.
  - Tras derrotar al último enemigo, los proyectiles enemigos restantes podían convertir una victoria en derrota; ahora se desintegran durante una pausa cinemática de victoria (cámara lenta al derribar al jefe).
  - Los choques con enemigos teletransportaban al jugador; ahora aplican un impulso físico.

### Versión 5.0.0 — Edición 3D
- **Salto Completo a Gráficos 3D en Tiempo Real (`motor3d.js` + `VentanaJuego3D.cs`)**:
  - Integración de **Three.js WebGL 2.0** acelerado por hardware dentro de **Microsoft WebView2** en `.NET 8.0`.
  - **3 Aviones Esculpidos en 3D**: *Cóndor Dorado X* (inspirado en el logo oficial con alas invertidas y detalles de oro), *Caza F-35 Relámpago* y *Bombardero B-2 Espíritu*, con cabinas de cristal reflectante, misiles bajo las alas y conos de postcombustión en 3D.
  - **Hangar 3D Interactivo en el Menú Principal**: Cámara orbital alrededor de una plataforma circular iluminada donde el jugador inspecciona su avión 3D y cambia de modelo o pintura en tiempo real.
  - **5 Mundos 3D y Jefes Finales Colosales**: *Valle del Titicaca*, *Cañón de la Tormenta*, *Glaciar Austral*, *Volcán en Erupción* y *Fortaleza Espacial*, con montañas 3D, agua/lava animada, nubes volumétricas y 5 Jefes Finales gigantes en 3D.
- **Integración del Emblema Oficial (`logo.png`)**:
  - Procesado con fondo transparente alfa e integrado en la Pantalla de Introducción, la cabecera del Menú Principal y el icono nativo de la ventana de Windows.
- **Diseño 100% Intuitivo para Cualquier Persona (Cero Tecnicismos)**:
  - Se eliminaron todos los términos técnicos internos, reemplazándolos por explicaciones claras, amigables y bien organizadas en español para todas las edades.

### Versiones Anteriores (v3.0.0 – v3.4.0)
- **v3.4.0**: Motor de FX cinemáticos (`EfectosVisualesCombate.cs`), HUD militar holográfico de 5ª generación (`HudTacticoMilitar.cs`) y sintetizador de audio procedural PCM en memoria (`SintetizadorAudioTactico.cs`).
- **v3.3.0**: Pantalla de carga e introducción táctica a 60 FPS (`PantallaCarga.cs`), pipeline de precalentamiento asíncrono (`CargadorJuego.cs`) y rasterización paralela (`CacheSpritesAeronaves.cs`).
- **v3.2.0**: Corrección definitiva de controles desde el arranque (`WndProc`/`ProcessDialogKey`) y subventanas modulares.
- **v3.0.0 – v3.1.0**: Modelo vectorial B-2 Spirit AF 82-1071 con 4x SSAA, grilla espacial hash $\mathcal{O}(N+K)$, streaming asíncrono con LRU (`GestorRecursosAsincrono.cs`) y profiling en vivo (`ProfilerMotor.cs`).

---

## 👥 Equipo de Desarrollo y Créditos Institucionales

**Institución:** Universidad Nacional de Juliaca (UNAJ)  
**Escuela Profesional:** Ingeniería de Sistemas  
**Proyecto:** Cóndor Táctico - Simulador de Combate Aéreo 2D  

**Autores:**
- **Ronal Abelin Larico Ccaso**
- **Clever Nandor Mamani Huallpa**
- **Jhon Yossep Hinojosa Apaza**
- **Jhon Alex Huaracallo Sucari**
- **Hector Carlos Torres Flores**
