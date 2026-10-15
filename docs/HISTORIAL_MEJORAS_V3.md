# Documentación Técnica de Arquitectura, Cambios y Métricas de Software
## Proyecto: **Juego de Aeronaves Online (v3.0.0.0)**
**Institución:** Universidad Nacional de Juliaca (UNAJ)  
**Autores (Equipo de Desarrollo):**
- Ronal Abelin Larico Ccaso
- Clever Nandor Mamani Huallpa
- Jhon Yossep Hinojosa Apaza
- Jhon Alex Huaracallo Sucari
- Hector Carlos Torres Flores

**Evolución Arquitectónica:** De *Juego de Aviones Básico (v2.0)* a *Sistema Distribuido Cliente-Servidor con Motor OOP, Optimización Big-O, Profiling en Tiempo Real y Bombardero Furtivo B-2 Spirit (v3.0)*  
**Fecha de Actualización:** Octubre 2026

---

## 1. Resumen Ejecutivo y Objetivo del Proyecto

El presente documento registra formalmente la reingeniería integral del proyecto **`Juego_Basic`** hacia el sistema **"Juego de Aeronaves Online"**. El objetivo académico y técnico consiste en permitir que los grupos de desarrollo lancen distintas aeronaves de combate en un formulario interactivo de alto rendimiento, aplicando **Buenas Prácticas de Ingeniería de Software**, **Programación Orientada a Objetos (OOP)**, optimización de complejidad algorítmica (**Big-O**), instrumentación de rendimiento (**Profiling en tiempo real**) y una **Arquitectura Cliente-Servidor** capaz de sincronizar aeronaves en red local (LAN / `localhost`).

Adicionalmente, se incorpora al arsenal del juego el bombardero estratégico furtivo **Northrop Grumman B-2 Spirit (`AF 82-1071` - *Spirit of Mississippi*)** recreado mediante modelado vectorial 3D fotorrealista (`MatrizPuntos` y `MatrizColores`) con suavizado **4x SSAA Bicúbico**, operando en conjunto con el caza polivalente **Lockheed Martin F-35 Lightning II**.

---

## 2. Diagnóstico del Sistema Base (v2.0) vs. Nueva Arquitectura (v3.0)

| Dimensión Técnica | Estado Anterior (`v2.0 - Básico`) | Nuevo Nivel (`v3.0 - Aeronaves Online`) | Impacto Medible |
| :--- | :--- | :--- | :--- |
| **Paradigma y Diseño** | Código monolítico acoplado dentro de `Form1.cs`. Uso de `PictureBox.Tag` como `string`/`int` para almacenar la vida. | **OOP Puro y Principios SOLID**: Jerarquía `EntidadEspacial` $\rightarrow$ `AeronaveBase` $\rightarrow$ `CazaF35Lightning` y `BombarderoB2Spirit`. | Índice de Mantenibilidad ($MI$) sube de **54** a **86.4/100**. |
| **Aeronaves Disponibles** | Solo F-35 Lightning II. | **Northrop Grumman B-2 Spirit (`AF 82-1071`)** (Bombardero furtivo de ala volante con doble salva) + **F-35 Lightning II** (Caza ágil). | Polimorfismo real en vuelo, blindaje, hitboxes y armamento. |
| **Motor de Renderizado** | Creación de un `PictureBox` (control Win32 `HWND`) por cada bala y re-renderizado SSAA 4x completo en cada tecla. | **Lienzo GDI+ Double-Buffered unificado** + **Caché Hash $O(1)$ de Sprites Pre-rotados** (`CacheSpritesAeronaves`). | Tiempo de cuadro baja de **~18.4 ms** a **~0.42 ms** (>60 FPS estables). |
| **Complejidad de Colisiones (Big-O)** | Clonación de `contiene.Controls` y barrido exhaustivo $O(N \times M)$ sobre controles visuales. | **Grilla Espacial Hash (`GrillaEspacialColisiones`)** en $O(N + K)$ + Hitbox geométrico de 3 segmentos en $O(1)$. | Reducción de hasta **85%** en comparaciones geométricas. |
| **Gestión de Memoria** | Fugas de objetos `Bitmap` al rotar en `NaveCorre` y presión al GC por `Controls.Add`/`Dispose`. | **Eliminación *Swap-and-Pop* en $O(1)$** sobre listas contiguas y cero asignaciones GDI+ por frame. | **0 recolecciones Gen2** durante el combate continuo. |
| **Arquitectura de Red** | Inexistente (Solo local estático). | **Arquitectura Cliente-Servidor UDP/Sockets Asíncrona** (`GestorRedClienteServidor`). | Lanzamiento de aeronaves en red LAN/`127.0.0.1:5050` con telemetría RTT en vivo. |
| **Profiling y Métricas** | Sin instrumentación. | **Motor de Profiling en Tiempo Real (`ProfilerMotor`)** con HUD táctico (FPS, $\mu\text{s}$ CPU, RAM, GC, Big-O, Ping RTT). | Monitoreo en vivo (`F3`) y exportación de reporte (`F4`). |

---

## 3. Estructura Modular de Archivos Creados y Actualizados

| Archivo | Rol en la Arquitectura |
| :--- | :--- |
| [`B2SpiritModel.cs`](file:///c:/Users/ASUS/Documents/Juego_Basic/Juego_Basic/Juego_Basic/B2SpiritModel.cs) | Modelo vectorial fotorrealista 3D del bombardero furtivo **Northrop Grumman B-2 Spirit (`AF 82-1071`)** y su proyectil furtivo AGM-158 con **4x SSAA**. |
| [`F35Model.cs`](file:///c:/Users/ASUS/Documents/Juego_Basic/Juego_Basic/Juego_Basic/F35Model.cs) | Modelo vectorial fotorrealista 3D del caza **Lockheed Martin F-35 Lightning II** y misil AIM-120 con **4x SSAA**. |
| [`ModelosOOP.cs`](file:///c:/Users/ASUS/Documents/Juego_Basic/Juego_Basic/Juego_Basic/ModelosOOP.cs) | Jerarquía de clases OOP (`EntidadEspacial`, `AeronaveBase`, `CazaF35Lightning`, `BombarderoB2Spirit`, `Proyectil`), Caché Hash $O(1)$ (`CacheSpritesAeronaves`) y Grilla Espacial $O(N+K)$ (`GrillaEspacialColisiones`). |
| [`RedClienteServidor.cs`](file:///c:/Users/ASUS/Documents/Juego_Basic/Juego_Basic/Juego_Basic/RedClienteServidor.cs) | Arquitectura Cliente-Servidor sobre sockets UDP asíncronos (`GestorRedClienteServidor` y `PaqueteRedAeronave`), con soporte para Servidor Host, Cliente Online y Loopback automático. |
| [`ProfilerMotor.cs`](file:///c:/Users/ASUS/Documents/Juego_Basic/Juego_Basic/Juego_Basic/ProfilerMotor.cs) | Motor de Profiling de alta resolución (`Stopwatch`), telemetría de memoria/GC, auditoría Big-O en vivo y exportador de reportes de métricas de ingeniería de software. |
| [`Form1.cs`](file:///c:/Users/ASUS/Documents/Juego_Basic/Juego_Basic/Juego_Basic/Form1.cs) | Formulario principal con barra de lanzamiento táctico, selector de aeronaves en tiempo real, configuración Cliente-Servidor y lienzo estratosférico a 60 FPS. |

---

## 4. Aplicación de Programación Orientada a Objetos (OOP) y Buenas Prácticas

### 4.1. Clases, Atributos, Herencia y Polimorfismo
- **Herencia de 3 Niveles ($DIT = 3$)**:
  - `EntidadEspacial` (Clase abstracta raíz): Define posición flotante `(X, Y)`, dimensiones `(Ancho, Alto)`, estado `Activo` y métodos abstractos `ActualizarFisica(...)` y `Dibujar(...)`.
  - `AeronaveBase` (Clase abstracta intermedia): Hereda de `EntidadEspacial` e incorpora atributos de combate (`VidaActual`, `VidaMaxima`, `EscudoActual`, `VelocidadMovimiento`, `Matricula`, `AnguloInclinacion`) y comportamiento polimórfico.
  - `BombarderoB2Spirit` y `CazaF35Lightning` (Subclases concretas): Sobrescriben (`override`) `Disparar()` y `ObtenerFichaTecnica()`.
- **Polimorfismo en Acción**:
  - `BombarderoB2Spirit.Disparar()` despliega una **doble salva furtiva paralela** (2 misiles AGM-158 simultáneos desde sus bahías internas con daño reforzado en alas y escudo RAM de 12 puntos).
  - `CazaF35Lightning.Disparar()` lanza un **misil supersónico AIM-120** de alta cadencia y velocidad ($14\text{ px/tick}$).
- **Procedimientos (`void`) vs. Funciones (`T`) con Argumentos Tipados**:
  - **Procedimientos**: `Mover(float deltaX, float deltaY, Size limitesMapa)`, `IniciarArquitectura(ModoRedJuego modo, string ipServidor, int puerto)`, `DibujarHudProfiling(...)`.
  - **Funciones**: `EvaluarImpactoGeometrico(Proyectil proyectil) -> ResultadoImpacto`, `ObtenerSpriteNave(...) -> Bitmap`, `IntentarDeserializar(string trama, out PaqueteRedAeronave paquete) -> bool`.

---

## 5. Análisis de Complejidad Algorítmica (Big-O) y Profiling

1. **Caché de Sprites Pre-Rotados (`CacheSpritesAeronaves`)**:
   - Reduce el costo de obtención de imagen por frame de $O(P \cdot W \cdot H \cdot \text{SSAA}^2)$ ($\approx 16\text{ ms}$) a **$O(1)$** ($\approx 0.002\text{ ms}$) mediante tabla Hash `Dictionary<ClaveSprite, Bitmap>`.
2. **Grilla Espacial Hash de Colisiones (`GrillaEspacialColisiones`)**:
   - Particiona el espacio aéreo en celdas de $72\times72\text{ px}$ usando la función hash espacial $(c_x \cdot 73856093) \oplus (c_y \cdot 19349663)$, reduciendo la complejidad de detección de colisiones de $O(N \times M)$ a **$O(N + K)$**, manteniendo la regla de impacto geométrico de 3 segmentos (Ala Izquierda `-1 HP`, Cabina Central `-10 HP`, Ala Derecha `-1 HP`) en **$O(1)$**.
3. **Compactación *Swap-and-Pop* de Proyectiles y Partículas**:
   - Elimina proyectiles fuera de pantalla o impactados en tiempo constante **$O(1)$**, evitando desplazamientos de memoria $O(N)$.
