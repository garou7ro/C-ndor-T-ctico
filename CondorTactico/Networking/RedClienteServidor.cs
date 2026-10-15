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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
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
    /// DTO (Data Transfer Object) que representa un paquete de sincronización de aeronave en red.
    /// </summary>
    public struct PaqueteRedAeronave
    {
        public string TipoMensaje;      // "STATE", "FIRE", "PING", "PONG"
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

        /// <summary>
        /// Función de serialización compacta en O(1) para transmisión por datagrama UDP.
        /// </summary>
        public string Serializar()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}|{1}|{2}|{3}|{4:F1}|{5:F1}|{6:F0}|{7}|{8}|{9}|{10}",
                TipoMensaje,
                IdAeronave,
                NombrePiloto.Replace('|', '_'),
                (int)Modelo,
                X,
                Y,
                Angulo,
                Vida,
                Acelerando ? 1 : 0,
                DisparoActivo ? 1 : 0,
                TimestampTicks);
        }

        /// <summary>
        /// Función de deserialización segura desde trama UDP.
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
                paquete.Modelo = (TipoAeronave)int.Parse(partes[3], CultureInfo.InvariantCulture);
                paquete.X = float.Parse(partes[4], CultureInfo.InvariantCulture);
                paquete.Y = float.Parse(partes[5], CultureInfo.InvariantCulture);
                paquete.Angulo = float.Parse(partes[6], CultureInfo.InvariantCulture);
                paquete.Vida = int.Parse(partes[7], CultureInfo.InvariantCulture);
                paquete.Acelerando = partes[8] == "1";
                paquete.DisparoActivo = partes[9] == "1";
                paquete.TimestampTicks = long.Parse(partes[10], CultureInfo.InvariantCulture);
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
    /// Permite lanzar aeronaves en red local (LAN / 127.0.0.1) y medir telemetría de red en tiempo real.
    /// </summary>
    public sealed class GestorRedClienteServidor : IDisposable
    {
        public ModoRedJuego ModoActual { get; private set; } = ModoRedJuego.Servidor_Host_Online;
        public string DireccionServidor { get; private set; } = "127.0.0.1";
        public int PuertoServidor { get; private set; } = 15050;
        public bool EstaActivo { get; private set; }

        // Métricas de red para el módulo de Profiling
        public long PaquetesEnviados { get; private set; }
        public long PaquetesRecibidos { get; private set; }
        public int PaquetesPorSegundo { get; private set; }
        public double LatenciaRttMs { get; private set; } = 0.25;
        public int ClientesRemotosConectados => _clientesConectados.Count;
        public string EstadoConexionTexto { get; private set; } = "SERVIDOR HOST ACTIVO (UDP :15050)";

        private UdpClient? _socketServidor;
        private UdpClient? _socketCliente;
        private CancellationTokenSource? _cts;
        private readonly ConcurrentQueue<PaqueteRedAeronave> _colaEntrada = new ConcurrentQueue<PaqueteRedAeronave>();
        private readonly ConcurrentDictionary<string, IPEndPoint> _clientesConectados = new ConcurrentDictionary<string, IPEndPoint>();
        private readonly Queue<string> _bitacoraEventos = new Queue<string>(12);

        private long _ultimoConteoPaquetes;
        private readonly Stopwatch _relojTasa = Stopwatch.StartNew();

        private static void ConfigurarSocketUdpSeguro(UdpClient udp)
        {
            try
            {
                // Evita SocketException (WSAECONNRESET) en Windows cuando un par UDP se desconecta
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
                    _socketCliente = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
                    ConfigurarSocketUdpSeguro(_socketCliente);
                    EstaActivo = true;
                    EstadoConexionTexto = $"HOST SERVIDOR UDP :{PuertoServidor} [ACTIVO]";
                    RegistrarEvento($"Servidor iniciado en 0.0.0.0:{PuertoServidor}");

                    Task.Run(() => BucleEscuchaServidorAsync(_cts.Token));
                    Task.Run(() => BucleEscuchaClienteAsync(_cts.Token));
                }
                else if (modo == ModoRedJuego.Cliente_Remoto_Online)
                {
                    _socketCliente = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
                    ConfigurarSocketUdpSeguro(_socketCliente);
                    EstaActivo = true;
                    EstadoConexionTexto = $"CLIENTE -> {DireccionServidor}:{PuertoServidor}";
                    RegistrarEvento($"Cliente conectado a {DireccionServidor}:{PuertoServidor}");

                    try
                    {
                        _socketServidor = new UdpClient(new IPEndPoint(IPAddress.Any, PuertoServidor));
                        ConfigurarSocketUdpSeguro(_socketServidor);
                        Task.Run(() => BucleEscuchaServidorAsync(_cts.Token));
                        RegistrarEvento($"Nodo Loopback autoiniciado en :{PuertoServidor}");
                    }
                    catch
                    {
                        RegistrarEvento($"Conectado a Servidor Maestro existente en :{PuertoServidor}");
                    }

                    Task.Run(() => BucleEscuchaClienteAsync(_cts.Token));
                }
                else
                {
                    _socketCliente = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
                    ConfigurarSocketUdpSeguro(_socketCliente);
                    EstaActivo = true;
                    EstadoConexionTexto = "MODO ESCUADRÓN LOCAL + TELEMETRÍA";
                    RegistrarEvento("Simulación local con bus de telemetría activo");
                }
            }
            catch (SocketException)
            {
                ModoActual = ModoRedJuego.Cliente_Remoto_Online;
                _socketCliente = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
                ConfigurarSocketUdpSeguro(_socketCliente);
                EstaActivo = true;
                EstadoConexionTexto = $"CLIENTE MULTIJUGADOR -> {DireccionServidor}:{PuertoServidor}";
                RegistrarEvento($"Puerto {PuertoServidor} activo: unido como Cliente Remoto");
                Task.Run(() => BucleEscuchaClienteAsync(_cts.Token));
            }
        }

        /// <summary>
        /// Procedimiento que transmite el estado de una aeronave al servidor o a los clientes conectados.
        /// </summary>
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
                TimestampTicks = Stopwatch.GetTimestamp()
            };

            string trama = paquete.Serializar();
            byte[] bytes = Encoding.UTF8.GetBytes(trama);

            try
            {
                if (_socketServidor != null || ModoActual == ModoRedJuego.Cliente_Remoto_Online)
                {
                    IPEndPoint destino = new IPEndPoint(IPAddress.Parse(DireccionServidor), PuertoServidor);
                    _socketCliente.Send(bytes, bytes.Length, destino);
                    PaquetesEnviados++;
                }
                else
                {
                    PaquetesEnviados++;
                    PaquetesRecibidos++;
                    LatenciaRttMs = 0.15;
                }
            }
            catch
            {
                // Ignorar desconexiones transitorias UDP
            }

            ActualizarTasaPaquetes();
        }

        /// <summary>
        /// Función que extrae todos los paquetes recibidos en la cola sin bloquear el hilo de UI.
        /// </summary>
        public int ExtraerPaquetesRecibidos(List<PaqueteRedAeronave> destino)
        {
            destino.Clear();
            while (_colaEntrada.TryDequeue(out PaqueteRedAeronave pkt))
            {
                destino.Add(pkt);
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
                        _clientesConectados[pkt.IdAeronave] = res.RemoteEndPoint;

                        // Calcular RTT de ida y vuelta
                        long deltaTicks = Stopwatch.GetTimestamp() - pkt.TimestampTicks;
                        double ms = (double)deltaTicks * 1000.0 / Stopwatch.Frequency;
                        LatenciaRttMs = LatenciaRttMs * 0.8 + Math.Max(0.05, ms) * 0.2;

                        // Retransmitir (Broadcast) a todos los demás clientes conectados
                        foreach (var kvp in _clientesConectados)
                        {
                            if (kvp.Key != pkt.IdAeronave)
                            {
                                await _socketServidor.SendAsync(res.Buffer, res.Buffer.Length, kvp.Value);
                                PaquetesEnviados++;
                            }
                            else
                            {
                                // Enviar confirmación PONG para medición de latencia en el cliente
                                string pong = $"PONG|{pkt.IdAeronave}|{pkt.NombrePiloto}|{(int)pkt.Modelo}|0|0|0|{pkt.Vida}|0|0|{pkt.TimestampTicks}";
                                byte[] bPong = Encoding.UTF8.GetBytes(pong);
                                await _socketServidor.SendAsync(bPong, bPong.Length, res.RemoteEndPoint);
                            }
                        }

                        _colaEntrada.Enqueue(pkt);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Continuar escuchando ante paquetes malformados
                }
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
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                }
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

        private void RegistrarEvento(string evento)
        {
            lock (_bitacoraEventos)
            {
                if (_bitacoraEventos.Count >= 6) _bitacoraEventos.Dequeue();
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
            _socketServidor = null;
            _socketCliente = null;
            _clientesConectados.Clear();
        }

        public void Dispose()
        {
            Detener();
        }
    }
}


