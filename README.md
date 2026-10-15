# 🦅 Cóndor Táctico: Tactical Air Combat Simulator

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0--windows-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C# 12](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/dotnet/csharp/)
[![Platform: Windows](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D6?logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![Architecture: Clean OOP + Spatial Hash](https://img.shields.io/badge/Architecture-Modular%20Game%20Engine-brightgreen)](#arquitectura-del-motor)
[![Streaming: Async Background Channels](https://img.shields.io/badge/Streaming-Zero--Stutter%20Channels-orange)](#gestión-de-recursos-y-streaming-en-segundo-plano)
[![Network: UDP Async](https://img.shields.io/badge/Network-UDP%20Client--Server-blue)](#multijugador-cliente-servidor-udp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE.md)

**Cóndor Táctico** es un simulador y videojuego táctico de combate aéreo 2D en tiempo real desarrollado en **C# (.NET 8.0)** por estudiantes de la **Universidad Nacional de Juliaca (UNAJ)**. Incorpora un motor gráfico vectorial procedimental con suavizado bicúbico (**4x SSAA**), particionamiento espacial en grilla hash $\mathcal{O}(N+K)$, gestión de memoria *Swap-and-Pop* de cero asignaciones en frame caliente, un **subsistema de streaming de recursos asíncrono en segundo plano**, arquitectura de red **Cliente-Servidor UDP asíncrona** y un motor de instrumentación y auditoría de software (**Profiling en vivo a 60 FPS**).

---

## 📸 Vista Previa del Simulador

```
===================================================================================
[ PANEL DE LANZAMIENTO ] PILOTO: CÓNDOR-01 | MI NAVE: [ B-2 Spirit ] | MODO: [ HOST UDP :5050 ]
===================================================================================
   ▲  Cielo Estratosférico (Paralaje de Nubes & Efectos de Viento a 60 FPS)
   │
   │      [═══════ B-2 Spirit Rival (AF 82-1071) [58 HP] ═══════]
   │                         ▲   ▲
   │                         │   │  (Salva Doble AGM-158 JASSM)
   │                         ▼   ▼
   │
   │
   │                         ▲
   │                         │  (AIM-120 AMRAAM Supersónico)
   │                  [ Caza F-35A Lightning II [25 HP] ]
   ▼
-----------------------------------------------------------------------------------
[ HUD TACTICO F3 ] 60.0 FPS | CPU: 0.41 ms | RAM: 1.5 MB | Streaming Bg: 16 | RTT: 0.7 ms
===================================================================================
```

> Renders vectoriales y capturas disponibles en la carpeta [`assets/`](./assets/).

---

## 🌟 Características Principales

### 1. 🛩️ Arsenal Aéreo Asimétrico (OOP)
- **Lockheed Martin F-35A Lightning II (`AF-15-5104`)**:
  - Caza polivalente de quinta generación con alta maniobrabilidad y velocidad supersónica ($12.0\text{ px/tick}$).
  - Armamento: Misil supersónico aire-aire **AIM-120 AMRAAM** con cadencia rápida ($8\text{ ticks}$).
- **Northrop Grumman B-2 Spirit (`AF-82-1071` "Spirit of Mississippi")**:
  - Bombardero estratégico furtivo de ala volante con blindaje reforzado y escudo RAM de absorción ($12\text{ pts}$).
  - Armamento: **Doble salva simultánea** de misiles de crucero furtivos **AGM-158 JASSM** desde bahías de carga interna izquierda y derecha.

### 2. ⚡ Motor de Renderizado Vectorial con 4x SSAA Bicúbico
- Modelos fotorrealistas sintetizados mediante matrices matemáticas de curvas y colores (`MatrizPuntos` y `MatrizColores`).
- **Super-Sampling Anti-Aliasing (4x SSAA)** con interpolación bicúbica de alta calidad y doble búfer de hardware (`LienzoTactico`), eliminando cualquier parpadeo de pantalla (*flickering*).
- **Caché Hash $\mathcal{O}(1)$ de Sprites Pre-rotados** (`CacheSpritesAeronaves`): Convierte una rasterización intensiva de $\approx 16.4\text{ ms}$ en un acceso de lookup hash de $\approx 0.002\text{ ms}$.

### 3. 🎯 Particionamiento Espacial en Grilla Hash $\mathcal{O}(N+K)$
- División dinámica del espacio aéreo en celdas espaciales de $72\times72\text{ px}$ mediante la función hash:
  $$\text{Hash}(c_x, c_y) = (c_x \cdot 73856093) \oplus (c_y \cdot 19349663)$$
- Reduce la complejidad de colisiones de $\mathcal{O}(N \times M)$ a **$\mathcal{O}(N+K)$**, optimizando más del **85%** de comparaciones geométricas innecesarias.
- **Hitbox de 3 Segmentos en $\mathcal{O}(1)$**:
  - *Ala Izquierda*: Daño colateral leve ($-1\text{ HP}$).
  - *Cabina Central*: Impacto crítico perforante ($-10\text{ HP}$).
  - *Ala Derecha*: Daño colateral leve ($-1\text{ HP}$).

### 4. 📦 Gestión de Recursos y Streaming Asíncrono en Segundo Plano
- Diseñado para la carga ultrarrápida y optimizada de activos futuros (audio, texturas de terreno, nuevas aeronaves y mapas) **sin congelar el hilo de renderizado**:
  - Implementado sobre **`System.Threading.Channels.Channel<T>`** para despacho de tareas sin contención ni bloqueos.
  - **Mecanismo de Fallback Inmediato en $\mathcal{O}(1)$**: Si un recurso aún se está cargando, retorna un sustituto seguro sin detener el juego y encola la carga prioritaria.
  - **Política de Presupuesto LRU (Least Recently Used)**: Desalojo automático de memoria RAM cuando el consumo excede el presupuesto límite (256 MB).
  - Consulta la documentación técnica en [`docs/RESOURCE_STREAMING.md`](./docs/RESOURCE_STREAMING.md).

### 5. 🌐 Arquitectura de Red Cliente-Servidor UDP Asíncrona
- Comunicación sin bloqueo mediante datagramas UDP (`GestorRedClienteServidor`).
- **Tres Modos de Operación**:
  1. `Local_Combate_IA`: Práctica en solitario contra IA táctica evasiva con oscilación senoidal.
  2. `Servidor_Host_Online`: Abre un socket de escucha UDP en el puerto `5050` sincronizando múltiples clientes.
  3. `Cliente_Remoto_Online`: Conexión de baja latencia hacia la IP y puerto del servidor remoto.
- DTO compacto `PaqueteRedAeronave` con serialización tabular delimitada por *pipe* (`|`), minimizando el ancho de banda y la sobrecarga de serialización.

### 6. 📊 Telemetría y Profiling en Tiempo Real
- Monitor táctico en vivo alternable con la tecla **`F3`**:
  - FPS reales y tiempo de CPU por subsistema (*Update* vs *Render* en $\mu\text{s}$).
  - Consumo de memoria RAM gestionada (Heap) y Working Set del proceso.
  - Telemetría del gestor de streaming en segundo plano (recursos en caché y tareas pendientes en cola).
  - Auditoría de recolecciones del Garbage Collector (Gen 0, Gen 1, Gen 2).
  - Telemetría de red: Latencia de ida y vuelta (**RTT Ping** en ms), paquetes transmitidos (TX) y recibidos (RX).
  - Exportación instantánea de reporte formal de métricas mediante la tecla **`F4`** (`reporte_profiling_metricas.txt`).

---

## 📐 Estructura del Repositorio

```
JuegoAviones/
├── .gitignore                      # Reglas de exclusión para .NET, Caches y Binarios
├── README.md                       # Documento principal del proyecto
├── GDD.md                          # Game Design Document (Diseño de mecánicas y lore)
├── ARCHITECTURE.md                 # Arquitectura de software y optimizaciones Big-O
├── CONTRIBUTING.md                 # Guía para desarrolladores y reglas de hot loop
├── CHANGELOG.md                    # Historial cronológico de versiones (SemVer)
├── ROADMAP.md                      # Plan de evolución, audio estéreo y nuevas aeronaves
├── CODE_OF_CONDUCT.md              # Código de conducta de la comunidad
├── LICENSE.md                      # Licencia MIT de código abierto
├── CondorTactico.slnx              # Archivo de solución XML moderno para .NET 8 / VS 2022+
│
├── docs/                           # Especificaciones técnicas detalladas
│   ├── RESOURCE_STREAMING.md       # Pipeline de streaming asíncrono y canales
│   ├── NETWORK_PROTOCOL.md         # Protocolo de red UDP, formato de trama y RTT
│   ├── COMBAT_SYSTEM.md            # Fórmulas de daño, cinemática y hitboxes
│   └── HISTORIAL_MEJORAS_V3.md     # Documentación técnica de transición previa
│
├── assets/                         # Arte, renders y recursos del juego
│   ├── reference/                  # Planos e imágenes de referencia histórica
│   ├── renders/                    # Renders HD de F-35 y B-2 Spirit
│   ├── screenshots/                # Capturas del simulador en ejecución
│   └── sprites/                    # Sprites vectoriales y texturas base
│
├── tools/                          # Herramientas auxiliares y scripts de generación
│   ├── GenerarModeloB2Spirit.ps1   # Script generador del B-2 Spirit
│   ├── GenerarModeloF35.ps1        # Script generador del F-35A
│   ├── GenerarSpriteMisil.ps1      # Generador de proyectiles
│   └── TestRenderF35.cs            # Validador de rasterización
│
└── CondorTactico/                  # Código Fuente Principal (C# 12 / .NET 8)
    ├── CondorTactico.csproj        # Configuración del proyecto SDK (CondorTactico.dll)
    ├── Program.cs                  # Punto de entrada de la aplicación WinForms
    ├── Core/                       # Tipos primitivos, enums y estructuras base
    │   └── TiposCombate.cs
    ├── Resources/                  # MOTOR DE STREAMING ASÍNCRONO
    │   ├── IRecursoJuego.cs
    │   ├── EstadoRecurso.cs
    │   ├── RecursoGrafico.cs
    │   ├── SolicitudCargaRecurso.cs
    │   └── GestorRecursosAsincrono.cs
    ├── Entities/                   # Jerarquía polimórfica de entidades (DIT=3)
    │   ├── EntidadEspacial.cs      # Nivel 1: Clase abstracta raíz
    │   ├── AeronaveBase.cs         # Nivel 2: Clase abstracta de aeronave
    │   ├── CazaF35Lightning.cs     # Nivel 3: Caza F-35A
    │   ├── BombarderoB2Spirit.cs   # Nivel 3: Bombardero B-2 Spirit
    │   └── Proyectil.cs            # Entidad de proyectiles balísticos
    ├── Systems/                    # Motores y subsistemas de procesamiento
    │   └── GrillaEspacialColisiones.cs # Particionamiento espacial Hash O(N+K)
    ├── Graphics/                   # Pipeline gráfico vectorial y efectos
    │   ├── CacheSpritesAeronaves.cs# Caché O(1) integrado con el gestor de recursos
    │   ├── F35Model.cs             # Modelo vectorial procedimental F-35A
    │   ├── B2SpiritModel.cs        # Modelo vectorial procedimental B-2 Spirit
    │   ├── ParticulaCombate.cs     # Partículas de combustión (Value Type)
    │   └── TextoFlotanteImpacto.cs # Textos flotantes de daño crítico
    ├── Networking/                 # Subsistema Cliente-Servidor UDP
    │   └── RedClienteServidor.cs
    ├── Diagnostics/                # Telemetría, Big-O y Profiling
    │   └── ProfilerMotor.cs
    └── UI/                         # Interfaz gráfica de usuario y HUD
        ├── LienzoTactico.cs        # Canvas Double-Buffered nativo y foco prioritario
        ├── VentanaCombateTactico.cs# Formulario principal y Game Loop a 60 FPS
        ├── VentanaCombateTactico.Designer.cs
        ├── VentanaCombateTactico.resx
        ├── VentanaConfiguracionRed.cs # Subventana para configuración de sockets UDP
        ├── VentanaAjustesMision.cs    # Subventana de escuadrones y parámetros tácticos
        ├── VentanaReporteMetricas.cs  # Subventana de telemetría y profiling Big-O
        └── VentanaAcercaDe.cs         # Subventana de créditos institucionales UNAJ
```

---

## 🎮 Controles de Combate

| Acción | Tecla / Control | Descripción |
| :--- | :--- | :--- |
| **Volar Izquierda / Derecha** | `A` / `D` o `🠔` / `🠖` | Desplazamiento horizontal con inclinación visual de ala ($\pm15^\circ$). |
| **Acelerar / Desacelerar** | `W` / `S` o `🠕` / `🠗` | Movimiento vertical activando el efecto de postcombustión / empuje. |
| **Fuego de Armamento** | `Espacio` | Lanza el patrón de armamento específico de la aeronave seleccionada. |
| **Cambiar Aeronave** | Menú Superior / `MI NAVE` | Alterna instantáneamente entre el F-35A y el B-2 Spirit en combate. |
| **HUD de Telemetría** | `F3` | Alterna la visualización del panel de diagnóstico (FPS, CPU, RAM, Big-O, Streaming). |
| **Exportar Métricas** | `F4` / Botón Exportar | Genera una auditoría formal del motor en disco (`reporte_profiling_metricas.txt`). |

---

## 🚀 Compilación y Ejecución

```powershell
# Ubicarse en la raíz del repositorio
cd JuegoAviones

# Compilar en modo Release
dotnet build -c Release

# Ejecutar el simulador Cóndor Táctico
dotnet run --project CondorTactico/CondorTactico.csproj -c Release
```

---

## 👥 Equipo de Desarrollo y Créditos Institucionales

**Institución:** Universidad Nacional de Juliaca (UNAJ)  
**Facultad / Escuela:** Ingeniería de Sistemas  
**Proyecto:** Cóndor Táctico - Simulador de Combate Aéreo 2D  

**Desarrolladores:**
- **Ronal Abelin Larico Ccaso**
- **Clever Nandor Mamani Huallpa**
- **Jhon Yossep Hinojosa Apaza**
- **Jhon Alex Huaracallo Sucari**
- **Hector Carlos Torres Flores**

---

## 📄 Licencia

Este proyecto está distribuido bajo la licencia **MIT**. Consulte el archivo [`LICENSE.md`](./LICENSE.md) para más detalles.
