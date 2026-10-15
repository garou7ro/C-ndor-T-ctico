// ==============================================================================
// PROYECTO: CÓNDOR TÁCTICO (SIMULADOR DE COMBATE AÉREO 2D)
// AUTORES:
//   - RONAL ABELIN LARICO CCASO
//   - CLEVER NANDOR MAMANI HUALLPA
//   - JHON YOSSEP HINOJOSA APAZA
//   - JHON ALEX HUARACALLO SUCARI
//   - HECTOR CARLOS TORRES FLORES
// INSTITUCION: UNIVERSIDAD NACIONAL DE JULIACA (UNAJ)
// VERSION: 3.3.0
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CondorTactico
{
    /// <summary>Instantánea inmutable del progreso de carga reportada a la UI.</summary>
    public readonly struct ProgresoCarga
    {
        public ProgresoCarga(double porcentaje, int indiceEtapa, int totalEtapas, string etapa, string detalle, bool etapaCompletada)
        {
            Porcentaje = porcentaje;
            IndiceEtapa = indiceEtapa;
            TotalEtapas = totalEtapas;
            Etapa = etapa;
            Detalle = detalle;
            EtapaCompletada = etapaCompletada;
        }

        /// <summary>Progreso global ponderado entre 0.0 y 1.0.</summary>
        public double Porcentaje { get; }
        public int IndiceEtapa { get; }
        public int TotalEtapas { get; }
        public string Etapa { get; }
        public string Detalle { get; }
        public bool EtapaCompletada { get; }
    }

    /// <summary>Resultado final del arranque, consumido por la ventana principal.</summary>
    public sealed class ResultadoCarga
    {
        public Size TamanoVentanaPrincipal { get; init; }
        public bool PuertoUdpDisponible { get; init; }
        public TimeSpan Duracion { get; init; }
    }

    /// <summary>
    /// Orquestador del arranque del motor. Ejecuta etapas ponderadas en un hilo de fondo
    /// para que la pantalla de carga se mantenga a 60 FPS mientras se prepara todo lo necesario:
    /// sprites 4x SSAA, paleta GDI+, fondo, precalentamiento JIT, red y limpieza del GC.
    /// </summary>
    public sealed class CargadorJuego
    {
        private sealed record EtapaCarga(string Nombre, double Peso, Action<Action<double, string>> Ejecutar);

        private readonly List<EtapaCarga> _etapas = new List<EtapaCarga>();
        private Size _tamanoVentana;
        private bool _puertoUdpDisponible;

        public CargadorJuego()
        {
            _etapas.Add(new EtapaCarga("Inicializando núcleo del motor", 4, EtapaNucleo));
            _etapas.Add(new EtapaCarga("Rasterizando flota vectorial 4x SSAA", 58, EtapaSprites));
            _etapas.Add(new EtapaCarga("Cargando tipografías y paleta táctica", 6, EtapaPaleta));
            _etapas.Add(new EtapaCarga("Pre-renderizando cielo estratosférico", 6, EtapaFondo));
            _etapas.Add(new EtapaCarga("Precalentando física, colisiones y render (JIT)", 14, EtapaPrecalentamiento));
            _etapas.Add(new EtapaCarga("Verificando enlace de red UDP", 6, EtapaRed));
            _etapas.Add(new EtapaCarga("Optimizando memoria y recolector de basura", 6, EtapaMemoria));
        }

        public int TotalEtapas => _etapas.Count;

        /// <summary>Ejecuta todas las etapas en segundo plano reportando progreso ponderado.</summary>
        public Task<ResultadoCarga> EjecutarAsync(IProgress<ProgresoCarga> progreso, CancellationToken ct)
        {
            return Task.Run(() =>
            {
                var reloj = Stopwatch.StartNew();
                double pesoTotal = 0;
                foreach (var e in _etapas) pesoTotal += e.Peso;

                double pesoAcumulado = 0;
                for (int i = 0; i < _etapas.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    EtapaCarga etapa = _etapas[i];
                    int indice = i;
                    double baseEtapa = pesoAcumulado;

                    progreso.Report(new ProgresoCarga(baseEtapa / pesoTotal, indice, _etapas.Count, etapa.Nombre, "Iniciando...", false));

                    etapa.Ejecutar((fraccion, detalle) =>
                    {
                        ct.ThrowIfCancellationRequested();
                        double global = (baseEtapa + etapa.Peso * Math.Clamp(fraccion, 0, 1)) / pesoTotal;
                        progreso.Report(new ProgresoCarga(global, indice, _etapas.Count, etapa.Nombre, detalle, false));
                    });

                    pesoAcumulado += etapa.Peso;
                    progreso.Report(new ProgresoCarga(pesoAcumulado / pesoTotal, indice, _etapas.Count, etapa.Nombre, "Completado", true));
                }

                return new ResultadoCarga
                {
                    TamanoVentanaPrincipal = _tamanoVentana,
                    PuertoUdpDisponible = _puertoUdpDisponible,
                    Duracion = reloj.Elapsed
                };
            }, ct);
        }

        // ---------------------------------------------------------------------
        // ETAPAS
        // ---------------------------------------------------------------------
        private void EtapaNucleo(Action<double, string> reportar)
        {
            reportar(0.2, "Configurando latencia del GC (SustainedLowLatency)");
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            reportar(0.5, "Arrancando gestor de streaming asíncrono");
            _ = GestorRecursosAsincrono.Instancia.TotalRecursosEnCache;

            reportar(0.8, "Calculando resolución óptima de despliegue");
            _tamanoVentana = ConfiguracionPantalla.CalcularTamanoVentanaPrincipal();
            reportar(1.0, $"Resolución de combate: {_tamanoVentana.Width}x{_tamanoVentana.Height}");
        }

        private static void EtapaSprites(Action<double, string> reportar)
        {
            CacheSpritesAeronaves.InicializarCache((hechos, total, descripcion) =>
                reportar((double)hechos / total, $"[{hechos:00}/{total:00}] {descripcion}"));
        }

        private static void EtapaPaleta(Action<double, string> reportar)
        {
            reportar(0.3, "Fuentes Segoe UI / Consolas");
            RecursosGraficosHud.Inicializar();
            reportar(1.0, "Brochas y plumas reutilizables listas (0 asignaciones por frame)");
        }

        private void EtapaFondo(Action<double, string> reportar)
        {
            reportar(0.2, "Gradiente de alta atmósfera");
            // El lienzo ocupa la ventana menos la barra superior (44px) y la barra de estado (~26px)
            var tamanoLienzo = new Size(
                Math.Max(1, _tamanoVentana.Width - 16),
                Math.Max(1, _tamanoVentana.Height - 44 - 26 - 39));
            RecursosGraficosHud.PrecalentarFondo(tamanoLienzo);
            reportar(1.0, $"Cielo {tamanoLienzo.Width}x{tamanoLienzo.Height} en caché");
        }

        private static void EtapaPrecalentamiento(Action<double, string> reportar)
        {
            var limites = new Size(960, 640);
            var naves = new List<AeronaveBase>
            {
                new CazaF35Lightning("JIT-A", "JIT", false, 400, 500),
                new BombarderoB2Spirit("JIT-B", "JIT", true, 300, 40),
                new CazaF35Lightning("JIT-C", "JIT", true, 600, 60),
                new BombarderoB2Spirit("JIT-D", "JIT", false, 200, 480)
            };
            var proyectiles = new List<Proyectil>(64);
            var grilla = new GrillaEspacialColisiones(72);

            const int ciclos = 90;
            using var lienzo = new Bitmap(limites.Width, limites.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using Graphics g = Graphics.FromImage(lienzo);

            for (int i = 0; i < ciclos; i++)
            {
                foreach (AeronaveBase nave in naves)
                {
                    nave.Mover((i % 3) - 1, (i % 2 == 0) ? -0.5f : 0.5f, limites);
                    nave.ActualizarFisica(limites);
                    if (i % 10 == 0) proyectiles.AddRange(nave.Disparar());
                }

                for (int p = proyectiles.Count - 1; p >= 0; p--)
                {
                    proyectiles[p].ActualizarFisica(limites);
                    if (!proyectiles[p].Activo) proyectiles.RemoveAt(p);
                }

                grilla.ProcesarColisiones(proyectiles, naves, (_, _, _) => { });

                if (i % 15 == 0)
                {
                    foreach (AeronaveBase nave in naves) nave.Dibujar(g);
                    foreach (Proyectil pr in proyectiles) pr.Dibujar(g);
                }

                if (i % 9 == 0) reportar((double)(i + 1) / ciclos, $"Simulando ciclo {i + 1}/{ciclos} · {proyectiles.Count} proyectiles");
            }
            reportar(1.0, "Rutas calientes compiladas por el JIT");
        }

        private void EtapaRed(Action<double, string> reportar)
        {
            const int puerto = 15050;
            reportar(0.3, $"Sondeando puerto UDP :{puerto}");
            try
            {
                using var sonda = new UdpClient(new IPEndPoint(IPAddress.Any, puerto));
                _puertoUdpDisponible = true;
                reportar(1.0, $"Puerto :{puerto} libre · modo Servidor Host disponible");
            }
            catch (SocketException)
            {
                _puertoUdpDisponible = false;
                reportar(1.0, $"Puerto :{puerto} ocupado · se unirá como Cliente");
            }
        }

        private static void EtapaMemoria(Action<double, string> reportar)
        {
            reportar(0.3, "Compactando heap gestionado");
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            reportar(1.0, $"Heap: {GC.GetTotalMemory(false) / (1024.0 * 1024.0):F1} MB · recursos en caché: {GestorRecursosAsincrono.Instancia.TotalRecursosEnCache}");
        }
    }

    /// <summary>Utilidades de resolución y escalado de ventanas según el monitor del usuario.</summary>
    public static class ConfiguracionPantalla
    {
        public static readonly Size TamanoMinimoVentana = new Size(980, 700);
        public static readonly Size TamanoPreferidoVentana = new Size(1280, 860);

        /// <summary>Calcula el tamaño ideal de la ventana de combate (≤ 92% del área de trabajo).</summary>
        public static Size CalcularTamanoVentanaPrincipal()
        {
            Rectangle area = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1366, 728);
            int ancho = Math.Min(TamanoPreferidoVentana.Width, (int)(area.Width * 0.92));
            int alto = Math.Min(TamanoPreferidoVentana.Height, (int)(area.Height * 0.92));
            return new Size(Math.Max(TamanoMinimoVentana.Width, ancho), Math.Max(TamanoMinimoVentana.Height, alto));
        }

        /// <summary>Tamaño de la pantalla de carga en 16:9, adaptado al monitor.</summary>
        public static Size CalcularTamanoPantallaCarga()
        {
            Rectangle area = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1366, 728);
            int ancho = Math.Min(1120, (int)(area.Width * 0.75));
            ancho = Math.Max(800, ancho);
            return new Size(ancho, ancho * 9 / 16);
        }
    }
}
