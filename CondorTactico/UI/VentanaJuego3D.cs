// ==============================================================================
// PROYECTO: CÓNDOR TÁCTICO 3D (MOTOR WEBGL 2.0 + WEBVIEW2 GPU + RED UDP NATIVA)
// AUTORES:
//   - RONAL ABELIN LARICO CCASO
//   - CLEVER NANDOR MAMANI HUALLPA
//   - JHON YOSSEP HINOJOSA APAZA
//   - JHON ALEX HUARACALLO SUCARI
//   - HECTOR CARLOS TORRES FLORES
// INSTITUCION: UNIVERSIDAD NACIONAL DE JULIACA (UNAJ)
// VERSION: 5.0.0
// ==============================================================================

using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace CondorTactico.UI
{
    /// <summary>
    /// Ventana nativa acelerada por GPU (WebView2 + Three.js WebGL 2.0) que aloja
    /// la interfaz moderna 3D y la conecta en tiempo real con la persistencia JSON
    /// (GestorEstadoGlobal) y el motor multijugador UDP (GestorRedClienteServidor).
    /// </summary>
    public sealed class VentanaJuego3D : Form
    {
        private readonly WebView2 _vista3D = new WebView2();
        private readonly GestorRedClienteServidor _gestorRed = new GestorRedClienteServidor();
        private readonly System.Windows.Forms.Timer _timerRed = new System.Windows.Forms.Timer { Interval = 80 };

        public VentanaJuego3D()
        {
            ConfigurarVentana();
            InicializarMotor3DAsincrono();
        }

        private void ConfigurarVentana()
        {
            Text = "Cóndor Táctico 3D";
            Size = new Size(1366, 768);
            MinimumSize = new Size(1024, 640);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(5, 11, 22);
            KeyPreview = true;

            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F11)
                {
                    e.Handled = true;
                    AlternarModoPantallaCompleta();
                }
            };

            // Cargar el icono de la ventana desde logo.png
            try
            {
                string rutaLogo = ResolverRutaWwwRoot("assets/logo.png");
                if (File.Exists(rutaLogo))
                {
                    using var bmp = new Bitmap(rutaLogo);
                    Icon = Icon.FromHandle(bmp.GetHicon());
                }
            }
            catch
            {
                // Ignorar si el entorno restringe HICON
            }

            _vista3D.Dock = DockStyle.Fill;
            _vista3D.DefaultBackgroundColor = Color.FromArgb(5, 11, 22);
            Controls.Add(_vista3D);

            _timerRed.Tick += (s, e) => EnviarActualizacionRedAlMotor3D();
            _timerRed.Start();
        }

        public void AlternarModoPantallaCompleta()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(AlternarModoPantallaCompleta));
                return;
            }

            if (WindowState == FormWindowState.Maximized && FormBorderStyle == FormBorderStyle.None)
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Normal;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
        }

        private static string ResolverDirectorioWwwRoot()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string candidato1 = Path.Combine(baseDir, "wwwroot");
            if (Directory.Exists(candidato1)) return candidato1;

            string candidato2 = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "wwwroot"));
            if (Directory.Exists(candidato2)) return candidato2;

            string candidato3 = Path.GetFullPath(Path.Combine(baseDir, "CondorTactico", "wwwroot"));
            if (Directory.Exists(candidato3)) return candidato3;

            return candidato1;
        }

        private static string ResolverRutaWwwRoot(string subRuta)
        {
            return Path.Combine(ResolverDirectorioWwwRoot(), subRuta.Replace('/', Path.DirectorySeparatorChar));
        }

        private async void InicializarMotor3DAsincrono()
        {
            try
            {
                string carpetaDatosWeb = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "CondorTacticoUNAJ",
                    "WebView2Cache");

                // Configurar argumentos para forzar aceleración por GPU dedicada
                var opcionesGpu = new CoreWebView2EnvironmentOptions(
                    "--enable-gpu-rasterization --enable-zero-copy --ignore-gpu-blocklist --enable-features=VaapiVideoDecoder,WebAssemblySimd --disable-gpu-driver-bug-workarounds --force_high_performance_gpu --enable-accelerated-2d-canvas --enable-webgl --enable-webgl2-compute-context"
                );

                var env = await CoreWebView2Environment.CreateAsync(null, carpetaDatosWeb, opcionesGpu);
                await _vista3D.EnsureCoreWebView2Async(env);

                _vista3D.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                _vista3D.CoreWebView2.Settings.AreDevToolsEnabled = false;
                _vista3D.CoreWebView2.Settings.IsStatusBarEnabled = false;

                string dirWwwRoot = ResolverDirectorioWwwRoot();
                _vista3D.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "condor.local",
                    dirWwwRoot,
                    CoreWebView2HostResourceAccessKind.Allow);

                _vista3D.CoreWebView2.WebMessageReceived += AlRecibirMensajeDesdeMotor3D;
                _vista3D.CoreWebView2.Navigate("https://condor.local/index.html");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"No se pudo inicializar el motor gráfico 3D WebView2: {ex.Message}",
                    "Cóndor Táctico 3D",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void AlRecibirMensajeDesdeMotor3D(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string json = e.TryGetWebMessageAsString();
                if (string.IsNullOrWhiteSpace(json)) return;

                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;
                if (!root.TryGetProperty("accion", out JsonElement propAccion)) return;

                string accion = propAccion.GetString() ?? "";

                switch (accion)
                {
                    case "solicitarEstadoInicial":
                        EnviarEstadoInicialAlMotor3D();
                        break;

                    case "guardarEstado":
                        SincronizarEstadoDesdeMotor3D(root);
                        break;

                    case "iniciarSalaRed":
                        {
                            string ipLocal = CodificadorSalaTactica.ObtenerIpLocalActiva();
                            if (root.TryGetProperty("modalidad", out JsonElement propMod))
                            {
                                string sMod = propMod.GetString() ?? "";
                                if (Enum.TryParse<ModalidadPartida>(sMod, out var parsedMod))
                                {
                                    _gestorRed.ModalidadMultijugador = parsedMod;
                                }
                            }
                            if (root.TryGetProperty("equipo", out JsonElement propEq))
                            {
                                _gestorRed.EquipoLocal = propEq.GetInt32();
                            }
                            else
                            {
                                _gestorRed.EquipoLocal = 1;
                            }
                            if (root.TryGetProperty("nombreSala", out JsonElement propNomSala))
                            {
                                _gestorRed.NombreSala = propNomSala.GetString() ?? "";
                            }
                            else
                            {
                                _gestorRed.NombreSala = "";
                            }
                            if (root.TryGetProperty("password", out JsonElement propPass))
                            {
                                _gestorRed.PasswordSala = propPass.GetString() ?? "";
                            }
                            else
                            {
                                _gestorRed.PasswordSala = "";
                            }
                            _gestorRed.IniciarArquitectura(ModoRedJuego.Servidor_Host_Online, ipLocal, 15050);
                            EnviarActualizacionRedAlMotor3D();
                        }
                        break;

                    case "unirseConCodigo":
                        if (root.TryGetProperty("codigo", out JsonElement propCod))
                        {
                            string codigo = propCod.GetString() ?? "";
                            string passIngresada = root.TryGetProperty("password", out JsonElement pPass) ? (pPass.GetString() ?? "") : "";
                            if (CodificadorSalaTactica.IntentarDecodificarCodigo(codigo, out string ipHost, out int puertoHost, out ModalidadPartida mod))
                            {
                                _gestorRed.ModalidadMultijugador = mod;
                                if (root.TryGetProperty("equipo", out JsonElement propEqClient))
                                {
                                    _gestorRed.EquipoLocal = propEqClient.GetInt32();
                                }
                                else
                                {
                                    _gestorRed.EquipoLocal = 2;
                                }
                                _gestorRed.IniciarArquitectura(ModoRedJuego.Cliente_Remoto_Online, ipHost, puertoHost);
                                string idPayload = string.IsNullOrWhiteSpace(passIngresada)
                                    ? GestorEstadoGlobal.Perfil.NombrePiloto
                                    : $"{GestorEstadoGlobal.Perfil.NombrePiloto}:::PASS={passIngresada}";
                                _gestorRed.EnviarMensajeControl("JOIN", idPayload, GestorEstadoGlobal.Perfil.NombrePiloto, TipoAeronave.F35_Lightning_II);
                                EnviarActualizacionRedAlMotor3D();
                            }
                        }
                        break;

                    case "marcarListoRed":
                        {
                            bool listo = root.TryGetProperty("listo", out var pl) && pl.GetBoolean();
                            _gestorRed.EnviarMensajeControl(listo ? "READY" : "UNREADY", GestorEstadoGlobal.Perfil.NombrePiloto, GestorEstadoGlobal.Perfil.NombrePiloto, TipoAeronave.F35_Lightning_II);
                        }
                        break;

                    case "iniciarCombateMultijugador":
                        {
                            _gestorRed.EnviarMensajeControl("START", GestorEstadoGlobal.Perfil.NombrePiloto, GestorEstadoGlobal.Perfil.NombrePiloto, TipoAeronave.F35_Lightning_II);
                        }
                        break;

                    case "estadoCombateRed":
                        {
                            float x = root.TryGetProperty("x", out var px) ? (float)px.GetDouble() : 0f;
                            float z = root.TryGetProperty("z", out var pz) ? (float)pz.GetDouble() : 0f;
                            int vida = root.TryGetProperty("vida", out var pv) ? pv.GetInt32() : 100;
                            bool disparo = root.TryGetProperty("disparo", out var pd) && pd.GetBoolean();

                            var pkt = new PaqueteRedAeronave
                            {
                                TipoMensaje = disparo ? "FIRE" : "STATE",
                                IdAeronave = GestorEstadoGlobal.Perfil.NombrePiloto,
                                NombrePiloto = GestorEstadoGlobal.Perfil.NombrePiloto,
                                Modelo = TipoAeronave.F35_Lightning_II,
                                X = x,
                                Y = z,
                                Angulo = 0,
                                Vida = vida,
                                Acelerando = true,
                                DisparoActivo = disparo,
                                TimestampTicks = System.Diagnostics.Stopwatch.GetTimestamp(),
                                Equipo = _gestorRed.EquipoLocal
                            };
                            _gestorRed.TransmitirPaquete(pkt);
                        }
                        break;

                    case "alternarPantallaCompleta":
                        AlternarModoPantallaCompleta();
                        break;

                    case "salirJuego":
                        GestorEstadoGlobal.GuardarTodo();
                        Close();
                        break;
                }
            }
            catch
            {
                // Ignorar mensajes malformados
            }
        }

        private void EnviarEstadoInicialAlMotor3D()
        {
            if (_vista3D.CoreWebView2 == null) return;

            PerfilJugador p = GestorEstadoGlobal.Perfil;
            if (!p.ArticulosComprados.Contains("NAVE_CONDOR_X"))
            {
                p.ArticulosComprados.Add("NAVE_CONDOR_X");
                p.AeronaveEquipadaId = "NAVE_CONDOR_X";
                GestorEstadoGlobal.GuardarTodo();
            }
            string ipLocal = CodificadorSalaTactica.ObtenerIpLocalActiva();
            string codigoSala = CodificadorSalaTactica.GenerarCodigoInvitacion(ipLocal, 15050, ModalidadPartida.Multijugador_1v1);

            var payload = new
            {
                tipo = "sincronizarPerfil",
                datos = new
                {
                    nombrePiloto = p.NombrePiloto,
                    monedas = p.Monedas,
                    xp = p.ExperienciaTotal,
                    nivelMaxCampana = p.NivelMaximoCampanaDesbloqueado,
                    aeronaveEquipada = p.AeronaveEquipadaId,
                    skinEquipada = p.SkinEquipadaId,
                    poderEquipado = p.PoderEquipadoId,
                    partidas = p.PartidasJugadas,
                    victorias = p.VictoriasTotales,
                    bajas = p.BajasTotales,
                    jefes = p.JefesDerrotados,
                    comboMax = p.ComboMaximoHistorico,
                    red = new
                    {
                        ipLocal,
                        puerto = 15050,
                        codigoSala
                    }
                }
            };

            _vista3D.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(payload));
        }

        private static void SincronizarEstadoDesdeMotor3D(JsonElement root)
        {
            if (!root.TryGetProperty("estado", out JsonElement est)) return;
            PerfilJugador p = GestorEstadoGlobal.Perfil;

            if (est.TryGetProperty("nombrePiloto", out var vNom)) p.NombrePiloto = vNom.GetString() ?? p.NombrePiloto;
            if (est.TryGetProperty("monedas", out var vMon)) p.Monedas = vMon.GetInt32();
            if (est.TryGetProperty("xp", out var vXp)) p.ExperienciaTotal = vXp.GetInt32();
            if (est.TryGetProperty("nivelMaxCampana", out var vNiv)) p.NivelMaximoCampanaDesbloqueado = vNiv.GetInt32();
            if (est.TryGetProperty("aeronaveEquipada", out var vNav)) p.AeronaveEquipadaId = vNav.GetString() ?? p.AeronaveEquipadaId;
            if (est.TryGetProperty("skinEquipada", out var vSkin)) p.SkinEquipadaId = vSkin.GetString() ?? p.SkinEquipadaId;
            if (est.TryGetProperty("poderEquipado", out var vPod)) p.PoderEquipadoId = vPod.GetString() ?? p.PoderEquipadoId;

            GestorEstadoGlobal.GuardarTodo();
        }

        private void EnviarActualizacionRedAlMotor3D()
        {
            if (_vista3D.CoreWebView2 == null) return;
            var salas = _gestorRed.ObtenerSalasLanActivas();
            var listaSalas = new System.Collections.Generic.List<object>(salas.Count);
            for (int i = 0; i < salas.Count; i++)
            {
                var s = salas[i];
                listaSalas.Add(new
                {
                    codigo = s.CodigoSala,
                    nombre = string.IsNullOrWhiteSpace(s.NombreSala) ? $"Escuadrón de {s.NombreHost}" : s.NombreSala,
                    host = s.NombreHost,
                    ip = s.DireccionIp,
                    puerto = s.Puerto,
                    modalidad = s.Modalidad.ToString(),
                    jugadores = s.JugadoresConectados,
                    maxJugadores = s.MaxJugadores,
                    requierePassword = s.RequierePassword
                });
            }

            var paquetes = new System.Collections.Generic.List<PaqueteRedAeronave>();
            _gestorRed.ExtraerPaquetesRecibidos(paquetes);
            var listaPkts = new System.Collections.Generic.List<object>(paquetes.Count);
            foreach (var p in paquetes)
            {
                listaPkts.Add(new
                {
                    tipo = p.TipoMensaje,
                    id = p.IdAeronave,
                    piloto = p.NombrePiloto,
                    x = p.X,
                    z = p.Y,
                    vida = p.Vida,
                    disparo = p.DisparoActivo,
                    equipo = p.Equipo
                });
            }

            string ipLocal = CodificadorSalaTactica.ObtenerIpLocalActiva();
            var payload = new
            {
                tipo = "estadoRed",
                red = new
                {
                    ipLocal,
                    puerto = _gestorRed.PuertoServidor,
                    codigoSala = CodificadorSalaTactica.GenerarCodigoInvitacion(ipLocal, _gestorRed.PuertoServidor, _gestorRed.ModalidadMultijugador),
                    salasLan = listaSalas,
                    conectados = _gestorRed.ClientesRemotosConectados + 1,
                    modalidad = _gestorRed.ModalidadMultijugador.ToString(),
                    equipoLocal = _gestorRed.EquipoLocal,
                    paquetes = listaPkts
                }
            };

            try
            {
                _vista3D.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(payload));
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _timerRed.Stop();
            _timerRed.Dispose();
            _gestorRed.Dispose();
            GestorEstadoGlobal.GuardarTodo();
            base.OnFormClosing(e);
        }
    }
}
