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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CondorTactico.Audio;
using CondorTactico.GraphicsEffects;
using CondorTactico.UI;

namespace CondorTactico
{
    public partial class VentanaCombateTactico : Form
    {
        // =========================================================================
        // ATRIBUTOS ENCAPSULADOS DEL MOTOR DE JUEGO, OOP, BIG-O, PROFILING Y RED
        // =========================================================================
        private readonly LienzoTactico contiene = new LienzoTactico();
        private readonly Panel panelLanzador = new Panel();
        private readonly Button btnLanzar = new Button();
        private readonly Button btnAlternarModelo = new Button();
        private readonly Button btnEscolta = new Button();
        private readonly Button btnMision = new Button();
        private readonly Button btnRed = new Button();
        private readonly Button btnMetricas = new Button();
        private readonly Button btnAcercaDe = new Button();
        private readonly Button btnTiendaHangar = new Button();
        private readonly Button btnPausa = new Button();
        private readonly Button btnMenuPrincipal = new Button();

        // Efectos visuales cinematográficos (Screen Shake, Ondas expansivas, Sombras pseudo-3D, Casquillos, Vórtices) y HUD militar
        private readonly GestorEfectosVisualesCombate _efectosVisuales = new GestorEfectosVisualesCombate();
        private readonly HudTacticoMilitar _hudMilitar = new HudTacticoMilitar();

        // Parámetros de misión, campaña, progresión y enlace táctico
        private ModalidadPartida _modalidadActual = ModalidadPartida.CampanaCasual;
        private int _nivelCampanaActual = 1;
        private int _oleadaActual = 1;
        private int _totalOleadasNivel = 2;
        private bool _enFaseJefe = false;
        private int _vidaMaximaJefeActual = 50;
        private string _nombreJefeActual = "COMANDANTE RIVAL";

        private string _nombrePilotoActual = "CÓNDOR-01";
        private TipoAeronave _modeloJugadorActual = TipoAeronave.B2_Spirit_Stealth;
        private TipoAeronave _modeloRivalActual = TipoAeronave.F35_Lightning_II;
        private bool _incluirEscoltaActual = true;
        private ModoRedJuego _modoRedActual = ModoRedJuego.Local_Combate_IA;
        private string _ipRedActual = "127.0.0.1";
        private int _puertoRedActual = 15050;
        private int _equipoLocal = 1;

        // Sistemas de Jugabilidad: Poderes, Combos, Suministros, Peligros, Checkpoints, Pausa y Tutorial
        private readonly List<SuministroCombate> _suministros = new List<SuministroCombate>(16);
        private readonly List<ObstaculoAmbiental> _obstaculosAmbientales = new List<ObstaculoAmbiental>(16);
        private readonly ControladorTutorial _tutorial = new ControladorTutorial();

        private float _cargaPoderEspecial = 100f;
        private int _ticksPoderActivo = 0;
        private int _ticksInvulnerableJugador = 0;
        private int _ticksEnemigosParalizados = 0;
        private int _comboActual = 1;
        private int _comboMaximoPartida = 1;
        private int _ticksExpiracionCombo = 0;
        private int _monedasRecolectadasPartida = 0;
        private int _vidasReserva = 1;
        private bool _checkpointJefeAlcanzado = false;
        private bool _usoReaparicionEnNivel = false;
        private int _disparosPartida = 0;
        private int _impactosPartida = 0;
        private int _impactosCriticosPartida = 0;
        private bool _juegoPausado = false;

        // Resumen de Recompensas al finalizar partida
        private int _ultimasMonedasGanadas = 0;
        private int _ultimaXPGanada = 0;
        private int _ultimasEstrellasGanadas = 0;
        private string _textoLogrosRecientes = "";

        // Entidades Polimórficas OOP
        private AeronaveBase _naveJugador = null!;
        private AeronaveBase _naveRivalPrincipal = null!;
        private readonly List<AeronaveBase> _aeronaves = new List<AeronaveBase>(16);
        private readonly Dictionary<string, AeronaveBase> _aeronavesRemotas = new Dictionary<string, AeronaveBase>();
        private readonly List<Proyectil> _proyectiles = new List<Proyectil>(256);

        // Estructuras de Optimización Big-O, Profiling y Red Cliente-Servidor
        private readonly GrillaEspacialColisiones _grillaColisiones = new GrillaEspacialColisiones(72);
        private readonly ProfilerMotor _profiler = new ProfilerMotor();
        private readonly GestorRedClienteServidor _gestorRed = new GestorRedClienteServidor();
        private readonly List<PaqueteRedAeronave> _bufferRed = new List<PaqueteRedAeronave>(32);

        // Efectos visuales de combate (Partículas, nubes estratosféricas y textos flotantes)
        private readonly List<ParticulaCombate> _particulas = new List<ParticulaCombate>(256);
        private readonly List<TextoFlotanteImpacto> _textosImpacto = new List<TextoFlotanteImpacto>(32);
        private readonly PointF[] _nubesEstratosfera = new PointF[14];
        private readonly Random _rng = new Random();

        // Control de bucle de juego y entrada fluida
        private readonly System.Windows.Forms.Timer tiempo = new System.Windows.Forms.Timer();
        private readonly HashSet<Keys> _teclasActivas = new HashSet<Keys>();
        private Point _ultimaPosicionRaton = new Point(400, 450);
        private bool _mostrarHudProfiling = true;
        private bool _juegoTerminado = false;
        private bool _victoriaJugador = false;
        private string _mensajeResultado = "";
        private int _contadorTickIA = 0;
        private int _contadorTickRed = 0;
        private float _offsetRadarY = 0f;

        // Variables de compatibilidad con la versión base
        private float angulo = 0f;
        private int tiempoImpacto = 0;
        private int tiempoImpactoNavex = 0;

        public VentanaCombateTactico() : this(null)
        {
        }

        public VentanaCombateTactico(ResultadoCarga? resultado)
        {
            InitializeComponent();
            Iniciar(resultado);
        }

        // =========================================================================
        // INICIALIZACIÓN DEL FORMULARIO Y ARQUITECTURA CLIENTE-SERVIDOR
        // =========================================================================
        public void Iniciar(ResultadoCarga? resultado = null)
        {
            if (!CacheSpritesAeronaves.EstaInicializado)
            {
                CacheSpritesAeronaves.InicializarCache();
            }

            if (!RecursosGraficosHud.EstaInicializado)
            {
                RecursosGraficosHud.Inicializar();
            }

            AplicarPreferenciasConfiguracion();

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(920, 680);
            this.Size = resultado?.TamanoVentanaPrincipal ?? GestorEstadoGlobal.Configuracion.ObtenerTamanoResolucion();
            if (GestorEstadoGlobal.Configuracion.PantallaCompleta)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "CÓNDOR TÁCTICO v4.0 | SIMULADOR DE COMBATE AÉREO 2D | UNAJ";
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            ConstruirBarraLanzamientoTactico();

            // Configurar el lienzo principal donde vuelan las aeronaves
            contiene.Dock = DockStyle.Fill;
            contiene.BackColor = Color.FromArgb(24, 58, 98);
            contiene.Paint += RenderizarEscenaCompleta;
            contiene.MouseMove += (s, e) => _ultimaPosicionRaton = e.Location;
            contiene.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && !_juegoPausado && !_juegoTerminado)
                {
                    EjecutarDisparoJugador();
                }
                else if (e.Button == MouseButtons.Right && !_juegoPausado && !_juegoTerminado)
                {
                    IntentarActivarPoderEspecial();
                }
            };
            this.Controls.Add(contiene);
            contiene.BringToFront();

            for (int i = 0; i < _nubesEstratosfera.Length; i++)
            {
                _nubesEstratosfera[i] = new PointF(_rng.Next(20, 900), _rng.Next(20, 650));
            }

            this.KeyDown += EventoKeyDown;
            this.KeyUp += EventoKeyUp;

            _gestorRed.IniciarArquitectura(_modoRedActual, _ipRedActual, _puertoRedActual);

            DesplegarMisionAerea(_modeloJugadorActual, _modeloRivalActual, _incluirEscoltaActual);

            tiempo.Interval = 16;
            tiempo.Tick += ImpactarTick;
            tiempo.Start();

            DevolverFocoAlLienzo();
        }

        private void AplicarPreferenciasConfiguracion()
        {
            ConfiguracionJuego cfg = GestorEstadoGlobal.Configuracion;
            _nombrePilotoActual = GestorEstadoGlobal.Perfil.NombrePiloto;
            _modeloJugadorActual = GestorEstadoGlobal.ObtenerModeloAeronaveEquipada();
            _mostrarHudProfiling = cfg.MostrarTelemetria;
            _nivelCampanaActual = Math.Clamp(GestorEstadoGlobal.Perfil.NivelMaximoCampanaDesbloqueado, 1, 5);
        }

        /// <summary>
        /// Configura la modalidad exacta (Campaña Casual, Tutorial, Batalla Libre o Multijugador 1v1/2v2/Coop)
        /// invocada desde el Menú Principal.
        /// </summary>
        public void ConfigurarPartidaEspecifica(
            ModalidadPartida modalidad,
            int nivelCampana,
            ModoRedJuego modoRed = ModoRedJuego.Local_Combate_IA,
            string ipRed = "127.0.0.1",
            int puertoRed = 15050,
            int equipoLocal = 1)
        {
            AplicarPreferenciasConfiguracion();
            _modalidadActual = modalidad;
            _nivelCampanaActual = Math.Clamp(nivelCampana, 1, GestorEstadoGlobal.NivelesCampana.Count);
            _modoRedActual = modoRed;
            _ipRedActual = ipRed;
            _puertoRedActual = puertoRed;
            _equipoLocal = equipoLocal;

            _gestorRed.ModalidadMultijugador = modalidad;
            _gestorRed.EquipoLocal = equipoLocal;
            _gestorRed.IniciarArquitectura(_modoRedActual, _ipRedActual, _puertoRedActual);

            _checkpointJefeAlcanzado = false;
            DesplegarMisionAerea(_modeloJugadorActual, _modeloRivalActual, _incluirEscoltaActual);
        }

        /// <summary>
        /// Construye la barra superior táctica con accesos directos a subventanas modulares y al Menú Principal.
        /// </summary>
        private void ConstruirBarraLanzamientoTactico()
        {
            panelLanzador.Dock = DockStyle.Top;
            panelLanzador.Height = 44;
            panelLanzador.BackColor = Color.FromArgb(12, 20, 32);
            panelLanzador.Padding = new Padding(6, 5, 6, 4);

            Font fBtn = new Font("Segoe UI", 8.0f, FontStyle.Bold);
            int curX = 6;

            // 1. Botón Volver al Menú Principal
            ConfigurarBotonBarra(btnMenuPrincipal, "🏠 MENÚ", Color.FromArgb(130, 45, 60), curX, 78, fBtn);
            btnMenuPrincipal.Click += (s, e) =>
            {
                GestorEstadoGlobal.GuardarTodo();
                Close();
            };
            panelLanzador.Controls.Add(btnMenuPrincipal);
            curX += 82;

            // 2. Botón Pausa Táctica
            ConfigurarBotonBarra(btnPausa, "⏸ PAUSA [P]", Color.FromArgb(65, 85, 115), curX, 92, fBtn);
            btnPausa.Click += (s, e) =>
            {
                AlternarPausa();
                DevolverFocoAlLienzo();
            };
            panelLanzador.Controls.Add(btnPausa);
            curX += 96;

            // 3. Botón Nueva Misión / Reiniciar
            ConfigurarBotonBarra(btnLanzar, "🚀 REINICIAR [R]", Color.FromArgb(0, 150, 200), curX, 112, fBtn);
            btnLanzar.Click += (s, e) =>
            {
                _checkpointJefeAlcanzado = false;
                DesplegarMisionAerea(_modeloJugadorActual, _modeloRivalActual, _incluirEscoltaActual);
                DevolverFocoAlLienzo();
            };
            panelLanzador.Controls.Add(btnLanzar);
            curX += 116;

            // 4. Botón Alternar Aeronave Rápido (B-2 Spirit / F-35A)
            ConfigurarBotonBarra(btnAlternarModelo, "✈️ NAVE [1/2]", Color.FromArgb(20, 130, 160), curX, 138, fBtn);
            btnAlternarModelo.Click += (s, e) =>
            {
                TipoAeronave nuevoTipo = _modeloJugadorActual == TipoAeronave.B2_Spirit_Stealth
                    ? TipoAeronave.F35_Lightning_II
                    : TipoAeronave.B2_Spirit_Stealth;
                CambiarAeronaveJugadorDirecto(nuevoTipo);
            };
            panelLanzador.Controls.Add(btnAlternarModelo);
            curX += 142;

            // 5. Botón Añadir Escolta
            ConfigurarBotonBarra(btnEscolta, "➕ ESCOLTA", Color.FromArgb(30, 140, 95), curX, 88, fBtn);
            btnEscolta.Click += (s, e) =>
            {
                LanzarEscoltaAdicional();
                DevolverFocoAlLienzo();
            };
            panelLanzador.Controls.Add(btnEscolta);
            curX += 92;

            // 6. Botón Tienda y Hangar
            ConfigurarBotonBarra(btnTiendaHangar, "🛒 TIENDA", Color.FromArgb(195, 130, 20), curX, 86, fBtn);
            btnTiendaHangar.Click += (s, e) =>
            {
                bool estabaPausado = _juegoPausado;
                _juegoPausado = true;
                using var dlg = new VentanaCentroMando(PestanaCentroMando.Tienda);
                dlg.ShowDialog(this);
                AplicarMejorasANaveJugador();
                _juegoPausado = estabaPausado;
                DevolverFocoAlLienzo();
            };
            panelLanzador.Controls.Add(btnTiendaHangar);
            curX += 90;

            // 7. Botón Subventana Misión Táctica
            ConfigurarBotonBarra(btnMision, "🎯 MISIÓN", Color.FromArgb(170, 110, 30), curX, 84, fBtn);
            btnMision.Click += (s, e) =>
            {
                using var dlg = new VentanaAjustesMision(
                    _nombrePilotoActual,
                    _modeloJugadorActual,
                    _modeloRivalActual,
                    _incluirEscoltaActual,
                    (piloto, tJug, tRiv, escolta) =>
                    {
                        _nombrePilotoActual = piloto;
                        GestorEstadoGlobal.Perfil.NombrePiloto = piloto;
                        _modeloJugadorActual = tJug;
                        _modeloRivalActual = tRiv;
                        _incluirEscoltaActual = escolta;
                        ActualizarBotonModelo();
                        DesplegarMisionAerea(_modeloJugadorActual, _modeloRivalActual, _incluirEscoltaActual);
                    });
                dlg.ShowDialog(this);
                DevolverFocoAlLienzo();
            };
            panelLanzador.Controls.Add(btnMision);
            curX += 88;

            // 8. Botón Subventana Red Multijugador UDP
            ConfigurarBotonBarra(btnRed, "🌐 LOBBY RED", Color.FromArgb(110, 65, 195), curX, 98, fBtn);
            btnRed.Click += (s, e) =>
            {
                using var dlg = new VentanaLobbyMultijugador(_gestorRed);
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.IniciarPartidaConfirmada)
                {
                    ConfigurarPartidaEspecifica(
                        dlg.ModalidadSeleccionada,
                        _nivelCampanaActual,
                        dlg.ModoRedSeleccionado,
                        dlg.IpSeleccionada,
                        dlg.PuertoSeleccionado,
                        dlg.EquipoSeleccionado);
                }
                DevolverFocoAlLienzo();
            };
            panelLanzador.Controls.Add(btnRed);
            curX += 102;

            // 9. Botón Subventana Telemetría y Profiling (F3/F4)
            ConfigurarBotonBarra(btnMetricas, "📊 F3", Color.FromArgb(75, 65, 140), curX, 64, fBtn);
            btnMetricas.Click += (s, e) =>
            {
                using var dlg = new VentanaReporteMetricas(_profiler, _gestorRed, _naveJugador);
                dlg.ShowDialog(this);
                DevolverFocoAlLienzo();
            };
            panelLanzador.Controls.Add(btnMetricas);
            curX += 68;

            // 10. Botón Subventana Acerca de y Créditos UNAJ
            ConfigurarBotonBarra(btnAcercaDe, "ℹ️ UNAJ", Color.FromArgb(55, 70, 90), curX, 72, fBtn);
            btnAcercaDe.Click += (s, e) =>
            {
                using var dlg = new VentanaAcercaDe();
                dlg.ShowDialog(this);
                DevolverFocoAlLienzo();
            };
            panelLanzador.Controls.Add(btnAcercaDe);

            this.Controls.Add(panelLanzador);
        }

        private static void ConfigurarBotonBarra(Button btn, string texto, Color fondo, int x, int ancho, Font fuente)
        {
            btn.Text = texto;
            btn.Font = fuente;
            btn.Location = new Point(x, 6);
            btn.Size = new Size(ancho, 30);
            btn.BackColor = fondo;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(80, 255, 255, 255);
            btn.TabStop = false;
            btn.Cursor = Cursors.Hand;
        }

        private void ActualizarBotonModelo()
        {
            btnAlternarModelo.Text = _modeloJugadorActual == TipoAeronave.B2_Spirit_Stealth
                ? "✈️ B-2 SPIRIT [1/2]"
                : "✈️ F-35A [1/2]";
        }

        public void DevolverFocoAlLienzo()
        {
            if (contiene.CanFocus)
            {
                contiene.Focus();
            }
            this.ActiveControl = contiene;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            DevolverFocoAlLienzo();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            DevolverFocoAlLienzo();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            _teclasActivas.Clear();
            base.OnDeactivate(e);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            Keys k = keyData & Keys.KeyCode;
            if (k == Keys.Left || k == Keys.Right || k == Keys.Up || k == Keys.Down)
            {
                return false;
            }
            return base.ProcessDialogKey(keyData);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_KEYDOWN = 0x0100;
            const int WM_KEYUP = 0x0101;
            const int WM_SYSKEYDOWN = 0x0104;
            const int WM_SYSKEYUP = 0x0105;

            if (m.Msg == WM_KEYDOWN || m.Msg == WM_SYSKEYDOWN)
            {
                Keys k = (Keys)(int)m.WParam;
                if (EsTeclaJuego(k))
                {
                    _teclasActivas.Add(k);
                }
            }
            else if (m.Msg == WM_KEYUP || m.Msg == WM_SYSKEYUP)
            {
                Keys k = (Keys)(int)m.WParam;
                if (EsTeclaJuego(k))
                {
                    _teclasActivas.Remove(k);
                }
            }

            base.WndProc(ref m);
        }

        private static bool EsTeclaJuego(Keys k)
        {
            return k == Keys.Left || k == Keys.Right || k == Keys.Up || k == Keys.Down ||
                   k == Keys.W || k == Keys.A || k == Keys.S || k == Keys.D ||
                   k == Keys.Space || k == Keys.Enter;
        }

        // =========================================================================
        // DESPLIEGUE DE MISIONES, OLEADAS Y JEFES FINALES
        // =========================================================================
        public void DesplegarMisionAerea(TipoAeronave modeloJugador, TipoAeronave modeloRival, bool incluirEscolta)
        {
            _modeloJugadorActual = modeloJugador;
            _modeloRivalActual = modeloRival;
            _incluirEscoltaActual = incluirEscolta;

            _aeronaves.Clear();
            _aeronavesRemotas.Clear();
            _proyectiles.Clear();
            _particulas.Clear();
            _textosImpacto.Clear();
            _suministros.Clear();
            _obstaculosAmbientales.Clear();
            _teclasActivas.Clear();

            _juegoTerminado = false;
            _juegoPausado = false;
            _victoriaJugador = false;
            _mensajeResultado = "";
            _comboActual = 1;
            _comboMaximoPartida = 1;
            _ticksExpiracionCombo = 0;
            _monedasRecolectadasPartida = 0;
            _disparosPartida = 0;
            _impactosPartida = 0;
            _impactosCriticosPartida = 0;
            _cargaPoderEspecial = 100f;
            _ticksPoderActivo = 0;
            _ticksInvulnerableJugador = 60; // 1s de protección inicial
            _ticksEnemigosParalizados = 0;
            _vidasReserva = 1;
            _usoReaparicionEnNivel = false;

            int w = Math.Max(600, contiene.Width);
            int h = Math.Max(500, contiene.Height);
            string nombrePiloto = string.IsNullOrWhiteSpace(_nombrePilotoActual) ? "CÓNDOR-01" : _nombrePilotoActual.Trim();

            // Calcular estadísticas mejoradas del jugador desde su inventario/tienda
            PerfilJugador perf = GestorEstadoGlobal.Perfil;
            int vidaJugador = 30 + (perf.ObtenerNivelMejora("MEJORA_BLINDAJE") * 6) + (perf.AeronaveEquipadaId == "NAVE_CONDOR_X" ? 8 : 0);

            float xJug = (w * 0.5f) - 60f;
            float yJug = h - 165f;
            _naveJugador = CrearInstanciaPolimorfica(
                $"JUG-{Environment.ProcessId}",
                nombrePiloto,
                modeloJugador,
                esRival: false,
                xJug,
                yJug,
                vidaPersonalizada: vidaJugador);
            AplicarMejorasANaveJugador();
            _aeronaves.Add(_naveJugador);

            // Configurar oleadas según modalidad
            if (_modalidadActual == ModalidadPartida.TutorialInteractivo)
            {
                _tutorial.Reiniciar();
                _oleadaActual = 1;
                _totalOleadasNivel = 1;
                _enFaseJefe = false;
                // Generar un suministro inicial para el Paso 3 del tutorial
                GenerarSuministroEnPosicion(w * 0.5f, h * 0.52f, TipoSuministro.EnergiaHabilidad);
            }
            else if (_modalidadActual == ModalidadPartida.CampanaCasual)
            {
                var defNivel = GestorEstadoGlobal.NivelesCampana[Math.Clamp(_nivelCampanaActual - 1, 0, 4)];
                _totalOleadasNivel = defNivel.CantidadOleadasPrevias;
                if (_checkpointJefeAlcanzado)
                {
                    _oleadaActual = _totalOleadasNivel + 1;
                    DesplegarJefeFinalCampana(defNivel, w);
                }
                else
                {
                    _oleadaActual = 1;
                    _enFaseJefe = false;
                    DesplegarOleadaEnemiga(_oleadaActual, w);
                }
            }
            else
            {
                // Batalla Libre o Multijugador (1v1, 2v2, Cooperativo)
                _oleadaActual = 1;
                _totalOleadasNivel = 1;
                _enFaseJefe = false;

                int vidaRival = _modalidadActual == ModalidadPartida.Multijugador_Cooperativo ? 80 : 50;
                _naveRivalPrincipal = CrearInstanciaPolimorfica(
                    "RIVAL-LIDER",
                    modeloRival == TipoAeronave.B2_Spirit_Stealth ? "B2-SPIRIT-BOSS" : "F35-RIVAL-01",
                    modeloRival,
                    esRival: true,
                    x: w * 0.35f,
                    y: 32f,
                    vidaPersonalizada: vidaRival);
                _aeronaves.Add(_naveRivalPrincipal);

                if (incluirEscolta || _modalidadActual == ModalidadPartida.Multijugador_2v2 || _modalidadActual == ModalidadPartida.Multijugador_Cooperativo)
                {
                    TipoAeronave segundoRival = (modeloRival == TipoAeronave.B2_Spirit_Stealth)
                        ? TipoAeronave.F35_Lightning_II
                        : TipoAeronave.B2_Spirit_Stealth;

                    AeronaveBase escoltaRival = CrearInstanciaPolimorfica(
                        "RIVAL-ESCOLTA",
                        segundoRival == TipoAeronave.B2_Spirit_Stealth ? "B2-1071-RIVAL" : "F35-WING-02",
                        segundoRival,
                        esRival: true,
                        x: w * 0.68f,
                        y: 44f,
                        vidaPersonalizada: 40);
                    _aeronaves.Add(escoltaRival);
                }
            }

            ActualizarBotonModelo();
            ActualizarBarraEstadoInferior();
            DevolverFocoAlLienzo();
        }

        private void AplicarMejorasANaveJugador()
        {
            if (_naveJugador == null) return;
            PerfilJugador perf = GestorEstadoGlobal.Perfil;
            int bonusEscudo = perf.ObtenerNivelMejora("MEJORA_ESCUDO") * 4;
            _naveJugador.EscudoActual = Math.Max(_naveJugador.EscudoActual, 12 + bonusEscudo);
        }

        private void DesplegarOleadaEnemiga(int numeroOleada, int anchoLienzo)
        {
            _enFaseJefe = false;
            int cantidadEnemigos = Math.Min(4, 1 + numeroOleada);
            int vidaBase = 22 + (_nivelCampanaActual * 6) + (numeroOleada * 4);

            for (int i = 0; i < cantidadEnemigos; i++)
            {
                TipoAeronave tipo = ((i + numeroOleada) % 2 == 0)
                    ? TipoAeronave.F35_Lightning_II
                    : TipoAeronave.B2_Spirit_Stealth;

                float x = 80f + (i * ((anchoLienzo - 220f) / Math.Max(1, cantidadEnemigos)));
                float y = 26f + (i % 2) * 28f;

                var enemigo = CrearInstanciaPolimorfica(
                    $"OLEADA-{numeroOleada}-{i + 1}",
                    $"HOSTIL-N{_nivelCampanaActual}-{i + 1}",
                    tipo,
                    esRival: true,
                    x: x,
                    y: y,
                    vidaPersonalizada: vidaBase);

                if (i == 0) _naveRivalPrincipal = enemigo;
                _aeronaves.Add(enemigo);
            }
        }

        private void DesplegarJefeFinalCampana(DefinicionNivelCampana defNivel, int anchoLienzo)
        {
            _enFaseJefe = true;
            _checkpointJefeAlcanzado = true;
            _nombreJefeActual = defNivel.NombreJefeFinal;
            _vidaMaximaJefeActual = defNivel.VidaJefe;

            _naveRivalPrincipal = CrearInstanciaPolimorfica(
                $"JEFE-NIVEL-{defNivel.NumeroNivel}",
                defNivel.NombreJefeFinal,
                defNivel.ModeloJefe,
                esRival: true,
                x: (anchoLienzo * 0.5f) - 70f,
                y: 34f,
                vidaPersonalizada: defNivel.VidaJefe);

            _aeronaves.Add(_naveRivalPrincipal);

            // Escolta de guardia para niveles avanzados
            if (defNivel.NumeroNivel >= 2)
            {
                var guardia = CrearInstanciaPolimorfica(
                    $"GUARDIA-JEFE-{defNivel.NumeroNivel}",
                    "ESCOLTA-PRETORIANA",
                    TipoAeronave.F35_Lightning_II,
                    esRival: true,
                    x: anchoLienzo * 0.22f,
                    y: 52f,
                    vidaPersonalizada: 28 + defNivel.NumeroNivel * 5);
                _aeronaves.Add(guardia);
            }

            _efectosVisuales.Destello.Disparar(Color.FromArgb(255, 255, 80, 80), 0.45f);
            SintetizadorAudioTactico.ReproducirAlertaRadar();
        }

        public static AeronaveBase CrearInstanciaPolimorfica(
            string id,
            string piloto,
            TipoAeronave tipo,
            bool esRival,
            float x,
            float y,
            int vidaPersonalizada = 0)
        {
            return tipo switch
            {
                TipoAeronave.B2_Spirit_Stealth => new BombarderoB2Spirit(id, piloto, esRival, x, y, vidaPersonalizada),
                _ => new CazaF35Lightning(id, piloto, esRival, x, y, vidaPersonalizada)
            };
        }

        public void CambiarAeronaveJugadorDirecto(TipoAeronave nuevoTipo)
        {
            if (_naveJugador == null) return;
            if (_naveJugador.Tipo == nuevoTipo && _naveJugador.Activo) return;

            _modeloJugadorActual = nuevoTipo;
            int idx = _aeronaves.IndexOf(_naveJugador);
            AeronaveBase nueva = CrearInstanciaPolimorfica(
                _naveJugador.IdAeronave,
                _nombrePilotoActual,
                nuevoTipo,
                esRival: false,
                _naveJugador.X,
                _naveJugador.Y,
                vidaPersonalizada: Math.Max(20, _naveJugador.VidaActual));

            nueva.PuntajeTactico = _naveJugador.PuntajeTactico;
            nueva.BajasConfirmadas = _naveJugador.BajasConfirmadas;

            _naveJugador = nueva;
            AplicarMejorasANaveJugador();
            if (idx >= 0) _aeronaves[idx] = nueva;
            else _aeronaves.Add(nueva);

            GenerarExplosionParticulas(_naveJugador.X + _naveJugador.Ancho * 0.5f, _naveJugador.Y + _naveJugador.Alto * 0.5f, Color.Cyan, 16);
            ActualizarBotonModelo();
            ActualizarBarraEstadoInferior();
            DevolverFocoAlLienzo();
        }

        public void LanzarEscoltaAdicional()
        {
            TipoAeronave tipoEscolta = (_aeronaves.Count % 2 == 0)
                ? TipoAeronave.B2_Spirit_Stealth
                : TipoAeronave.F35_Lightning_II;

            float x = _rng.Next(60, Math.Max(120, contiene.Width - 150));
            var escolta = CrearInstanciaPolimorfica(
                $"RIVAL-EXTRA-{_aeronaves.Count}",
                tipoEscolta == TipoAeronave.B2_Spirit_Stealth ? "B2-SPIRIT-1071" : "F35-INTERCEPTOR",
                tipoEscolta,
                esRival: true,
                x: x,
                y: _rng.Next(18, 75),
                vidaPersonalizada: 35);

            _aeronaves.Add(escolta);
            GenerarExplosionParticulas(x + 50, 50, Color.OrangeRed, 15);
        }

        private void AlternarPausa()
        {
            if (_juegoTerminado) return;
            _juegoPausado = !_juegoPausado;
            _teclasActivas.Clear();
            contiene.Invalidate();
        }

        // =========================================================================
        // PODERES ESPECIALES ACTIVOS [Q] / [E]
        // =========================================================================
        private void IntentarActivarPoderEspecial()
        {
            if (_juegoPausado || _juegoTerminado || _naveJugador == null || !_naveJugador.Activo) return;
            if (_cargaPoderEspecial < 100f) return;

            _cargaPoderEspecial = 0f;
            TipoPoderEspecial poder = GestorEstadoGlobal.ObtenerPoderEspecialEquipado();
            float cx = _naveJugador.X + _naveJugador.Ancho * 0.5f;
            float cy = _naveJugador.Y + _naveJugador.Alto * 0.5f;

            if (_modalidadActual == ModalidadPartida.TutorialInteractivo)
            {
                _tutorial.PoderEspecialUsado = true;
            }

            switch (poder)
            {
                case TipoPoderEspecial.PulsoElectromagnetico_EMP:
                    // Destruye todos los proyectiles rivales y paraliza enemigos por 3.5 s (210 ticks)
                    for (int i = 0; i < _proyectiles.Count; i++)
                    {
                        if (_proyectiles[i].EsRival) _proyectiles[i].Activo = false;
                    }
                    _ticksEnemigosParalizados = 210;
                    _efectosVisuales.CrearOndaExpansiva(cx, cy, radioMax: 480f, Color.Cyan, velocidad: 550f);
                    _efectosVisuales.Destello.Disparar(Color.FromArgb(255, 80, 230, 255), 0.50f);
                    break;

                case TipoPoderEspecial.EscudoCuanticoSobrecarga:
                    _ticksInvulnerableJugador = 270; // 4.5 s invulnerable
                    _naveJugador.EscudoActual += 8;
                    _efectosVisuales.CrearOndaExpansiva(cx, cy, radioMax: 220f, Color.FromArgb(80, 255, 160), velocidad: 380f);
                    _efectosVisuales.Destello.Disparar(Color.FromArgb(255, 80, 255, 170), 0.40f);
                    break;

                case TipoPoderEspecial.SobrecargaVulcan_Overdrive:
                    _ticksPoderActivo = 330; // 5.5 s de fuego triple acelerado
                    _efectosVisuales.CrearOndaExpansiva(cx, cy, radioMax: 200f, Color.Orange, velocidad: 400f);
                    break;

                case TipoPoderEspecial.SalvaTermobarica:
                    _efectosVisuales.CrearOndaExpansiva(cx, cy, radioMax: 520f, Color.FromArgb(255, 70, 110), velocidad: 600f);
                    _efectosVisuales.Destello.Disparar(Color.FromArgb(255, 255, 90, 60), 0.65f);
                    _efectosVisuales.Camara.AgregarTrauma(0.55f);
                    for (int i = 0; i < _aeronaves.Count; i++)
                    {
                        var a = _aeronaves[i];
                        if (a.EsRival && a.Activo)
                        {
                            a.VidaActual = Math.Max(0, a.VidaActual - 18);
                            GenerarExplosionParticulas(a.X + a.Ancho * 0.5f, a.Y + a.Alto * 0.5f, Color.OrangeRed, 28);
                            if (a.VidaActual <= 0)
                            {
                                a.Activo = false;
                                _naveJugador.BajasConfirmadas++;
                                _naveJugador.PuntajeTactico += 250;
                            }
                        }
                    }
                    break;
            }

            SintetizadorAudioTactico.ReproducirMisil();
        }

        // =========================================================================
        // BUCLE PRINCIPAL DE TIEMPO REAL (FÍSICA, COLISIONES BIG-O, RED Y PROFILING)
        // =========================================================================
        private void ImpactarTick(object? sender, EventArgs e)
        {
            if (_juegoPausado)
            {
                contiene.Invalidate();
                return;
            }

            _profiler.IniciarMedicionUpdate();

            Size limites = contiene.ClientSize;
            if (limites.Width < 100 || limites.Height < 100)
            {
                limites = new Size(800, 600);
            }

            _offsetRadarY = (_offsetRadarY + 1.2f) % 64f;

            // 1. Mover nubes estratosféricas
            for (int i = 0; i < _nubesEstratosfera.Length; i++)
            {
                float ny = _nubesEstratosfera[i].Y + (i % 3 + 1) * 0.85f;
                float nx = _nubesEstratosfera[i].X;
                if (ny > limites.Height + 40)
                {
                    ny = -30f;
                    nx = _rng.Next(20, Math.Max(100, limites.Width - 40));
                }
                _nubesEstratosfera[i] = new PointF(nx, ny);
            }

            if (!_juegoTerminado)
            {
                // Actualizar temporizadores de estados tácticos
                if (_ticksPoderActivo > 0) _ticksPoderActivo--;
                if (_ticksInvulnerableJugador > 0) _ticksInvulnerableJugador--;
                if (_ticksEnemigosParalizados > 0) _ticksEnemigosParalizados--;
                if (_cargaPoderEspecial < 100f)
                {
                    _cargaPoderEspecial = Math.Min(100f, _cargaPoderEspecial + 0.08f);
                }

                if (_ticksExpiracionCombo > 0)
                {
                    _ticksExpiracionCombo--;
                    if (_ticksExpiracionCombo == 0) _comboActual = 1;
                }

                // 2. Procesar entrada continua del piloto local
                ProcesarMovimientoContinuoJugador(limites);

                // 3. Actualizar Tutorial si está activo
                if (_modalidadActual == ModalidadPartida.TutorialInteractivo)
                {
                    int pasoPrevio = _tutorial.PasoActual;
                    _tutorial.AvanzarPasoSiCorresponde(out bool avanzo);
                    if (avanzo)
                    {
                        SintetizadorAudioTactico.ReproducirAlertaRadar();
                        if (_tutorial.PasoActual == 5 && !_aeronaves.Any(a => a.EsRival && a.Activo))
                        {
                            var dron = CrearInstanciaPolimorfica("DRON-TUTORIAL", "DRON-INSTRUCTOR", TipoAeronave.F35_Lightning_II, true, limites.Width * 0.45f, 45f, 20);
                            _naveRivalPrincipal = dron;
                            _aeronaves.Add(dron);
                        }
                    }
                }

                // 4. Actualizar física, IA y patrones de Jefe Final
                _contadorTickIA++;
                for (int i = 0; i < _aeronaves.Count; i++)
                {
                    AeronaveBase nave = _aeronaves[i];
                    if (!nave.Activo) continue;

                    if (nave.EsRival && _ticksEnemigosParalizados > 0)
                    {
                        // Enemigo paralizado por Pulso EMP
                        continue;
                    }

                    nave.ActualizarFisica(limites);

                    if (nave.Acelerando && (_contadorTickIA % 2 == 0))
                    {
                        GenerarEstelaPropulsion(nave);
                    }

                    if (nave.VidaActual < 20 && (_contadorTickIA % 3 == 0))
                    {
                        GenerarExplosionParticulas(
                            nave.X + nave.Ancho * 0.5f + _rng.Next(-8, 9),
                            nave.Y + (nave.EsRival ? 10f : nave.Alto - 10f),
                            Color.FromArgb(255, 65, 20),
                            4);
                    }

                    // Disparo de IA y Patrones Especiales de Jefe Final
                    if (nave.EsRival && !nave.ControladaPorRed)
                    {
                        int cadenciaIA = _enFaseJefe ? Math.Max(22, 38 - _nivelCampanaActual * 3) : 42;
                        if (_contadorTickIA % cadenciaIA == 0)
                        {
                            List<Proyectil> salvasRival = nave.Disparar();
                            for (int s = 0; s < salvasRival.Count; s++)
                            {
                                _proyectiles.Add(salvasRival[s]);
                            }

                            // Si es el Jefe Final, ejecutar patrón de abanico o ráfaga lateral
                            if (_enFaseJefe && nave == _naveRivalPrincipal && (_contadorTickIA % (cadenciaIA * 2) == 0))
                            {
                                EjecutarPatronAtaqueJefe(nave);
                            }
                        }
                    }
                }

                // 5. Actualizar Peligros Ambientales y Suministros en Vuelo
                ActualizarPeligrosAmbientales(limites);
                ActualizarSuministros(limites);

                // 6. Actualizar proyectiles y compactar inactivos en O(1) mediante Swap-and-Pop
                for (int i = _proyectiles.Count - 1; i >= 0; i--)
                {
                    Proyectil p = _proyectiles[i];
                    p.ActualizarFisica(limites);
                    if (!p.Activo)
                    {
                        int ultimo = _proyectiles.Count - 1;
                        _proyectiles[i] = _proyectiles[ultimo];
                        _proyectiles.RemoveAt(ultimo);
                    }
                }

                // 7. Detección de colisiones mediante Grilla Espacial Hash en O(N + K)
                _grillaColisiones.ProcesarColisiones(_proyectiles, _aeronaves, ProcesarImpactoConfirmado);

                for (int i = _proyectiles.Count - 1; i >= 0; i--)
                {
                    if (!_proyectiles[i].Activo)
                    {
                        int ultimo = _proyectiles.Count - 1;
                        _proyectiles[i] = _proyectiles[ultimo];
                        _proyectiles.RemoveAt(ultimo);
                    }
                }

                // 8. Sincronización de Red Cliente-Servidor
                _contadorTickRed++;
                if (_contadorTickRed % 2 == 0 && _naveJugador.Activo)
                {
                    _gestorRed.EnviarEstadoAeronave(_naveJugador, disparoEnEsteTick: false);
                }
                ProcesarPaquetesRedEntrantes();

                if (_contadorTickIA == 30 || _contadorTickIA % 300 == 0)
                {
                    try { _profiler.ExportarReporteMetricas(_gestorRed, _naveJugador); } catch { }
                }

                // 9. Verificar avance de oleadas, checkpoints o Victoria/Derrota
                VerificarEstadoPartida(limites);
            }

            // 10. Actualizar partículas y textos flotantes
            ActualizarEfectosVisuales();

            float escalaShake = GestorEstadoGlobal.Configuracion.IntensidadSacudidaCamara / 100f;
            _efectosVisuales.Actualizar(0.016f * Math.Max(0.2f, escalaShake));

            if (_naveJugador != null && _naveJugador.Activo)
            {
                _hudMilitar.Actualizar(0.016f, _naveJugador, _aeronaves);
            }

            if (tiempoImpacto > 0)
            {
                tiempoImpacto--;
                if (tiempoImpacto == 0) ActualizarBarraEstadoInferior();
            }
            if (tiempoImpactoNavex > 0)
            {
                tiempoImpactoNavex--;
                if (tiempoImpactoNavex == 0) ActualizarBarraEstadoInferior();
            }

            _profiler.FinalizarMedicionUpdate(
                _proyectiles.Count,
                _aeronaves.Count,
                _grillaColisiones.UltimasComparacionesFuerzaBruta,
                _grillaColisiones.UltimasComparacionesReales);

            contiene.Invalidate();
        }

        private void EjecutarPatronAtaqueJefe(AeronaveBase jefe)
        {
            float cx = jefe.X + jefe.Ancho * 0.5f;
            float cy = jefe.Y + jefe.Alto - 8f;

            // Salva en abanico proporcional al nivel del jefe
            int numProyectiles = Math.Min(5, 2 + _nivelCampanaActual);
            float anguloInicial = -3.2f;
            float paso = (numProyectiles > 1) ? (6.4f / (numProyectiles - 1)) : 0f;

            for (int i = 0; i < numProyectiles; i++)
            {
                float vx = anguloInicial + (i * paso);
                _proyectiles.Add(new Proyectil(
                    jefe.IdAeronave,
                    esRival: true,
                    jefe.Tipo,
                    cx - 4f,
                    cy,
                    vx,
                    10.5f + (_nivelCampanaActual * 0.8f)));
            }

            // En Nivel 5 (Apu-Supay), regenera 2 puntos de escudo cada salva especial
            if (_nivelCampanaActual == 5 && jefe.EscudoActual < 16)
            {
                jefe.EscudoActual += 2;
            }
        }

        private void ActualizarPeligrosAmbientales(Size limites)
        {
            if (_modalidadActual != ModalidadPartida.CampanaCasual) return;

            var defNivel = GestorEstadoGlobal.NivelesCampana[Math.Clamp(_nivelCampanaActual - 1, 0, 4)];
            TipoPeligroAmbiental tipoPeligro = defNivel.PeligroAmbiental;

            // Nivel 1: Viento cruzado suave
            if (tipoPeligro == TipoPeligroAmbiental.VientoCruzadoAndino && _naveJugador.Activo)
            {
                float empujeViento = (float)Math.Sin(_contadorTickIA * 0.03) * 0.12f;
                _naveJugador.Mover(empujeViento, 0f, limites);
            }

            // Generar nuevos obstáculos periódicamente en Niveles 2 a 5
            if (tipoPeligro != TipoPeligroAmbiental.VientoCruzadoAndino && (_contadorTickIA % 115 == 0))
            {
                float xAzar = _rng.Next(70, Math.Max(150, limites.Width - 70));
                if (tipoPeligro == TipoPeligroAmbiental.TormentaElectrica || tipoPeligro == TipoPeligroAmbiental.LaserOrbitalSatelital)
                {
                    _obstaculosAmbientales.Add(new ObstaculoAmbiental
                    {
                        X = xAzar,
                        Y = 0,
                        RadioOAncho = tipoPeligro == TipoPeligroAmbiental.LaserOrbitalSatelital ? 36f : 26f,
                        Tipo = tipoPeligro,
                        TicksAdvertencia = 55,
                        TicksActivos = 28,
                        Activo = true
                    });
                }
                else
                {
                    _obstaculosAmbientales.Add(new ObstaculoAmbiental
                    {
                        X = xAzar,
                        Y = -20f,
                        VX = (_rng.NextSingle() - 0.5f) * 2.4f,
                        VY = tipoPeligro == TipoPeligroAmbiental.MeteoritosPiroclasticos ? 5.8f : 4.2f,
                        RadioOAncho = tipoPeligro == TipoPeligroAmbiental.MeteoritosPiroclasticos ? 16f : 12f,
                        Tipo = tipoPeligro,
                        TicksAdvertencia = 0,
                        TicksActivos = 300,
                        Activo = true
                    });
                }
            }

            // Actualizar física y colisión de obstáculos con el jugador
            for (int i = _obstaculosAmbientales.Count - 1; i >= 0; i--)
            {
                ObstaculoAmbiental obs = _obstaculosAmbientales[i];
                if (!obs.Activo)
                {
                    _obstaculosAmbientales.RemoveAt(i);
                    continue;
                }

                if (obs.TicksAdvertencia > 0)
                {
                    obs.TicksAdvertencia--;
                    if (obs.TicksAdvertencia == 0)
                    {
                        SintetizadorAudioTactico.ReproducirExplosion(esCritica: false);
                    }
                }
                else if (obs.TicksActivos > 0)
                {
                    obs.TicksActivos--;
                    obs.X += obs.VX;
                    obs.Y += obs.VY;

                    if (obs.Y > limites.Height + 40 || obs.TicksActivos <= 0)
                    {
                        obs.Activo = false;
                    }
                    else if (_naveJugador.Activo && _ticksInvulnerableJugador <= 0 && obs.IntersectaAeronave(_naveJugador))
                    {
                        obs.Activo = false;
                        _naveJugador.VidaActual = Math.Max(0, _naveJugador.VidaActual - 4);
                        _comboActual = 1;
                        _ticksInvulnerableJugador = 35;
                        _efectosVisuales.Camara.AgregarTrauma(0.28f);
                        GenerarExplosionParticulas(_naveJugador.X + _naveJugador.Ancho * 0.5f, _naveJugador.Y + 20f, Color.Gold, 16);
                    }
                }

                _obstaculosAmbientales[i] = obs;
            }
        }

        private void GenerarSuministroEnPosicion(float x, float y, TipoSuministro? tipoForzado = null)
        {
            TipoSuministro tipo = tipoForzado ?? (TipoSuministro)_rng.Next(0, 5);
            _suministros.Add(new SuministroCombate
            {
                X = x,
                Y = y,
                VY = 1.8f,
                Tipo = tipo,
                Activo = true,
                TicksVida = 600
            });
        }

        private void ActualizarSuministros(Size limites)
        {
            if (_naveJugador == null || !_naveJugador.Activo) return;
            RectangleF rectJug = new RectangleF(_naveJugador.X, _naveJugador.Y, _naveJugador.Ancho, _naveJugador.Alto);

            for (int i = _suministros.Count - 1; i >= 0; i--)
            {
                SuministroCombate s = _suministros[i];
                if (!s.Activo)
                {
                    _suministros.RemoveAt(i);
                    continue;
                }

                if (_modalidadActual != ModalidadPartida.TutorialInteractivo)
                {
                    s.Y += s.VY;
                }
                s.TicksVida--;

                if (s.Y > limites.Height + 30 || s.TicksVida <= 0)
                {
                    s.Activo = false;
                }
                else if (rectJug.IntersectsWith(s.ObtenerHitbox()))
                {
                    s.Activo = false;
                    AplicarSuministroRecolectado(s.Tipo);
                }

                _suministros[i] = s;
            }
        }

        private void AplicarSuministroRecolectado(TipoSuministro tipo)
        {
            SintetizadorAudioTactico.ReproducirAlertaRadar();
            if (_modalidadActual == ModalidadPartida.TutorialInteractivo)
            {
                _tutorial.SuministroRecolectado = true;
            }

            string etiqueta = "";
            switch (tipo)
            {
                case TipoSuministro.ReparacionCasco:
                    _naveJugador.VidaActual = Math.Min(65, _naveJugador.VidaActual + 8);
                    etiqueta = "+8 HP CASCO";
                    break;
                case TipoSuministro.RecargaEscudo:
                    _naveJugador.EscudoActual = Math.Min(35, _naveJugador.EscudoActual + 6);
                    etiqueta = "+6 ESCUDO RAM";
                    break;
                case TipoSuministro.EnergiaHabilidad:
                    _cargaPoderEspecial = Math.Min(100f, _cargaPoderEspecial + 45f);
                    etiqueta = "+45% PODER [Q]";
                    break;
                case TipoSuministro.CreditosExtra:
                    _monedasRecolectadasPartida += 35;
                    etiqueta = "+35 CRÉDITOS";
                    break;
                case TipoSuministro.PotenciadorDano:
                    _ticksPoderActivo = Math.Max(_ticksPoderActivo, 240);
                    etiqueta = "¡HIPER-FUEGO!";
                    break;
            }

            _textosImpacto.Add(new TextoFlotanteImpacto
            {
                X = _naveJugador.X,
                Y = _naveJugador.Y - 18f,
                Texto = etiqueta,
                ColorTexto = Color.FromArgb(255, 80, 255, 160),
                TicksRestantes = 45,
                EsCritico = false
            });
        }

        private void ProcesarMovimientoContinuoJugador(Size limites)
        {
            if (!_naveJugador.Activo) return;

            ConfiguracionJuego cfg = GestorEstadoGlobal.Configuracion;
            float factorSensibilidad = Math.Clamp(cfg.SensibilidadVuelo / 100f, 0.5f, 1.6f);

            float dx = 0f;
            float dy = 0f;

            bool permitirWASD = cfg.EsquemaControl == 0 || cfg.EsquemaControl == 1;
            bool permitirFlechas = cfg.EsquemaControl == 0 || cfg.EsquemaControl == 2;

            if (permitirFlechas)
            {
                if (_teclasActivas.Contains(Keys.Left)) dx -= 1.0f;
                if (_teclasActivas.Contains(Keys.Right)) dx += 1.0f;
                if (_teclasActivas.Contains(Keys.Up)) dy -= 1.0f;
                if (_teclasActivas.Contains(Keys.Down)) dy += 1.0f;
            }
            if (permitirWASD)
            {
                if (_teclasActivas.Contains(Keys.A)) dx -= 1.0f;
                if (_teclasActivas.Contains(Keys.D)) dx += 1.0f;
                if (_teclasActivas.Contains(Keys.W)) dy -= 1.0f;
                if (_teclasActivas.Contains(Keys.S)) dy += 1.0f;
            }

            // Esquema guiado por ratón si está seleccionado
            if (cfg.EsquemaControl == 3)
            {
                float targetX = _ultimaPosicionRaton.X - (_naveJugador.Ancho * 0.5f);
                float targetY = _ultimaPosicionRaton.Y - (_naveJugador.Alto * 0.5f);
                float diffX = targetX - _naveJugador.X;
                float diffY = targetY - _naveJugador.Y;
                if (Math.Abs(diffX) > 12f) dx = Math.Sign(diffX);
                if (Math.Abs(diffY) > 12f) dy = Math.Sign(diffY);
            }

            if (Math.Abs(dx) > 0.01f || Math.Abs(dy) > 0.01f)
            {
                if (Math.Abs(dx) > 0.01f && Math.Abs(dy) > 0.01f)
                {
                    dx *= 0.7071f;
                    dy *= 0.7071f;
                }
                _naveJugador.Mover(dx * 0.65f * factorSensibilidad, dy * 0.65f * factorSensibilidad, limites);
                angulo = _naveJugador.AnguloInclinacion;

                if (_modalidadActual == ModalidadPartida.TutorialInteractivo)
                {
                    _tutorial.DistanciaVolada += 4.5f;
                }

                if (Math.Abs(_naveJugador.AnguloInclinacion) > 2.5f)
                {
                    _efectosVisuales.EmitirVorticeAla(_naveJugador.X + 6f, _naveJugador.Y + _naveJugador.Alto * 0.6f);
                    _efectosVisuales.EmitirVorticeAla(_naveJugador.X + _naveJugador.Ancho - 6f, _naveJugador.Y + _naveJugador.Alto * 0.6f);
                }
            }
            else
            {
                _naveJugador.AnguloInclinacion = 0f;
                _naveJugador.Acelerando = false;
                angulo = 0f;
            }

            if (_teclasActivas.Contains(Keys.Enter) || _teclasActivas.Contains(Keys.Space) || _teclasActivas.Contains(cfg.TeclaDisparo))
            {
                EjecutarDisparoJugador();
            }
        }

        public void EjecutarDisparoJugador()
        {
            if (!_naveJugador.Activo || _juegoPausado || _juegoTerminado) return;

            List<Proyectil> salvas = _naveJugador.Disparar();
            if (salvas.Count > 0)
            {
                _disparosPartida += salvas.Count;
                if (_modalidadActual == ModalidadPartida.TutorialInteractivo)
                {
                    _tutorial.DisparosRealizados++;
                }

                int nivelCanon = GestorEstadoGlobal.Perfil.ObtenerNivelMejora("MEJORA_CANON");
                if (nivelCanon > 0)
                {
                    _naveJugador.EnfriamientoDisparo = Math.Max(2, _naveJugador.EnfriamientoDisparo - nivelCanon);
                }
                for (int i = 0; i < salvas.Count; i++)
                {
                    _proyectiles.Add(salvas[i]);
                }

                // Si el poder Hiper-Vulcan Overdrive está activo, añadir dos proyectiles angulados extra
                if (_ticksPoderActivo > 0)
                {
                    float cx = _naveJugador.X + _naveJugador.Ancho * 0.5f;
                    _proyectiles.Add(new Proyectil(_naveJugador.IdAeronave, false, _naveJugador.Tipo, cx - 22f, _naveJugador.Y, -2.0f, -15f));
                    _proyectiles.Add(new Proyectil(_naveJugador.IdAeronave, false, _naveJugador.Tipo, cx + 18f, _naveJugador.Y, 2.0f, -15f));
                }

                _gestorRed.EnviarEstadoAeronave(_naveJugador, disparoEnEsteTick: true);

                _efectosVisuales.Camara.AgregarTrauma(0.08f);
                _efectosVisuales.EyectarCasquillo(
                    _naveJugador.X + _naveJugador.Ancho * 0.5f,
                    _naveJugador.Y + 20f,
                    haciaIzquierda: _contadorTickIA % 2 == 0);

                if (_naveJugador.Tipo == TipoAeronave.B2_Spirit_Stealth)
                {
                    SintetizadorAudioTactico.ReproducirMisil();
                }
                else
                {
                    SintetizadorAudioTactico.ReproducirCanon();
                }
            }
        }

        /// <summary>
        /// Procedimiento callback invocado por la Grilla Espacial O(N+K) al confirmar un impacto geométrico.
        /// </summary>
        private void ProcesarImpactoConfirmado(AeronaveBase naveImpactada, Proyectil proyectil, ResultadoImpacto resultado)
        {
            //Si el jugador tiene invulnerabilidad activa (Escudo Cuántico o reaparición), ignorar daño recibido
            if (!naveImpactada.EsRival && _ticksInvulnerableJugador > 0)
            {
                return;
            }

            bool esCritico = resultado.Zona == ZonaImpacto.CabinaCentralCritica;
            Color colorTexto = esCritico ? Color.FromArgb(255, 255, 55, 55) : Color.FromArgb(255, 255, 220, 90);

            _textosImpacto.Add(new TextoFlotanteImpacto
            {
                X = naveImpactada.X + (naveImpactada.Ancho * 0.5f) - 45f,
                Y = naveImpactada.EsRival ? (naveImpactada.Y + naveImpactada.Alto + 6f) : (naveImpactada.Y - 22f),
                Texto = esCritico ? $"CRÍTICO -{resultado.DanoTotal} (x{_comboActual})" : $"impact -{resultado.DanoRealVida}",
                ColorTexto = colorTexto,
                TicksRestantes = 42,
                EsCritico = esCritico
            });

            GenerarExplosionParticulas(
                proyectil.X + proyectil.Ancho * 0.5f,
                proyectil.Y + proyectil.Alto * 0.5f,
                esCritico ? Color.OrangeRed : Color.DeepSkyBlue,
                esCritico ? 22 : 10);

            if (esCritico)
            {
                _efectosVisuales.Camara.AgregarTrauma(0.38f);
                _efectosVisuales.Destello.Disparar(Color.FromArgb(255, 255, 130, 70), 0.42f);
                _efectosVisuales.CrearOndaExpansiva(proyectil.X, proyectil.Y, radioMax: 70f, Color.FromArgb(255, 110, 40));
                SintetizadorAudioTactico.ReproducirExplosion(esCritica: true);
            }
            else
            {
                _efectosVisuales.Camara.AgregarTrauma(0.12f);
                _efectosVisuales.CrearOndaExpansiva(proyectil.X, proyectil.Y, radioMax: 35f, Color.DeepSkyBlue);
                SintetizadorAudioTactico.ReproducirImpacto();
            }

            if (naveImpactada.EsRival)
            {
                _impactosPartida++;
                if (esCritico) _impactosCriticosPartida++;

                // Incrementar multiplicador de Combo y energía de Poder Especial
                _ticksExpiracionCombo = 190; // ~3 segundos para mantener la racha
                if (_impactosPartida % 2 == 0 && _comboActual < 8)
                {
                    _comboActual++;
                    if (_comboActual > _comboMaximoPartida) _comboMaximoPartida = _comboActual;
                }

                _cargaPoderEspecial = Math.Min(100f, _cargaPoderEspecial + (esCritico ? 14f : 6f));
                _naveJugador.PuntajeTactico += resultado.DanoTotal * 10 * _comboActual;

                if (!naveImpactada.Activo)
                {
                    _naveJugador.BajasConfirmadas++;
                    _monedasRecolectadasPartida += 20;

                    float cx = naveImpactada.X + naveImpactada.Ancho * 0.5f;
                    float cy = naveImpactada.Y + naveImpactada.Alto * 0.5f;

                    GenerarExplosionParticulas(cx, cy, Color.Gold, 45);
                    _efectosVisuales.Camara.AgregarTrauma(0.68f);
                    _efectosVisuales.Destello.Disparar(Color.FromArgb(255, 255, 230, 160), 0.65f);
                    _efectosVisuales.CrearOndaExpansiva(cx, cy, radioMax: 140f, Color.Gold, velocidad: 320f);
                    SintetizadorAudioTactico.ReproducirExplosion(esCritica: true);

                    // Soltar suministro aéreo al derribar enemigos
                    if (_rng.NextDouble() < 0.65)
                    {
                        GenerarSuministroEnPosicion(cx, cy);
                    }
                }

                if (esCritico)
                {
                    tiempoImpacto = 45;
                    toolStripStatusLabel1.ForeColor = Color.Red;
                    toolStripStatusLabel1.Text = $"Vida del Rival : {naveImpactada.VidaActual} (impact -10)";
                }
                else
                {
                    ActualizarBarraEstadoInferior();
                }
            }
            else
            {
                // El jugador recibió daño: reiniciar racha de combo
                _comboActual = 1;
                if (esCritico)
                {
                    tiempoImpactoNavex = 45;
                    toolStripStatusLabel2.ForeColor = Color.Red;
                    toolStripStatusLabel2.Text = $"Mi Nave : {_naveJugador.VidaActual} (impact -10)";
                }
                else
                {
                    ActualizarBarraEstadoInferior();
                }
            }
        }

        private void ProcesarPaquetesRedEntrantes()
        {
            int recibidos = _gestorRed.ExtraerPaquetesRecibidos(_bufferRed);
            if (recibidos == 0) return;

            for (int i = 0; i < _bufferRed.Count; i++)
            {
                PaqueteRedAeronave pkt = _bufferRed[i];
                if (pkt.IdAeronave == _naveJugador.IdAeronave) continue;

                if (pkt.TipoMensaje == "REMATCH")
                {
                    DesplegarMisionAerea(_modeloJugadorActual, _modeloRivalActual, _incluirEscoltaActual);
                    return;
                }

                bool esMismoEquipo = (_modalidadActual == ModalidadPartida.Multijugador_Cooperativo) ||
                                     (_modalidadActual == ModalidadPartida.Multijugador_2v2 && pkt.Equipo == _equipoLocal);

                if (!_aeronavesRemotas.TryGetValue(pkt.IdAeronave, out AeronaveBase? remota))
                {
                    remota = CrearInstanciaPolimorfica(
                        pkt.IdAeronave,
                        pkt.NombrePiloto,
                        pkt.Modelo,
                        esRival: !esMismoEquipo,
                        pkt.X,
                        esMismoEquipo ? pkt.Y : 80f,
                        vidaPersonalizada: Math.Max(20, pkt.Vida));
                    remota.ControladaPorRed = true;
                    _aeronavesRemotas[pkt.IdAeronave] = remota;
                    _aeronaves.Add(remota);
                }

                remota.X = pkt.X;
                remota.Y = esMismoEquipo ? pkt.Y : Math.Clamp(pkt.Y, 15f, 180f);
                remota.AnguloInclinacion = pkt.Angulo;
                remota.Acelerando = pkt.Acelerando;
                remota.VidaActual = pkt.Vida;

                if (pkt.DisparoActivo)
                {
                    List<Proyectil> disparos = remota.Disparar();
                    for (int d = 0; d < disparos.Count; d++)
                    {
                        _proyectiles.Add(disparos[d]);
                    }
                }
            }
        }

        private void VerificarEstadoPartida(Size limites)
        {
            // 1. Verificar si el jugador cayó y tiene vidas de reserva / checkpoint
            if (!_naveJugador.Activo || _naveJugador.VidaActual <= 0)
            {
                if (_vidasReserva > 0)
                {
                    _vidasReserva--;
                    _usoReaparicionEnNivel = true;
                    _naveJugador.Activo = true;
                    _naveJugador.VidaActual = 30 + (GestorEstadoGlobal.Perfil.ObtenerNivelMejora("MEJORA_BLINDAJE") * 6);
                    _naveJugador.EscudoActual = 12 + (GestorEstadoGlobal.Perfil.ObtenerNivelMejora("MEJORA_ESCUDO") * 4);
                    _ticksInvulnerableJugador = 150; // 2.5s de invulnerabilidad tras reaparecer
                    _efectosVisuales.CrearOndaExpansiva(_naveJugador.X + 50, _naveJugador.Y + 40, 180f, Color.LimeGreen);
                    _textosImpacto.Add(new TextoFlotanteImpacto
                    {
                        X = _naveJugador.X - 20f,
                        Y = _naveJugador.Y - 24f,
                        Texto = "¡REAPARICIÓN DE EMERGENCIA ACTIVA!",
                        ColorTexto = Color.LimeGreen,
                        TicksRestantes = 65,
                        EsCritico = true
                    });
                    return;
                }

                FinalizarPartida(victoria: false);
                return;
            }

            // 2. Verificar si quedan enemigos activos en la oleada actual
            bool quedaAlgunRival = false;
            for (int i = 0; i < _aeronaves.Count; i++)
            {
                if (_aeronaves[i].EsRival && _aeronaves[i].Activo && _aeronaves[i].VidaActual > 0)
                {
                    quedaAlgunRival = true;
                    break;
                }
            }

            if (!quedaAlgunRival)
            {
                if (_modalidadActual == ModalidadPartida.TutorialInteractivo)
                {
                    if (_tutorial.PasoActual >= 5)
                    {
                        GestorEstadoGlobal.Perfil.TutorialCompletado = true;
                        FinalizarPartida(victoria: true);
                    }
                    return;
                }

                if (_modalidadActual == ModalidadPartida.CampanaCasual)
                {
                    var defNivel = GestorEstadoGlobal.NivelesCampana[Math.Clamp(_nivelCampanaActual - 1, 0, 4)];
                    if (_oleadaActual < _totalOleadasNivel)
                    {
                        _oleadaActual++;
                        DesplegarOleadaEnemiga(_oleadaActual, limites.Width);
                        return;
                    }
                    else if (!_enFaseJefe)
                    {
                        _oleadaActual = _totalOleadasNivel + 1;
                        DesplegarJefeFinalCampana(defNivel, limites.Width);
                        return;
                    }
                }

                FinalizarPartida(victoria: true);
            }
        }

        private void FinalizarPartida(bool victoria)
        {
            if (_juegoTerminado) return;
            _juegoTerminado = true;
            _victoriaJugador = victoria;
            _mensajeResultado = victoria ? "¡MISIÓN CUMPLIDA — VICTORIA!" : "AERONAVE DERRIBADA — DERROTA";

            try { File.WriteAllText("resultado.txt", victoria ? "Ganaste" : "Perdiste"); } catch { }

            // Evaluar estrellas (1 a 3) según desempeño y objetivo secundario
            int estrellas = 0;
            if (victoria)
            {
                estrellas = 1;
                if (_naveJugador.VidaActual >= 15) estrellas++;
                if (!_usoReaparicionEnNivel && (_comboMaximoPartida >= 3 || _impactosCriticosPartida >= 2)) estrellas++;
            }
            _ultimasEstrellasGanadas = estrellas;

            var nuevosLogros = GestorEstadoGlobal.RegistrarResultadoPartida(
                _modalidadActual,
                _nivelCampanaActual,
                victoria,
                _naveJugador.BajasConfirmadas,
                jefeDerrotado: victoria && _enFaseJefe,
                _disparosPartida,
                _impactosPartida,
                _comboMaximoPartida,
                _naveJugador.PuntajeTactico,
                estrellas,
                out int monedasReg,
                out int xpReg);

            GestorEstadoGlobal.Perfil.Monedas += _monedasRecolectadasPartida;
            GestorEstadoGlobal.GuardarTodo();

            _ultimasMonedasGanadas = monedasReg + _monedasRecolectadasPartida;
            _ultimaXPGanada = xpReg;
            _textoLogrosRecientes = nuevosLogros.Count > 0
                ? "🏆 LOGRO DESBLOQUEADO: " + string.Join(", ", nuevosLogros.Select(l => l.Titulo))
                : "";
        }

        private void ActualizarBarraEstadoInferior()
        {
            int vidaRivales = 0;
            int rivalesActivos = 0;
            for (int i = 0; i < _aeronaves.Count; i++)
            {
                if (_aeronaves[i].EsRival && _aeronaves[i].Activo)
                {
                    vidaRivales += _aeronaves[i].VidaActual;
                    rivalesActivos++;
                }
            }

            toolStripStatusLabel1.ForeColor = Color.Black;
            toolStripStatusLabel1.Text = $"Vida del Rival : {vidaRivales} ({rivalesActivos} activas)";
            toolStripStatusLabel2.ForeColor = Color.Black;
            toolStripStatusLabel2.Text = $"Mi Nave ({_naveJugador.Tipo}): {_naveJugador.VidaActual} HP | Escudo: {_naveJugador.EscudoActual} | Puntaje: {_naveJugador.PuntajeTactico}";
        }

        // =========================================================================
        // RENDERIZADO GDI+ DE ALTA DEFINICIÓN (FONDO ESTRATOSFÉRICO, NAVES, HUD)
        // =========================================================================
        private void RenderizarEscenaCompleta(object? sender, PaintEventArgs e)
        {
            _profiler.IniciarMedicionRender();

            Graphics g = e.Graphics;
            int calidad = GestorEstadoGlobal.Configuracion.CalidadGrafica;
            g.SmoothingMode = calidad == 0 ? SmoothingMode.HighSpeed : SmoothingMode.HighQuality;
            g.InterpolationMode = calidad == 2 ? InterpolationMode.HighQualityBicubic : InterpolationMode.Bilinear;

            int w = Math.Max(100, contiene.Width);
            int h = Math.Max(100, contiene.Height);

            var estadoCamara = g.Save();

            // 1. Aplicar sacudida de pantalla cinemática (Screen Shake)
            if (Math.Abs(_efectosVisuales.Camara.OffsetX) > 0.04f || Math.Abs(_efectosVisuales.Camara.OffsetY) > 0.04f)
            {
                g.TranslateTransform(_efectosVisuales.Camara.OffsetX, _efectosVisuales.Camara.OffsetY);
                g.RotateTransform(_efectosVisuales.Camara.AnguloRotacion);
            }

            // 2. Cielo del escenario actual
            if (_modalidadActual == ModalidadPartida.CampanaCasual && _nivelCampanaActual > 1)
            {
                var defNiv = GestorEstadoGlobal.NivelesCampana[Math.Clamp(_nivelCampanaActual - 1, 0, 4)];
                using var gradEscenario = new LinearGradientBrush(new Rectangle(0, 0, w, h), defNiv.ColorCieloSuperior, defNiv.ColorCieloInferior, LinearGradientMode.Vertical);
                g.FillRectangle(gradEscenario, 0, 0, w, h);
            }
            else
            {
                Bitmap fondoCielo = RecursosGraficosHud.ObtenerFondoCielo(w, h);
                g.DrawImageUnscaled(fondoCielo, 0, 0);
            }

            // 3. Retícula táctica de radar
            Pen pRadar = RecursosGraficosHud.PlumaRadar;
            for (int rx = 0; rx < w; rx += 72) g.DrawLine(pRadar, rx, 0, rx, h);
            for (float ry = _offsetRadarY; ry < h; ry += 72f) g.DrawLine(pRadar, 0, ry, w, ry);

            // 4. Peligros Ambientales y Suministros
            RenderizadorSistemasJuego.DibujarObstaculosAmbientales(g, _obstaculosAmbientales, h);
            RenderizadorSistemasJuego.DibujarSuministros(g, _suministros);

            // 5. Sombras atmosféricas suaves pseudo-3D
            _efectosVisuales.DibujarSombrasAeronaves(g, _aeronaves, RecursosGraficosHud.BrochaDinamica);

            // 6. Nubes estratosféricas
            SolidBrush bNube = RecursosGraficosHud.BrochaNube;
            for (int i = 0; i < _nubesEstratosfera.Length; i++)
            {
                PointF n = _nubesEstratosfera[i];
                float nw = 90 + (i % 4) * 35;
                float nh = 22 + (i % 3) * 10;
                g.FillEllipse(bNube, n.X, n.Y, nw, nh);
            }

            // 7. Vórtices de ala y casquillos
            _efectosVisuales.DibujarVorticesYCasquillos(g, RecursosGraficosHud.BrochaDinamica);

            // 8. Proyectiles activos
            for (int i = 0; i < _proyectiles.Count; i++)
            {
                _proyectiles[i].Dibujar(g);
            }

            // 9. Aeronaves Polimórficas y Aura de Skin / Invulnerabilidad
            for (int i = 0; i < _aeronaves.Count; i++)
            {
                _aeronaves[i].Dibujar(g);
            }

            if (_naveJugador != null && _naveJugador.Activo && _ticksInvulnerableJugador > 0)
            {
                using var penInv = new Pen(Color.FromArgb(180, 80, 255, 190), 2.5f);
                g.DrawEllipse(penInv, _naveJugador.X - 8, _naveJugador.Y - 8, _naveJugador.Ancho + 16, _naveJugador.Alto + 16);
            }

            // 10. Partículas de Combate
            SolidBrush bPart = RecursosGraficosHud.BrochaDinamica;
            for (int i = 0; i < _particulas.Count; i++)
            {
                ParticulaCombate pt = _particulas[i];
                int alpha = Math.Clamp((int)(255f * pt.VidaRestante / Math.Max(1, pt.VidaInicial)), 0, 255);
                bPart.Color = Color.FromArgb(alpha, pt.ColorBase);
                g.FillEllipse(bPart, pt.X - pt.Radio, pt.Y - pt.Radio, pt.Radio * 2f, pt.Radio * 2f);
            }

            // 11. Ondas expansivas ópticas
            _efectosVisuales.DibujarOndasExpansivas(g, pRadar);

            // 12. Textos Flotantes de Impacto
            Font fImpacto = RecursosGraficosHud.FuenteImpacto;
            SolidBrush bSombra = RecursosGraficosHud.BrochaSombra;
            for (int i = 0; i < _textosImpacto.Count; i++)
            {
                TextoFlotanteImpacto ti = _textosImpacto[i];
                g.DrawString(ti.Texto, fImpacto, bSombra, ti.X + 1, ti.Y + 1);
                bPart.Color = ti.ColorTexto;
                g.DrawString(ti.Texto, fImpacto, bPart, ti.X, ti.Y);
            }

            // 13. Destello atmosférico
            _efectosVisuales.Destello.Dibujar(g, w, h, RecursosGraficosHud.BrochaDinamica);

            // 14. HUD militar holográfico de 5ª generación
            if (_naveJugador != null && GestorEstadoGlobal.Configuracion.MostrarHudMilitar)
            {
                _hudMilitar.Dibujar(g, w, h, _naveJugador, RecursosGraficosHud.BrochaDinamica, pRadar);
            }

            g.Restore(estadoCamara);

            // 15. Barra de Jefe Final o Instrucción de Tutorial en la parte superior
            if (_modalidadActual == ModalidadPartida.TutorialInteractivo)
            {
                using var brTut = new SolidBrush(Color.FromArgb(220, 10, 28, 48));
                using var penTut = new Pen(Color.Cyan, 1.5f);
                g.FillRectangle(brTut, (w - 680) / 2, 10, 680, 36);
                g.DrawRectangle(penTut, (w - 680) / 2, 10, 680, 36);
                g.DrawString(_tutorial.ObtenerInstruccionActual(), RecursosGraficosHud.FuenteFichaTitulo, Brushes.White, (w - 680) / 2 + 14, 19);
            }
            else if (_enFaseJefe && _naveRivalPrincipal != null && _naveRivalPrincipal.Activo)
            {
                RenderizadorSistemasJuego.DibujarBarraJefeFinal(g, w, _nombreJefeActual, _naveRivalPrincipal.VidaActual, _vidaMaximaJefeActual);
            }

            // 16. Panel de Habilidad Especial [Q], Combo, Monedas y Oleada
            string textoFase = _modalidadActual switch
            {
                ModalidadPartida.CampanaCasual => _enFaseJefe
                    ? $"NIVEL {_nivelCampanaActual} — FASE JEFE FINAL"
                    : $"NIVEL {_nivelCampanaActual} — OLEADA {_oleadaActual}/{_totalOleadasNivel}",
                ModalidadPartida.TutorialInteractivo => $"TUTORIAL PASO {_tutorial.PasoActual}/5",
                ModalidadPartida.Multijugador_1v1 => "MULTIJUGADOR 1 VS 1",
                ModalidadPartida.Multijugador_2v2 => $"MULTIJUGADOR 2 VS 2 (EQ {_equipoLocal})",
                ModalidadPartida.Multijugador_Cooperativo => "MULTIJUGADOR COOPERATIVO",
                _ => "BATALLA LIBRE"
            };

            RenderizadorSistemasJuego.DibujarPanelEstadoTacticoInferior(
                g,
                w,
                h,
                _cargaPoderEspecial,
                _ticksPoderActivo,
                _comboActual,
                _monedasRecolectadasPartida,
                _vidasReserva,
                _checkpointJefeAlcanzado,
                textoFase);

            // 17. Overlay de Profiling (Tecla F3)
            if (_mostrarHudProfiling && _naveJugador != null)
            {
                _profiler.DibujarHudProfiling(g, _gestorRed, _naveJugador, Math.Max(10, w - 288), 10);
            }

            // 18. Menú de Pausa Superpuesto
            if (_juegoPausado && !_juegoTerminado)
            {
                RenderizadorSistemasJuego.DibujarMenuPausaSuperpuesto(g, w, h, _checkpointJefeAlcanzado);
            }

            // 19. Pantalla Completa de Resumen de Fin de Misión (Victoria / Derrota)
            if (_juegoTerminado)
            {
                DibujarBannerResultado(g, w, h);
            }

            _profiler.FinalizarMedicionRender();
        }

        private void DibujarBannerResultado(Graphics g, int w, int h)
        {
            int bw = 540;
            int bh = 240;
            int bx = (w - bw) / 2;
            int by = (h - bh) / 2;

            g.FillRectangle(RecursosGraficosHud.BrochaBannerFondo, bx, by, bw, bh);
            g.DrawRectangle(_victoriaJugador ? RecursosGraficosHud.PlumaBannerVictoria : RecursosGraficosHud.PlumaBannerDerrota, bx, by, bw, bh);

            SolidBrush bTitulo = RecursosGraficosHud.BrochaDinamica;
            bTitulo.Color = _victoriaJugador ? Color.FromArgb(255, 70, 230, 255) : Color.FromArgb(255, 255, 80, 80);

            string txtEstrellas = new string('★', _ultimasEstrellasGanadas) + new string('☆', 3 - _ultimasEstrellasGanadas);
            g.DrawString(_mensajeResultado, RecursosGraficosHud.FuenteBannerTitulo, bTitulo, bx + 28, by + 18);
            g.DrawString($"Calificación: {txtEstrellas}  |  Puntaje: {_naveJugador.PuntajeTactico:N0} pts  |  Bajas: {_naveJugador.BajasConfirmadas}  |  Combo Máx: x{_comboMaximoPartida}", RecursosGraficosHud.FuenteBannerSub, RecursosGraficosHud.BrochaBlanco, bx + 28, by + 62);
            g.DrawString($"Recompensas: 🪙 +{_ultimasMonedasGanadas} Créditos Tácticos   |   +{_ultimaXPGanada} XP", RecursosGraficosHud.FuenteBannerSub, Brushes.Gold, bx + 28, by + 92);

            if (!string.IsNullOrEmpty(_textoLogrosRecientes))
            {
                g.DrawString(_textoLogrosRecientes, RecursosGraficosHud.FuenteFichaTitulo, Brushes.LimeGreen, bx + 28, by + 122);
            }

            string acciones = (_victoriaJugador && _modalidadActual == ModalidadPartida.CampanaCasual && _nivelCampanaActual < 5)
                ? "[N] Siguiente Nivel   |   [R] Repetir Misión   |   [M] Volver al Menú Principal"
                : "[R] Reintentar / Solicitar Revancha   |   [M] Volver al Menú Principal";

            g.DrawString(acciones, RecursosGraficosHud.FuenteBannerSub, Brushes.Cyan, bx + 28, by + 162);
            g.DrawString($"Saldo Total: 🪙 {GestorEstadoGlobal.Perfil.Monedas:N0}  ·  Rango: {GestorEstadoGlobal.Perfil.ObtenerNombreRangoMilitar()}", RecursosGraficosHud.FuenteFichaTexto, Brushes.Silver, bx + 28, by + 196);
        }

        // =========================================================================
        // GESTIÓN DE PARTÍCULAS Y EFECTOS DE POSTCOMBUSTIÓN
        // =========================================================================
        private void GenerarExplosionParticulas(float cx, float cy, Color colorBase, int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
            {
                double ang = _rng.NextDouble() * Math.PI * 2.0;
                float vel = 1.2f + (float)_rng.NextDouble() * 4.5f;
                int vida = _rng.Next(14, 30);
                _particulas.Add(new ParticulaCombate
                {
                    X = cx,
                    Y = cy,
                    VX = (float)Math.Cos(ang) * vel,
                    VY = (float)Math.Sin(ang) * vel,
                    Radio = 2.0f + (float)_rng.NextDouble() * 2.8f,
                    VidaRestante = vida,
                    VidaInicial = vida,
                    ColorBase = colorBase
                });
            }
        }

        private void GenerarEstelaPropulsion(AeronaveBase nave)
        {
            float cy = nave.EsRival ? nave.Y + 8f : nave.Y + nave.Alto - 10f;
            float cx = nave.X + nave.Ancho * 0.5f;
            Color cEstela = nave.EsRival
                ? Color.FromArgb(200, 255, 120, 45)
                : GestorEstadoGlobal.ObtenerColorSkinEquipada();

            _particulas.Add(new ParticulaCombate
            {
                X = cx + (_rng.Next(-8, 9)),
                Y = cy,
                VX = (_rng.NextSingle() - 0.5f) * 0.6f,
                VY = nave.EsRival ? -2.2f : 2.5f,
                Radio = 2.4f,
                VidaRestante = 12,
                VidaInicial = 12,
                ColorBase = cEstela
            });
        }

        private void ActualizarEfectosVisuales()
        {
            for (int i = _particulas.Count - 1; i >= 0; i--)
            {
                ParticulaCombate p = _particulas[i];
                p.X += p.VX;
                p.Y += p.VY;
                p.VidaRestante--;
                if (p.VidaRestante <= 0)
                {
                    int ult = _particulas.Count - 1;
                    _particulas[i] = _particulas[ult];
                    _particulas.RemoveAt(ult);
                }
                else
                {
                    _particulas[i] = p;
                }
            }

            for (int i = _textosImpacto.Count - 1; i >= 0; i--)
            {
                TextoFlotanteImpacto t = _textosImpacto[i];
                t.Y -= 0.6f;
                t.TicksRestantes--;
                if (t.TicksRestantes <= 0)
                {
                    _textosImpacto.RemoveAt(i);
                }
                else
                {
                    _textosImpacto[i] = t;
                }
            }
        }

        // =========================================================================
        // MANEJADORES DE TECLADO Y MÉTODOS PÚBLICOS DE COMPATIBILIDAD
        // =========================================================================
        private void EventoKeyDown(object? sender, KeyEventArgs e)
        {
            // Teclas de Pausa y Menú
            if (e.KeyCode == Keys.P || e.KeyCode == Keys.Escape)
            {
                AlternarPausa();
                return;
            }

            if (_juegoPausado)
            {
                if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) AlternarPausa();
                else if (e.KeyCode == Keys.K && _checkpointJefeAlcanzado) DesplegarMisionAerea(_modeloJugadorActual, _modeloRivalActual, _incluirEscoltaActual);
                else if (e.KeyCode == Keys.R) { _checkpointJefeAlcanzado = false; DesplegarMisionAerea(_modeloJugadorActual, _modeloRivalActual, _incluirEscoltaActual); }
                else if (e.KeyCode == Keys.C) { using var dlg = new VentanaCentroMando(PestanaCentroMando.Configuracion); dlg.ShowDialog(this); }
                else if (e.KeyCode == Keys.T) { using var dlg = new VentanaCentroMando(PestanaCentroMando.Tienda); dlg.ShowDialog(this); AplicarMejorasANaveJugador(); }
                else if (e.KeyCode == Keys.M || e.KeyCode == Keys.Back) { GestorEstadoGlobal.GuardarTodo(); Close(); }
                return;
            }

            if (_juegoTerminado)
            {
                if (e.KeyCode == Keys.N && _victoriaJugador && _modalidadActual == ModalidadPartida.CampanaCasual && _nivelCampanaActual < 5)
                {
                    _nivelCampanaActual++;
                    _checkpointJefeAlcanzado = false;
                    DesplegarMisionAerea(_modeloJugadorActual, _modeloRivalActual, _incluirEscoltaActual);
                    return;
                }
                if (e.KeyCode == Keys.R)
                {
                    if (!_victoriaJugador && _checkpointJefeAlcanzado)
                    {
                        // Reintentar desde el checkpoint del jefe
                        DesplegarMisionAerea(_modeloJugadorActual, _modeloRivalActual, _incluirEscoltaActual);
                    }
                    else
                    {
                        _checkpointJefeAlcanzado = false;
                        _gestorRed.EnviarMensajeControl("REMATCH", _naveJugador.IdAeronave, _nombrePilotoActual, _modeloJugadorActual);
                        DesplegarMisionAerea(_modeloJugadorActual, _modeloRivalActual, _incluirEscoltaActual);
                    }
                    return;
                }
                if (e.KeyCode == Keys.M || e.KeyCode == Keys.Back)
                {
                    GestorEstadoGlobal.GuardarTodo();
                    Close();
                    return;
                }
            }

            _teclasActivas.Add(e.KeyCode);

            if (e.KeyCode == Keys.Q || e.KeyCode == Keys.E || e.KeyCode == GestorEstadoGlobal.Configuracion.TeclaPoderEspecial)
            {
                IntentarActivarPoderEspecial();
            }
            else if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1)
            {
                CambiarAeronaveJugadorDirecto(TipoAeronave.B2_Spirit_Stealth);
            }
            else if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2)
            {
                CambiarAeronaveJugadorDirecto(TipoAeronave.F35_Lightning_II);
            }
            else if (e.KeyCode == Keys.F3)
            {
                _mostrarHudProfiling = !_mostrarHudProfiling;
            }
            else if (e.KeyCode == Keys.F4)
            {
                string ruta = _profiler.ExportarReporteMetricas(_gestorRed, _naveJugador);
                toolStripStatusLabel1.Text = $"Reporte guardado: {Path.GetFileName(ruta)}";
            }
            else if (e.KeyCode == Keys.H)
            {
                _hudMilitar.AlternarModo();
                toolStripStatusLabel1.Text = $"HUD Militar: {_hudMilitar.ModoActual}";
            }
            else if (e.KeyCode == Keys.M)
            {
                SintetizadorAudioTactico.AlternarAudio();
                toolStripStatusLabel1.Text = SintetizadorAudioTactico.AudioHabilitado ? "Audio táctico: ACTIVADO" : "Audio táctico: SILENCIADO";
            }
            else if (e.KeyCode == Keys.R)
            {
                DesplegarMisionAerea(_modeloJugadorActual, _modeloRivalActual, _incluirEscoltaActual);
            }

            ActividadTecla(sender ?? this, e);
        }

        private void EventoKeyUp(object? sender, KeyEventArgs e)
        {
            _teclasActivas.Remove(e.KeyCode);
        }

        public void ActividadTecla(object sender, KeyEventArgs e)
        {
            if (_naveJugador == null || !_naveJugador.Activo || _juegoPausado) return;

            Size lim = contiene.ClientSize;
            switch (e.KeyValue)
            {
                case 37:
                    _naveJugador.Mover(-0.5f, 0f, lim);
                    angulo = -15f;
                    break;
                case 38:
                    _naveJugador.Mover(0f, -0.5f, lim);
                    angulo = 0f;
                    break;
                case 39:
                    _naveJugador.Mover(0.5f, 0f, lim);
                    angulo = 15f;
                    break;
                case 40:
                    _naveJugador.Mover(0f, 0.5f, lim);
                    angulo = 0f;
                    break;
                case 13:
                case 32:
                    EjecutarDisparoJugador();
                    break;
            }
        }

        public void CrearMisil(int AngRotar, Color pintar, string nombre, int x, int y)
        {
            bool esRival = (nombre == "Rival" || AngRotar == 180);
            TipoAeronave tipo = _naveJugador != null ? _naveJugador.Tipo : TipoAeronave.F35_Lightning_II;
            _proyectiles.Add(new Proyectil(
                esRival ? "RIVAL" : (_naveJugador?.IdAeronave ?? "JUG"),
                esRival,
                tipo,
                x,
                y,
                0f,
                esRival ? 11f : -13f));
        }

        public void CrearNave(PictureBox Avion, int AngRotar, int Tipox, Color Pintar, int Vida)
        {
            bool esRival = (AngRotar == 180);
            TipoAeronave tipo = (Tipox == 2) ? TipoAeronave.B2_Spirit_Stealth : TipoAeronave.F35_Lightning_II;
            Avion.Image = CacheSpritesAeronaves.ObtenerSpriteNave(tipo, esRival, false, 0f);
            Avion.Tag = Vida;
        }

        public void NaveCorre(PictureBox Avion, int AngRotar, int velox)
        {
            bool acelerando = (velox == 1);
            TipoAeronave tipo = _naveJugador != null ? _naveJugador.Tipo : TipoAeronave.F35_Lightning_II;
            Avion.Image = CacheSpritesAeronaves.ObtenerSpriteNave(tipo, false, acelerando, angulo);
        }

        public static Image RotateImage(Image img, float rotationAngle)
        {
            if (Math.Abs(rotationAngle) < 0.01f)
            {
                return (Image)img.Clone();
            }
            Bitmap bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gfx.CompositingQuality = CompositingQuality.HighQuality;
                gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);
                gfx.RotateTransform(rotationAngle);
                gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);
                gfx.DrawImage(img, new Point(0, 0));
            }
            return bmp;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            tiempo.Stop();
            _gestorRed.Dispose();
            base.OnFormClosing(e);
        }
    }
}
