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
using System.Windows.Forms;

namespace CondorTactico.UI
{
    /// <summary>
    /// Sala de Espera (Lobby) y Centro de Operaciones Multijugador para LAN e Internet.
    /// Soporta creación/búsqueda de salas, unión por código de invitación, selección de equipos
    /// y modalidades 1 vs 1, 2 vs 2 y Cooperativo.
    /// </summary>
    public sealed class VentanaLobbyMultijugador : Form
    {
        public bool IniciarPartidaConfirmada { get; private set; }
        public ModoRedJuego ModoRedSeleccionado { get; private set; } = ModoRedJuego.Servidor_Host_Online;
        public ModalidadPartida ModalidadSeleccionada { get; private set; } = ModalidadPartida.Multijugador_1v1;
        public bool EsModoInternet { get; private set; }
        public string IpSeleccionada { get; private set; } = "127.0.0.1";
        public int PuertoSeleccionado { get; private set; } = 15050;
        public int EquipoSeleccionado { get; private set; } = 1;
        public TipoAeronave AeronaveSeleccionada { get; private set; }

        private readonly GestorRedClienteServidor _gestorRed;
        private readonly System.Windows.Forms.Timer _timerLobby = new System.Windows.Forms.Timer { Interval = 500 };

        // Controles de pestaña Red (LAN vs Internet)
        private readonly Button _btnTabLan = new Button();
        private readonly Button _btnTabInternet = new Button();
        private readonly Label _lblBannerRed = new Label();

        // Controles de Modalidad (1v1, 2v2, Cooperativo)
        private readonly ComboBox _cmbModalidad = new ComboBox();
        private readonly ComboBox _cmbRol = new ComboBox();
        private readonly ComboBox _cmbAeronave = new ComboBox();
        private readonly ComboBox _cmbEquipo = new ComboBox();

        // Conexión e Invitación
        private readonly TextBox _txtIp = new TextBox();
        private readonly NumericUpDown _numPuerto = new NumericUpDown();
        private readonly TextBox _txtCodigoInvitacion = new TextBox();
        private readonly Button _btnCopiarCodigo = new Button();
        private readonly Button _btnUnirsePorCodigo = new Button();
        private readonly Button _btnActivarSala = new Button();

        // Buscador LAN y Sala de Espera
        private readonly ListBox _lstSalasLan = new ListBox();
        private readonly ListBox _lstJugadoresLobby = new ListBox();
        private readonly Label _lblEstadoConexion = new Label();
        private readonly Label _lblLatencia = new Label();
        private readonly Button _btnDesplegarCombate = new Button();

        public VentanaLobbyMultijugador(GestorRedClienteServidor gestorExistente)
        {
            _gestorRed = gestorExistente;
            AeronaveSeleccionada = GestorEstadoGlobal.ObtenerModeloAeronaveEquipada();
            IpSeleccionada = CodificadorSalaTactica.ObtenerIpLocalActiva();

            ConfigurarVentana();
            ConstruirInterfaz();
            CambiarPestanaRed(esInternet: false);
            ActualizarCodigoGenerado();

            _timerLobby.Tick += (s, e) => ActualizarTelemetriaLobby();
            _timerLobby.Start();
        }

        private void ConfigurarVentana()
        {
            Text = "🌐 CENTRO DE OPERACIONES MULTIJUGADOR — LAN & INTERNET | CÓNDOR TÁCTICO";
            Size = new Size(920, 650);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(12, 18, 30);
            ForeColor = Color.White;
            DoubleBuffered = true;
        }

        private void ConstruirInterfaz()
        {
            Font fTitulo = new Font("Segoe UI", 13f, FontStyle.Bold);
            Font fSub = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            Font fNormal = new Font("Segoe UI", 9f, FontStyle.Regular);
            Font fMono = new Font("Consolas", 9.5f, FontStyle.Bold);

            // Cabecera
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(16, 26, 44)
            };
            var lblTit = new Label
            {
                Text = "🌐 LOBBY MULTIJUGADOR Y ENLACE DE ESCUADRONES",
                Font = fTitulo,
                ForeColor = Color.FromArgb(0, 220, 255),
                Location = new Point(18, 14),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTit);

            // Selector claramente diferenciado: LAN vs INTERNET
            _btnTabLan.Text = "◈ RED LOCAL (LAN)";
            _btnTabLan.Bounds = new Rectangle(480, 10, 200, 36);
            _btnTabLan.FlatStyle = FlatStyle.Flat;
            _btnTabLan.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _btnTabLan.Cursor = Cursors.Hand;
            _btnTabLan.Click += (s, e) => CambiarPestanaRed(false);

            _btnTabInternet.Text = "✦ INTERNET (IP / CÓDIGO)";
            _btnTabInternet.Bounds = new Rectangle(688, 10, 208, 36);
            _btnTabInternet.FlatStyle = FlatStyle.Flat;
            _btnTabInternet.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _btnTabInternet.Cursor = Cursors.Hand;
            _btnTabInternet.Click += (s, e) => CambiarPestanaRed(true);

            pnlHeader.Controls.Add(_btnTabLan);
            pnlHeader.Controls.Add(_btnTabInternet);
            Controls.Add(pnlHeader);

            // Banner explicativo de la modalidad de red activa
            _lblBannerRed.Bounds = new Rectangle(18, 66, 870, 40);
            _lblBannerRed.Font = fNormal;
            _lblBannerRed.BackColor = Color.FromArgb(20, 34, 56);
            _lblBannerRed.ForeColor = Color.FromArgb(180, 230, 255);
            _lblBannerRed.Padding = new Padding(10, 6, 10, 6);
            Controls.Add(_lblBannerRed);

            // Columna Izquierda: Configuración de Partida, Personaje/Aeronave y Código de Invitación
            var grpConfig = new GroupBox
            {
                Text = " 1. Modalidad, Aeronave y Equipo ",
                Font = fSub,
                ForeColor = Color.FromArgb(100, 215, 255),
                Bounds = new Rectangle(18, 114, 425, 235),
                BackColor = Color.FromArgb(16, 24, 38)
            };

            AgregarEtiqueta(grpConfig, "Modalidad de Juego:", 16, 28, fNormal);
            _cmbModalidad.Bounds = new Rectangle(160, 25, 245, 26);
            _cmbModalidad.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbModalidad.Font = fNormal;
            _cmbModalidad.Items.AddRange(new object[]
            {
                "⚔️ 1 vs. 1 — Duelo Directo",
                "🛡️ 2 vs. 2 — Competitivo por Equipos",
                "🤝 Cooperativo — Aliados vs Jefes y Oleadas"
            });
            _cmbModalidad.SelectedIndex = 0;
            _cmbModalidad.SelectedIndexChanged += (s, e) =>
            {
                ModalidadSeleccionada = _cmbModalidad.SelectedIndex switch
                {
                    1 => ModalidadPartida.Multijugador_2v2,
                    2 => ModalidadPartida.Multijugador_Cooperativo,
                    _ => ModalidadPartida.Multijugador_1v1
                };
                if (ModalidadSeleccionada == ModalidadPartida.Multijugador_Cooperativo)
                {
                    _cmbEquipo.SelectedIndex = 0;
                    _cmbEquipo.Enabled = false;
                }
                else
                {
                    _cmbEquipo.Enabled = true;
                }
                ActualizarCodigoGenerado();
            };
            grpConfig.Controls.Add(_cmbModalidad);

            AgregarEtiqueta(grpConfig, "Rol de Conexión:", 16, 66, fNormal);
            _cmbRol.Bounds = new Rectangle(160, 63, 245, 26);
            _cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbRol.Font = fNormal;
            _cmbRol.Items.AddRange(new object[]
            {
                "👑 Crear Sala (Host Servidor)",
                "🔗 Unirse a Sala (Cliente)"
            });
            _cmbRol.SelectedIndex = 0;
            _cmbRol.SelectedIndexChanged += (s, e) =>
            {
                ModoRedSeleccionado = _cmbRol.SelectedIndex == 0
                    ? ModoRedJuego.Servidor_Host_Online
                    : ModoRedJuego.Cliente_Remoto_Online;
                ActualizarCodigoGenerado();
            };
            grpConfig.Controls.Add(_cmbRol);

            AgregarEtiqueta(grpConfig, "Aeronave del Piloto:", 16, 104, fNormal);
            _cmbAeronave.Bounds = new Rectangle(160, 101, 245, 26);
            _cmbAeronave.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbAeronave.Font = fNormal;
            _cmbAeronave.Items.AddRange(new object[]
            {
                "⚡ Caza F-35A Lightning II (Rápido)",
                "🦅 Bombardero B-2 Spirit Stealth (Doble Salva)"
            });
            _cmbAeronave.SelectedIndex = AeronaveSeleccionada == TipoAeronave.B2_Spirit_Stealth ? 1 : 0;
            _cmbAeronave.SelectedIndexChanged += (s, e) =>
            {
                AeronaveSeleccionada = _cmbAeronave.SelectedIndex == 1
                    ? TipoAeronave.B2_Spirit_Stealth
                    : TipoAeronave.F35_Lightning_II;
            };
            grpConfig.Controls.Add(_cmbAeronave);

            AgregarEtiqueta(grpConfig, "Escuadrón / Equipo:", 16, 142, fNormal);
            _cmbEquipo.Bounds = new Rectangle(160, 139, 245, 26);
            _cmbEquipo.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbEquipo.Font = fNormal;
            _cmbEquipo.Items.AddRange(new object[]
            {
                "🔵 Escuadrón Cóndor Alfa (Equipo 1)",
                "🔴 Escuadrón Halcón Bravo (Equipo 2)"
            });
            _cmbEquipo.SelectedIndex = 0;
            _cmbEquipo.SelectedIndexChanged += (s, e) => EquipoSeleccionado = _cmbEquipo.SelectedIndex + 1;
            grpConfig.Controls.Add(_cmbEquipo);

            AgregarEtiqueta(grpConfig, "IP Host / Puerto UDP:", 16, 182, fNormal);
            _txtIp.Bounds = new Rectangle(160, 179, 155, 26);
            _txtIp.Font = fMono;
            _txtIp.BackColor = Color.FromArgb(10, 16, 26);
            _txtIp.ForeColor = Color.Cyan;
            _txtIp.Text = IpSeleccionada;
            _txtIp.TextChanged += (s, e) =>
            {
                IpSeleccionada = _txtIp.Text.Trim();
                ActualizarCodigoGenerado();
            };
            grpConfig.Controls.Add(_txtIp);

            _numPuerto.Bounds = new Rectangle(325, 179, 80, 26);
            _numPuerto.Minimum = 1024;
            _numPuerto.Maximum = 65535;
            _numPuerto.Value = 15050;
            _numPuerto.Font = fMono;
            _numPuerto.BackColor = Color.FromArgb(10, 16, 26);
            _numPuerto.ForeColor = Color.Gold;
            _numPuerto.ValueChanged += (s, e) =>
            {
                PuertoSeleccionado = (int)_numPuerto.Value;
                ActualizarCodigoGenerado();
            };
            grpConfig.Controls.Add(_numPuerto);

            Controls.Add(grpConfig);

            // Grupo Invitaciones por Código
            var grpCodigo = new GroupBox
            {
                Text = " 2. Invitaciones y Unión Rápida por Código ",
                Font = fSub,
                ForeColor = Color.FromArgb(255, 210, 90),
                Bounds = new Rectangle(18, 358, 425, 175),
                BackColor = Color.FromArgb(16, 24, 38)
            };

            AgregarEtiqueta(grpCodigo, "Código de Sala (compártelo o pega uno para unirte):", 16, 26, fNormal);
            _txtCodigoInvitacion.Bounds = new Rectangle(16, 50, 265, 28);
            _txtCodigoInvitacion.Font = new Font("Consolas", 10.5f, FontStyle.Bold);
            _txtCodigoInvitacion.BackColor = Color.FromArgb(8, 14, 24);
            _txtCodigoInvitacion.ForeColor = Color.FromArgb(100, 255, 160);
            grpCodigo.Controls.Add(_txtCodigoInvitacion);

            _btnCopiarCodigo.Text = "📋 COPIAR";
            _btnCopiarCodigo.Bounds = new Rectangle(290, 49, 115, 29);
            _btnCopiarCodigo.FlatStyle = FlatStyle.Flat;
            _btnCopiarCodigo.BackColor = Color.FromArgb(30, 85, 130);
            _btnCopiarCodigo.ForeColor = Color.White;
            _btnCopiarCodigo.Cursor = Cursors.Hand;
            _btnCopiarCodigo.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(_txtCodigoInvitacion.Text);
                    MessageBox.Show(this, $"Código '{_txtCodigoInvitacion.Text}' copiado al portapapeles.\nEnvíalo a otro jugador para que se una.", "Código Copiado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch { }
            };
            grpCodigo.Controls.Add(_btnCopiarCodigo);

            _btnUnirsePorCodigo.Text = "🔑 DECODIFICAR Y UNIRSE CON ESTE CÓDIGO";
            _btnUnirsePorCodigo.Bounds = new Rectangle(16, 88, 389, 32);
            _btnUnirsePorCodigo.FlatStyle = FlatStyle.Flat;
            _btnUnirsePorCodigo.BackColor = Color.FromArgb(160, 110, 20);
            _btnUnirsePorCodigo.ForeColor = Color.White;
            _btnUnirsePorCodigo.Cursor = Cursors.Hand;
            _btnUnirsePorCodigo.Click += (s, e) =>
            {
                if (CodificadorSalaTactica.IntentarDecodificarCodigo(_txtCodigoInvitacion.Text, out string ipDec, out int portDec, out ModalidadPartida modDec))
                {
                    _txtIp.Text = ipDec;
                    _numPuerto.Value = portDec;
                    _cmbRol.SelectedIndex = 1; // Cliente
                    _cmbModalidad.SelectedIndex = modDec switch
                    {
                        ModalidadPartida.Multijugador_2v2 => 1,
                        ModalidadPartida.Multijugador_Cooperativo => 2,
                        _ => 0
                    };
                    ConectarAlLobbyActual();
                }
                else
                {
                    MessageBox.Show(this, "El formato del código no es válido. Ejemplo válido: CT-7F000001-3ACA-3", "Código Inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            grpCodigo.Controls.Add(_btnUnirsePorCodigo);

            _btnActivarSala.Text = "📡 INICIAR / CONECTAR SALA EN VIVO";
            _btnActivarSala.Bounds = new Rectangle(16, 128, 389, 34);
            _btnActivarSala.FlatStyle = FlatStyle.Flat;
            _btnActivarSala.BackColor = Color.FromArgb(0, 140, 190);
            _btnActivarSala.ForeColor = Color.White;
            _btnActivarSala.Cursor = Cursors.Hand;
            _btnActivarSala.Click += (s, e) => ConectarAlLobbyActual();
            grpCodigo.Controls.Add(_btnActivarSala);

            Controls.Add(grpCodigo);

            // Columna Derecha: Buscador de Partidas LAN y Sala de Espera (Lobby)
            var grpLobby = new GroupBox
            {
                Text = " 3. Buscador de Partidas y Sala de Espera (Lobby) ",
                Font = fSub,
                ForeColor = Color.FromArgb(100, 240, 160),
                Bounds = new Rectangle(458, 114, 430, 419),
                BackColor = Color.FromArgb(16, 24, 38)
            };

            AgregarEtiqueta(grpLobby, "Partidas detectadas en Red Local (UDP Broadcast :15051):", 16, 26, fNormal);
            _lstSalasLan.Bounds = new Rectangle(16, 48, 396, 110);
            _lstSalasLan.Font = fMono;
            _lstSalasLan.BackColor = Color.FromArgb(8, 14, 22);
            _lstSalasLan.ForeColor = Color.FromArgb(110, 230, 255);
            _lstSalasLan.DoubleClick += (s, e) => UnirseASalaLanSeleccionada();
            grpLobby.Controls.Add(_lstSalasLan);

            var btnUnirseLan = new Button
            {
                Text = "⚡ UNIRSE A LA SALA LAN SELECCIONADA",
                Bounds = new Rectangle(16, 164, 396, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 110, 140),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUnirseLan.Click += (s, e) => UnirseASalaLanSeleccionada();
            grpLobby.Controls.Add(btnUnirseLan);

            AgregarEtiqueta(grpLobby, "Pilotos en la Sala de Espera Actual y Bitácora:", 16, 206, fNormal);
            _lstJugadoresLobby.Bounds = new Rectangle(16, 228, 396, 125);
            _lstJugadoresLobby.Font = fMono;
            _lstJugadoresLobby.BackColor = Color.FromArgb(8, 14, 22);
            _lstJugadoresLobby.ForeColor = Color.FromArgb(140, 255, 160);
            grpLobby.Controls.Add(_lstJugadoresLobby);

            _lblEstadoConexion.Bounds = new Rectangle(16, 360, 396, 22);
            _lblEstadoConexion.Font = fNormal;
            _lblEstadoConexion.ForeColor = Color.Gold;
            _lblEstadoConexion.Text = "Estado: Sala lista para iniciar.";
            grpLobby.Controls.Add(_lblEstadoConexion);

            _lblLatencia.Bounds = new Rectangle(16, 384, 396, 22);
            _lblLatencia.Font = fMono;
            _lblLatencia.ForeColor = Color.Cyan;
            _lblLatencia.Text = "Latencia RTT: 0.2 ms | Paquetes/s: 0";
            grpLobby.Controls.Add(_lblLatencia);

            Controls.Add(grpLobby);

            // Barra Inferior: Desplegar Partida o Volver
            _btnDesplegarCombate.Text = "🚀 DESPLEGAR PARTIDA MULTIJUGADOR AHORA";
            _btnDesplegarCombate.Bounds = new Rectangle(18, 548, 580, 48);
            _btnDesplegarCombate.FlatStyle = FlatStyle.Flat;
            _btnDesplegarCombate.BackColor = Color.FromArgb(20, 165, 95);
            _btnDesplegarCombate.ForeColor = Color.White;
            _btnDesplegarCombate.Font = new Font("Segoe UI", 11.5f, FontStyle.Bold);
            _btnDesplegarCombate.Cursor = Cursors.Hand;
            _btnDesplegarCombate.Click += (s, e) =>
            {
                ConectarAlLobbyActual();
                IniciarPartidaConfirmada = true;
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(_btnDesplegarCombate);

            var btnCerrar = new Button
            {
                Text = "✖ VOLVER AL MENÚ",
                Bounds = new Rectangle(614, 548, 274, 48),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(75, 45, 55),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCerrar.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            Controls.Add(btnCerrar);
        }

        private static void AgregarEtiqueta(Control padre, string texto, int x, int y, Font fuente)
        {
            padre.Controls.Add(new Label
            {
                Text = texto,
                Location = new Point(x, y),
                AutoSize = true,
                Font = fuente,
                ForeColor = Color.FromArgb(215, 225, 240)
            });
        }

        private void CambiarPestanaRed(bool esInternet)
        {
            EsModoInternet = esInternet;
            _gestorRed.EsConexionInternet = esInternet;

            if (!esInternet)
            {
                _btnTabLan.BackColor = Color.FromArgb(0, 150, 210);
                _btnTabLan.ForeColor = Color.White;
                _btnTabInternet.BackColor = Color.FromArgb(28, 40, 60);
                _btnTabInternet.ForeColor = Color.Silver;

                _lblBannerRed.Text = "📡 MODO RED LOCAL (LAN): Conecta computadoras dentro de la misma red Wi-Fi o Ethernet. " +
                                     "El sistema busca salas automáticamente por UDP Broadcast (:15051) con latencia < 2 ms.";
                _txtIp.Text = CodificadorSalaTactica.ObtenerIpLocalActiva();
            }
            else
            {
                _btnTabInternet.BackColor = Color.FromArgb(140, 70, 210);
                _btnTabInternet.ForeColor = Color.White;
                _btnTabLan.BackColor = Color.FromArgb(28, 40, 60);
                _btnTabLan.ForeColor = Color.Silver;

                _lblBannerRed.Text = "🌍 MODO INTERNET (WAN / P2P): Conecta jugadores en distintas redes ingresando la IP Pública del Host " +
                                     "(requiere puerto UDP abierto/NAT) o mediante adaptadores P2P (ZeroTier, Tailscale, Radmin VPN).";
            }

            ActualizarCodigoGenerado();
        }

        private void ActualizarCodigoGenerado()
        {
            string ip = string.IsNullOrWhiteSpace(_txtIp.Text) ? "127.0.0.1" : _txtIp.Text.Trim();
            int puerto = (int)_numPuerto.Value;
            _txtCodigoInvitacion.Text = CodificadorSalaTactica.GenerarCodigoInvitacion(ip, puerto, ModalidadSeleccionada);
        }

        private void ConectarAlLobbyActual()
        {
            _gestorRed.EsConexionInternet = EsModoInternet;
            _gestorRed.ModalidadMultijugador = ModalidadSeleccionada;
            _gestorRed.EquipoLocal = EquipoSeleccionado;
            _gestorRed.IniciarArquitectura(ModoRedSeleccionado, IpSeleccionada, PuertoSeleccionado);
            _gestorRed.EnviarMensajeControl("STATE", $"JUG-{Environment.ProcessId}", GestorEstadoGlobal.Perfil.NombrePiloto, AeronaveSeleccionada);
            ActualizarTelemetriaLobby();
        }

        private void UnirseASalaLanSeleccionada()
        {
            List<SalaMultijugadorDetectada> salas = _gestorRed.ObtenerSalasLanActivas();
            int idx = _lstSalasLan.SelectedIndex;
            if (idx < 0 || idx >= salas.Count)
            {
                MessageBox.Show(this, "Selecciona primero una sala activa de la lista LAN.", "Seleccionar Sala", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SalaMultijugadorDetectada sala = salas[idx];
            _txtIp.Text = sala.DireccionIp;
            _numPuerto.Value = sala.Puerto;
            _cmbRol.SelectedIndex = 1; // Cliente
            _txtCodigoInvitacion.Text = sala.CodigoSala;
            ConectarAlLobbyActual();
        }

        private void ActualizarTelemetriaLobby()
        {
            // 1. Actualizar lista de salas LAN detectadas
            List<SalaMultijugadorDetectada> salas = _gestorRed.ObtenerSalasLanActivas();
            int selPrev = _lstSalasLan.SelectedIndex;
            _lstSalasLan.BeginUpdate();
            _lstSalasLan.Items.Clear();
            if (salas.Count == 0)
            {
                _lstSalasLan.Items.Add("Escaneando red local... (Crea una sala para anunciarla)");
            }
            else
            {
                foreach (var s in salas)
                {
                    string mod = s.Modalidad switch
                    {
                        ModalidadPartida.Multijugador_2v2 => "2v2",
                        ModalidadPartida.Multijugador_Cooperativo => "COOP",
                        _ => "1v1"
                    };
                    _lstSalasLan.Items.Add($"[{mod}] Host: {s.NombreHost} ({s.DireccionIp}:{s.Puerto}) [{s.JugadoresConectados}/{s.MaxJugadores}]");
                }
            }
            if (selPrev >= 0 && selPrev < _lstSalasLan.Items.Count) _lstSalasLan.SelectedIndex = selPrev;
            _lstSalasLan.EndUpdate();

            // 2. Actualizar lista de jugadores y eventos del lobby
            _lstJugadoresLobby.BeginUpdate();
            _lstJugadoresLobby.Items.Clear();
            string eqTexto = EquipoSeleccionado == 1 ? "ALFA" : "BRAVO";
            _lstJugadoresLobby.Items.Add($"★ [LOCAL] {GestorEstadoGlobal.Perfil.NombrePiloto} | {AeronaveSeleccionada} | Eq: {eqTexto} (LISTO)");
            _lstJugadoresLobby.Items.Add($"   Pilotos remotos enlazados: {_gestorRed.ClientesRemotosConectados}");
            foreach (string ev in _gestorRed.ObtenerBitacoraReciente())
            {
                _lstJugadoresLobby.Items.Add("   " + ev);
            }
            _lstJugadoresLobby.EndUpdate();

            _lblEstadoConexion.Text = $"Estado: {_gestorRed.EstadoConexionTexto}";
            _lblLatencia.Text = $"Latencia RTT: {_gestorRed.LatenciaRttMs:F2} ms | Tráfico: {_gestorRed.PaquetesPorSegundo} pkt/s | Remotos: {_gestorRed.ClientesRemotosConectados}";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _timerLobby.Stop();
            _timerLobby.Dispose();
            base.OnFormClosing(e);
        }
    }
}
