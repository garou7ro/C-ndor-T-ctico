# 🦅 Cóndor Táctico - Simulador de Combate Aéreo 2D

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0--windows-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C# 12](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/dotnet/csharp/)
[![Platform: Windows](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D6?logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![Architecture: Clean OOP + Spatial Hash](https://img.shields.io/badge/Architecture-Modular%20Game%20Engine-brightgreen)](#-mapa-ágil-de-arquitectura-y-memoria-del-código)
[![Streaming: Async Background Channels](https://img.shields.io/badge/Streaming-Zero--Stutter%20Channels-orange)](#subsistema-de-recursos-y-streaming-asíncrono)
[![Network: UDP Async](https://img.shields.io/badge/Network-UDP%20Client--Server-blue)](#subsistema-de-red-cliente-servidor-udp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE.md)

**Cóndor Táctico** es un simulador y videojuego táctico de combate aéreo 2D en tiempo real desarrollado en **C# (.NET 8.0 Windows Forms)** por estudiantes de la **Universidad Nacional de Juliaca (UNAJ)**.

Incorpora un motor gráfico vectorial procedimental con suavizado bicúbico (**4x SSAA**), particionamiento espacial en grilla hash $\mathcal{O}(N+K)$, recolección de memoria *Swap-and-Pop* en $\mathcal{O}(1)$, **streaming asíncrono en segundo plano**, arquitectura de red **Cliente-Servidor UDP asíncrona** y un motor de instrumentación (**Profiling en vivo a 60 FPS**).

---

## 🗺️ Mapa Ágil de Arquitectura y Memoria del Código

> *Este índice sirve como memoria de navegación rápida para localizar y entender las responsabilidades de cada archivo del proyecto sin necesidad de releer el código fuente completo.*

| Directorio | Archivo | Responsabilidad Principal |
| :--- | :--- | :--- |
| **Raíz** | [`CondorTactico.slnx`](./CondorTactico.slnx) | Solución XML moderna de Visual Studio / .NET 8. |
| **Proyecto** | [`CondorTactico/Program.cs`](./CondorTactico/Program.cs) | Punto de entrada: despliega `PantallaCarga` e inicia `VentanaCombateTactico(resultado)`. |
| **Core** | [`CondorTactico/Core/CargadorJuego.cs`](./CondorTactico/Core/CargadorJuego.cs) | **Motor de arranque multietapa en segundo plano**: rasterización paralela, paleta GDI+, precalentamiento JIT, sockets UDP y GC `SustainedLowLatency`. |
| **UI** | [`CondorTactico/UI/PantallaCarga.cs`](./CondorTactico/UI/PantallaCarga.cs) | **Pantalla de carga e introducción táctica a 60 FPS**: cielo andino en paralaje, estrellas, siluetas, barrido de radar, flyby de cazas y tips tácticos. |
| **UI** | [`CondorTactico/UI/VentanaCombateTactico.cs`](./CondorTactico/UI/VentanaCombateTactico.cs) | **Ventana principal y Game Loop a 60 FPS** (Timer 16ms). Intercepción nativa de teclas (`WndProc`, `ProcessDialogKey`), física, colisiones y renderizado sin asignaciones GDI+. |
| **UI** | [`CondorTactico/UI/LienzoTactico.cs`](./CondorTactico/UI/LienzoTactico.cs) | Control `PictureBox` especializado con `ControlStyles.Selectable`, `IsInputKey` para flechas/WASD y foco prioritario sin bloqueos. |
| **UI** | [`CondorTactico/UI/VentanaConfiguracionRed.cs`](./CondorTactico/UI/VentanaConfiguracionRed.cs) | Subventana modal para enlace UDP: Servidor Host, Cliente Remoto o Modo Local, IP, Puerto y telemetría RTT. |
| **UI** | [`CondorTactico/UI/VentanaAjustesMision.cs`](./CondorTactico/UI/VentanaAjustesMision.cs) | Subventana modal para configuración táctica: indicativo del piloto, selección de aeronave aliada (B-2 / F-35A), líderes rivales y escoltas. |
| **UI** | [`CondorTactico/UI/VentanaReporteMetricas.cs`](./CondorTactico/UI/VentanaReporteMetricas.cs) | Subventana de telemetría y diagnóstico en vivo: FPS, CPU ($\mu\text{s}$), memoria RAM, eficiencia Big-O y exportación de informe a disco. |
| **UI** | [`CondorTactico/UI/VentanaAcercaDe.cs`](./CondorTactico/UI/VentanaAcercaDe.cs) | Subventana institucional con la nómina de autores de la UNAJ y manual de mandos de vuelo. |
| **Entities** | [`CondorTactico/Entities/EntidadEspacial.cs`](./CondorTactico/Entities/EntidadEspacial.cs) | Nivel 1 de herencia ($DIT=1$): Clase abstracta raíz con coordenadas flotantes $(X, Y)$, dimensiones y estado `Activo`. |
| **Entities** | [`CondorTactico/Entities/AeronaveBase.cs`](./CondorTactico/Entities/AeronaveBase.cs) | Nivel 2 de herencia ($DIT=2$): Vida, escudo RAM, alabeo ($\pm15^\circ$), velocidad, cinemática e IA evasiva senoidal. |
| **Entities** | [`CondorTactico/Entities/BombarderoB2Spirit.cs`](./CondorTactico/Entities/BombarderoB2Spirit.cs) | Nivel 3 de herencia ($DIT=3$): Bombardero furtivo B-2 Spirit AF 82-1071 con **doble salva simultánea** de misiles AGM-158 JASSM y escudo de 12 pts. |
| **Entities** | [`CondorTactico/Entities/CazaF35Lightning.cs`](./CondorTactico/Entities/CazaF35Lightning.cs) | Nivel 3 de herencia ($DIT=3$): Caza polivalente supersónico F-35A con misiles aire-aire AIM-120 AMRAAM de alta cadencia. |
| **Entities** | [`CondorTactico/Entities/Proyectil.cs`](./CondorTactico/Entities/Proyectil.cs) | Proyectil balístico/furtivo con ciclo de vida y compactación $\mathcal{O}(1)$ *Swap-and-Pop*. |
| **Systems** | [`CondorTactico/Systems/GrillaEspacialColisiones.cs`](./CondorTactico/Systems/GrillaEspacialColisiones.cs) | **Particionamiento Espacial Hash $\mathcal{O}(N+K)$** en celdas de $72\times72\text{ px}$. Detección de impacto de 3 segmentos (alas $-1$, cabina crítica $-10$). |
| **Resources**| [`CondorTactico/Resources/GestorRecursosAsincrono.cs`](./CondorTactico/Resources/GestorRecursosAsincrono.cs) | **Motor de Streaming Asíncrono en segundo plano** sobre `System.Threading.Channels.Channel<T>`. Fallback inmediato en $\mathcal{O}(1)$ y política de desalojo LRU (límite 256 MB). |
| **Resources**| [`CondorTactico/Resources/RecursoGrafico.cs`](./CondorTactico/Resources/RecursoGrafico.cs) | Contenedor de texturas gestionadas con marcas de último acceso para LRU. |
| **Graphics** | [`CondorTactico/Graphics/CacheSpritesAeronaves.cs`](./CondorTactico/Graphics/CacheSpritesAeronaves.cs) | **Caché multihilo (`Parallel.ForEach`)** de sprites vectoriales 3D pre-rotados con reporte de progreso en tiempo real. |
| **Graphics** | [`CondorTactico/Graphics/RecursosGraficosHud.cs`](./CondorTactico/Graphics/RecursosGraficosHud.cs) | **Paleta GDI+ precargada** (fuentes, brochas, plumas y fondo estratosférico PArgb) para cero asignaciones por frame. |
| **Graphics** | [`CondorTactico/Graphics/EfectosVisualesCombate.cs`](./CondorTactico/Graphics/EfectosVisualesCombate.cs) | **Motor de FX cinemáticos**: Screen Shake por trauma cuadrático, ondas expansivas ópticas, destellos atmosféricos, sombras pseudo-3D, vórtices alares y casquillos de 20mm. |
| **Graphics** | [`CondorTactico/Graphics/HudTacticoMilitar.cs`](./CondorTactico/Graphics/HudTacticoMilitar.cs) | **HUD militar holográfico de 5ª gen** (F-35 / B-2): horizonte artificial, cabeceo/alabeo, FPM, radar AESA, retícula predictiva PIPER y cintas de rumbo/altitud. |
| **Graphics** | [`CondorTactico/Graphics/B2SpiritModel.cs`](./CondorTactico/Graphics/B2SpiritModel.cs) | Modelo vectorial procedimental fotorrealista del B-2 Spirit AF 82-1071 y misil AGM-158 con 4x SSAA. |
| **Graphics** | [`CondorTactico/Graphics/F35Model.cs`](./CondorTactico/Graphics/F35Model.cs) | Modelo vectorial procedimental del caza F-35A y misil AIM-120 con 4x SSAA. |
| **Graphics** | [`CondorTactico/Graphics/ParticulaCombate.cs`](./CondorTactico/Graphics/ParticulaCombate.cs) | Estructura de valor (*struct*) para estelas de postcombustión y explosiones aéreas sin recolector de basura (cero GC). |
| **Graphics** | [`CondorTactico/Graphics/TextoFlotanteImpacto.cs`](./CondorTactico/Graphics/TextoFlotanteImpacto.cs) | Textos flotantes dinámicos de daño crítico ("impact -10" / "impact -1"). |
| **Audio**    | [`CondorTactico/Audio/SintetizadorAudioTactico.cs`](./CondorTactico/Audio/SintetizadorAudioTactico.cs) | **Sintetizador de audio procedural en memoria** (PCM 16-bit): cañón Vulcan 20mm, misiles, explosiones, alertas de lock y estampido sónico. |
| **Networking**| [`CondorTactico/Networking/RedClienteServidor.cs`](./CondorTactico/Networking/RedClienteServidor.cs) | Sockets UDP no bloqueantes (`GestorRedClienteServidor`) y DTO de serialización compacta `PaqueteRedAeronave`. |
| **Diagnostics**| [`CondorTactico/Diagnostics/ProfilerMotor.cs`](./CondorTactico/Diagnostics/ProfilerMotor.cs) | Instrumentación de alta precisión (`Stopwatch`), telemetría de CPU en microsegundos, RAM, GC y exportador de informes (`F4`). |
| **Tools** | [`tools/CapturadorRemoto/`](./tools/CapturadorRemoto/) | **Herramienta aislada externa**: renderizado offscreen y capturas vía reflexión sin alterar el código fuente del juego. |
| **Scripts** | [`scripts/generar_capturas_remotas.ps1`](./scripts/generar_capturas_remotas.ps1) | **Script de telemetría remota**: genera capturas PNG, video MP4 y GIF optimizados para inspección móvil. |

---

## 🎮 Controles de Combate

| Acción | Control / Tecla | Detalle de Operación |
| :--- | :--- | :--- |
| **Volar / Maniobrar** | `🠔 🠖 🠕 🠗` o `W A S D` | Vuelo multidireccional continuo con alabeo e inclinación alar ($\pm15^\circ$). Activo desde el primer tick. |
| **Disparo Táctico** | `Espacio` o `Enter` | Dispara el patrón de armamento específico de la aeronave seleccionada (Doble salva en B-2 / Misil AIM-120 en F-35A). |
| **Aeronave B-2 Spirit** | `1` o Botón Superior | Selecciona el bombardero furtivo B-2 Spirit AF 82-1071 en tiempo real. |
| **Aeronave F-35A** | `2` o Botón Superior | Selecciona el caza supersónico F-35A Lightning II en tiempo real. |
| **Escolta de Apoyo** | Botón `➕ ESCOLTA` | Despliega una aeronave adicional aliada o rival en formación de combate. |
| **HUD Militar Holográfico** | `H` | Conmuta modos de visualización: Cabina de Vidrio Completa, Retícula Táctica Mínima o Desactivado. |
| **Audio Táctico Militar** | `M` | Activa o silencia el sintetizador de audio en memoria. |
| **HUD de Telemetría** | `F3` | Conmuta el panel en vivo de métricas (FPS, CPU $\mu\text{s}$, RAM, Big-O, RTT). |
| **Exportar Métricas** | `F4` | Genera un archivo de informe de rendimiento en disco (`reporte_profiling_metricas.txt`). |
| **Reiniciar Misión** | `R` o Botón `🚀 NUEVA MISIÓN` | Reinicia el espacio aéreo y redespliega las aeronaves. |

---

## 🚀 Compilación y Ejecución

```powershell
# Ubicarse en el directorio raíz del proyecto
cd JuegoAviones

# Compilar la solución (.NET 8.0 Windows)
dotnet build CondorTactico.slnx

# Ejecutar el simulador Cóndor Táctico
dotnet run --project CondorTactico/CondorTactico.csproj
```

---

## 📝 Registro de Estado y Avances del Proyecto

### Versión 3.4.0 (Versión Actual)
- **Motor de Efectos Cinemáticos de Combate (`EfectosVisualesCombate.cs`)**:
  - **Screen Shake por Trauma Cuadrático**: sacudida cinematográfica de pantalla que simula retroceso balístico de cañón de 20mm y detonaciones aéreas sin afectar la estabilidad de las tarjetas estáticas del HUD.
  - **Sombras Atmosféricas Pseudo-3D**: proyección de siluetas de aeronaves sobre el manto estratosférico con perspectiva de altitud y cizalladura por alabeo ($\pm15^\circ$).
  - **Ondas Expansivas y Destellos Ópticos**: anillos supersónicos expansivos con halo refractivo y destello lumínico en toda la escena durante impactos críticos y destrucción de cazas.
  - **Física de Casquillos de Munición**: eyección lateral de casquillos de 20mm con velocidad tangencial, gravedad y giro angular.
  - **Vórtices Alares y Humo de Daño**: condensación aerodinámica en puntas de ala durante giros y estelas de humo negro/ignición cuando la salud desciende del 50% y 25%.
- **HUD Militar Holográfico de 5ª Generación (`HudTacticoMilitar.cs`)**:
  - Simbología de cabina de cristal de cazas modernos (F-35 Gen 5 / B-2 Spirit):
    - Línea de horizonte artificial y escala de cabeceo/alabeo ($\pm10^\circ$).
    - Vector de trayectoria de vuelo (Flight Path Marker - FPM).
    - Cono de barrido frontal de radar AESA de 55°.
    - Retícula predictiva de impacto (PIPER) calculando tiempo de vuelo y deflexión balística.
    - Anillos de enganche *Radar Lock* con telemetría de distancia y velocidad de aproximación.
    - Cintas tácticas de rumbo militar ($000^\circ - 360^\circ$), velocidad (KTS) y altitud (FT).
    - Conmutable con la tecla `H`.
- **Sintetizador de Audio Militar Procedural en Memoria (`SintetizadorAudioTactico.cs`)**:
  - Generación en tiempo real de formas de onda PCM mono 16-bit 22.05kHz (RIFF WAVE) 100% nativo C# sin archivos ni librerías externas.
  - Efectos sintetizados: ráfaga de cañón rotativo Vulcan 20mm, ignición y aceleración de misiles AGM-158/AIM-120, explosiones subsónicas con saturación analógica, alertas tácticas de radar lock, impacto metálico y estampido sónico (*sonic boom*).
  - Conmutable con la tecla `M`.
- **Evolución Cinemática de la Pantalla de Carga (`PantallaCarga.cs`)**:
  - Cintas ondulantes de aurora austral estratosférica en cian, esmeralda y violeta sobre la cordillera andina.
  - Partículas de viento andino de gran altitud (cristales de hielo y polvo estelar) en deriva horizontal continua.
  - Terminal de diagnóstico de aviónica militar con códigos de telemetría en tiempo real.
  - Estampido sónico procedural al confirmar el despegue.

### Versión 3.3.0
- **Pantalla de Carga e Introducción Táctica Cinematográfica (`PantallaCarga.cs`)**:
  - Renderizado a 60 FPS con paralaje multicapa: cielo nocturno andino, constelaciones parpadeantes, siluetas de montañas, nubes en desplazamiento y emblema del Cóndor con barrido de radar.
  - Flyby dinámico de cazas B-2 Spirit y F-35A con estelas de condensación vectoriales.
  - Barra de progreso con gradiente cian/azul, bitácora de telemetría de subsistemas en vivo y rotación de consejos tácticos de combate.
  - Transición cinemática suave (fade-in / fade-out) y soporte para cancelación (`Escape`).
- **Motor de Carga y Precalentamiento en Segundo Plano (`CargadorJuego.cs`)**:
  - Pipeline asíncrono con 7 etapas ponderadas: configuración de latencia de GC (`SustainedLowLatency`), rasterización paralela de sprites 4x SSAA, paleta GDI+, pre-renderizado del fondo estratosférico, precalentamiento JIT de bucles críticos (física, colisiones, serialización), verificación de socket UDP y compactación final del LOH.
  - Adaptación automática de resolución de pantalla según área de trabajo del monitor (`ConfiguracionPantalla`).
- **Optimización Cero Asignaciones GDI+ (`RecursosGraficosHud.cs`)**:
  - Reutilización estática de brochas, plumas, fuentes y textura de cielo PArgb en el bucle a 60 FPS de `VentanaCombateTactico`, eliminando la presión sobre el Garbage Collector en combate.
- **Rasterización Multihilo Paralela (`CacheSpritesAeronaves.cs`)**:
  - Renderizado en paralelo (`Parallel.ForEach`) con reporte de progreso en tiempo real (`Action<int, int, string>`), reduciendo el tiempo de inicialización en CPUs multinúcleo.

### Versión 3.2.0
- **Corrección Crítica de Controles en Arranque**:
  - Resuelto el bug que impedía moverse al iniciar si no se disparaba previamente (`Enter`).
  - Se eliminaron controles de entrada de texto del lienzo de combate que robaban el foco.
  - Se implementó captura nativa de mensajes de teclado en `WndProc` (`WM_KEYDOWN` y `WM_KEYUP`), `ProcessDialogKey` y `IsInputKey` en `LienzoTactico`, asegurando movimiento fluido inmediato desde el tick 0.
- **Arquitectura Modular de Subventanas (`CondorTactico.UI`)**:
  - Creación de subventanas dedicadas: `VentanaConfiguracionRed`, `VentanaAjustesMision`, `VentanaReporteMetricas` y `VentanaAcercaDe`.
  - Barra de herramientas superior simplificada con botones desacoplados (`TabStop = false`).
- **Limpieza y Optimización de Documentación**:
  - Consolidación de la memoria técnica ágil en este `README.md`, eliminando archivos `.md` redundantes del repositorio para mantener el árbol limpio y de alto valor informativo.

### Versión 3.1.0
- **Identidad Oficial "Cóndor Táctico"**:
  - Renombramiento integral de ensamblados, clases y solución a `CondorTactico`.
  - Subsistema de streaming de recursos asíncrono en segundo plano (`GestorRecursosAsincrono`) sobre canales `System.Threading.Channels` con política LRU.

### Versión 3.0.0
- **Reingeniería OOP, Big-O y Red**:
  - Incorporación del bombardero furtivo B-2 Spirit AF 82-1071 con 4x SSAA.
  - Grilla espacial hash $\mathcal{O}(N+K)$ reduciendo en más del 85% las comparaciones geométricas.
  - Arquitectura Cliente-Servidor UDP en tiempo real.
  - Motor de profiling en vivo (`F3` / `F4`).

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
