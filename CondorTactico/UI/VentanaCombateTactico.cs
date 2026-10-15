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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
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

        // Parámetros de misión y enlace táctico
        private string _nombrePilotoActual = "CÓNDOR-01";
        private TipoAeronave _modeloJugadorActual = TipoAeronave.B2_Spirit_Stealth;
        private TipoAeronave _modeloRivalActual = TipoAeronave.F35_Lightning_II;
        private bool _incluirEscoltaActual = true;
        private ModoRedJuego _modoRedActual = ModoRedJuego.Servidor_Host_Online;
        private string _ipRedActual = "127.0.0.1";
        private int _puertoRedActual = 15050;

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

        public VentanaCombateTactico()
        {
            InitializeComponent();
            Iniciar();
        }

        // =========================================================================
        // INICIALIZACIÓN DEL FORMULARIO Y ARQUITECTURA CLIENTE-SERVIDOR
        // =========================================================================
        public void Iniciar()
        {
            // Pre-cargar en O(1) todos los modelos vectoriales 3D (F-35 Lightning II y B-2 Spirit AF 82-1071)
            CacheSpritesAeronaves.InicializarCache();

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(860, 680);
            this.Size = new Size(980, 740);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "CÓNDOR TÁCTICO v3.1 | SIMULADOR DE COMBATE AÉREO 2D | UNAJ | B-2 SPIRIT & F-35";
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            ConstruirBarraLanzamientoTactico();

            // Configurar el lienzo principal donde vuelan las aeronaves
            contiene.Dock = DockStyle.Fill;
            contiene.BackColor = Color.FromArgb(24, 58, 98);
            contiene.Paint += RenderizarEscenaCompleta;
            this.Controls.Add(contiene);
            contiene.BringToFront();

            // Inicializar nubes estratosféricas para efecto de paralaje aéreo
            for (int i = 0; i < _nubesEstratosfera.Length; i++)
            {
                _nubesEstratosfera[i] = new PointF(_rng.Next(20, 900), _rng.Next(20, 650));
            }

            // Suscribir eventos de teclado para vuelo fluido multidireccional
            this.KeyDown += EventoKeyDown;
            this.KeyUp += EventoKeyUp;

            // Iniciar arquitectura Cliente-Servidor por defecto en Modo Host Online (UDP :15050)
            _gestorRed.IniciarArquitectura(_modoRedActual, _ipRedActual, _puertoRedActual);

            // Desplegar aeronaves iniciales en el espacio aéreo (B-2 Spirit AF 82-1071 + F-35 Lightning II)
            DesplegarMisionAerea(_modeloJugadorActual, _modeloRivalActual, _incluirEscoltaActual);

            // Configurar Timer de alta frecuencia (~60 FPS = 16 ms)
            tiempo.Interval = 16;
            tiempo.Tick += ImpactarTick;
            tiempo.Start();

            DevolverFocoAlLienzo();
        }

        /// <summary>
        /// Construye la barra superior táctica con accesos directos a subventanas modulares (Red, Misión, Profiling, Info).
        /// </summary>
        private void ConstruirBarraLanzamientoTactico()
        {
            panelLanzador.Dock = DockStyle.Top;
            panelLanzador.Height = 44;
            panelLanzador.BackColor = Color.FromArgb(12, 20, 32);
            panelLanzador.Padding = new Padding(6, 5, 6, 4);

            Font fBtn = new Font("Segoe UI", 8.2f, FontStyle.Bold);
            int curX = 8;

            // 1. Botón Nueva Misión / Reiniciar
            ConfigurarBotonBarra(btnLanzar, "🚀 NUEVA MISIÓN", Color.FromArgb(0, 150, 200), curX, 120, fBtn);
            btnLanzar.Click += (s, e) =>
            {
                DesplegarMisionAerea(_modeloJugadorActual, _modeloRivalActual, _incluirEscoltaActual);
                DevolverFocoAlLienzo();
            };
            panelLanzador.Controls.Add(btnLanzar);
            curX += 126;

            // 2. Botón Alternar Aeronave Rápido (B-2 Spirit / F-35A)
            ConfigurarBotonBarra(btnAlternarModelo, "✈️ NAVE: B-2 SPIRIT [1/2]", Color.FromArgb(20, 130, 160), curX, 170, fBtn);
            btnAlternarModelo.Click += (s, e) =>
            {
                TipoAeronave nuevoTipo = _modeloJugadorActual == TipoAeronave.B2_Spirit_Stealth
                    ? TipoAeronave.F35_Lightning_II
                    : TipoAeronave.B2_Spirit_Stealth;
                CambiarAeronaveJugadorDirecto(nuevoTipo);
            };
            panelLanzador.Controls.Add(btnAlternarModelo);
            curX += 176;

            // 3. Botón Añadir Escolta
            ConfigurarBotonBarra(btnEscolta, "➕ ESCOLTA", Color.FromArgb(30, 140, 95), curX, 90, fBtn);
            btnEscolta.Click += (s, e) =>
            {
                LanzarEscoltaAdicional();
                DevolverFocoAlLienzo();
            };
            panelLanzador.Controls.Add(btnEscolta);
            curX += 96;

            // 4. Botón Subventana Misión Táctica
            ConfigurarBotonBarra(btnMision, "🎯 MISIÓN", Color.FromArgb(170, 110, 30), curX, 95, fBtn);
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
            curX += 101;

            // 5. Botón Subventana Red Multijugador UDP
            ConfigurarBotonBarra(btnRed, "🌐 RED UDP", Color.FromArgb(50, 100, 180), curX, 95, fBtn);
            btnRed.Click += (s, e) =>
            {
                using var dlg = new VentanaConfiguracionRed(
                    _gestorRed,
                    (modo, ip, puerto) =>
                    {
                        _modoRedActual = modo;
                        _ipRedActual = ip;
                        _puertoRedActual = puerto;
                        _gestorRed.IniciarArquitectura(modo, ip, puerto);
                    },
                    _modoRedActual,
                    _ipRedActual,
                    _puertoRedActual);
                dlg.ShowDialog(this);
                DevolverFocoAlLienzo();
            };
            panelLanzador.Controls.Add(btnRed);
            curX += 101;

            // 6. Botón Subventana Telemetría y Profiling (F3/F4)
            ConfigurarBotonBarra(btnMetricas, "📊 TELEMETRÍA [F3]", Color.FromArgb(90, 70, 160), curX, 130, fBtn);
            btnMetricas.Click += (s, e) =>
            {
                using var dlg = new VentanaReporteMetricas(_profiler, _gestorRed, _naveJugador);
                dlg.ShowDialog(this);
                DevolverFocoAlLienzo();
            };
            panelLanzador.Controls.Add(btnMetricas);
            curX += 136;

            // 7. Botón Subventana Acerca de y Créditos UNAJ
            ConfigurarBotonBarra(btnAcercaDe, "ℹ️ INFO UNAJ", Color.FromArgb(60, 75, 95), curX, 95, fBtn);
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
            btn.TabStop = false; // NUNCA robar foco al lienzo de combate
            btn.Cursor = Cursors.Hand;
        }

        private void ActualizarBotonModelo()
        {
            btnAlternarModelo.Text = _modeloJugadorActual == TipoAeronave.B2_Spirit_Stealth
                ? "✈️ NAVE: B-2 SPIRIT [1/2]"
                : "✈️ NAVE: F-35A LIGHTNING [1/2]";
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
            // Evitar que Windows Forms intercepte las teclas de flechas para navegar entre botones
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

        /// <summary>
        /// Procedimiento polimórfico que instancia y lanza las aeronaves en el espacio aéreo.
        /// </summary>
        public void DesplegarMisionAerea(TipoAeronave modeloJugador, TipoAeronave modeloRival, bool incluirEscolta)
        {
            _modeloJugadorActual = modeloJugador;
            _modeloRivalActual = modeloRival;
            _incluirEscoltaActual = incluirEscolta;

            _aeronaves.Clear();
            _proyectiles.Clear();
            _particulas.Clear();
            _textosImpacto.Clear();
            _teclasActivas.Clear();
            _juegoTerminado = false;
            _victoriaJugador = false;
            _mensajeResultado = "";

            int w = Math.Max(600, contiene.Width);
            int h = Math.Max(500, contiene.Height);
            string nombrePiloto = string.IsNullOrWhiteSpace(_nombrePilotoActual) ? "UNAJ-G1" : _nombrePilotoActual.Trim();

            // 1. Instanciar aeronave del jugador mediante Polimorfismo OOP
            float xJug = (w * 0.5f) - 60f;
            float yJug = h - 155f;
            _naveJugador = CrearInstanciaPolimorfica(
                $"JUG-{Environment.ProcessId}",
                nombrePiloto,
                modeloJugador,
                esRival: false,
                xJug,
                yJug,
                vidaPersonalizada: 30);
            _aeronaves.Add(_naveJugador);

            // 2. Instanciar comandante rival principal
            _naveRivalPrincipal = CrearInstanciaPolimorfica(
                "RIVAL-LIDER",
                modeloRival == TipoAeronave.B2_Spirit_Stealth ? "B2-SPIRIT-BOSS" : "F35-RIVAL-01",
                modeloRival,
                esRival: true,
                x: w * 0.35f,
                y: 28f,
                vidaPersonalizada: 50);
            _aeronaves.Add(_naveRivalPrincipal);

            // 3. Si está en modo Escuadrón Mixto o Escolta, lanzar también el otro modelo de aeronave
            if (incluirEscolta)
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
                    y: 42f,
                    vidaPersonalizada: 40);
                _aeronaves.Add(escoltaRival);
            }

            ActualizarBotonModelo();
            ActualizarBarraEstadoInferior();
            DevolverFocoAlLienzo();
        }

        /// <summary>
        /// Función Factoría (Factory Method Pattern) que retorna una instancia polimórfica de AeronaveBase.
        /// </summary>
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

            _naveJugador = nueva;
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

        // =========================================================================
        // BUCLE PRINCIPAL DE TIEMPO REAL (FÍSICA, COLISIONES BIG-O, RED Y PROFILING)
        // =========================================================================
        private void ImpactarTick(object? sender, EventArgs e)
        {
            _profiler.IniciarMedicionUpdate();

            Size limites = contiene.ClientSize;
            if (limites.Width < 100 || limites.Height < 100)
            {
                limites = new Size(800, 600);
            }

            _offsetRadarY = (_offsetRadarY + 1.2f) % 64f;

            // 1. Mover nubes estratosféricas para sensación de velocidad supersónica
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
                // 2. Procesar entrada continua del piloto local (funciona de inmediato al iniciar)
                ProcesarMovimientoContinuoJugador(limites);

                // 3. Actualizar física e IA de todas las aeronaves activas
                _contadorTickIA++;
                for (int i = 0; i < _aeronaves.Count; i++)
                {
                    AeronaveBase nave = _aeronaves[i];
                    if (!nave.Activo) continue;

                    nave.ActualizarFisica(limites);

                    // Estela de postcombustión si está acelerando
                    if (nave.Acelerando && (_contadorTickIA % 2 == 0))
                    {
                        GenerarEstelaPropulsion(nave);
                    }

                    // Disparo automático táctico de aeronaves rivales controladas por IA
                    if (nave.EsRival && !nave.ControladaPorRed && (_contadorTickIA % 42 == 0))
                    {
                        List<Proyectil> salvasRival = nave.Disparar();
                        for (int s = 0; s < salvasRival.Count; s++)
                        {
                            _proyectiles.Add(salvasRival[s]);
                        }
                    }
                }

                // 4. Actualizar proyectiles y compactar inactivos en O(1) mediante Swap-and-Pop
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

                // 5. Detección de colisiones mediante Grilla Espacial Hash en O(N + K)
                _grillaColisiones.ProcesarColisiones(_proyectiles, _aeronaves, ProcesarImpactoConfirmado);

                // Compactar proyectiles que impactaron en este tick
                for (int i = _proyectiles.Count - 1; i >= 0; i--)
                {
                    if (!_proyectiles[i].Activo)
                    {
                        int ultimo = _proyectiles.Count - 1;
                        _proyectiles[i] = _proyectiles[ultimo];
                        _proyectiles.RemoveAt(ultimo);
                    }
                }

                // 6. Sincronización de Red Cliente-Servidor (Envío y Recepción UDP no bloqueante)
                _contadorTickRed++;
                if (_contadorTickRed % 2 == 0 && _naveJugador.Activo)
                {
                    _gestorRed.EnviarEstadoAeronave(_naveJugador, disparoEnEsteTick: false);
                }
                ProcesarPaquetesRedEntrantes();

                // Exportar reporte de métricas periódicamente sin bloquear
                if (_contadorTickIA == 30 || _contadorTickIA % 300 == 0)
                {
                    try { _profiler.ExportarReporteMetricas(_gestorRed, _naveJugador); } catch { }
                }

                // 7. Verificar condiciones de Victoria o Derrota
                VerificarEstadoPartida();
            }

            // 8. Actualizar partículas y textos flotantes de impacto
            ActualizarEfectosVisuales();

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

            // Solicitar redibujado GDI+ con DoubleBuffering
            contiene.Invalidate();
        }

        private void ProcesarMovimientoContinuoJugador(Size limites)
        {
            if (!_naveJugador.Activo) return;

            float dx = 0f;
            float dy = 0f;

            if (_teclasActivas.Contains(Keys.Left) || _teclasActivas.Contains(Keys.A)) dx -= 1.0f;
            if (_teclasActivas.Contains(Keys.Right) || _teclasActivas.Contains(Keys.D)) dx += 1.0f;
            if (_teclasActivas.Contains(Keys.Up) || _teclasActivas.Contains(Keys.W)) dy -= 1.0f;
            if (_teclasActivas.Contains(Keys.Down) || _teclasActivas.Contains(Keys.S)) dy += 1.0f;

            if (Math.Abs(dx) > 0.01f || Math.Abs(dy) > 0.01f)
            {
                // Normalización vectorial diagonal para mantener velocidad constante
                if (Math.Abs(dx) > 0.01f && Math.Abs(dy) > 0.01f)
                {
                    dx *= 0.7071f;
                    dy *= 0.7071f;
                }
                _naveJugador.Mover(dx * 0.65f, dy * 0.65f, limites);
                angulo = _naveJugador.AnguloInclinacion;
            }
            else
            {
                _naveJugador.AnguloInclinacion = 0f;
                _naveJugador.Acelerando = false;
                angulo = 0f;
            }

            if (_teclasActivas.Contains(Keys.Enter) || _teclasActivas.Contains(Keys.Space))
            {
                EjecutarDisparoJugador();
            }
        }

        public void EjecutarDisparoJugador()
        {
            if (!_naveJugador.Activo) return;

            List<Proyectil> salvas = _naveJugador.Disparar();
            if (salvas.Count > 0)
            {
                for (int i = 0; i < salvas.Count; i++)
                {
                    _proyectiles.Add(salvas[i]);
                }
                _gestorRed.EnviarEstadoAeronave(_naveJugador, disparoEnEsteTick: true);
            }
        }

        /// <summary>
        /// Procedimiento callback invocado por la Grilla Espacial O(N+K) al confirmar un impacto geométrico.
        /// </summary>
        private void ProcesarImpactoConfirmado(AeronaveBase naveImpactada, Proyectil proyectil, ResultadoImpacto resultado)
        {
            bool esCritico = resultado.Zona == ZonaImpacto.CabinaCentralCritica;
            Color colorTexto = esCritico ? Color.FromArgb(255, 255, 55, 55) : Color.FromArgb(255, 255, 220, 90);

            // Agregar texto flotante de impacto ("impact -10" en cabina o "impact -1" en alas)
            _textosImpacto.Add(new TextoFlotanteImpacto
            {
                X = naveImpactada.X + (naveImpactada.Ancho * 0.5f) - 45f,
                Y = naveImpactada.EsRival ? (naveImpactada.Y + naveImpactada.Alto + 6f) : (naveImpactada.Y - 22f),
                Texto = esCritico ? "impact -10" : $"impact -{resultado.DanoRealVida}",
                ColorTexto = colorTexto,
                TicksRestantes = 42,
                EsCritico = esCritico
            });

            // Generar partículas de impacto en el punto exacto del proyectil
            GenerarExplosionParticulas(
                proyectil.X + proyectil.Ancho * 0.5f,
                proyectil.Y + proyectil.Alto * 0.5f,
                esCritico ? Color.OrangeRed : Color.DeepSkyBlue,
                esCritico ? 22 : 10);

            if (naveImpactada.EsRival)
            {
                _naveJugador.PuntajeTactico += resultado.DanoTotal * 10;
                if (!naveImpactada.Activo)
                {
                    _naveJugador.BajasConfirmadas++;
                    GenerarExplosionParticulas(
                        naveImpactada.X + naveImpactada.Ancho * 0.5f,
                        naveImpactada.Y + naveImpactada.Alto * 0.5f,
                        Color.Gold,
                        45);
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

                if (!_aeronavesRemotas.TryGetValue(pkt.IdAeronave, out AeronaveBase? remota))
                {
                    remota = CrearInstanciaPolimorfica(
                        pkt.IdAeronave,
                        pkt.NombrePiloto,
                        pkt.Modelo,
                        esRival: true,
                        pkt.X,
                        80f,
                        vidaPersonalizada: Math.Max(20, pkt.Vida));
                    remota.ControladaPorRed = true;
                    _aeronavesRemotas[pkt.IdAeronave] = remota;
                    _aeronaves.Add(remota);
                }

                remota.X = pkt.X;
                remota.Y = Math.Clamp(pkt.Y, 15f, 160f);
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

        private void VerificarEstadoPartida()
        {
            if (!_naveJugador.Activo || _naveJugador.VidaActual <= 0)
            {
                _juegoTerminado = true;
                _victoriaJugador = false;
                _mensajeResultado = "Perdiste el Juego";
                try { File.WriteAllText("resultado.txt", "Perdiste"); } catch { }
                return;
            }

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
                _juegoTerminado = true;
                _victoriaJugador = true;
                _mensajeResultado = "Felicitaciones Ganaste !";
                try { File.WriteAllText("resultado.txt", "Ganaste"); } catch { }
            }
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
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int w = contiene.Width;
            int h = contiene.Height;

            // 1. Cielo estratosférico inspirado en la fotografía real del B-2 Spirit (AF 82-1071)
            using (LinearGradientBrush bCielo = new LinearGradientBrush(
                new Rectangle(0, 0, Math.Max(1, w), Math.Max(1, h)),
                Color.FromArgb(255, 18, 48, 86),
                Color.FromArgb(255, 72, 136, 192),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(bCielo, 0, 0, w, h);
            }

            // 2. Retícula táctica de radar de baja opacidad
            using (Pen pRadar = new Pen(Color.FromArgb(22, 140, 220, 255), 1f))
            {
                for (int rx = 0; rx < w; rx += 72)
                {
                    g.DrawLine(pRadar, rx, 0, rx, h);
                }
                for (float ry = _offsetRadarY; ry < h; ry += 72f)
                {
                    g.DrawLine(pRadar, 0, ry, w, ry);
                }
            }

            // 3. Nubes estratosféricas semitransparentes en paralaje
            using (SolidBrush bNube = new SolidBrush(Color.FromArgb(28, 235, 245, 255)))
            {
                for (int i = 0; i < _nubesEstratosfera.Length; i++)
                {
                    PointF n = _nubesEstratosfera[i];
                    float nw = 90 + (i % 4) * 35;
                    float nh = 22 + (i % 3) * 10;
                    g.FillEllipse(bNube, n.X, n.Y, nw, nh);
                }
            }

            // 4. Dibujar Proyectiles activos
            for (int i = 0; i < _proyectiles.Count; i++)
            {
                _proyectiles[i].Dibujar(g);
            }

            // 5. Dibujar Aeronaves Polimórficas (B-2 Spirit AF 82-1071 y F-35 Lightning II)
            for (int i = 0; i < _aeronaves.Count; i++)
            {
                _aeronaves[i].Dibujar(g);
            }

            // 6. Dibujar Partículas de Combate y Postcombustión
            for (int i = 0; i < _particulas.Count; i++)
            {
                ParticulaCombate pt = _particulas[i];
                int alpha = Math.Clamp((int)(255f * pt.VidaRestante / Math.Max(1, pt.VidaInicial)), 0, 255);
                using (SolidBrush bPart = new SolidBrush(Color.FromArgb(alpha, pt.ColorBase)))
                {
                    g.FillEllipse(bPart, pt.X - pt.Radio, pt.Y - pt.Radio, pt.Radio * 2f, pt.Radio * 2f);
                }
            }

            // 7. Dibujar Textos Flotantes de Impacto Geométrico ("impact -10")
            using (Font fImpacto = new Font("Segoe UI", 10.5f, FontStyle.Bold))
            {
                for (int i = 0; i < _textosImpacto.Count; i++)
                {
                    TextoFlotanteImpacto ti = _textosImpacto[i];
                    using (SolidBrush bSombra = new SolidBrush(Color.FromArgb(190, 0, 0, 0)))
                    using (SolidBrush bColor = new SolidBrush(ti.ColorTexto))
                    {
                        g.DrawString(ti.Texto, fImpacto, bSombra, ti.X + 1, ti.Y + 1);
                        g.DrawString(ti.Texto, fImpacto, bColor, ti.X, ti.Y);
                    }
                }
            }

            // 8. Dibujar Tarjeta Táctica Inferior Izquierda de la Aeronave Activa
            DibujarFichaAeronaveActiva(g, 10, h - 72);

            // 9. Dibujar Overlay de Profiling, Big-O y Telemetría Cliente-Servidor (Tecla F3)
            if (_mostrarHudProfiling && _naveJugador != null)
            {
                _profiler.DibujarHudProfiling(g, _gestorRed, _naveJugador, Math.Max(10, w - 288), 10);
            }

            // 10. Banner de Fin de Misión (Victoria / Derrota)
            if (_juegoTerminado)
            {
                DibujarBannerResultado(g, w, h);
            }

            _profiler.FinalizarMedicionRender();
        }

        private void DibujarFichaAeronaveActiva(Graphics g, int x, int y)
        {
            if (_naveJugador == null) return;

            int boxW = 520;
            int boxH = 60;
            using (SolidBrush bBg = new SolidBrush(Color.FromArgb(185, 10, 18, 30)))
            using (Pen pBorder = new Pen(Color.FromArgb(160, 60, 195, 255), 1f))
            {
                g.FillRectangle(bBg, x, y, boxW, boxH);
                g.DrawRectangle(pBorder, x, y, boxW, boxH);
            }

            using (Font fBold = new Font("Consolas", 8.2f, FontStyle.Bold))
            using (Font fReg = new Font("Consolas", 7.5f, FontStyle.Regular))
            using (SolidBrush bCian = new SolidBrush(Color.FromArgb(255, 80, 230, 255)))
            using (SolidBrush bBlanco = new SolidBrush(Color.FromArgb(240, 235, 245, 255)))
            {
                g.DrawString($"AERONAVE ACTIVA: {_naveJugador.NombreModelo} [{_naveJugador.Matricula}]", fBold, bCian, x + 8, y + 6);
                g.DrawString(_naveJugador.ObtenerFichaTecnica(), fReg, bBlanco, x + 8, y + 23);
                g.DrawString("CONTROLES: [Flechas/WASD] Volar | [Enter/Espacio] Disparar | [1] B-2 Spirit | [2] F-35A | [F3] HUD Profiling", fReg, bBlanco, x + 8, y + 40);
            }
        }

        private void DibujarBannerResultado(Graphics g, int w, int h)
        {
            int bw = 430;
            int bh = 130;
            int bx = (w - bw) / 2;
            int by = (h - bh) / 2;

            using (SolidBrush bFondo = new SolidBrush(Color.FromArgb(225, 10, 16, 28)))
            using (Pen pBorde = new Pen(_victoriaJugador ? Color.DeepSkyBlue : Color.Crimson, 2.5f))
            {
                g.FillRectangle(bFondo, bx, by, bw, bh);
                g.DrawRectangle(pBorde, bx, by, bw, bh);
            }

            using (Font fTitulo = new Font("Segoe UI", 18f, FontStyle.Bold))
            using (Font fSub = new Font("Consolas", 9.5f, FontStyle.Bold))
            using (SolidBrush bTitulo = new SolidBrush(_victoriaJugador ? Color.FromArgb(255, 70, 230, 255) : Color.FromArgb(255, 255, 80, 80)))
            using (SolidBrush bBlanco = new SolidBrush(Color.White))
            {
                g.DrawString(_mensajeResultado, fTitulo, bTitulo, bx + 55, by + 22);
                g.DrawString($"Puntaje Táctico: {_naveJugador.PuntajeTactico} pts | Bajas: {_naveJugador.BajasConfirmadas}", fSub, bBlanco, bx + 55, by + 66);
                g.DrawString("Presiona [R] o el botón [🚀 NUEVA MISIÓN] para reiniciar", fSub, bBlanco, bx + 28, by + 92);
            }
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
            Color cEstela = nave.Tipo == TipoAeronave.B2_Spirit_Stealth
                ? Color.FromArgb(200, 60, 210, 255)
                : Color.FromArgb(200, 255, 165, 45);

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
            _teclasActivas.Add(e.KeyCode);

            if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1)
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
            if (_naveJugador == null || !_naveJugador.Activo) return;

            Size lim = contiene.ClientSize;
            switch (e.KeyValue)
            {
                case 37: // Flecha izquierda
                    _naveJugador.Mover(-0.5f, 0f, lim);
                    angulo = -15f;
                    break;
                case 38: // Flecha arriba
                    _naveJugador.Mover(0f, -0.5f, lim);
                    angulo = 0f;
                    break;
                case 39: // Flecha derecha
                    _naveJugador.Mover(0.5f, 0f, lim);
                    angulo = 15f;
                    break;
                case 40: // Flecha abajo
                    _naveJugador.Mover(0f, 0.5f, lim);
                    angulo = 0f;
                    break;
                case 13: // Enter: Disparo polimórfico
                    EjecutarDisparoJugador();
                    break;
                case 32: // Espacio: Disparo polimórfico
                    EjecutarDisparoJugador();
                    break;
            }
        }

        /// <summary>
        /// Método público preservado por compatibilidad histórica con la versión base.
        /// </summary>
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

        /// <summary>
        /// Método público preservado por compatibilidad histórica con la versión base.
        /// </summary>
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
            _gestorRed.Dispose();
            base.OnFormClosing(e);
        }
    }
}
