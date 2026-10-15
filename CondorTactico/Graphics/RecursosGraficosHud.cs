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
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CondorTactico
{
    /// <summary>
    /// Paleta GDI+ precargada (fuentes, brochas, plumas y fondo) para el bucle de render.
    /// Se inicializa una sola vez en la pantalla de carga y elimina toda asignación GDI+ por frame.
    /// IMPORTANTE: solo debe usarse desde el hilo de UI una vez inicializada.
    /// </summary>
    public static class RecursosGraficosHud
    {
        private static volatile bool _inicializado;

        public static Font FuenteImpacto { get; private set; } = null!;
        public static Font FuenteFichaTitulo { get; private set; } = null!;
        public static Font FuenteFichaTexto { get; private set; } = null!;
        public static Font FuenteBannerTitulo { get; private set; } = null!;
        public static Font FuenteBannerSub { get; private set; } = null!;

        public static SolidBrush BrochaSombra { get; private set; } = null!;
        public static SolidBrush BrochaCian { get; private set; } = null!;
        public static SolidBrush BrochaBlanco { get; private set; } = null!;
        public static SolidBrush BrochaNube { get; private set; } = null!;
        public static SolidBrush BrochaFichaFondo { get; private set; } = null!;
        public static SolidBrush BrochaBannerFondo { get; private set; } = null!;

        /// <summary>Brocha mutable reutilizable (se le cambia el Color) para partículas y textos dinámicos.</summary>
        public static SolidBrush BrochaDinamica { get; private set; } = null!;

        public static Pen PlumaRadar { get; private set; } = null!;
        public static Pen PlumaFichaBorde { get; private set; } = null!;
        public static Pen PlumaBannerVictoria { get; private set; } = null!;
        public static Pen PlumaBannerDerrota { get; private set; } = null!;

        private static Bitmap? _fondoCielo;
        private static Size _tamanoFondo;

        public static bool EstaInicializado => _inicializado;

        /// <summary>Crea todos los objetos GDI+ reutilizables del HUD. Idempotente.</summary>
        public static void Inicializar()
        {
            if (_inicializado) return;

            FuenteImpacto = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            FuenteFichaTitulo = new Font("Consolas", 8.2f, FontStyle.Bold);
            FuenteFichaTexto = new Font("Consolas", 7.5f, FontStyle.Regular);
            FuenteBannerTitulo = new Font("Segoe UI", 18f, FontStyle.Bold);
            FuenteBannerSub = new Font("Consolas", 9.5f, FontStyle.Bold);

            BrochaSombra = new SolidBrush(Color.FromArgb(190, 0, 0, 0));
            BrochaCian = new SolidBrush(Color.FromArgb(255, 80, 230, 255));
            BrochaBlanco = new SolidBrush(Color.FromArgb(240, 235, 245, 255));
            BrochaNube = new SolidBrush(Color.FromArgb(28, 235, 245, 255));
            BrochaFichaFondo = new SolidBrush(Color.FromArgb(185, 10, 18, 30));
            BrochaBannerFondo = new SolidBrush(Color.FromArgb(225, 10, 16, 28));
            BrochaDinamica = new SolidBrush(Color.White);

            PlumaRadar = new Pen(Color.FromArgb(22, 140, 220, 255), 1f);
            PlumaFichaBorde = new Pen(Color.FromArgb(160, 60, 195, 255), 1f);
            PlumaBannerVictoria = new Pen(Color.DeepSkyBlue, 2.5f);
            PlumaBannerDerrota = new Pen(Color.Crimson, 2.5f);

            _inicializado = true;
        }

        /// <summary>
        /// Devuelve el cielo estratosférico pre-renderizado para el tamaño indicado.
        /// Solo se regenera al redimensionar la ventana (O(1) por frame en estado estable).
        /// </summary>
        public static Bitmap ObtenerFondoCielo(int ancho, int alto)
        {
            ancho = Math.Max(1, ancho);
            alto = Math.Max(1, alto);

            if (_fondoCielo != null && _tamanoFondo.Width == ancho && _tamanoFondo.Height == alto)
            {
                return _fondoCielo;
            }

            Bitmap nuevo = GenerarFondoCielo(ancho, alto);
            Bitmap? viejo = _fondoCielo;
            _fondoCielo = nuevo;
            _tamanoFondo = new Size(ancho, alto);
            viejo?.Dispose();
            return nuevo;
        }

        /// <summary>Genera el cielo con gradiente y horizonte luminoso (apto para hilos de fondo).</summary>
        public static Bitmap GenerarFondoCielo(int ancho, int alto)
        {
            var bmp = new Bitmap(ancho, alto, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            using (var gradiente = new LinearGradientBrush(
                new Rectangle(0, 0, ancho, alto),
                Color.FromArgb(255, 18, 48, 86),
                Color.FromArgb(255, 72, 136, 192),
                LinearGradientMode.Vertical))
            {
                var mezcla = new ColorBlend
                {
                    Colors = new[]
                    {
                        Color.FromArgb(255, 10, 28, 56),
                        Color.FromArgb(255, 22, 60, 104),
                        Color.FromArgb(255, 58, 118, 176),
                        Color.FromArgb(255, 92, 150, 200)
                    },
                    Positions = new[] { 0f, 0.35f, 0.8f, 1f }
                };
                gradiente.InterpolationColors = mezcla;
                g.FillRectangle(gradiente, 0, 0, ancho, alto);
            }
            return bmp;
        }

        /// <summary>Precarga el fondo para el tamaño esperado de la ventana principal.</summary>
        public static void PrecalentarFondo(Size tamanoEsperado)
        {
            Bitmap generado = GenerarFondoCielo(Math.Max(1, tamanoEsperado.Width), Math.Max(1, tamanoEsperado.Height));
            _fondoCielo?.Dispose();
            _fondoCielo = generado;
            _tamanoFondo = tamanoEsperado;
        }
    }
}
