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

namespace CondorTactico
{
    /// <summary>
    /// Contrato base unificado para todos los recursos que se gestionan y transmiten
    /// en segundo plano dentro del motor (imágenes, mallas vectoriales, audio, telemetría).
    /// </summary>
    public interface IRecursoJuego : IDisposable
    {
        string Id { get; }
        TipoRecursoJuego Tipo { get; }
        EstadoRecurso Estado { get; }
        long TamanoBytesAproximado { get; }
        long UltimoAccesoTicks { get; }

        /// <summary>
        /// Registra la marca temporal de acceso para la política de desalojo LRU (Least Recently Used).
        /// </summary>
        void MarcarAcceso();
    }
}


