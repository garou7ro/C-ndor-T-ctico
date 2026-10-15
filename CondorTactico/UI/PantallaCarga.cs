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
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading;
using System.Windows.Forms;

namespace CondorTactico.UI
{
    /// <summary>
    /// Pantalla de introducción y carga de Cóndor Táctico.
    /// Renderiza a ~60 FPS una escena andina nocturna con emblema del cóndor, HUD táctico y barra de progreso,
    /// mientras <see cref="CargadorJuego"/> prepara en segundo plano todos los recursos del motor.
    /// Arquitectura por capas: fondo estático y primer plano pre-renderizados + elementos animados ligeros.
    /// </summary>
    public sealed class PantallaCarga : Form
    {
        private const double DuracionFadeIn = 0.45;
        private const double DuracionFadeOut = 0.40;
        private const double EsperaAutoDespegue = 2.6;

        private static readonly string[] Consejos =
        {
            "El B-2 Spirit dispara doble salva AGM-158: ideal contra formaciones.",
            "Impactar la cabina causa -10 HP; las alas solo -1 HP.",
            "Pulsa F3 para ver la telemetría del motor en tiempo real.",
            "El F-35A es más ágil: esquiva primero y contraataca después.",
            "Pulsa 1 o 2 para cambiar de aeronave en pleno vuelo.",
            "Crea una partida multijugador desde el botón RED UDP."
        };

        private readonly CargadorJuego _cargador = new CargadorJuego();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly System.Windows.Forms.Timer _temporizador = new System.Windows.Forms.Timer { Interval = 15 };
        private readonly Stopwatch _reloj = new Stopwatch();
        private readonly Random _rng = new Random(1071);

        // Estado de carga
        private double _progresoReal;
        private double _progresoVisual;
        private string _etapa = "Preparando sistemas";
        private string _detalle = "Conectando subsistemas...";
        private int _indiceEtapa;
        private int _totalEtapas = 1;
        private double _inicioEtapaSeg;
        private readonly List<(string Texto, double Segundos)> _bitacora = new List<(string, double)>();
        private bool _cargaTerminada;
        private bool _listo;
        private double _instanteListo;
        private bool _cerrando;
        private double _instanteCierre;
        private double _ultimoSeg;

        public ResultadoCarga? Resultado { get; private set; }

        // Capas pre-renderizadas
        private Bitmap? _capaFondo;
        private Bitmap? _capaFrente;
        private Bitmap? _bmpEmblema;
        private Bitmap? _bmpTitulo;
        private GraphicsPath? _rutaTitulo;
        private PointF _origenTitulo;

        // Geometría (calculada en OnLoad)
        private int W, H;
        private PointF _centroEmblema;
        private float _tamEmblema;
        private RectangleF _barra;
        private float _ySubtitulo;

        // Elementos animados
        private struct Estrella { public float X, Y, Fase, Velocidad, Tamano; }
        private struct Nube { public float X, Y, Ancho, Alto, Velocidad; public int Alfa; }
        private struct Estela { public float X, Y, Vida, Radio; public Color Color; }
        private Estrella[] _estrellas = Array.Empty<Estrella>();
        private Nube[] _nubes = Array.Empty<Nube>();
        private readonly List<Estela> _estelas = new List<Estela>(256);

        // Recursos GDI+ propios de la pantalla (se liberan en Dispose)
        private readonly Font _fTitulo;
        private readonly Font _fSubtitulo;
        private readonly Font _fEtapa;
        private readonly Font _fPorcentaje;
        private readonly Font _fDetalle;
        private readonly Font _fMono;
        private readonly Font _fEsquina;
        private readonly SolidBrush _brochaDinamica = new SolidBrush(Color.White);
        private readonly Pen _plumaDinamica = new Pen(Color.White, 1f);
        private LinearGradientBrush? _brochaBarra;
        private LinearGradientBrush? _brochaBrillo;
        private LinearGradientBrush? _brochaBrilloTitulo;

        public PantallaCarga()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = ConfiguracionPantalla.CalcularTamanoPantallaCarga();
            BackColor = Color.FromArgb(3, 7, 15);
            ShowInTaskbar = true;
            Text = "Cóndor Táctico";
            KeyPreview = true;
            Opacity = 0;
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint | ControlStyles.Opaque, true);

            float escala = ClientSize.Height / 630f;
            _fTitulo = new Font("Segoe UI", 1f, FontStyle.Bold); // solo para FontFamily del GraphicsPath
            _fSubtitulo = new Font("Segoe UI Semibold", 10.5f * escala, FontStyle.Regular, GraphicsUnit.Point);
            _fEtapa = new Font("Segoe UI Semibold", 9.5f * escala, FontStyle.Regular);
            _fPorcentaje = new Font("Consolas", 20f * escala, FontStyle.Bold);
            _fDetalle = new Font("Consolas", 8.5f * escala, FontStyle.Regular);
            _fMono = new Font("Consolas", 8f * escala, FontStyle.Regular);
            _fEsquina = new Font("Consolas", 7.8f * escala, FontStyle.Bold);

            _temporizador.Tick += (_, _) => Actualizar();
        }

        private double Seg => _reloj.Elapsed.TotalSeconds;

        // =====================================================================
        // CICLO DE VIDA
        // =====================================================================
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            W = ClientSize.Width;
            H = ClientSize.Height;

            _tamEmblema = H * 0.30f;
            _centroEmblema = new PointF(W / 2f, H * 0.255f);
            _barra = new RectangleF(W * 0.2f, H * 0.785f, W * 0.6f, Math.Max(10f, H * 0.02f));

            GenerarElementosAnimados();
            _capaFondo = GenerarCapaFondo();
            _capaFrente = GenerarCapaFrente();
            _bmpEmblema = GenerarEmblema((int)_tamEmblema);
            GenerarTitulo();

            _brochaBarra = new LinearGradientBrush(_barra, Color.FromArgb(0, 200, 255), Color.FromArgb(255, 196, 70), LinearGradientMode.Horizontal);
            _brochaBrillo = CrearBrochaBrillo(140f, 150);
            _brochaBrilloTitulo = CrearBrochaBrillo(H * 0.22f, 120);

            _reloj.Start();
            _temporizador.Start();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            var progreso = new Progress<ProgresoCarga>(AlReportarProgreso);
            try
            {
                Resultado = await _cargador.EjecutarAsync(progreso, _cts.Token);
                _progresoReal = 1.0;
                _cargaTerminada = true;
            }
            catch (OperationCanceledException)
            {
                // Cierre solicitado por el usuario (ESC)
            }
            catch (Exception ex)
            {
                _temporizador.Stop();
                MessageBox.Show(this,
                    $"No se pudo completar la carga del motor:\n\n{ex.Message}",
                    "Cóndor Táctico · Error de arranque",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Abort;
                Close();
            }
        }

        private void AlReportarProgreso(ProgresoCarga p)
        {
            if (p.Detalle == "Iniciando...") _inicioEtapaSeg = Seg;

            _progresoReal = Math.Max(_progresoReal, p.Porcentaje);
            _etapa = p.Etapa;
            _detalle = p.Detalle;
            _indiceEtapa = p.IndiceEtapa;
            _totalEtapas = p.TotalEtapas;

            if (p.EtapaCompletada)
            {
                _bitacora.Add((p.Etapa, Seg - _inicioEtapaSeg));
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape && !_listo)
            {
                _cts.Cancel();
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
            SolicitarDespegue();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            SolicitarDespegue();
        }

        private void SolicitarDespegue()
        {
            if (_listo && !_cerrando && Seg - _instanteListo > 0.35)
            {
                _cerrando = true;
                _instanteCierre = Seg;
            }
        }

        // =====================================================================
        // ACTUALIZACIÓN POR FRAME
        // =====================================================================
        private void Actualizar()
        {
            double t = Seg;
            float dt = (float)Math.Min(0.05, t - _ultimoSeg);
            _ultimoSeg = t;

            // Suavizado exponencial independiente del framerate
            double factor = 1.0 - Math.Pow(0.0015, dt);
            _progresoVisual += (_progresoReal - _progresoVisual) * factor;
            if (_progresoReal >= 1.0 && _progresoVisual > 0.997) _progresoVisual = 1.0;

            if (_cargaTerminada && !_listo && _progresoVisual >= 1.0)
            {
                _listo = true;
                _instanteListo = t;
            }

            if (_listo && !_cerrando && t - _instanteListo > EsperaAutoDespegue)
            {
                _cerrando = true;
                _instanteCierre = t;
            }

            // Opacidad (fade-in / fade-out)
            if (_cerrando)
            {
                double f = (t - _instanteCierre) / DuracionFadeOut;
                if (f >= 1.0)
                {
                    _temporizador.Stop();
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }
                Opacity = 1.0 - f;
            }
            else if (Opacity < 1.0)
            {
                Opacity = Math.Min(1.0, t / DuracionFadeIn);
            }

            for (int i = 0; i < _nubes.Length; i++)
            {
                _nubes[i].X += _nubes[i].Velocidad * dt;
                if (_nubes[i].X > W + 40) _nubes[i].X = -_nubes[i].Ancho - 40;
            }

            ActualizarEstelas(dt);
            Invalidate();
        }

        private void ActualizarEstelas(float dt)
        {
            for (int i = _estelas.Count - 1; i >= 0; i--)
            {
                Estela e = _estelas[i];
                e.Vida -= dt * 1.6f;
                e.Radio += dt * 10f;
                if (e.Vida <= 0)
                {
                    int ult = _estelas.Count - 1;
                    _estelas[i] = _estelas[ult];
                    _estelas.RemoveAt(ult);
                }
                else
                {
                    _estelas[i] = e;
                }
            }
        }

        // =====================================================================
        // RENDER
        // =====================================================================
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (_capaFondo == null || _capaFrente == null) return;

            double t = Seg;

            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.SmoothingMode = SmoothingMode.None;
            g.DrawImageUnscaled(_capaFondo, 0, 0);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            DibujarEstrellas(g, t);
            DibujarNubes(g);
            DibujarDespegue(g, t);

            g.SmoothingMode = SmoothingMode.None;
            g.DrawImageUnscaled(_capaFrente, 0, 0);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.InterpolationMode = InterpolationMode.Bilinear;

            DibujarEmblema(g, t);
            DibujarTitulo(g, t);
            DibujarPanelProgreso(g, t);
            DibujarBitacora(g, t);
            DibujarConsejo(g, t);
            DibujarMarco(g, t);
        }

        private void DibujarEstrellas(Graphics g, double t)
        {
            for (int i = 0; i < _estrellas.Length; i++)
            {
                Estrella s = _estrellas[i];
                int alfa = (int)(110 + 145 * (0.5 + 0.5 * Math.Sin(t * s.Velocidad + s.Fase)));
                _brochaDinamica.Color = Color.FromArgb(alfa, 220, 235, 255);
                g.FillEllipse(_brochaDinamica, s.X, s.Y, s.Tamano, s.Tamano);
            }
        }

        private void DibujarNubes(Graphics g)
        {
            for (int i = 0; i < _nubes.Length; i++)
            {
                Nube n = _nubes[i];
                _brochaDinamica.Color = Color.FromArgb(n.Alfa, 150, 175, 215);
                g.FillEllipse(_brochaDinamica, n.X, n.Y, n.Ancho, n.Alto);
                g.FillEllipse(_brochaDinamica, n.X + n.Ancho * 0.25f, n.Y - n.Alto * 0.35f, n.Ancho * 0.55f, n.Alto * 1.1f);
            }
        }

        /// <summary>
        /// Secuencia final de "despegue": la flota real (sprites ya rasterizados) cruza la pantalla.
        /// Solo se ejecuta cuando la caché terminó, evitando uso concurrente de los Bitmap entre hilos.
        /// </summary>
        private void DibujarDespegue(Graphics g, double t)
        {
            if (!_listo || !CacheSpritesAeronaves.EstaInicializado) return;
            double tv = t - _instanteListo;

            DibujarAeronaveDespegue(g, TipoAeronave.F35_Lightning_II, tv, W * 0.80f, 1.45f, 1.25f, Color.FromArgb(255, 170, 60));
            DibujarAeronaveDespegue(g, TipoAeronave.B2_Spirit_Stealth, tv - 0.35, W * 0.17f, 1.15f, 1.6f, Color.FromArgb(80, 210, 255));

            foreach (Estela es in _estelas)
            {
                int alfa = (int)Math.Clamp(es.Vida * 170, 0, 170);
                _brochaDinamica.Color = Color.FromArgb(alfa, es.Color);
                g.FillEllipse(_brochaDinamica, es.X - es.Radio, es.Y - es.Radio, es.Radio * 2, es.Radio * 2);
            }
        }

        private void DibujarAeronaveDespegue(Graphics g, TipoAeronave tipo, double tv, float xBase, float escala, float duracion, Color colorEstela)
        {
            if (tv < 0 || tv > duracion + 0.4) return;

            Bitmap sprite = CacheSpritesAeronaves.ObtenerSpriteNave(tipo, false, true, tv < duracion * 0.5 ? 0f : -15f);
            float k = (float)(tv / duracion);
            float facil = k * k * (3 - 2 * k);
            float ancho = sprite.Width * escala;
            float alto = sprite.Height * escala;
            float x = xBase - ancho / 2 - facil * W * 0.06f;
            float y = H + 40 - facil * (H + alto + 120);

            // Estela de postcombustión
            if (_estelas.Count < 240)
            {
                _estelas.Add(new Estela
                {
                    X = x + ancho / 2 + (float)(_rng.NextDouble() * 8 - 4),
                    Y = y + alto - 10,
                    Vida = 1f,
                    Radio = 3f + (float)_rng.NextDouble() * 3f,
                    Color = colorEstela
                });
            }

            g.DrawImage(sprite, x, y, ancho, alto);
        }

        private void DibujarEmblema(Graphics g, double t)
        {
            float cx = _centroEmblema.X;
            float cy = _centroEmblema.Y + (float)Math.Sin(t * 1.6) * 3f;
            float r1 = _tamEmblema * 0.60f;
            float r2 = _tamEmblema * 0.52f;
            float r3 = _tamEmblema * 0.68f;

            GraphicsState estado = g.Save();
            g.TranslateTransform(cx, cy);

            // Barrido de radar con estela decreciente
            float barrido = (float)(t * 110 % 360);
            for (int i = 0; i < 8; i++)
            {
                _brochaDinamica.Color = Color.FromArgb(60 - i * 7, 0, 210, 255);
                g.FillPie(_brochaDinamica, -r2, -r2, r2 * 2, r2 * 2, barrido - i * 5, 5);
            }

            // Anillo exterior segmentado (horario)
            g.RotateTransform((float)(t * 22));
            _plumaDinamica.Color = Color.FromArgb(170, 0, 210, 255);
            _plumaDinamica.Width = 2.2f;
            for (int i = 0; i < 6; i++) g.DrawArc(_plumaDinamica, -r1, -r1, r1 * 2, r1 * 2, i * 60, 40);

            // Anillo interior punteado (antihorario)
            g.RotateTransform((float)(-t * 22 - t * 38));
            _plumaDinamica.Color = Color.FromArgb(120, 255, 196, 70);
            _plumaDinamica.Width = 1.4f;
            for (int i = 0; i < 18; i++) g.DrawArc(_plumaDinamica, -r2, -r2, r2 * 2, r2 * 2, i * 20, 9);

            // Marcas de rumbo
            g.ResetTransform();
            g.TranslateTransform(cx, cy);
            _plumaDinamica.Width = 1f;
            for (int i = 0; i < 72; i++)
            {
                double a = i * Math.PI / 36.0;
                float largo = i % 6 == 0 ? 9f : 4f;
                _plumaDinamica.Color = Color.FromArgb(i % 6 == 0 ? 150 : 70, 140, 210, 255);
                float c = (float)Math.Cos(a), s = (float)Math.Sin(a);
                g.DrawLine(_plumaDinamica, c * r3, s * r3, c * (r3 + largo), s * (r3 + largo));
            }

            // Onda de pulso
            float pulso = (float)(t * 0.8 % 1.0);
            _plumaDinamica.Color = Color.FromArgb((int)(120 * (1 - pulso)), 0, 220, 255);
            _plumaDinamica.Width = 1.5f;
            float rp = r1 + pulso * _tamEmblema * 0.25f;
            g.DrawEllipse(_plumaDinamica, -rp, -rp, rp * 2, rp * 2);

            g.Restore(estado);

            if (_bmpEmblema != null)
            {
                g.DrawImageUnscaled(_bmpEmblema, (int)(cx - _bmpEmblema.Width / 2f), (int)(cy - _bmpEmblema.Height / 2f));
            }
        }

        private void DibujarTitulo(Graphics g, double t)
        {
            if (_bmpTitulo == null || _rutaTitulo == null || _brochaBrilloTitulo == null) return;

            g.DrawImageUnscaled(_bmpTitulo, (int)_origenTitulo.X, (int)_origenTitulo.Y);

            // Destello metálico que recorre las letras cada 3.2 s
            RectangleF limites = _rutaTitulo.GetBounds();
            double ciclo = t % 3.2;
            if (ciclo < 1.3)
            {
                float ancho = H * 0.22f;
                float x = limites.Left - ancho + (float)(ciclo / 1.3) * (limites.Width + ancho * 2);
                GraphicsState estado = g.Save();
                g.SetClip(_rutaTitulo);
                _brochaBrilloTitulo.ResetTransform();
                _brochaBrilloTitulo.TranslateTransform(x, 0);
                g.FillRectangle(_brochaBrilloTitulo, x, limites.Top, ancho, limites.Height);
                g.Restore(estado);
            }

            // Subtítulo espaciado con líneas laterales
            const string subtitulo = "S I M U L A D O R    D E    C O M B A T E    A É R E O";
            SizeF tam = g.MeasureString(subtitulo, _fSubtitulo);
            float sx = (W - tam.Width) / 2f;
            _brochaDinamica.Color = Color.FromArgb(230, 255, 200, 90);
            g.DrawString(subtitulo, _fSubtitulo, _brochaDinamica, sx, _ySubtitulo);

            float ly = _ySubtitulo + tam.Height / 2f;
            float largo = W * 0.12f;
            _plumaDinamica.Width = 1f;
            _plumaDinamica.Color = Color.FromArgb(150, 255, 200, 90);
            g.DrawLine(_plumaDinamica, sx - 18 - largo, ly, sx - 18, ly);
            g.DrawLine(_plumaDinamica, sx + tam.Width + 18, ly, sx + tam.Width + 18 + largo, ly);
            _brochaDinamica.Color = Color.FromArgb(220, 255, 200, 90);
            g.FillPolygon(_brochaDinamica, Rombo(sx - 12, ly, 3.5f));
            g.FillPolygon(_brochaDinamica, Rombo(sx + tam.Width + 12, ly, 3.5f));
        }

        private void DibujarPanelProgreso(Graphics g, double t)
        {
            if (_brochaBarra == null || _brochaBrillo == null) return;
            RectangleF b = _barra;

            // Encabezado: etapa y porcentaje
            string encabezado = _listo
                ? "SISTEMAS EN LÍNEA  ·  LISTO PARA DESPEGUE"
                : $"ETAPA {_indiceEtapa + 1}/{_totalEtapas}  ·  {_etapa.ToUpperInvariant()}";
            _brochaDinamica.Color = _listo ? Color.FromArgb(255, 255, 205, 90) : Color.FromArgb(235, 170, 225, 255);
            g.DrawString(encabezado, _fEtapa, _brochaDinamica, b.Left - 2, b.Top - _fEtapa.Height - 10);

            string pct = $"{(int)Math.Round(_progresoVisual * 100)}%";
            SizeF tamPct = g.MeasureString(pct, _fPorcentaje);
            _brochaDinamica.Color = Color.FromArgb(255, 235, 248, 255);
            g.DrawString(pct, _fPorcentaje, _brochaDinamica, b.Right - tamPct.Width + 6, b.Top - tamPct.Height - 4);

            // Pista
            _brochaDinamica.Color = Color.FromArgb(170, 8, 18, 34);
            g.FillRectangle(_brochaDinamica, b);

            // Relleno con degradado cian → oro
            float relleno = (float)(b.Width * _progresoVisual);
            if (relleno > 1f)
            {
                var r = new RectangleF(b.Left, b.Top, relleno, b.Height);
                g.FillRectangle(_brochaBarra, r);

                GraphicsState estado = g.Save();
                g.SetClip(r);
                float xb = b.Left - 140 + (float)(t * 0.9 % 1.0) * (b.Width + 280);
                _brochaBrillo.ResetTransform();
                _brochaBrillo.TranslateTransform(xb, 0);
                g.FillRectangle(_brochaBrillo, xb, b.Top, 140, b.Height);
                g.Restore(estado);

                _plumaDinamica.Width = 2f;
                _plumaDinamica.Color = Color.FromArgb(140, 3, 8, 18);
                for (float x = b.Left + 7; x < b.Left + relleno; x += 7)
                    g.DrawLine(_plumaDinamica, x, b.Top, x, b.Bottom);

                _brochaDinamica.Color = Color.FromArgb(110, 255, 255, 255);
                g.FillEllipse(_brochaDinamica, b.Left + relleno - 9, b.Top - 6, 18, b.Height + 12);
            }

            // Marco técnico
            _plumaDinamica.Width = 1f;
            _plumaDinamica.Color = Color.FromArgb(150, 0, 200, 255);
            g.DrawRectangle(_plumaDinamica, b.Left - 4, b.Top - 4, b.Width + 8, b.Height + 8);
            for (int i = 1; i < 4; i++)
            {
                float mx = b.Left + b.Width * i / 4f;
                g.DrawLine(_plumaDinamica, mx, b.Bottom + 5, mx, b.Bottom + 10);
            }

            // Detalle / llamada a la acción
            if (_listo)
            {
                bool visible = ((t - _instanteListo) * 2.2 % 1.0) < 0.65;
                if (visible)
                {
                    const string cta = "PRESIONE CUALQUIER TECLA PARA DESPEGAR";
                    SizeF tc = g.MeasureString(cta, _fEtapa);
                    _brochaDinamica.Color = Color.FromArgb(255, 255, 255, 255);
                    g.DrawString(cta, _fEtapa, _brochaDinamica, (W - tc.Width) / 2f, b.Bottom + 16);
                }
            }
            else
            {
                _brochaDinamica.Color = Color.FromArgb(170, 150, 185, 220);
                g.DrawString(_detalle, _fDetalle, _brochaDinamica, b.Left - 2, b.Bottom + 14);
            }
        }

        private void DibujarBitacora(Graphics g, double t)
        {
            float x = 30;
            float lineaAlto = _fMono.Height + 2;
            float y = H - 26 - lineaAlto * 4;

            int inicio = Math.Max(0, _bitacora.Count - (_listo ? 4 : 3));
            for (int i = inicio; i < _bitacora.Count; i++)
            {
                var (texto, segundos) = _bitacora[i];
                _brochaDinamica.Color = Color.FromArgb(230, 90, 255, 150);
                g.DrawString("[ OK ]", _fMono, _brochaDinamica, x, y);
                _brochaDinamica.Color = Color.FromArgb(190, 160, 190, 220);
                g.DrawString($"{texto}  ({segundos:F2}s)", _fMono, _brochaDinamica, x + 56, y);
                y += lineaAlto;
            }

            if (!_listo)
            {
                char giro = "|/-\\"[(int)(t * 10) % 4];
                _brochaDinamica.Color = Color.FromArgb(230, 0, 210, 255);
                g.DrawString($"[ {giro}{giro} ]", _fMono, _brochaDinamica, x, y);
                _brochaDinamica.Color = Color.FromArgb(220, 210, 230, 255);
                g.DrawString(_etapa + "...", _fMono, _brochaDinamica, x + 56, y);
            }
        }

        private void DibujarConsejo(Graphics g, double t)
        {
            const double periodo = 4.0;
            int indice = (int)(t / periodo) % Consejos.Length;
            double fase = t % periodo;
            double alfa = fase < 0.4 ? fase / 0.4 : (fase > periodo - 0.4 ? (periodo - fase) / 0.4 : 1.0);

            string texto = "CONSEJO TÁCTICO  ›  " + Consejos[indice];
            SizeF tam = g.MeasureString(texto, _fMono);
            _brochaDinamica.Color = Color.FromArgb((int)(200 * alfa), 255, 210, 120);
            g.DrawString(texto, _fMono, _brochaDinamica, W - tam.Width - 30, H - 26 - _fMono.Height);
        }

        private void DibujarMarco(Graphics g, double t)
        {
            _plumaDinamica.Width = 2f;
            _plumaDinamica.Color = Color.FromArgb(170, 0, 200, 255);
            const float m = 14, l = 30;
            g.DrawLines(_plumaDinamica, new[] { new PointF(m, m + l), new PointF(m, m), new PointF(m + l, m) });
            g.DrawLines(_plumaDinamica, new[] { new PointF(W - m - l, m), new PointF(W - m, m), new PointF(W - m, m + l) });
            g.DrawLines(_plumaDinamica, new[] { new PointF(m, H - m - l), new PointF(m, H - m), new PointF(m + l, H - m) });
            g.DrawLines(_plumaDinamica, new[] { new PointF(W - m - l, H - m), new PointF(W - m, H - m), new PointF(W - m, H - m - l) });

            _plumaDinamica.Width = 1f;
            _plumaDinamica.Color = Color.FromArgb(70, 0, 200, 255);
            g.DrawRectangle(_plumaDinamica, 0, 0, W - 1, H - 1);

            _brochaDinamica.Color = Color.FromArgb(200, 120, 200, 255);
            g.DrawString("CÓNDOR TÁCTICO // SISTEMA DE ARRANQUE v3.3.0", _fEsquina, _brochaDinamica, m + 12, m + 6);

            const string derecha = "UNAJ · INGENIERÍA DE SISTEMAS";
            SizeF tam = g.MeasureString(derecha, _fEsquina);
            g.DrawString(derecha, _fEsquina, _brochaDinamica, W - m - 12 - tam.Width, m + 6);

            bool encendido = (t * 1.5 % 1.0) < 0.6;
            _brochaDinamica.Color = _listo
                ? Color.FromArgb(255, 90, 255, 150)
                : Color.FromArgb(encendido ? 255 : 70, 255, 190, 60);
            string estado = _listo ? "EN LÍNEA" : "CARGANDO";
            SizeF te = g.MeasureString(estado, _fEsquina);
            float ex = W - m - 12 - te.Width;
            float ey = m + 8 + tam.Height;
            g.FillEllipse(_brochaDinamica, ex - 12, ey + te.Height / 2f - 4, 8, 8);
            g.DrawString(estado, _fEsquina, _brochaDinamica, ex, ey);
        }

        // =====================================================================
        // GENERACIÓN DE CAPAS PRE-RENDERIZADAS (una sola vez)
        // =====================================================================
        private void GenerarElementosAnimados()
        {
            _estrellas = new Estrella[70];
            for (int i = 0; i < _estrellas.Length; i++)
            {
                _estrellas[i] = new Estrella
                {
                    X = (float)_rng.NextDouble() * W,
                    Y = (float)_rng.NextDouble() * H * 0.6f,
                    Fase = (float)(_rng.NextDouble() * Math.PI * 2),
                    Velocidad = 1.2f + (float)_rng.NextDouble() * 2.8f,
                    Tamano = 1.2f + (float)_rng.NextDouble() * 1.6f
                };
            }

            _nubes = new Nube[7];
            for (int i = 0; i < _nubes.Length; i++)
            {
                float ancho = W * (0.14f + (float)_rng.NextDouble() * 0.16f);
                _nubes[i] = new Nube
                {
                    X = (float)_rng.NextDouble() * W,
                    Y = H * (0.42f + (float)_rng.NextDouble() * 0.28f),
                    Ancho = ancho,
                    Alto = ancho * 0.18f,
                    Velocidad = 6f + (float)_rng.NextDouble() * 14f,
                    Alfa = 10 + _rng.Next(16)
                };
            }
        }

        private Bitmap GenerarCapaFondo()
        {
            var bmp = new Bitmap(W, H, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Cielo nocturno andino con transición al amanecer
            using (var cielo = new LinearGradientBrush(new Rectangle(0, 0, W, H), Color.Black, Color.Black, LinearGradientMode.Vertical))
            {
                cielo.InterpolationColors = new ColorBlend
                {
                    Colors = new[]
                    {
                        Color.FromArgb(255, 2, 5, 12),
                        Color.FromArgb(255, 6, 16, 34),
                        Color.FromArgb(255, 16, 34, 64),
                        Color.FromArgb(255, 58, 50, 78),
                        Color.FromArgb(255, 120, 62, 44)
                    },
                    Positions = new[] { 0f, 0.35f, 0.62f, 0.8f, 1f }
                };
                g.FillRectangle(cielo, 0, 0, W, H);
            }

            // Estrellas estáticas tenues
            using (var brocha = new SolidBrush(Color.White))
            {
                for (int i = 0; i < 320; i++)
                {
                    float x = (float)_rng.NextDouble() * W;
                    float y = (float)Math.Pow(_rng.NextDouble(), 1.6) * H * 0.7f;
                    float s = 0.6f + (float)_rng.NextDouble() * 1.1f;
                    brocha.Color = Color.FromArgb(40 + _rng.Next(120), 200, 220, 255);
                    g.FillEllipse(brocha, x, y, s, s);
                }
            }

            // Resplandor solar tras la cordillera
            using (var ruta = new GraphicsPath())
            {
                ruta.AddEllipse(W * -0.1f, H * 0.55f, W * 1.2f, H * 0.6f);
                using var resplandor = new PathGradientBrush(ruta)
                {
                    CenterColor = Color.FromArgb(150, 255, 140, 60),
                    SurroundColors = new[] { Color.FromArgb(0, 255, 140, 60) },
                    CenterPoint = new PointF(W / 2f, H * 0.86f)
                };
                g.FillPath(resplandor, ruta);
            }

            // Cordillera de los Andes: capas lejana y media con nieve
            DibujarCordillera(g, H * 0.80f, H * 0.20f, 34, Color.FromArgb(255, 26, 32, 54), Color.FromArgb(120, 210, 220, 240));
            DibujarCordillera(g, H * 0.87f, H * 0.13f, 26, Color.FromArgb(255, 14, 20, 36), Color.FromArgb(60, 180, 190, 220));
            return bmp;
        }

        private Bitmap GenerarCapaFrente()
        {
            var bmp = new Bitmap(W, H, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DibujarCordillera(g, H * 0.95f, H * 0.08f, 18, Color.FromArgb(255, 4, 7, 14), Color.Empty);

            // Viñeta cinematográfica
            using (var ruta = new GraphicsPath())
            {
                ruta.AddEllipse(-W * 0.25f, -H * 0.25f, W * 1.5f, H * 1.5f);
                using var vineta = new PathGradientBrush(ruta)
                {
                    CenterColor = Color.FromArgb(0, 0, 0, 0),
                    SurroundColors = new[] { Color.FromArgb(235, 0, 0, 0) },
                    FocusScales = new PointF(0.55f, 0.45f)
                };
                g.FillRectangle(vineta, 0, 0, W, H);
            }

            // Líneas de escaneo tipo monitor táctico
            using (var pluma = new Pen(Color.FromArgb(22, 0, 0, 0), 1f))
            {
                for (int y = 0; y < H; y += 3) g.DrawLine(pluma, 0, y, W, y);
            }
            return bmp;
        }

        private void DibujarCordillera(Graphics g, float baseY, float amplitud, int segmentos, Color relleno, Color nieve)
        {
            var puntos = new List<PointF> { new PointF(-10, H + 10) };
            float paso = (W + 20f) / segmentos;
            for (int i = 0; i <= segmentos; i++)
            {
                float altura = (float)(_rng.NextDouble() * 0.75 + 0.25) * amplitud;
                if (i % 2 == 1) altura *= 0.45f; // valles
                puntos.Add(new PointF(-10 + i * paso + (float)(_rng.NextDouble() - 0.5) * paso * 0.4f, baseY - altura));
            }
            puntos.Add(new PointF(W + 10, H + 10));

            using (var brocha = new SolidBrush(relleno)) g.FillPolygon(brocha, puntos.ToArray());

            if (nieve != Color.Empty)
            {
                using var pluma = new Pen(nieve, 1.4f);
                for (int i = 1; i < puntos.Count - 2; i++)
                {
                    PointF a = puntos[i], b = puntos[i + 1];
                    if (a.Y < b.Y && baseY - a.Y > amplitud * 0.55f)
                    {
                        // Ladera iluminada desde la cumbre
                        g.DrawLine(pluma, a, new PointF(a.X + (b.X - a.X) * 0.35f, a.Y + (b.Y - a.Y) * 0.35f));
                    }
                }
            }
        }

        /// <summary>Emblema vectorial del cóndor andino con alas desplegadas, contorno dorado y halo cian.</summary>
        private static Bitmap GenerarEmblema(int tam)
        {
            var bmp = new Bitmap(tam, tam, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float escala = tam * 0.46f;
            float cx = tam / 2f, cy = tam / 2f + tam * 0.03f;

            PointF[] alaDerecha =
            {
                new(0.00f, -0.20f), new(0.10f, -0.12f), new(0.35f, -0.21f), new(0.60f, -0.24f), new(0.80f, -0.19f),
                new(0.99f, -0.13f), new(0.90f, -0.07f), new(1.00f, -0.02f), new(0.88f, 0.00f), new(0.96f, 0.06f),
                new(0.84f, 0.05f), new(0.88f, 0.12f), new(0.73f, 0.07f), new(0.55f, 0.10f), new(0.35f, 0.08f),
                new(0.18f, 0.06f), new(0.10f, 0.12f), new(0.08f, 0.30f), new(0.00f, 0.25f)
            };

            using var silueta = new GraphicsPath();
            var contorno = new List<PointF>();
            foreach (PointF p in alaDerecha) contorno.Add(new PointF(cx + p.X * escala, cy + p.Y * escala));
            for (int i = alaDerecha.Length - 2; i >= 1; i--)
                contorno.Add(new PointF(cx - alaDerecha[i].X * escala, cy + alaDerecha[i].Y * escala));
            silueta.AddPolygon(contorno.ToArray());

            // Halo cian
            for (int i = 7; i >= 1; i--)
            {
                using var halo = new Pen(Color.FromArgb(14, 0, 210, 255), i * 2.6f) { LineJoin = LineJoin.Round };
                g.DrawPath(halo, silueta);
            }

            // Cuerpo metálico oscuro
            RectangleF lim = silueta.GetBounds();
            using (var metal = new LinearGradientBrush(lim, Color.FromArgb(255, 58, 68, 92), Color.FromArgb(255, 14, 18, 28), LinearGradientMode.Vertical))
            {
                g.FillPath(metal, silueta);
            }

            // Banda blanca de plumas secundarias (rasgo del cóndor andino)
            using (var banda = new SolidBrush(Color.FromArgb(225, 225, 232, 242)))
            {
                PointF[] bandaDer =
                {
                    new(0.28f, 0.01f), new(0.60f, -0.04f), new(0.72f, 0.03f), new(0.55f, 0.085f), new(0.32f, 0.065f)
                };
                var der = new PointF[bandaDer.Length];
                var izq = new PointF[bandaDer.Length];
                for (int i = 0; i < bandaDer.Length; i++)
                {
                    der[i] = new PointF(cx + bandaDer[i].X * escala, cy + bandaDer[i].Y * escala);
                    izq[i] = new PointF(cx - bandaDer[i].X * escala, cy + bandaDer[i].Y * escala);
                }
                g.FillPolygon(banda, der);
                g.FillPolygon(banda, izq);
            }

            // Collar blanco y cabeza
            using (var collar = new SolidBrush(Color.FromArgb(240, 240, 244, 250)))
                g.FillEllipse(collar, cx - escala * 0.09f, cy - escala * 0.24f, escala * 0.18f, escala * 0.09f);
            using (var cabeza = new SolidBrush(Color.FromArgb(255, 150, 70, 60)))
                g.FillEllipse(cabeza, cx - escala * 0.055f, cy - escala * 0.36f, escala * 0.11f, escala * 0.13f);

            // Contorno dorado
            using (var oro = new Pen(Color.FromArgb(255, 255, 196, 70), Math.Max(1.4f, tam / 140f)) { LineJoin = LineJoin.Round })
                g.DrawPath(oro, silueta);

            return bmp;
        }

        private void GenerarTitulo()
        {
            const string titulo = "CÓNDOR TÁCTICO";
            float tamFuente = H * 0.085f;

            var ruta = new GraphicsPath();
            ruta.AddString(titulo, _fTitulo.FontFamily, (int)FontStyle.Bold, tamFuente, PointF.Empty, StringFormat.GenericTypographic);
            RectangleF lim = ruta.GetBounds();

            float yTitulo = H * 0.505f;
            float dx = (W - lim.Width) / 2f - lim.Left;
            float dy = yTitulo - lim.Top;
            using (var m = new Matrix()) { m.Translate(dx, dy); ruta.Transform(m); }
            lim = ruta.GetBounds();
            _rutaTitulo = ruta;
            _ySubtitulo = lim.Bottom + H * 0.028f;

            const int margen = 26;
            _origenTitulo = new PointF(lim.Left - margen, lim.Top - margen);
            _bmpTitulo = new Bitmap((int)lim.Width + margen * 2, (int)lim.Height + margen * 2, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            using Graphics g = Graphics.FromImage(_bmpTitulo);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(-_origenTitulo.X, -_origenTitulo.Y);

            for (int i = 9; i >= 1; i--)
            {
                using var halo = new Pen(Color.FromArgb(11, 0, 200, 255), i * 2.4f) { LineJoin = LineJoin.Round };
                g.DrawPath(halo, ruta);
            }

            using (var relleno = new LinearGradientBrush(lim, Color.White, Color.White, LinearGradientMode.Vertical))
            {
                relleno.InterpolationColors = new ColorBlend
                {
                    Colors = new[] { Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 170, 232, 255), Color.FromArgb(255, 70, 130, 185) },
                    Positions = new[] { 0f, 0.5f, 1f }
                };
                g.FillPath(relleno, ruta);
            }

            using (var borde = new Pen(Color.FromArgb(150, 255, 255, 255), 1f))
                g.DrawPath(borde, ruta);
        }

        private static LinearGradientBrush CrearBrochaBrillo(float ancho, int alfaMax)
        {
            var brocha = new LinearGradientBrush(new RectangleF(0, 0, ancho, 10), Color.Transparent, Color.Transparent, LinearGradientMode.Horizontal);
            brocha.InterpolationColors = new ColorBlend
            {
                Colors = new[] { Color.FromArgb(0, 255, 255, 255), Color.FromArgb(alfaMax, 255, 255, 255), Color.FromArgb(0, 255, 255, 255) },
                Positions = new[] { 0f, 0.5f, 1f }
            };
            return brocha;
        }

        private static PointF[] Rombo(float cx, float cy, float r) =>
            new[] { new PointF(cx, cy - r), new PointF(cx + r, cy), new PointF(cx, cy + r), new PointF(cx - r, cy) };

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _temporizador.Stop();
            if (!_cts.IsCancellationRequested && Resultado == null) _cts.Cancel();
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _temporizador.Dispose();
                _cts.Dispose();
                _capaFondo?.Dispose();
                _capaFrente?.Dispose();
                _bmpEmblema?.Dispose();
                _bmpTitulo?.Dispose();
                _rutaTitulo?.Dispose();
                _brochaBarra?.Dispose();
                _brochaBrillo?.Dispose();
                _brochaBrilloTitulo?.Dispose();
                _brochaDinamica.Dispose();
                _plumaDinamica.Dispose();
                _fTitulo.Dispose();
                _fSubtitulo.Dispose();
                _fEtapa.Dispose();
                _fPorcentaje.Dispose();
                _fDetalle.Dispose();
                _fMono.Dispose();
                _fEsquina.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
