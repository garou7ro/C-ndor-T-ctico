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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CondorTactico
{
    /// <summary>
    /// Modos de operación de la Arquitectura Cliente-Servidor para "Juego de Aeronaves Online".
    /// </summary>
    public enum ModoRedJuego
    {
        Local_Combate_IA = 0,
        Servidor_Host_Online = 1,
        Cliente_Remoto_Online = 2
    }

    /// <summary>
    /// Información de una sala multijugador detectada en la red local (LAN) mediante UDP Broadcast.
    /// </summary>
    public sealed class SalaMultijugadorDetectada
    {
        public string CodigoSala { get; set; } = "";
        public string NombreSala { get; set; } = "";
        public string NombreHost { get; set; } = "";
        public string DireccionIp { get; set; } = "";
        public int Puerto { get; set; }
        public ModalidadPartida Modalidad { get; set; } = ModalidadPartida.Multijugador_1v1;
        public int JugadoresConectados { get; set; } = 1;
        public int MaxJugadores { get; set; } = 2;
        public bool RequierePassword { get; set; } = false;
        public string Password { get; set; } = "";
        public DateTime UltimoAnuncio { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Codificador determinista de Códigos de Invitación para unión rápida en LAN o Internet.
    /// Codifica IP IPv4 + Puerto + Modalidad en un token compacto verificable (ej. CT-7F000001-3ACA-3).
    /// </summary>
    public static class CodificadorSalaTactica
    {
        public static string GenerarCodigoInvitacion(string ip, int puerto, ModalidadPartida modalidad)
        {
            if (!IPAddress.TryParse(ip, out IPAddress? addr))
            {
                addr = IPAddress.Loopback;
            }
            byte[] b = addr.GetAddressBytes();
            if (b.Length != 4) b = new byte[] { 127, 0, 0, 1 };

            string hexIp = $"{b[0]:X2}{b[1]:X2}{b[2]:X2}{b[3]:X2}";
            string hexPort = $"{Math.Clamp(puerto, 1024, 65535):X4}";
            int mod = (int)modalidad;
            return $"CT-{hexIp}-{hexPort}-{mod}";
        }

        public static bool IntentarDecodificarCodigo(string codigo, out string ip, out int puerto, out ModalidadPartida modalidad)
        {
            ip = "127.0.0.1";
            puerto = 15050;
            modalidad = ModalidadPartida.Multijugador_1v1;

            if (string.IsNullOrWhiteSpace(codigo)) return false;
            string limpio = codigo.Trim().ToUpperInvariant();
            string[] partes = limpio.Split('-');
            if (partes.Length < 3 || partes[0] != "CT") return false;

            try
            {
                string hexIp = partes[1];
                if (hexIp.Length != 8) return false;
                byte b0 = Convert.ToByte(hexIp.Substring(0, 2), 16);
                byte b1 = Convert.ToByte(hexIp.Substring(2, 2), 16);
                byte b2 = Convert.ToByte(hexIp.Substring(4, 2), 16);
                byte b3 = Convert.ToByte(hexIp.Substring(6, 2), 16);
                ip = $"{b0}.{b1}.{b2}.{b3}";

                puerto = Convert.ToInt32(partes[2], 16);
                if (partes.Length >= 4 && int.TryParse(partes[3], out int m) && Enum.IsDefined(typeof(ModalidadPartida), m))
                {
                    modalidad = (ModalidadPartida)m;
                }
                return puerto >= 1024 && puerto <= 65535;
            }
            catch
            {
                return false;
            }
        }

        public static string ObtenerIpLocalActiva()
        {
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up ||
                        ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                        continue;

                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }
    }

    /// <summary>
    /// DTO (Data Transfer Object) que representa un paquete de sincronización de aeronave en red.
    /// </summary>
    public struct PaqueteRedAeronave
    {
        public string TipoMensaje;      // "STATE", "FIRE", "PING", "PONG", "START", "REMATCH", "DISCONNECT"
        public string IdAeronave;
        public string NombrePiloto;
        public TipoAeronave Modelo;
        public float X;
        public float Y;
        public float Angulo;
        public int Vida;
        public bool Acelerando;
        public bool DisparoActivo;
        public long TimestampTicks;
        public int Equipo;              // 1 = Equipo Alfa (Aliado), 2 = Equipo Bravo (Rival)

        /// <summary>
        /// Función de serialización compacta en O(1) para transmisión por datagrama UDP.
        /// </summary>
        public string Serializar()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}|{1}|{2}|{3}|{4:F1}|{5:F1}|{6:F0}|{7}|{8}|{9}|{10}|{11}",
                TipoMensaje,
                IdAeronave,
                (NombrePiloto ?? "PILOTO").Replace('|', '_'),
                (int)Modelo,
                X,
                Y,
                Angulo,
                Vida,
                Acelerando ? 1 : 0,
                DisparoActivo ? 1 : 0,
                TimestampTicks,
                Equipo <= 0 ? 1 : Equipo);
        }

        /// <summary>
        /// Función de deserialización segura desde trama UDP con validación anti-trampas básica.
        /// </summary>
        public static bool IntentarDeserializar(string trama, out PaqueteRedAeronave paquete)
        {
            paquete = default;
            if (string.IsNullOrWhiteSpace(trama)) return false;

            string[] partes = trama.Split('|');
            if (partes.Length < 11) return false;

            try
            {
                paquete.TipoMensaje = partes[0];
                paquete.IdAeronave = partes[1];
                paquete.NombrePiloto = partes[2];
                paquete.Modelo = (TipoAeronave)Math.Clamp(int.Parse(partes[3], CultureInfo.InvariantCulture), 0, 1);
                paquete.X = Math.Clamp(float.Parse(partes[4], CultureInfo.InvariantCulture), -100f, 3000f);
                paquete.Y = Math.Clamp(float.Parse(partes[5], CultureInfo.InvariantCulture), -100f, 2000f);
                paquete.Angulo = Math.Clamp(float.Parse(partes[6], CultureInfo.InvariantCulture), -45f, 45f);
                paquete.Vida = Math.Clamp(int.Parse(partes[7], CultureInfo.InvariantCulture), 0, 200);
                paquete.Acelerando = partes[8] == "1";
                paquete.DisparoActivo = partes[9] == "1";
                paquete.TimestampTicks = long.Parse(partes[10], CultureInfo.InvariantCulture);
                paquete.Equipo = partes.Length >= 12 ? Math.Clamp(int.Parse(partes[11], CultureInfo.InvariantCulture), 1, 2) : 1;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Gestor de Arquitectura Cliente-Servidor sobre UDP Asíncrono (Sockets no bloqueantes).
    /// Soporta autodescubrimiento LAN por Broadcast, unión por código, modos 1v1, 2v2 y Cooperativo.
    /// </summary>
    public sealed class GestorRedClienteServidor : IDisposable
    {
        public const int PuertoDescubrimientoLan = 15051;

        public ModoRedJuego ModoActual { get; private set; } = ModoRedJuego.Servidor_Host_Online;
        public ModalidadPartida ModalidadMultijugador { get; set; } = ModalidadPartida.Multijugador_1v1;
        public bool EsConexionInternet { get; set; } = false;
        public int EquipoLocal { get; set; } = 1; // 1 = Alfa, 2 = Bravo
        public string DireccionServidor { get; private set; } = "127.0.0.1";
        public int PuertoServidor { get; private set; } = 15050;
        public bool EstaActivo { get; private set; }
        public string NombreSala { get; set; } = "";
        public string PasswordSala { get; set; } = "";
        public bool RequierePassword => !string.IsNullOrWhiteSpace(PasswordSala);

        // Métricas de red para el módulo de Profiling y HUD
        public long PaquetesEnviados { get; private set; }
        public long PaquetesRecibidos { get; private set; }
        public int PaquetesPorSegundo { get; private set; }
        public double LatenciaRttMs { get; private set; } = 0.25;
        public int ClientesRemotosConectados => _clientesConectados.Count;
        public string EstadoConexionTexto { get; private set; } = "SERVIDOR HOST ACTIVO (UDP :15050)";

        private UdpClient? _socketServidor;
        private UdpClient? _socketCliente;
        private UdpClient? _socketBroadcastLan;
        private CancellationTokenSource? _cts;
        private readonly ConcurrentQueue<PaqueteRedAeronave> _colaEntrada = new ConcurrentQueue<PaqueteRedAeronave>();
        private readonly ConcurrentDictionary<string, IPEndPoint> _clientesConectados = new ConcurrentDictionary<string, IPEndPoint>();
        private readonly ConcurrentDictionary<string, DateTime> _ultimaActividadClientes = new ConcurrentDictionary<string, DateTime>();
        private readonly ConcurrentDictionary<string, SalaMultijugadorDetectada> _salasDetectadas = new ConcurrentDictionary<string, SalaMultijugadorDetectada>();
        private readonly Queue<string> _bitacoraEventos = new Queue<string>(16);

        private long _ultimoConteoPaquetes;
        private readonly Stopwatch _relojTasa = Stopwatch.StartNew();

        private static void ConfigurarSocketUdpSeguro(UdpClient udp)
        {
            try
            {
                const int SIO_UDP_CONNRESET = -1744830452;
                udp.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
            }
            catch { }
        }

        public void IniciarArquitectura(ModoRedJuego modo, string ipServidor, int puerto)
        {
            Detener();

            ModoActual = modo;
            DireccionServidor = string.IsNullOrWhiteSpace(ipServidor) ? "127.0.0.1" : ipServidor.Trim();
            PuertoServidor = puerto <= 0 ? 15050 : puerto;
            _cts = new CancellationTokenSource();

            try
            {
                if (modo == ModoRedJuego.Servidor_Host_Online)
                {
                    _socketServidor = new UdpClient(new IPEndPoint(IPAddress.Any, PuertoServidor));
                    ConfigurarSocketUdpSeguro(_socketServidor);
                    _socketCliente = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
                    ConfigurarSocketUdpSeguro(_socketCliente);
                    EstaActivo = true;
                    string tipoRed = EsConexionInternet ? "INTERNET HOST" : "LAN HOST";
                    EstadoConexionTexto = $"{tipoRed} UDP :{PuertoServidor} [ACTIVO]";
                    RegistrarEvento($"Servidor {tipoRed} iniciado en 0.0.0.0:{PuertoServidor}");

                    Task.Run(() => BucleEscuchaServidorAsync(_cts.Token));
                    Task.Run(() => BucleEscuchaClienteAsync(_cts.Token));
                    Task.Run(() => BucleAnuncioLanBroadcastAsync(_cts.Token));
                }
                else if (modo == ModoRedJuego.Cliente_Remoto_Online)
                {
                    _socketCliente = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
                    ConfigurarSocketUdpSeguro(_socketCliente);
                    EstaActivo = true;
                    string tipoRed = EsConexionInternet ? "INTERNET" : "LAN";
                    EstadoConexionTexto = $"CLIENTE {tipoRed} -> {DireccionServidor}:{PuertoServidor}";
                    RegistrarEvento($"Cliente {tipoRed} conectado a {DireccionServidor}:{PuertoServidor}");

                    try
                    {
                        if (DireccionServidor == "127.0.0.1" || DireccionServidor == "localhost")
                        {
                            _socketServidor = new UdpClient(new IPEndPoint(IPAddress.Any, PuertoServidor));
                            ConfigurarSocketUdpSeguro(_socketServidor);
                            Task.Run(() => BucleEscuchaServidorAsync(_cts.Token));
                            RegistrarEvento($"Nodo Loopback autoiniciado en :{PuertoServidor}");
                        }
                    }
                    catch
                    {
                        RegistrarEvento($"Unido a Host existente en {DireccionServidor}:{PuertoServidor}");
                    }

                    Task.Run(() => BucleEscuchaClienteAsync(_cts.Token));
                }
                else
                {
                    _socketCliente = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
                    ConfigurarSocketUdpSeguro(_socketCliente);
                    EstaActivo = true;
                    EstadoConexionTexto = "MODO UN JUGADOR + TELEMETRÍA LOCAL";
                    RegistrarEvento("Simulación local activa");
                }

                // Iniciar escucha de descubrimiento LAN en segundo plano
                IniciarEscuchaDescubrimientoLan(_cts.Token);
            }
            catch (SocketException)
            {
                ModoActual = ModoRedJuego.Cliente_Remoto_Online;
                _socketCliente = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
                ConfigurarSocketUdpSeguro(_socketCliente);
                EstaActivo = true;
                EstadoConexionTexto = $"CLIENTE -> {DireccionServidor}:{PuertoServidor}";
                RegistrarEvento($"Puerto {PuertoServidor} ocupado: operando como Cliente");
                Task.Run(() => BucleEscuchaClienteAsync(_cts.Token));
            }
        }

        private void IniciarEscuchaDescubrimientoLan(CancellationToken token)
        {
            try
            {
                _socketBroadcastLan = new UdpClient();
                _socketBroadcastLan.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _socketBroadcastLan.ExclusiveAddressUse = false;
                _socketBroadcastLan.Client.Bind(new IPEndPoint(IPAddress.Any, PuertoDescubrimientoLan));
                _socketBroadcastLan.EnableBroadcast = true;
                ConfigurarSocketUdpSeguro(_socketBroadcastLan);
                Task.Run(() => BucleEscuchaBroadcastLanAsync(token));
            }
            catch
            {
                // Si el puerto de broadcast ya está en uso en la misma máquina, continuar sin bloquear
            }
        }

        private async Task BucleAnuncioLanBroadcastAsync(CancellationToken token)
        {
            using var emisor = new UdpClient();
            emisor.EnableBroadcast = true;
            IPEndPoint epBroadcast = new IPEndPoint(IPAddress.Broadcast, PuertoDescubrimientoLan);

            while (!token.IsCancellationRequested && EstaActivo && ModoActual == ModoRedJuego.Servidor_Host_Online)
            {
                try
                {
                    string ipLocal = CodificadorSalaTactica.ObtenerIpLocalActiva();
                    string codigo = CodificadorSalaTactica.GenerarCodigoInvitacion(ipLocal, PuertoServidor, ModalidadMultijugador);
                    int maxJug = ModalidadMultijugador == ModalidadPartida.Multijugador_1v1 ? 2 : 4;
                    int jugActuales = Math.Min(maxJug, 1 + _clientesConectados.Count);
                    string pilotoHost = GestorEstadoGlobal.Perfil.NombrePiloto.Replace('|', '_');
                    string nombreSala = string.IsNullOrWhiteSpace(NombreSala) ? $"Escuadrón de {pilotoHost}" : NombreSala.Replace('|', '_');
                    int tienePass = RequierePassword ? 1 : 0;

                    string anuncio = $"BEACON|{codigo}|{pilotoHost}|{ipLocal}|{PuertoServidor}|{(int)ModalidadMultijugador}|{jugActuales}|{maxJug}|{tienePass}|{nombreSala}";
                    byte[] datos = Encoding.UTF8.GetBytes(anuncio);
                    await emisor.SendAsync(datos, datos.Length, epBroadcast);
                }
                catch { }

                try { await Task.Delay(1500, token); } catch { break; }
            }
        }

        private async Task BucleEscuchaBroadcastLanAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _socketBroadcastLan != null)
            {
                try
                {
                    UdpReceiveResult res = await _socketBroadcastLan.ReceiveAsync(token);
                    string trama = Encoding.UTF8.GetString(res.Buffer);
                    if (trama.StartsWith("BEACON|"))
                    {
                        string[] p = trama.Split('|');
                        if (p.Length >= 8)
                        {
                            string codigo = p[1];
                            bool reqPass = p.Length >= 9 && p[8] == "1";
                            string nomSala = p.Length >= 10 ? p[9] : $"Escuadrón de {p[2]}";
                            var sala = new SalaMultijugadorDetectada
                            {
                                CodigoSala = codigo,
                                NombreHost = p[2],
                                NombreSala = nomSala,
                                DireccionIp = p[3],
                                Puerto = int.Parse(p[4], CultureInfo.InvariantCulture),
                                Modalidad = (ModalidadPartida)int.Parse(p[5], CultureInfo.InvariantCulture),
                                JugadoresConectados = int.Parse(p[6], CultureInfo.InvariantCulture),
                                MaxJugadores = int.Parse(p[7], CultureInfo.InvariantCulture),
                                RequierePassword = reqPass,
                                UltimoAnuncio = DateTime.Now
                            };
                            _salasDetectadas[codigo] = sala;
                        }
                    }
                }
                catch (OperationCanceledException) { break; }
                catch { }
            }
        }

        public List<SalaMultijugadorDetectada> ObtenerSalasLanActivas()
        {
            DateTime umbral = DateTime.Now.AddSeconds(-6);
            foreach (var kvp in _salasDetectadas)
            {
                if (kvp.Value.UltimoAnuncio < umbral)
                {
                    _salasDetectadas.TryRemove(kvp.Key, out _);
                }
            }
            return _salasDetectadas.Values.OrderByDescending(s => s.UltimoAnuncio).ToList();
        }

        public void EnviarMensajeControl(string tipoControl, string idEmisor, string nombrePiloto, TipoAeronave modelo)
        {
            if (!EstaActivo || _socketCliente == null) return;

            var pkt = new PaqueteRedAeronave
            {
                TipoMensaje = tipoControl,
                IdAeronave = idEmisor,
                NombrePiloto = nombrePiloto,
                Modelo = modelo,
                X = 0,
                Y = 0,
                Angulo = 0,
                Vida = 30,
                Acelerando = false,
                DisparoActivo = false,
                TimestampTicks = Stopwatch.GetTimestamp(),
                Equipo = EquipoLocal
            };

            TransmitirPaquete(pkt);
        }

        public void EnviarEstadoAeronave(AeronaveBase nave, bool disparoEnEsteTick)
        {
            if (!EstaActivo || _socketCliente == null) return;

            var paquete = new PaqueteRedAeronave
            {
                TipoMensaje = disparoEnEsteTick ? "FIRE" : "STATE",
                IdAeronave = nave.IdAeronave,
                NombrePiloto = nave.NombrePiloto,
                Modelo = nave.Tipo,
                X = nave.X,
                Y = nave.Y,
                Angulo = nave.AnguloInclinacion,
                Vida = nave.VidaActual,
                Acelerando = nave.Acelerando,
                DisparoActivo = disparoEnEsteTick,
                TimestampTicks = Stopwatch.GetTimestamp(),
                Equipo = EquipoLocal
            };

            TransmitirPaquete(paquete);
        }

        public void TransmitirPaquete(PaqueteRedAeronave paquete)
        {
            if (_socketCliente == null) return;

            string trama = paquete.Serializar();
            byte[] bytes = Encoding.UTF8.GetBytes(trama);

            try
            {
                if (_socketServidor != null || ModoActual == ModoRedJuego.Cliente_Remoto_Online)
                {
                    if (IPAddress.TryParse(DireccionServidor, out IPAddress? ipDest))
                    {
                        IPEndPoint destino = new IPEndPoint(ipDest, PuertoServidor);
                        _socketCliente.Send(bytes, bytes.Length, destino);
                        PaquetesEnviados++;
                    }
                }
                else
                {
                    PaquetesEnviados++;
                    PaquetesRecibidos++;
                    LatenciaRttMs = 0.15;
                }
            }
            catch { }

            ActualizarTasaPaquetes();
        }

        public int ExtraerPaquetesRecibidos(List<PaqueteRedAeronave> destino)
        {
            destino.Clear();
            while (_colaEntrada.TryDequeue(out PaqueteRedAeronave pkt))
            {
                destino.Add(pkt);
            }

            // Limpiar clientes inactivos (>8 segundos sin paquetes)
            DateTime limiteInactividad = DateTime.Now.AddSeconds(-8);
            foreach (var kvp in _ultimaActividadClientes)
            {
                if (kvp.Value < limiteInactividad)
                {
                    _ultimaActividadClientes.TryRemove(kvp.Key, out _);
                    if (_clientesConectados.TryRemove(kvp.Key, out _))
                    {
                        RegistrarEvento($"Piloto desconectado por timeout: {kvp.Key}");
                    }
                }
            }

            return destino.Count;
        }

        private async Task BucleEscuchaServidorAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _socketServidor != null)
            {
                try
                {
                    UdpReceiveResult res = await _socketServidor.ReceiveAsync(token);
                    string trama = Encoding.UTF8.GetString(res.Buffer);
                    PaquetesRecibidos++;

                    if (PaqueteRedAeronave.IntentarDeserializar(trama, out PaqueteRedAeronave pkt))
                    {
                        if (pkt.TipoMensaje == "JOIN")
                        {
                            string passRecibida = "";
                            int idxPass = pkt.IdAeronave.IndexOf(":::PASS=");
                            if (idxPass >= 0)
                            {
                                passRecibida = pkt.IdAeronave.Substring(idxPass + 8);
                                pkt.IdAeronave = pkt.IdAeronave.Substring(0, idxPass);
                            }

                            if (RequierePassword && passRecibida != PasswordSala)
                            {
                                RegistrarEvento($"Unión rechazada para {pkt.NombrePiloto}: Clave incorrecta");
                                string denied = $"DENIED_PASSWORD|SERVER|SERVIDOR|0|0|0|0|0|0|0|0|1";
                                byte[] bDenied = Encoding.UTF8.GetBytes(denied);
                                await _socketServidor.SendAsync(bDenied, bDenied.Length, res.RemoteEndPoint);
                                continue;
                            }
                        }

                        bool eraNuevo = !_clientesConectados.ContainsKey(pkt.IdAeronave);
                        _clientesConectados[pkt.IdAeronave] = res.RemoteEndPoint;
                        _ultimaActividadClientes[pkt.IdAeronave] = DateTime.Now;

                        if (eraNuevo)
                        {
                            RegistrarEvento($"Piloto conectado: {pkt.NombrePiloto} ({res.RemoteEndPoint.Address})");
                        }

                        long deltaTicks = Stopwatch.GetTimestamp() - pkt.TimestampTicks;
                        double ms = (double)deltaTicks * 1000.0 / Stopwatch.Frequency;
                        LatenciaRttMs = LatenciaRttMs * 0.8 + Math.Max(0.05, ms) * 0.2;

                        // Retransmitir a los demás clientes conectados
                        foreach (var kvp in _clientesConectados)
                        {
                            if (kvp.Key != pkt.IdAeronave)
                            {
                                await _socketServidor.SendAsync(res.Buffer, res.Buffer.Length, kvp.Value);
                                PaquetesEnviados++;
                            }
                            else
                            {
                                string pong = $"PONG|{pkt.IdAeronave}|{pkt.NombrePiloto}|{(int)pkt.Modelo}|0|0|0|{pkt.Vida}|0|0|{pkt.TimestampTicks}|{pkt.Equipo}";
                                byte[] bPong = Encoding.UTF8.GetBytes(pong);
                                await _socketServidor.SendAsync(bPong, bPong.Length, res.RemoteEndPoint);
                            }
                        }

                        _colaEntrada.Enqueue(pkt);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch { }
            }
        }

        private async Task BucleEscuchaClienteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _socketCliente != null)
            {
                try
                {
                    UdpReceiveResult res = await _socketCliente.ReceiveAsync(token);
                    string trama = Encoding.UTF8.GetString(res.Buffer);
                    PaquetesRecibidos++;

                    if (PaqueteRedAeronave.IntentarDeserializar(trama, out PaqueteRedAeronave pkt))
                    {
                        long deltaTicks = Stopwatch.GetTimestamp() - pkt.TimestampTicks;
                        double ms = (double)deltaTicks * 1000.0 / Stopwatch.Frequency;
                        LatenciaRttMs = LatenciaRttMs * 0.8 + Math.Max(0.05, ms) * 0.2;

                        if (pkt.TipoMensaje != "PONG")
                        {
                            _colaEntrada.Enqueue(pkt);
                        }
                    }
                }
                catch (OperationCanceledException) { break; }
                catch { }
            }
        }

        private void ActualizarTasaPaquetes()
        {
            if (_relojTasa.ElapsedMilliseconds >= 1000)
            {
                long totalActual = PaquetesEnviados + PaquetesRecibidos;
                PaquetesPorSegundo = (int)(totalActual - _ultimoConteoPaquetes);
                _ultimoConteoPaquetes = totalActual;
                _relojTasa.Restart();
            }
        }

        public void RegistrarEvento(string evento)
        {
            lock (_bitacoraEventos)
            {
                if (_bitacoraEventos.Count >= 10) _bitacoraEventos.Dequeue();
                _bitacoraEventos.Enqueue($"[{DateTime.Now:HH:mm:ss}] {evento}");
            }
        }

        public string[] ObtenerBitacoraReciente()
        {
            lock (_bitacoraEventos)
            {
                return _bitacoraEventos.ToArray();
            }
        }

        public void Detener()
        {
            EstaActivo = false;
            try { _cts?.Cancel(); } catch { }
            try { _socketServidor?.Close(); _socketServidor?.Dispose(); } catch { }
            try { _socketCliente?.Close(); _socketCliente?.Dispose(); } catch { }
            try { _socketBroadcastLan?.Close(); _socketBroadcastLan?.Dispose(); } catch { }
            _socketServidor = null;
            _socketCliente = null;
            _socketBroadcastLan = null;
            _clientesConectados.Clear();
            _ultimaActividadClientes.Clear();
        }

        public void Dispose()
        {
            Detener();
        }
    }
}
