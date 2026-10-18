// ==============================================================================
// PROYECTO: CÓNDOR TÁCTICO (SIMULADOR DE COMBATE AÉREO 2D)
// AUTORES:
//   - RONAL ABELIN LARICO CCASO
//   - CLEVER NANDOR MAMANI HUALLPA
//   - JHON YOSSEP HINOJOSA APAZA
//   - JHON ALEX HUARACALLO SUCARI
//   - HECTOR CARLOS TORRES FLORES
// INSTITUCION: UNIVERSIDAD NACIONAL DE JULIACA (UNAJ)
// VERSION: 4.0.0
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using CondorTactico.Audio;

namespace CondorTactico.UI
{
    /// <summary>
    /// Menú Principal comercial a 60 FPS con Hangar Holográfico en tiempo real,
    /// navegación fluida con animaciones de hover y acceso a todas las modalidades del juego.
    /// </summary>
    public sealed class MenuPrincipal : Form
    {
        private sealed class BotonMenuTactico
        {
            public string Titulo { get; init; } = "";
            public string Subtitulo { get; init; } = "";
            public Color ColorAcento { get; init; }
            public RectangleF Rectangulo { get; set; }
            public float ProgresoHover { get; set; }
            public Action Accion { get; init; } = () => { };
        }

        private readonly ResultadoCarga? _resultadoCarga;
        private readonly GestorRedClienteServidor _gestorRedMenu = new GestorRedClienteServidor();
        private readonly System.Windows.Forms.Timer _timerAnimacion = new System.Windows.Forms.Timer { Interval = 16 };
        private readonly Stopwatch _reloj = Stopwatch.StartNew();
        private readonly List<BotonMenuTactico> _botones = new List<BotonMenuTactico>(10);
        private readonly PointF[] _estrellas = new PointF[85];
        private readonly Random _rng = new Random(2026);

        private int _indiceHover = -1;
        private RectangleF _rectBtnRecompensaDiaria;
        private RectangleF _rectBtnCambiarNaveRapido;
        private string _notificacionBanner = "BIENVENIDO AL CENTRO DE OPERACIONES AÉREAS CÓNDOR TÁCTICO — UNAJ";
        private double _tiempoNotificacion;

        public MenuPrincipal(ResultadoCarga? resultadoCarga = null)
        {
            _resultadoCarga = resultadoCarga;

            if (!CacheSpritesAeronaves.EstaInicializado)
            {
                CacheSpritesAeronaves.InicializarCache();
            }
            if (!RecursosGraficosHud.EstaInicializado)
            {
                RecursosGraficosHud.Inicializar();
            }

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            Text = "🦅 CÓNDOR TÁCTICO v4.0 — MENÚ PRINCIPAL Y HANGAR DE OPERACIONES | UNAJ";
            Size = GestorEstadoGlobal.Configuracion.ObtenerTamanoResolucion();
            MinimumSize = new Size(1020, 680);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(8, 14, 26);
            KeyPreview = true;

            for (int i = 0; i < _estrellas.Length; i++)
            {
                _estrellas[i] = new PointF((float)_rng.NextDouble(), (float)_rng.NextDouble() * 0.75f);
            }

            InicializarBotonesMenu();
            RecalcularDiseno();

            Resize += (s, e) => RecalcularDiseno();
            MouseMove += AlMoverRaton;
            MouseDown += AlHacerClic;
            KeyDown += AlPresionarTecla;

            _timerAnimacion.Tick += (s, e) =>
            {
                ActualizarAnimaciones();
                Invalidate();
            };
            _timerAnimacion.Start();
        }

        private void InicializarBotonesMenu()
        {
            _botones.Clear();

            _botones.Add(new BotonMenuTactico
            {
                Titulo = "▶  JUGAR",
                Subtitulo = "Acceso directo a Modos de Juego, Escaramuza y Tutorial",
                ColorAcento = Color.FromArgb(0, 230, 140),
                Accion = AbrirSelectorJugar
            });

            _botones.Add(new BotonMenuTactico
            {
                Titulo = "▲  MODO CASUAL (CAMPAÑA)",
                Subtitulo = "5 Operaciones progresivas, Jefes Finales y Peligros Ambientales",
                ColorAcento = Color.FromArgb(0, 195, 255),
                Accion = AbrirModoCasual
            });

            _botones.Add(new BotonMenuTactico
            {
                Titulo = "❖  MODO MULTIJUGADOR",
                Subtitulo = "Partidas 1v1, 2v2 y Cooperativo por LAN o Internet",
                ColorAcento = Color.FromArgb(160, 95, 255),
                Accion = AbrirModoMultijugador
            });

            _botones.Add(new BotonMenuTactico
            {
                Titulo = "◈  TIENDA DE ARMAMENTO",
                Subtitulo = "Adquiere aeronaves, skins, mejoras de ingeniería y poderes",
                ColorAcento = Color.FromArgb(255, 195, 45),
                Accion = () => AbrirCentroMando(PestanaCentroMando.Tienda)
            });

            _botones.Add(new BotonMenuTactico
            {
                Titulo = "✈  INVENTARIO Y HANGAR",
                Subtitulo = "Equipa tu aeronave, camuflaje y habilidad especial [Q]",
                ColorAcento = Color.FromArgb(70, 225, 210),
                Accion = () => AbrirCentroMando(PestanaCentroMando.Inventario)
            });

            _botones.Add(new BotonMenuTactico
            {
                Titulo = "★  PROGRESIÓN Y RANGOS",
                Subtitulo = "Consulta tu nivel, experiencia XP, rango militar y recompensas",
                ColorAcento = Color.FromArgb(255, 140, 60),
                Accion = () => AbrirCentroMando(PestanaCentroMando.Progresion)
            });

            _botones.Add(new BotonMenuTactico
            {
                Titulo = "⚙  CONFIGURACIÓN",
                Subtitulo = "Personaliza controles, audio, resolución, idioma y accesibilidad",
                ColorAcento = Color.FromArgb(140, 185, 230),
                Accion = () => AbrirCentroMando(PestanaCentroMando.Configuracion)
            });

            _botones.Add(new BotonMenuTactico
            {
                Titulo = "✦  ESTADÍSTICAS Y LOGROS",
                Subtitulo = "Revisa tu hoja de servicio, récords de combo y medallero",
                ColorAcento = Color.FromArgb(255, 215, 90),
                Accion = () => AbrirCentroMando(PestanaCentroMando.EstadisticasLogros)
            });

            _botones.Add(new BotonMenuTactico
            {
                Titulo = "✖  SALIR DEL JUEGO",
                Subtitulo = "Guardar todo el progreso en disco y cerrar el simulador",
                ColorAcento = Color.FromArgb(255, 80, 95),
                Accion = ConfirmarSalida
            });
        }

        private void RecalcularDiseno()
        {
            int w = Math.Max(1000, ClientSize.Width);
            int h = Math.Max(660, ClientSize.Height);

            float margenIzq = 38f;
            float topInicial = 108f;
            float altoDisponible = h - topInicial - 52f;
            float espaciado = 7f;
            float altoBoton = Math.Clamp((altoDisponible - (_botones.Count - 1) * espaciado) / _botones.Count, 44f, 58f);
            float anchoBoton = Math.Clamp(w * 0.40f, 410f, 490f);

            for (int i = 0; i < _botones.Count; i++)
            {
                float y = topInicial + i * (altoBoton + espaciado);
                _botones[i].Rectangulo = new RectangleF(margenIzq, y, anchoBoton, altoBoton);
            }

            float xDerecha = margenIzq + anchoBoton + 36f;
            float anchoDerecha = w - xDerecha - 38f;
            _rectBtnRecompensaDiaria = new RectangleF(xDerecha, h - 108f, (anchoDerecha * 0.52f) - 8f, 46f);
            _rectBtnCambiarNaveRapido = new RectangleF(xDerecha + (anchoDerecha * 0.52f) + 8f, h - 108f, (anchoDerecha * 0.48f) - 8f, 46f);
        }

        private void ActualizarAnimaciones()
        {
            for (int i = 0; i < _botones.Count; i++)
            {
                float objetivo = (i == _indiceHover) ? 1f : 0f;
                _botones[i].ProgresoHover += (objetivo - _botones[i].ProgresoHover) * 0.22f;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int w = ClientSize.Width;
            int h = ClientSize.Height;
            double t = _reloj.Elapsed.TotalSeconds;

            // 1. Fondo estratosférico nocturno con gradiente y estrellas
            using (var gradFondo = new LinearGradientBrush(
                new Rectangle(0, 0, w, h),
                Color.FromArgb(6, 12, 24),
                Color.FromArgb(18, 36, 64),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(gradFondo, 0, 0, w, h);
            }

            for (int i = 0; i < _estrellas.Length; i++)
            {
                float sx = _estrellas[i].X * w;
                float sy = _estrellas[i].Y * h;
                int alfa = 80 + (int)(Math.Sin(t * 2.2 + i) * 70);
                using var brEst = new SolidBrush(Color.FromArgb(Math.Clamp(alfa, 25, 220), 210, 235, 255));
                g.FillEllipse(brEst, sx, sy, (i % 3 == 0) ? 2.5f : 1.5f, (i % 3 == 0) ? 2.5f : 1.5f);
            }

            // Retícula sutil de fondo
            using (var penGrid = new Pen(Color.FromArgb(14, 0, 210, 255), 1f))
            {
                for (int x = 0; x < w; x += 64) g.DrawLine(penGrid, x, 0, x, h);
                for (int y = 0; y < h; y += 64) g.DrawLine(penGrid, 0, y, w, y);
            }

            // 2. Cabecera Superior con Perfil, Rango, XP y Saldo de Monedas
            DibujarCabeceraSuperior(g, w);

            // 3. Columna Izquierda: Los 9 Botones del Menú Principal
            DibujarBotonesMenu(g);

            // 4. Columna Derecha: Hangar Interactivo en Tiempo Real y Estado de Campaña
            DibujarPanelHangarDerecho(g, w, h, t);

            // 5. Barra de estado inferior
            DibujarPieDePagina(g, w, h);
        }

        private void DibujarCabeceraSuperior(Graphics g, int w)
        {
            PerfilJugador p = GestorEstadoGlobal.Perfil;
            int nivel = p.CalcularNivelJugador();

            using var brTop = new SolidBrush(Color.FromArgb(215, 10, 18, 32));
            g.FillRectangle(brTop, 0, 0, w, 88);
            using var penLinea = new Pen(Color.FromArgb(120, 0, 200, 255), 1.5f);
            g.DrawLine(penLinea, 0, 88, w, 88);

            using var fTitulo = new Font("Segoe UI", 20f, FontStyle.Bold);
            using var fSub = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            using var brCian = new SolidBrush(Color.FromArgb(0, 225, 255));
            using var brBlanco = new SolidBrush(Color.White);
            using var brPlata = new SolidBrush(Color.FromArgb(170, 200, 230));

            g.DrawString("✦ CÓNDOR TÁCTICO", fTitulo, brCian, 34, 12);
            g.DrawString("SIMULADOR DE COMBATE AÉREO 2D · UNIVERSIDAD NACIONAL DE JULIACA (UNAJ)", fSub, brPlata, 38, 52);

            // Tarjeta de Perfil y Monedas a la derecha de la cabecera
            float cardW = 500f;
            float cardX = w - cardW - 36f;
            RectangleF rectPerfil = new RectangleF(cardX, 12f, cardW, 64f);
            using var brCard = new SolidBrush(Color.FromArgb(200, 18, 32, 54));
            using var penCard = new Pen(Color.FromArgb(110, 0, 195, 255), 1f);
            g.FillRectangle(brCard, rectPerfil);
            g.DrawRectangle(penCard, rectPerfil.X, rectPerfil.Y, rectPerfil.Width, rectPerfil.Height);

            using var fPiloto = new Font("Consolas", 9.8f, FontStyle.Bold);
            using var fMonedas = new Font("Consolas", 11.5f, FontStyle.Bold);
            using var brOro = new SolidBrush(Color.FromArgb(255, 215, 55));

            g.DrawString($"✈ {p.NombrePiloto} | Nv.{nivel} {p.ObtenerNombreRangoMilitar()}", fPiloto, brBlanco, cardX + 12, 18);
            var sfDer = new StringFormat { Alignment = StringAlignment.Far };
            g.DrawString($"★ {p.Monedas:N0} CR", fMonedas, brOro, new RectangleF(cardX + 280, 16, cardW - 292, 24), sfDer);

            // Barra de XP
            int xpBase = p.ObtenerXPInicioNivelActual();
            int xpMeta = p.ObtenerXPParaSiguienteNivel();
            float ratioXp = Math.Clamp((float)(p.ExperienciaTotal - xpBase) / Math.Max(1, xpMeta - xpBase), 0f, 1f);

            RectangleF rectBarraFondo = new RectangleF(cardX + 14, 46, cardW - 28, 16);
            using var brFondoXp = new SolidBrush(Color.FromArgb(12, 20, 34));
            g.FillRectangle(brFondoXp, rectBarraFondo);
            if (ratioXp > 0.01f)
            {
                using var brFillXp = new LinearGradientBrush(rectBarraFondo, Color.FromArgb(0, 180, 255), Color.FromArgb(80, 255, 170), LinearGradientMode.Horizontal);
                g.FillRectangle(brFillXp, rectBarraFondo.X, rectBarraFondo.Y, rectBarraFondo.Width * ratioXp, rectBarraFondo.Height);
            }
            using var fXp = new Font("Consolas", 8f, FontStyle.Bold);
            g.DrawString($"XP: {p.ExperienciaTotal:N0} / {xpMeta:N0}  ({(int)(ratioXp * 100)}%)", fXp, brBlanco, rectBarraFondo.X + 8, rectBarraFondo.Y + 1);
        }

        private void DibujarBotonesMenu(Graphics g)
        {
            using var fTit = new Font("Segoe UI", 11f, FontStyle.Bold);
            using var fSub = new Font("Segoe UI", 8.3f, FontStyle.Regular);

            for (int i = 0; i < _botones.Count; i++)
            {
                BotonMenuTactico b = _botones[i];
                float despX = b.ProgresoHover * 10f;
                RectangleF r = new RectangleF(b.Rectangulo.X + despX, b.Rectangulo.Y, b.Rectangulo.Width, b.Rectangulo.Height);

                int rCol = (int)(16 + b.ProgresoHover * 22);
                int gCol = (int)(26 + b.ProgresoHover * 36);
                int bCol = (int)(44 + b.ProgresoHover * 55);

                using var brFondo = new SolidBrush(Color.FromArgb(225, rCol, gCol, bCol));
                g.FillRectangle(brFondo, r);

                // Franja lateral de acento
                using var brAcento = new SolidBrush(b.ColorAcento);
                g.FillRectangle(brAcento, r.X, r.Y, 6f + (b.ProgresoHover * 3f), r.Height);

                int alfaBorde = 75 + (int)(b.ProgresoHover * 180);
                using var penBorde = new Pen(Color.FromArgb(alfaBorde, b.ColorAcento), 1f + b.ProgresoHover * 0.8f);
                g.DrawRectangle(penBorde, r.X, r.Y, r.Width, r.Height);

                using var brTitulo = new SolidBrush(b.ProgresoHover > 0.3f ? Color.White : Color.FromArgb(230, 240, 255));
                using var brSub = new SolidBrush(Color.FromArgb(170, 195, 220));

                g.DrawString(b.Titulo, fTit, brTitulo, r.X + 18f, r.Y + 6f);
                g.DrawString(b.Subtitulo, fSub, brSub, r.X + 20f, r.Y + 27f);
            }
        }

        private void DibujarPanelHangarDerecho(Graphics g, int w, int h, double t)
        {
            float xIzq = _botones[0].Rectangulo.Right + 36f;
            float ancho = w - xIzq - 38f;
            float yTop = 108f;
            float altoHangar = h - yTop - 126f;

            RectangleF rectHangar = new RectangleF(xIzq, yTop, ancho, altoHangar);
            using var brHangar = new SolidBrush(Color.FromArgb(190, 12, 22, 38));
            g.FillRectangle(brHangar, rectHangar);

            Color colorSkin = GestorEstadoGlobal.ObtenerColorSkinEquipada();
            using var penMarco = new Pen(Color.FromArgb(120, colorSkin), 1.5f);
            g.DrawRectangle(penMarco, rectHangar.X, rectHangar.Y, rectHangar.Width, rectHangar.Height);

            // Anillo de radar giratorio en el centro del hangar
            float cx = rectHangar.X + rectHangar.Width * 0.5f;
            float cy = rectHangar.Y + rectHangar.Height * 0.40f;
            float radio = Math.Min(rectHangar.Width, rectHangar.Height) * 0.28f;

            using (var brHalo = new SolidBrush(Color.FromArgb(26, colorSkin)))
            {
                g.FillEllipse(brHalo, cx - radio, cy - radio, radio * 2, radio * 2);
            }
            using (var penAnillo = new Pen(Color.FromArgb(95, colorSkin), 1.5f) { DashStyle = DashStyle.Dash })
            {
                g.DrawEllipse(penAnillo, cx - radio, cy - radio, radio * 2, radio * 2);
            }

            // Barrido angular de radar
            float angRadar = (float)((t * 75.0) % 360.0);
            using (var brSweep = new SolidBrush(Color.FromArgb(40, colorSkin)))
            {
                g.FillPie(brSweep, cx - radio, cy - radio, radio * 2, radio * 2, angRadar, 42f);
            }

            // Dibujar aeronave equipada con oscilación de vuelo suave
            float offsetY = (float)(Math.Sin(t * 2.6) * 7.0);
            float alabeo = (float)(Math.Sin(t * 1.4) * 15.0);
            Bitmap sprite = CacheSpritesAeronaves.ObtenerSpriteNave(
                GestorEstadoGlobal.ObtenerModeloAeronaveEquipada(),
                esRival: false,
                acelerando: true,
                alabeo);

            g.DrawImage(sprite, cx - sprite.Width * 0.5f, cy - sprite.Height * 0.5f + offsetY);

            // Etiquetas del Hangar
            PerfilJugador p = GestorEstadoGlobal.Perfil;
            var naveEq = GestorEstadoGlobal.CatalogoTienda.FirstOrDefault(a => a.Id == p.AeronaveEquipadaId);
            var skinEq = GestorEstadoGlobal.CatalogoTienda.FirstOrDefault(a => a.Id == p.SkinEquipadaId);
            var poderEq = GestorEstadoGlobal.CatalogoTienda.FirstOrDefault(a => a.Id == p.PoderEquipadoId);
            var opActual = GestorEstadoGlobal.NivelesCampana[Math.Clamp(p.NivelMaximoCampanaDesbloqueado - 1, 0, 4)];

            using var fHangarTit = new Font("Segoe UI", 11.5f, FontStyle.Bold);
            using var fDatos = new Font("Consolas", 9.5f, FontStyle.Regular);
            using var brSkin = new SolidBrush(colorSkin);
            using var brTexto = new SolidBrush(Color.FromArgb(215, 235, 255));

            g.DrawString("◈ HANGAR TÁCTICO EN LÍNEA — AERONAVE LISTA", fHangarTit, brSkin, rectHangar.X + 18, rectHangar.Y + 14);

            int hpMax = 30 + p.ObtenerNivelMejora("MEJORA_BLINDAJE") * 6 + (p.AeronaveEquipadaId == "NAVE_CONDOR_X" ? 8 : 0);
            int escMax = 12 + p.ObtenerNivelMejora("MEJORA_ESCUDO") * 4;
            int totalEstrellas = p.EstrellasPorNivel.Values.Sum();

            string ficha =
                $"• Aeronave Equipada : {naveEq?.Nombre}\n" +
                $"• Apariencia Activa : {skinEq?.Nombre}\n" +
                $"• Poder Especial [Q]: {poderEq?.Nombre}\n" +
                $"• Casco / Escudo RAM: {hpMax} HP  |  {escMax} PTS Escudo\n" +
                $"• Operación Actual  : {opActual.CodigoOperacion}\n" +
                $"• Estrellas Campaña : {totalEstrellas} / 15 ★   |   Logros: {p.LogrosDesbloqueados.Count} / {GestorEstadoGlobal.CatalogoLogros.Count}";

            float yFicha = rectHangar.Bottom - 145f;
            using var brCajaFicha = new SolidBrush(Color.FromArgb(180, 8, 14, 26));
            g.FillRectangle(brCajaFicha, rectHangar.X + 14, yFicha - 8, rectHangar.Width - 28, 138);
            g.DrawString(ficha, fDatos, brTexto, rectHangar.X + 24, yFicha);

            // Botones rápidos debajo del hangar (Suministro Diario + Alternar Nave)
            DibujarBotonAuxiliar(g, _rectBtnRecompensaDiaria, "★ RECLAMAR SUMINISTRO DIARIO", Color.FromArgb(210, 140, 20));
            DibujarBotonAuxiliar(g, _rectBtnCambiarNaveRapido, "✈ CAMBIAR NAVE (F-35 / B-2)", Color.FromArgb(0, 145, 200));
        }

        private static void DibujarBotonAuxiliar(Graphics g, RectangleF r, string texto, Color colorBase)
        {
            using var br = new SolidBrush(Color.FromArgb(220, colorBase.R / 2, colorBase.G / 2, colorBase.B / 2));
            using var pen = new Pen(colorBase, 1.5f);
            g.FillRectangle(br, r);
            g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);

            using var f = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(texto, f, Brushes.White, r, sf);
        }

        private void DibujarPieDePagina(Graphics g, int w, int h)
        {
            using var brPie = new SolidBrush(Color.FromArgb(220, 10, 16, 28));
            g.FillRectangle(brPie, 0, h - 38, w, 38);

            using var fPie = new Font("Consolas", 8.8f, FontStyle.Regular);
            using var brTxt = new SolidBrush(Color.FromArgb(160, 210, 255));
            g.DrawString($"► {_notificacionBanner}", fPie, brTxt, 38, h - 26);
            g.DrawString("Atajos: [Enter] Jugar | [T] Tienda | [I] Inventario | [C] Ajustes | [Esc] Salir", fPie, Brushes.Gray, w - 570, h - 26);
        }

        private void AlMoverRaton(object? sender, MouseEventArgs e)
        {
            int nuevoHover = -1;
            for (int i = 0; i < _botones.Count; i++)
            {
                if (_botones[i].Rectangulo.Contains(e.Location))
                {
                    nuevoHover = i;
                    break;
                }
            }

            if (nuevoHover != _indiceHover)
            {
                _indiceHover = nuevoHover;
                Cursor = (_indiceHover >= 0 || _rectBtnRecompensaDiaria.Contains(e.Location) || _rectBtnCambiarNaveRapido.Contains(e.Location))
                    ? Cursors.Hand
                    : Cursors.Default;
            }
        }

        private void AlHacerClic(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            for (int i = 0; i < _botones.Count; i++)
            {
                if (_botones[i].Rectangulo.Contains(e.Location))
                {
                    SintetizadorAudioTactico.ReproducirAlertaRadar();
                    _botones[i].Accion.Invoke();
                    return;
                }
            }

            if (_rectBtnRecompensaDiaria.Contains(e.Location))
            {
                if (GestorEstadoGlobal.ReclamarRecompensaDiaria(out _, out _, out string msg))
                {
                    SintetizadorAudioTactico.ReproducirAlertaRadar();
                    MostrarNotificacion(msg.Replace('\n', ' '));
                    MessageBox.Show(this, msg, "Suministro Diario", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(this, msg, "Suministro Diario", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            if (_rectBtnCambiarNaveRapido.Contains(e.Location))
            {
                string actual = GestorEstadoGlobal.Perfil.AeronaveEquipadaId;
                string siguiente = actual == "NAVE_B2" ? "NAVE_F35" : "NAVE_B2";
                GestorEstadoGlobal.EquiparArticulo(siguiente, out string msg);
                SintetizadorAudioTactico.ReproducirAlertaRadar();
                MostrarNotificacion(msg);
            }
        }

        private void AlPresionarTecla(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) AbrirSelectorJugar();
            else if (e.KeyCode == Keys.T) AbrirCentroMando(PestanaCentroMando.Tienda);
            else if (e.KeyCode == Keys.I) AbrirCentroMando(PestanaCentroMando.Inventario);
            else if (e.KeyCode == Keys.C) AbrirCentroMando(PestanaCentroMando.Configuracion);
            else if (e.KeyCode == Keys.Escape) ConfirmarSalida();
        }

        private void MostrarNotificacion(string texto)
        {
            _notificacionBanner = texto;
            _tiempoNotificacion = _reloj.Elapsed.TotalSeconds;
        }

        // =========================================================================
        // ACCIONES DE NAVEGACIÓN A TODAS LAS PANTALLAS Y MODALIDADES
        // =========================================================================
        private void AbrirSelectorJugar()
        {
            AbrirModoCasual();
        }

        private void AbrirModoCasual()
        {
            using var dlg = new VentanaSelectorModoCasual();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                EjecutarPartidaCombate(
                    dlg.ModalidadElegida,
                    dlg.NivelElegido,
                    ModoRedJuego.Local_Combate_IA,
                    "127.0.0.1",
                    15050,
                    1);
            }
        }

        private void AbrirModoMultijugador()
        {
            using var dlg = new VentanaLobbyMultijugador(_gestorRedMenu);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.IniciarPartidaConfirmada)
            {
                GestorEstadoGlobal.EquiparArticulo(
                    dlg.AeronaveSeleccionada == TipoAeronave.B2_Spirit_Stealth ? "NAVE_B2" : "NAVE_F35",
                    out _);

                EjecutarPartidaCombate(
                    dlg.ModalidadSeleccionada,
                    nivelCampana: 1,
                    dlg.ModoRedSeleccionado,
                    dlg.IpSeleccionada,
                    dlg.PuertoSeleccionado,
                    dlg.EquipoSeleccionado);
            }
        }

        private void AbrirCentroMando(PestanaCentroMando pestana)
        {
            using var dlg = new VentanaCentroMando(pestana);
            dlg.ShowDialog(this);
            Size nuevaRes = GestorEstadoGlobal.Configuracion.ObtenerTamanoResolucion();
            if (Size != nuevaRes && WindowState == FormWindowState.Normal)
            {
                Size = nuevaRes;
            }
            Invalidate();
        }

        private void EjecutarPartidaCombate(
            ModalidadPartida modalidad,
            int nivelCampana,
            ModoRedJuego modoRed,
            string ipRed,
            int puertoRed,
            int equipoLocal)
        {
            _timerAnimacion.Stop();
            Hide();

            try
            {
                using var ventanaCombate = new VentanaCombateTactico(_resultadoCarga);
                ventanaCombate.ConfigurarPartidaEspecifica(modalidad, nivelCampana, modoRed, ipRed, puertoRed, equipoLocal);
                ventanaCombate.ShowDialog(this);
            }
            finally
            {
                Show();
                Activate();
                _timerAnimacion.Start();
                MostrarNotificacion($"Progreso sincronizado · Créditos: {GestorEstadoGlobal.Perfil.Monedas:N0} · Rango: {GestorEstadoGlobal.Perfil.ObtenerNombreRangoMilitar()}");
                Invalidate();
            }
        }

        private void ConfirmarSalida()
        {
            DialogResult res = MessageBox.Show(
                this,
                "¿Deseas guardar tu progreso y cerrar Cóndor Táctico?",
                "Salir de Cóndor Táctico",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (res == DialogResult.Yes)
            {
                GestorEstadoGlobal.GuardarTodo();
                Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _timerAnimacion.Stop();
            _timerAnimacion.Dispose();
            _gestorRedMenu.Dispose();
            GestorEstadoGlobal.GuardarTodo();
            base.OnFormClosing(e);
        }
    }
}
