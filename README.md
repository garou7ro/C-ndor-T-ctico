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
| **Proyecto** | [`CondorTactico/Program.cs`](./CondorTactico/Program.cs) | Punto de entrada del proceso (`Application.Run(new VentanaCombateTactico())`). |
| **UI** | [`CondorTactico/UI/VentanaCombateTactico.cs`](./CondorTactico/UI/VentanaCombateTactico.cs) | **Ventana principal y Game Loop a 60 FPS** (Timer 16ms). Intercepción nativa de teclas (`WndProc`, `ProcessDialogKey`), física, colisiones y renderizado GDI+ con DoubleBuffering. |
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
| **Graphics** | [`CondorTactico/Graphics/CacheSpritesAeronaves.cs`](./CondorTactico/Graphics/CacheSpritesAeronaves.cs) | Caché Hash $\mathcal{O}(1)$ de sprites vectoriales pre-rotados con pre-carga en arranque. |
| **Graphics** | [`CondorTactico/Graphics/B2SpiritModel.cs`](./CondorTactico/Graphics/B2SpiritModel.cs) | Modelo vectorial procedimental fotorrealista del B-2 Spirit AF 82-1071 y misil AGM-158 con 4x SSAA. |
| **Graphics** | [`CondorTactico/Graphics/F35Model.cs`](./CondorTactico/Graphics/F35Model.cs) | Modelo vectorial procedimental del caza F-35A y misil AIM-120 con 4x SSAA. |
| **Graphics** | [`CondorTactico/Graphics/ParticulaCombate.cs`](./CondorTactico/Graphics/ParticulaCombate.cs) | Estructura de valor (*struct*) para estelas de postcombustión y explosiones aéreas sin recolector de basura (cero GC). |
| **Graphics** | [`CondorTactico/Graphics/TextoFlotanteImpacto.cs`](./CondorTactico/Graphics/TextoFlotanteImpacto.cs) | Textos flotantes dinámicos de daño crítico ("impact -10" / "impact -1"). |
| **Networking**| [`CondorTactico/Networking/RedClienteServidor.cs`](./CondorTactico/Networking/RedClienteServidor.cs) | Sockets UDP no bloqueantes (`GestorRedClienteServidor`) y DTO de serialización compacta `PaqueteRedAeronave`. |
| **Diagnostics**| [`CondorTactico/Diagnostics/ProfilerMotor.cs`](./CondorTactico/Diagnostics/ProfilerMotor.cs) | Instrumentación de alta precisión (`Stopwatch`), telemetría de CPU en microsegundos, RAM, GC y exportador de informes (`F4`). |

---

## 🎮 Controles de Combate

| Acción | Control / Tecla | Detalle de Operación |
| :--- | :--- | :--- |
| **Volar / Maniobrar** | `🠔 🠖 🠕 🠗` o `W A S D` | Vuelo multidireccional continuo con alabeo e inclinación alar ($\pm15^\circ$). Activo desde el primer tick. |
| **Disparo Táctico** | `Espacio` o `Enter` | Dispara el patrón de armamento específico de la aeronave seleccionada (Doble salva en B-2 / Misil AIM-120 en F-35A). |
| **Aeronave B-2 Spirit** | `1` o Botón Superior | Selecciona el bombardero furtivo B-2 Spirit AF 82-1071 en tiempo real. |
| **Aeronave F-35A** | `2` o Botón Superior | Selecciona el caza supersónico F-35A Lightning II en tiempo real. |
| **Escolta de Apoyo** | Botón `➕ ESCOLTA` | Despliega una aeronave adicional aliada o rival en formación de combate. |
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

### Versión 3.2.0 (Versión Actual)
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
