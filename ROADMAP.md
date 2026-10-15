# 🗺️ Hoja de Ruta de Desarrollo (Roadmap)
# Cóndor Táctico: Tactical Air Combat Simulator

Este documento traza las fases de evolución estratégica, características planificadas y objetivos de ingeniería para las próximas iteraciones de **Cóndor Táctico**.

---

## 🎯 Estado Actual: Versión 3.0 (Completada ✅)
- [x] Reingeniería de arquitectura monolítica a capas desacopladas (`Core`, `Entities`, `Systems`, `Graphics`, `Networking`, `Diagnostics`, `UI`).
- [x] Modelado vectorial 3D fotorrealista del **Northrop Grumman B-2 Spirit** con doble salva AGM-158.
- [x] Particionamiento espacial en grilla hash $\mathcal{O}(N+K)$ con cero asignaciones GC.
- [x] Motor de red UDP asíncrono con modos Host, Cliente y telemetría de ping RTT.
- [x] Subsistema de telemetría y profiling en tiempo real con exportador ISO/IEC 25010.

---

## 🚀 Fase 1: Subsistema de Audio y Radar Táctico (v3.1 - Corto Plazo)
- [ ] **Motor de Audio Espacial Estéreo:**
  - Integración de biblioteca de audio de baja latencia (`NAudio` o bindings de OpenAL-Soft).
  - Efectos sonoros de combate: Rugido de postcombustión, detonación supersónica (*Sonic Boom*), alarma de bloqueo de radar enemiga (*Radar Missile Lock Warning*), impacto en fuselaje.
- [ ] **Radar Táctico Circular (Minimap):**
  - Pantalla de barrido en esquina inferior izquierda que mapea ecos de aeronaves enemigas y proyectiles en curso.
- [ ] **Contramedidas Térmicas (Chaff & Flares):**
  - Tecla `X` para lanzar bengalas térmicas que desvían proyectiles en trayectoria de aproximación.

---

## 🛰️ Fase 2: Expansión de Arsenal y Perfiles de Vuelo (v3.5 - Medio Plazo)
- [ ] **Nuevas Aeronaves Concretas:**
  - **Sukhoi Su-57 Felon:** Caza de supermaniobrabilidad con vectorización de empuje (maniobra de Cobra de Pugachev).
  - **Eurofighter Typhoon:** Caza delta con misiles de largo alcance Meteor.
  - **Lockheed SR-71 Blackbird:** Avión espía de reconocimiento supersónico ($\text{Mach } 3+$) disponible como soporte de escolta táctico.
- [ ] **Soporte para Controles Físicos (HOTAS & Gamepad):**
  - Integración de XInput / DirectInput para palancas de mando y gamepads de Xbox/PlayStation con respuesta háptica.

---

## 🌐 Fase 3: Multijugador Avanzado y Servidor Dedicado (v4.0 - Largo Plazo)
- [ ] **Servidor Dedicado con Matchmaking:**
  - Servidor central ligero en C# para registro de salas mediante códigos de invitación (*Room Codes*).
  - Técnicas de perforación de NAT (*UDP Hole Punching*) y compensación de retardo (*Lag Compensation / Dead Reckoning*).
- [ ] **Modos de Juego Adicionales:**
  - *Dogfight por Equipos (2 vs 2)*.
  - *Defensa de Base Estratégica*: Proteger un B-2 Spirit frente a oleadas de cazas interceptores.

---

## 🌟 Fase 4: Portabilidad Multiplataforma (v5.0 - Visión de Futuro)
- [ ] **Abstracción del Pipeline de Renderizado:**
  - Migración del backend de renderizado de GDI+ a una capa multiplataforma de aceleración por GPU (mediante **Veldrid**, **Silk.NET** o **Vulkan**).
  - Compilación nativa para **Linux** y **macOS** preservando la arquitectura lógica y de red intacta.
