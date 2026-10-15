# 📝 Historial de Cambios (Changelog)

Todos los cambios notables en este proyecto serán documentados en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/) y este proyecto se adhiere a [Semantic Versioning](https://semver.org/lang/es/).

## [3.2.0] - 2026-10-14
### Corrección Crítica de Entrada en Arranque y Arquitectura Modular de Subventanas

#### Corregido
- **Bug de Bloqueo de Movimiento al Iniciar:**
  - Corregido el defecto crítico por el cual el jugador no podía moverse al iniciar la partida si no disparaba previamente (`Enter`).
  - La causa raíz residía en que `txtPiloto` dentro de la barra de controles capturaba el foco inicial por tabulación por defecto de WinForms, descartando las pulsaciones de flechas/WASD hasta que la tecla de disparo (`Enter`) liberaba el foco al lienzo.
  - Se desacopló la entrada de teclado mediante intercepción nativa en `WndProc` (`WM_KEYDOWN`/`WM_KEYUP`), `ProcessDialogKey` y la habilitación de `ControlStyles.Selectable` e `IsInputKey` en `LienzoTactico`, asegurando movimiento fluido inmediato desde el tick 0.

#### Agregado
- **Arquitectura Modular de Subventanas y Diálogos Tácticos (`CondorTactico/UI/`):**
  - **`VentanaConfiguracionRed.cs`**: Diálogo dedicado para configuración de sockets UDP (Servidor Host, Cliente Online, Modo Local, Endpoint IP y Puerto).
  - **`VentanaAjustesMision.cs`**: Diálogo táctico para parametrización de escuadrones, elección de nave aliada (B-2 Spirit / F-35A), composición de agresores y escoltas de apoyo.
  - **`VentanaReporteMetricas.cs`**: Subventana de diagnóstico en tiempo real con monitoreo de FPS, consumo de RAM, eficiencia Big-O de la Grilla Espacial Hash y exportador de informes.
  - **`VentanaAcercaDe.cs`**: Subventana institucional con la nómina oficial de autores de la UNAJ, créditos y manual de mandos tácticos.
  - Barra superior simplificada en `VentanaCombateTactico` con botones planos desacoplados (`TabStop = false`) que no interfieren con el foco del lienzo principal.

---

## [3.1.0] - 2026-10-14
### Identidad Oficial "Cóndor Táctico" y Motor de Streaming Asíncrono en Segundo Plano

#### Agregado
- **Subsistema de Carga y Streaming Asíncrono en Segundo Plano (`CondorTactico/Resources/`):**
  - Motor central `GestorRecursosAsincrono` basado en canales sin bloqueo `System.Threading.Channels.Channel<T>`.
  - Contrato unificado `IRecursoJuego` y contenedor gestionado `RecursoGrafico`.
  - Mecanismo de fallback inmediato en $\mathcal{O}(1)$ para evitar micro-stuttering o congelamiento del frame rate durante el streaming de activos futuros.
  - Política de presupuesto y desalojo de memoria RAM por técnica LRU (Least Recently Used) con límite parametrizable de 256 MB.
- **Identidad Oficial "Cóndor Táctico":**
  - Actualización de metadatos del proyecto (`CondorTactico.dll`, `CondorTactico.exe`).
  - Configuración del distintivo del piloto insignia (`CÓNDOR-01`) y títulos de telemetría táctica.
  - Documento de especificación técnica completa: [`docs/RESOURCE_STREAMING.md`](./docs/RESOURCE_STREAMING.md).

---

## [3.0.0] - 2026-10-14
### Reingeniería Arquitectónica Mayor: Modularidad, B-2 Spirit, Grilla Hash $\mathcal{O}(N+K)$, UDP y Profiling

#### Agregado
- **Bombardero Estratégico Northrop Grumman B-2 Spirit (`AF-82-1071`):**
  - Modelado vectorial fotorrealista completo en 3D (`B2SpiritModel.cs`) con curvaturas b-spline y toberas de escape integradas.
  - Mecánica de disparo de **doble salva táctica paralela** (misiles de crucero furtivos AGM-158 JASSM).
  - Blindaje con escudo RAM deflector de absorción ($12\text{ pts}$).
- **Particionamiento Espacial en Grilla Hash $\mathcal{O}(N+K)$ (`GrillaEspacialColisiones.cs`):**
  - Celdas dinámicas de $72\times72\text{ px}$ con dispersión hash y Object Pooling de listas, reduciendo hasta un $85\%$ las pruebas geométricas.
- **Caché Hash $\mathcal{O}(1)$ de Sprites Vectoriales (`CacheSpritesAeronaves.cs`):**
  - Pre-renderizado 4x SSAA bicúbico durante la carga, reduciendo el costo por frame de $16.4\text{ ms}$ a $0.002\text{ ms}$.
- **Arquitectura de Red UDP Asíncrona Cliente-Servidor (`RedClienteServidor.cs`):**
  - Modos de operación Host Servidor (`UDP :5050`), Cliente Remoto y Loopback local.
  - Sincronización continua de aeronaves, ángulo de alabeo y telemetría de ping RTT.
- **Motor de Profiling y Auditoría ISO/IEC 25010 (`ProfilerMotor.cs`):**
  - HUD táctico en vivo (`F3`) que monitorea FPS, $\mu\text{s}$ CPU, consumo de RAM y recolecciones Gen 0/1/2.
  - Exportador de reportes formales de ingeniería de software a archivo de texto (`F4`).
- **Sistema de Feedback Visual de Combate:**
  - Textos flotantes dinámicos para impactos de cabina crítica, impactos alares y absorción de escudos.
  - Sistema de partículas para postcombustión y explosiones aéreas.
  - Capa de nubes estratosféricas con paralaje aéreo.

#### Modificado
- **Reorganización Estructural del Repositorio:**
  - Separación del código monolítico en módulos de responsabilidad única (`Core/`, `Entities/`, `Systems/`, `Graphics/`, `Networking/`, `Diagnostics/`, `UI/`).
  - Extracción de `LienzoTactico` en su propio archivo independiente con doble búfer nativo.
  - Desacoplamiento de las 12 clases/estructuras que estaban concentradas en `ModelosOOP.cs`.

#### Optimizado
- Eliminación de la creación de `PictureBox` (HWND de Windows) por cada bala disparada.
- Adopción de la técnica de compactación *Swap-and-Pop* en $\mathcal{O}(1)$ para remover proyectiles y partículas muertas.
- Índice de Mantenibilidad elevado de **54** a **86.4 / 100**.

---

## [2.0.0] - 2026-09-20
### Incorporación del Caza F-35 y Renderizado Vectorial

#### Agregado
- Modelo vectorial fotorrealista del **Lockheed Martin F-35A Lightning II** (`F35Model.cs`).
- Suavizado 4x SSAA con renderizado bicúbico.
- Sistema de colisiones por hitboxes transversales con daño diferenciado entre alas y cabina.
- Controles fluidos de teclado con registro continuo multidireccional (`HashSet<Keys>`).

#### Modificado
- Migración de .NET Framework clásico a **.NET 8.0 Windows Desktop**.

---

## [1.0.0] - 2026-08-15
### Versión Inicial Monolítica (Prototipo Base)

#### Agregado
- Formulario básico en Windows Forms con aeronave estática.
- Bucle de movimiento básico basado en `System.Windows.Forms.Timer`.
- Detección de colisiones elemental mediante iteración de `Controls.OfType<PictureBox>()`.
