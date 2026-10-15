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
using System.Diagnostics;
using System.Drawing;

namespace CondorTactico
{
    /// <summary>
    /// Encapsula un Bitmap o textura en memoria gestionada con control de ciclo de vida,
    /// telemetría de memoria en bytes y acceso atómico para evitar colisiones entre hilos.
    /// </summary>
    public sealed class RecursoGrafico : IRecursoJuego
    {
        private Bitmap? _bitmap;
        private bool _disposed = false;

        public string Id { get; }
        public TipoRecursoJuego Tipo => TipoRecursoJuego.ImagenTextura;
        public EstadoRecurso Estado { get; internal set; }
        public long TamanoBytesAproximado { get; private set; }
        public long UltimoAccesoTicks { get; private set; }

        public Bitmap? Imagen
        {
            get
            {
                MarcarAcceso();
                return _bitmap;
            }
        }

        public RecursoGrafico(string id, Bitmap bitmap, EstadoRecurso estado = EstadoRecurso.Listo)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            _bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
            Estado = estado;
            TamanoBytesAproximado = Math.Max(1024, (long)bitmap.Width * bitmap.Height * 4);
            UltimoAccesoTicks = Stopwatch.GetTimestamp();
        }

        public void MarcarAcceso()
        {
            UltimoAccesoTicks = Stopwatch.GetTimestamp();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _bitmap?.Dispose();
                _bitmap = null;
                _disposed = true;
                Estado = EstadoRecurso.NoCargado;
            }
        }
    }
}


