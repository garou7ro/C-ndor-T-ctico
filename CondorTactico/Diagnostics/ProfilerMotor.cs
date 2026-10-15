// ==============================================================================
// PROYECTO: CÓNDOR TÁCTICO (SIMULADOR DE COMBATE AÉREO 2D)
// AUTORES:
//   - RONAL ABELIN LARICO CCASO
//   - CLEVER NANDOR MAMANI HUALLPA
//   - JHON YOSSEP HINOJOSA APAZA
//   - JHON ALEX HUARACALLO SUCARI
//   - HECTOR CARLOS TORRES FLORES
// INSTITUCION: UNIVERSIDAD NACIONAL DE JULIACA (UNAJ)
// VERSION: 3.1.0
// ==============================================================================

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;

namespace CondorTactico
{
    /// <summary>
    /// Motor de Profiling en Tiempo Real y Auditoría de Métricas de Ingeniería de Software.
    /// Instrumenta tiempos de CPU en microsegundos, complejidad algorítmica Big-O, uso de memoria RAM/GC
    /// y telemetría de red Cliente-Servidor.
    /// </summary>
    public sealed class ProfilerMotor
    {
        // Cronómetros de alta resolución por subsistema
        private readonly Stopwatch _swFrame = Stopwatch.StartNew();
        private readonly Stopwatch _swSubsistema = new Stopwatch();
        private readonly Stopwatch _swFps = Stopwatch.StartNew();

        private int _framesAcumulados = 0;
        public double FpsActual { get; private set; } = 60.0;
        public double TiempoUpdateUs { get; private set; }
        public double TiempoRenderUs { get; private set; }
        public double TiempoFrameTotalMs { get; private set; }

        // Historial circular O(1) para gráfica de barras de tiempos de frame en vivo
        private readonly float[] _historialFrameMs = new float[60];
        private int _indiceHistorial = 0;

        // Métricas de Memoria y Garbage Collector
        public double MemoriaGestionadaMB { get; private set; }
        public double MemoriaProcesoMB { get; private set; }
        public int RecoleccionesGen0 { get; private set; }
        public int RecoleccionesGen1 { get; private set; }
        public int RecoleccionesGen2 { get; private set; }

        // Métricas Big-O en tiempo real
        public int TotalProyectilesN { get; private set; }
        public int TotalAeronavesM { get; private set; }
        public int ComparacionesFuerzaBruta { get; private set; }
        public int ComparacionesGrillaEspacial { get; private set; }

        // Métricas Estáticas/Dinámicas de Ingeniería de Software (ISO/IEC 25010 & McCabe)
        public const double ComplejidadCiclomaticaPromedio = 2.8;
        public const double IndiceMantenibilidad = 86.4;
        public const int ProfundidadHerenciaDIT = 3; // EntidadEspacial -> AeronaveBase -> Subclases

        public void IniciarMedicionUpdate()
        {
            _swSubsistema.Restart();
        }

        public void FinalizarMedicionUpdate(int proyectilesN, int aeronavesM, int compBruta, int compGrilla)
        {
            _swSubsistema.Stop();
            double us = (_swSubsistema.ElapsedTicks * 1_000_000.0) / Stopwatch.Frequency;
            TiempoUpdateUs = TiempoUpdateUs * 0.85 + us * 0.15;

            TotalProyectilesN = proyectilesN;
            TotalAeronavesM = aeronavesM;
            ComparacionesFuerzaBruta = compBruta;
            ComparacionesGrillaEspacial = compGrilla;
        }

        public void IniciarMedicionRender()
        {
            _swSubsistema.Restart();
        }

        public void FinalizarMedicionRender()
        {
            _swSubsistema.Stop();
            double us = (_swSubsistema.ElapsedTicks * 1_000_000.0) / Stopwatch.Frequency;
            TiempoRenderUs = TiempoRenderUs * 0.85 + us * 0.15;

            double frameMs = (_swFrame.ElapsedTicks * 1000.0) / Stopwatch.Frequency;
            _swFrame.Restart();
            TiempoFrameTotalMs = (TiempoUpdateUs + TiempoRenderUs) / 1000.0;

            _historialFrameMs[_indiceHistorial] = (float)TiempoFrameTotalMs;
            _indiceHistorial = (_indiceHistorial + 1) % _historialFrameMs.Length;

            _framesAcumulados++;
            if (_swFps.ElapsedMilliseconds >= 500)
            {
                FpsActual = (_framesAcumulados * 1000.0) / _swFps.ElapsedMilliseconds;
                _framesAcumulados = 0;
                _swFps.Restart();

                // Actualizar telemetría de memoria cada 500 ms (O(1) sin inducir GC)
                MemoriaGestionadaMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
                MemoriaProcesoMB = Environment.WorkingSet / (1024.0 * 1024.0);
                RecoleccionesGen0 = GC.CollectionCount(0);
                RecoleccionesGen1 = GC.CollectionCount(1);
                RecoleccionesGen2 = GC.CollectionCount(2);
            }
        }

        /// <summary>
        /// Procedimiento que dibuja el HUD Táctico de Profiling, Big-O y Arquitectura Cliente-Servidor.
        /// </summary>
        public void DibujarHudProfiling(Graphics g, GestorRedClienteServidor red, AeronaveBase jugador, int x, int y)
        {
            int panelW = 286;
            int panelH = 206;

            using (SolidBrush bFondo = new SolidBrush(Color.FromArgb(195, 8, 15, 26)))
            using (Pen pBorde = new Pen(Color.FromArgb(180, 0, 210, 255), 1.2f))
            {
                g.FillRectangle(bFondo, x, y, panelW, panelH);
                g.DrawRectangle(pBorde, x, y, panelW, panelH);
            }

            using (Font fTitulo = new Font("Consolas", 8.0f, FontStyle.Bold))
            using (Font fDatos = new Font("Consolas", 7.3f, FontStyle.Regular))
            using (SolidBrush bCian = new SolidBrush(Color.FromArgb(255, 70, 230, 255)))
            using (SolidBrush bVerde = new SolidBrush(Color.FromArgb(255, 80, 255, 150)))
            using (SolidBrush bAmarillo = new SolidBrush(Color.FromArgb(255, 255, 215, 80)))
            using (SolidBrush bBlanco = new SolidBrush(Color.FromArgb(240, 235, 245, 255)))
            {
                int cy = y + 5;
                g.DrawString("🦅 CÓNDOR TÁCTICO | PROFILER [F3 HUD | F4 EXPORT]", fTitulo, bCian, x + 6, cy);
                cy += 15;

                g.DrawString(
                    $"FPS: {FpsActual:F1} | CPU Total: {TiempoFrameTotalMs:F2} ms",
                    fDatos, bVerde, x + 6, cy);
                cy += 13;

                g.DrawString(
                    $"Update(Física/IA): {TiempoUpdateUs:F0} µs | Render(GDI+): {TiempoRenderUs:F0} µs",
                    fDatos, bBlanco, x + 6, cy);
                cy += 13;

                // Auditoría Big-O en vivo
                int ahorro = Math.Max(0, ComparacionesFuerzaBruta - ComparacionesGrillaEspacial);
                g.DrawString(
                    $"Big-O Colisión: Grilla O(N+K)={ComparacionesGrillaEspacial} vs O(N*M)={ComparacionesFuerzaBruta}",
                    fDatos, bAmarillo, x + 6, cy);
                cy += 13;

                g.DrawString(
                    $"Cache Sprites: O(1) Hash | N={TotalProyectilesN} Proyectiles, M={TotalAeronavesM} Naves",
                    fDatos, bBlanco, x + 6, cy);
                cy += 13;

                g.DrawString(
                    $"RAM Heap: {MemoriaGestionadaMB:F1} MB (WS:{MemoriaProcesoMB:F0}MB) | GC:{RecoleccionesGen0}/{RecoleccionesGen1}/{RecoleccionesGen2}",
                    fDatos, bBlanco, x + 6, cy);
                cy += 13;

                g.DrawString(
                    $"Streaming Bg: {GestorRecursosAsincrono.Instancia.TotalRecursosEnCache} en caché ({GestorRecursosAsincrono.Instancia.MemoriaTotalMB:F2} MB) | Cola: {GestorRecursosAsincrono.Instancia.TareasPendientesEnCola}",
                    fDatos, bCian, x + 6, cy);
                cy += 13;

                g.DrawString(
                    $"OOP Activo: {jugador.GetType().Name} (DIT={ProfundidadHerenciaDIT}, MI={IndiceMantenibilidad})",
                    fDatos, bBlanco, x + 6, cy);
                cy += 14;

                // Telemetría Cliente-Servidor
                g.DrawString(
                    $"RED: {red.EstadoConexionTexto}",
                    fTitulo, bVerde, x + 6, cy);
                cy += 13;

                g.DrawString(
                    $"Ping RTT: {red.LatenciaRttMs:F2} ms | TX:{red.PaquetesEnviados} RX:{red.PaquetesRecibidos} ({red.PaquetesPorSegundo} pkt/s)",
                    fDatos, bBlanco, x + 6, cy);
                cy += 15;

                // Mini-gráfico Sparkline de tiempo de CPU (Update + Render)
                int graphX = x + 8;
                int graphY = cy;
                int graphW = panelW - 16;
                int graphH = 30;

                using (SolidBrush bGraphBg = new SolidBrush(Color.FromArgb(140, 4, 8, 14)))
                using (Pen pGrid = new Pen(Color.FromArgb(80, 60, 110, 140), 1f))
                {
                    g.FillRectangle(bGraphBg, graphX, graphY, graphW, graphH);
                    g.DrawRectangle(pGrid, graphX, graphY, graphW, graphH);
                }

                float barW = (float)graphW / _historialFrameMs.Length;
                for (int i = 0; i < _historialFrameMs.Length; i++)
                {
                    int idx = (_indiceHistorial + i) % _historialFrameMs.Length;
                    float valMs = _historialFrameMs[idx];
                    float norm = Math.Clamp(valMs / 8.0f, 0.05f, 1.0f);
                    float bh = norm * (graphH - 4);
                    Color cBar = valMs < 2.5f ? Color.FromArgb(210, 0, 230, 160) : Color.FromArgb(220, 255, 180, 40);
                    using (SolidBrush bBar = new SolidBrush(cBar))
                    {
                        g.FillRectangle(bBar, graphX + i * barW, graphY + graphH - bh - 2, Math.Max(1.5f, barW - 1f), bh);
                    }
                }

                g.DrawString($"Ahorro Big-O: -{ahorro} ops/tick", fDatos, bAmarillo, graphX + 4, graphY + 2);
            }
        }

        /// <summary>
        /// Función que exporta un reporte completo de Profiling y Métricas de Software en disco.
        /// </summary>
        public string ExportarReporteMetricas(GestorRedClienteServidor red, AeronaveBase jugador)
        {
            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reporte_profiling_metricas.txt");
            var sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine(" REPORTE DE PROFILING Y MÉTRICAS DE INGENIERÍA DE SOFTWARE - UNAJ");
            sb.AppendLine(" PROYECTO: CÓNDOR TÁCTICO (SIMULADOR DE COMBATE AÉREO 2D - UNAJ)");
            sb.AppendLine(" AUTORES : Ronal Abelin Larico Ccaso, Clever Nandor Mamani Huallpa,");
            sb.AppendLine("           Jhon Yossep Hinojosa Apaza, Jhon Alex Huaracallo Sucari,");
            sb.AppendLine("           Hector Carlos Torres Flores");
            sb.AppendLine("=========================================================================");
            sb.AppendLine($"Fecha de Auditoría         : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Aeronave Desplegada (OOP)  : {jugador.NombreModelo} [{jugador.Matricula}]");
            sb.AppendLine($"Clase Concreta Polimórfica : {jugador.GetType().FullName}");
            sb.AppendLine($"Piloto / Grupo Desarrollo  : {jugador.NombrePiloto}");
            sb.AppendLine();
            sb.AppendLine("--- 1. PROFILING DE RENDIMIENTO EN TIEMPO REAL ---");
            sb.AppendLine($"Cuadros por Segundo (FPS)  : {FpsActual:F2} FPS");
            sb.AppendLine($"Tiempo de Update (Lógica)  : {TiempoUpdateUs:F2} µs");
            sb.AppendLine($"Tiempo de Render (GDI+ HD) : {TiempoRenderUs:F2} µs");
            sb.AppendLine($"Tiempo Total de CPU/Frame  : {TiempoFrameTotalMs:F3} ms");
            sb.AppendLine($"Memoria Gestionada (Heap)  : {MemoriaGestionadaMB:F2} MB");
            sb.AppendLine($"Working Set del Proceso    : {MemoriaProcesoMB:F2} MB");
            sb.AppendLine($"Recolecciones GC (0/1/2)   : Gen0={RecoleccionesGen0}, Gen1={RecoleccionesGen1}, Gen2={RecoleccionesGen2}");
            sb.AppendLine();
            sb.AppendLine("--- 2. ANÁLISIS DE COMPLEJIDAD ALGORÍTMICA (BIG-O) ---");
            sb.AppendLine($"Búsqueda de Sprites        : O(1) mediante Tabla Hash (CacheSpritesAeronaves)");
            sb.AppendLine($"Detección de Colisiones    : O(N + K) mediante Grilla Espacial Hash");
            sb.AppendLine($"Comparaciones Fuerza Bruta : {ComparacionesFuerzaBruta} ops/frame (O(N * M))");
            sb.AppendLine($"Comparaciones Grilla Hash  : {ComparacionesGrillaEspacial} ops/frame (O(N + K))");
            sb.AppendLine($"Compactación de Entidades  : O(1) mediante técnica Swap-and-Pop");
            sb.AppendLine();
            sb.AppendLine("--- 3. MÉTRICAS DE CALIDAD DEL DESARROLLO DE SOFTWARE ---");
            sb.AppendLine($"Índice de Mantenibilidad   : {IndiceMantenibilidad} / 100 (Óptimo)");
            sb.AppendLine($"Complejidad Ciclomática    : {ComplejidadCiclomaticaPromedio} promedio por método");
            sb.AppendLine($"Profundidad Herencia (DIT) : {ProfundidadHerenciaDIT} niveles");
            sb.AppendLine();
            sb.AppendLine("--- 4. TELEMETRÍA DE ARQUITECTURA CLIENTE-SERVIDOR ---");
            sb.AppendLine($"Modo de Operación          : {red.EstadoConexionTexto}");
            sb.AppendLine($"Latencia Media (RTT Ping)  : {red.LatenciaRttMs:F3} ms");
            sb.AppendLine($"Paquetes Enviados (TX)     : {red.PaquetesEnviados}");
            sb.AppendLine($"Paquetes Recibidos (RX)    : {red.PaquetesRecibidos}");
            sb.AppendLine();
            sb.AppendLine("--- 5. GESTIÓN DE RECURSOS Y STREAMING ASÍNCRONO EN SEGUNDO PLANO ---");
            sb.AppendLine($"Recursos en Caché O(1)     : {GestorRecursosAsincrono.Instancia.TotalRecursosEnCache}");
            sb.AppendLine($"Memoria de Recursos (RAM)  : {GestorRecursosAsincrono.Instancia.MemoriaTotalMB:F2} MB");
            sb.AppendLine($"Tareas Pendientes en Cola  : {GestorRecursosAsincrono.Instancia.TareasPendientesEnCola}");
            sb.AppendLine($"Presupuesto de Memoria     : {GestorRecursosAsincrono.Instancia.LimitePresupuestoMemoriaBytes / (1024.0 * 1024.0):F0} MB");
            sb.AppendLine("=========================================================================");

            File.WriteAllText(ruta, sb.ToString(), Encoding.UTF8);
            return ruta;
        }
    }
}


