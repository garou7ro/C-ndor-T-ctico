// ==============================================================================
// PROYECTO: CÓNDOR TÁCTICO (SIMULADOR DE COMBATE AÉREO 2D)
// AUTORES:
//   - RONAL ABELIN LARICO CCASO
//   - CLEVER NANDOR MAMANI HUALLPA
//   - JHON YOSSEP HINOJOSA APAZA
//   - JHON ALEX HUARACALLO SUCARI
//   - HECTOR CARLOS TORRES FLORES
// INSTITUCION: UNIVERSIDAD NACIONAL DE JULIACA (UNAJ)
// VERSION: 3.4.0
// ==============================================================================

using System;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;

namespace CondorTactico.Audio
{
    /// <summary>
    /// Sintetizador de audio militar procedural en memoria (100% C# nativo, cero dependencias).
    /// Genera y reproduce en tiempo real efectos de sonido PCM (disparos de cañón Vulcan 20mm,
    /// ignición de misiles, explosiones subsónicas, alertas de radar lock y estampidos sónicos).
    /// </summary>
    public static class SintetizadorAudioTactico
    {
        private static volatile bool _inicializado;
        private static volatile bool _audioSilenciado;

        private static byte[]? _wavCanon;
        private static byte[]? _wavMisil;
        private static byte[]? _wavExplosionNormal;
        private static byte[]? _wavExplosionCritica;
        private static byte[]? _wavAlertaLock;
        private static byte[]? _wavSonicBoom;
        private static byte[]? _wavImpactoMetal;

        private static long _ultimoTickCanon;
        private static long _ultimoTickLock;
        private static long _ultimoTickExplosion;

        public static bool AudioHabilitado
        {
            get => !_audioSilenciado;
            set => _audioSilenciado = !value;
        }

        public static bool EstaInicializado => _inicializado;

        /// <summary>
        /// Pre-sintetiza todas las formas de onda en memoria durante la pantalla de carga.
        /// Ejecución ultrarrápida (&lt; 20ms) e idempotente.
        /// </summary>
        public static void Inicializar()
        {
            if (_inicializado) return;

            try
            {
                _wavCanon = SintetizarDisparoCanon();
                _wavMisil = SintetizarLanzamientoMisil();
                _wavExplosionNormal = SintetizarExplosion(esCritica: false);
                _wavExplosionCritica = SintetizarExplosion(esCritica: true);
                _wavAlertaLock = SintetizarAlertaLock();
                _wavSonicBoom = SintetizarSonicBoom();
                _wavImpactoMetal = SintetizarImpactoMetalico();

                _inicializado = true;
            }
            catch
            {
                // En entornos sin dispositivo de audio el juego continúa en modo silencioso sin fallar
                _audioSilenciado = true;
            }
        }

        public static void AlternarAudio()
        {
            _audioSilenciado = !_audioSilenciado;
        }

        public static void ReproducirCanon()
        {
            if (_audioSilenciado || _wavCanon == null) return;
            long ahora = Environment.TickCount64;
            if (ahora - _ultimoTickCanon < 65) return; // Cooldown anti-saturación
            _ultimoTickCanon = ahora;

            ReproducirWavAsincrono(_wavCanon);
        }

        public static void ReproducirMisil()
        {
            if (_audioSilenciado || _wavMisil == null) return;
            ReproducirWavAsincrono(_wavMisil);
        }

        public static void ReproducirExplosion(bool esCritica)
        {
            if (_audioSilenciado) return;
            byte[]? wav = esCritica ? _wavExplosionCritica : _wavExplosionNormal;
            if (wav == null) return;

            long ahora = Environment.TickCount64;
            if (ahora - _ultimoTickExplosion < 85) return;
            _ultimoTickExplosion = ahora;

            ReproducirWavAsincrono(wav);
        }

        public static void ReproducirAlertaLock()
        {
            if (_audioSilenciado || _wavAlertaLock == null) return;
            long ahora = Environment.TickCount64;
            if (ahora - _ultimoTickLock < 450) return;
            _ultimoTickLock = ahora;

            ReproducirWavAsincrono(_wavAlertaLock);
        }

        public static void ReproducirAlertaRadar() => ReproducirAlertaLock();

        public static void ReproducirSonicBoom()
        {
            if (_audioSilenciado || _wavSonicBoom == null) return;
            ReproducirWavAsincrono(_wavSonicBoom);
        }

        public static void ReproducirImpacto()
        {
            if (_audioSilenciado || _wavImpactoMetal == null) return;
            ReproducirWavAsincrono(_wavImpactoMetal);
        }

        private static void ReproducirWavAsincrono(byte[] wav)
        {
            Task.Run(() =>
            {
                try
                {
                    using var ms = new MemoryStream(wav);
                    using var player = new SoundPlayer(ms);
                    player.Play();
                }
                catch
                {
                    // Ignorar excepciones de reproducción si el hardware de sonido está ocupado
                }
            });
        }

        // =====================================================================
        // GENERADORES PROCEDURALES DE AUDIO PCM (WAVE RIFF 16-BIT 22050 HZ)
        // =====================================================================
        private const int SampleRate = 22050;

        private static byte[] SintetizarDisparoCanon()
        {
            int totalMuestras = (int)(SampleRate * 0.11);
            short[] muestras = new short[totalMuestras];
            var rnd = new Random(42);

            for (int i = 0; i < totalMuestras; i++)
            {
                double t = (double)i / SampleRate;
                double envolvente = Math.Exp(-t * 36.0);
                double tono = Math.Sin(2.0 * Math.PI * 135.0 * t) * 0.7;
                double armonica = Math.Sin(2.0 * Math.PI * 270.0 * t) * 0.3;
                double ruido = (rnd.NextDouble() * 2.0 - 1.0) * 0.45;

                double senal = (tono + armonica + ruido) * envolvente;
                muestras[i] = (short)Math.Clamp((int)(senal * 24000), short.MinValue, short.MaxValue);
            }

            return EmpaquetarWavMono16Bit(muestras, SampleRate);
        }

        private static byte[] SintetizarLanzamientoMisil()
        {
            int totalMuestras = (int)(SampleRate * 0.28);
            short[] muestras = new short[totalMuestras];
            var rnd = new Random(101);

            for (int i = 0; i < totalMuestras; i++)
            {
                double t = (double)i / SampleRate;
                double frac = t / 0.28;
                double envolvente = frac < 0.15 ? (frac / 0.15) : Math.Exp(-(frac - 0.15) * 4.5);
                double freq = 160.0 + (frac * 320.0);
                double tono = Math.Sin(2.0 * Math.PI * freq * t) * 0.55;
                double siseo = (rnd.NextDouble() * 2.0 - 1.0) * 0.45;

                double senal = (tono + siseo) * envolvente;
                muestras[i] = (short)Math.Clamp((int)(senal * 22000), short.MinValue, short.MaxValue);
            }

            return EmpaquetarWavMono16Bit(muestras, SampleRate);
        }

        private static byte[] SintetizarExplosion(bool esCritica)
        {
            double duracion = esCritica ? 0.42 : 0.26;
            int totalMuestras = (int)(SampleRate * duracion);
            short[] muestras = new short[totalMuestras];
            var rnd = new Random(esCritica ? 777 : 555);

            double filtroRuido = 0;
            for (int i = 0; i < totalMuestras; i++)
            {
                double t = (double)i / SampleRate;
                double envolvente = Math.Exp(-t * (esCritica ? 7.0 : 12.0));

                double subBajo = Math.Sin(2.0 * Math.PI * (esCritica ? 42.0 : 58.0) * t);
                double ruidoCrudo = rnd.NextDouble() * 2.0 - 1.0;
                filtroRuido = (filtroRuido * 0.82) + (ruidoCrudo * 0.18);

                double senal = (subBajo * 0.6 + filtroRuido * 0.65) * envolvente;
                if (senal > 0.75) senal = 0.75 + (senal - 0.75) * 0.3; // Saturación cálida
                if (senal < -0.75) senal = -0.75 + (senal + 0.75) * 0.3;

                muestras[i] = (short)Math.Clamp((int)(senal * (esCritica ? 29000 : 23000)), short.MinValue, short.MaxValue);
            }

            return EmpaquetarWavMono16Bit(muestras, SampleRate);
        }

        private static byte[] SintetizarAlertaLock()
        {
            int totalMuestras = (int)(SampleRate * 0.16);
            short[] muestras = new short[totalMuestras];

            for (int i = 0; i < totalMuestras; i++)
            {
                double t = (double)i / SampleRate;
                double freq = t < 0.08 ? 960.0 : 1440.0;
                double senal = Math.Sign(Math.Sin(2.0 * Math.PI * freq * t)) * 0.45;
                double envolvente = Math.Min(1.0, (0.16 - t) * 15.0);

                muestras[i] = (short)Math.Clamp((int)(senal * envolvente * 16000), short.MinValue, short.MaxValue);
            }

            return EmpaquetarWavMono16Bit(muestras, SampleRate);
        }

        private static byte[] SintetizarSonicBoom()
        {
            int totalMuestras = (int)(SampleRate * 0.38);
            short[] muestras = new short[totalMuestras];

            for (int i = 0; i < totalMuestras; i++)
            {
                double t = (double)i / SampleRate;
                double envolvente = Math.Exp(-t * 9.0);
                double bajo1 = Math.Sin(2.0 * Math.PI * 34.0 * t);
                double bajo2 = Math.Sin(2.0 * Math.PI * 68.0 * t) * 0.5;

                double senal = (bajo1 + bajo2) * envolvente;
                muestras[i] = (short)Math.Clamp((int)(senal * 27000), short.MinValue, short.MaxValue);
            }

            return EmpaquetarWavMono16Bit(muestras, SampleRate);
        }

        private static byte[] SintetizarImpactoMetalico()
        {
            int totalMuestras = (int)(SampleRate * 0.08);
            short[] muestras = new short[totalMuestras];
            var rnd = new Random(88);

            for (int i = 0; i < totalMuestras; i++)
            {
                double t = (double)i / SampleRate;
                double envolvente = Math.Exp(-t * 60.0);
                double tonoPing = Math.Sin(2.0 * Math.PI * 1850.0 * t) * 0.6;
                double click = (rnd.NextDouble() * 2.0 - 1.0) * 0.4;

                double senal = (tonoPing + click) * envolvente;
                muestras[i] = (short)Math.Clamp((int)(senal * 22000), short.MinValue, short.MaxValue);
            }

            return EmpaquetarWavMono16Bit(muestras, SampleRate);
        }

        /// <summary>
        /// Empaqueta muestras PCM de 16-bit en formato estándar RIFF WAVE mono en memoria.
        /// </summary>
        private static byte[] EmpaquetarWavMono16Bit(short[] muestras, int sampleRate)
        {
            int subChunk2Size = muestras.Length * 2;
            int chunkSize = 36 + subChunk2Size;

            byte[] wav = new byte[44 + subChunk2Size];
            using var ms = new MemoryStream(wav);
            using var bw = new BinaryWriter(ms);

            // RIFF header
            bw.Write(new[] { 'R', 'I', 'F', 'F' });
            bw.Write(chunkSize);
            bw.Write(new[] { 'W', 'A', 'V', 'E' });

            // fmt subchunk
            bw.Write(new[] { 'f', 'm', 't', ' ' });
            bw.Write(16); // SubChunk1Size (PCM = 16)
            bw.Write((short)1); // AudioFormat (1 = PCM)
            bw.Write((short)1); // NumChannels (1 = Mono)
            bw.Write(sampleRate); // SampleRate
            bw.Write(sampleRate * 2); // ByteRate (SampleRate * NumChannels * BitsPerSample/8)
            bw.Write((short)2); // BlockAlign (NumChannels * BitsPerSample/8)
            bw.Write((short)16); // BitsPerSample

            // data subchunk
            bw.Write(new[] { 'd', 'a', 't', 'a' });
            bw.Write(subChunk2Size);

            for (int i = 0; i < muestras.Length; i++)
            {
                bw.Write(muestras[i]);
            }

            return wav;
        }
    }
}
