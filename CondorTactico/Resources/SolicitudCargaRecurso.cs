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
using System.Threading;
using System.Threading.Tasks;

namespace CondorTactico
{
    /// <summary>
    /// Representa un elemento de trabajo de carga en segundo plano despachado al hilo trabajador.
    /// </summary>
    public sealed class SolicitudCargaRecurso
    {
        public string Id { get; }
        public PrioridadCarga Prioridad { get; }
        public Func<IRecursoJuego> FactoriaCarga { get; }
        public TaskCompletionSource<IRecursoJuego> Tcs { get; }
        public CancellationToken CancellationToken { get; }

        public SolicitudCargaRecurso(
            string id,
            PrioridadCarga prioridad,
            Func<IRecursoJuego> factoriaCarga,
            CancellationToken cancellationToken = default)
        {
            Id = id;
            Prioridad = prioridad;
            FactoriaCarga = factoriaCarga;
            Tcs = new TaskCompletionSource<IRecursoJuego>(TaskCreationOptions.RunContinuationsAsynchronously);
            CancellationToken = cancellationToken;
        }
    }
}


