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

namespace CondorTactico
{
    /// <summary>
    /// Ciclo de vida y estado de sincronización de un recurso del motor.
    /// </summary>
    public enum EstadoRecurso
    {
        NoCargado = 0,
        EnCola = 1,
        Cargando = 2,
        Listo = 3,
        Error = 4
    }

    /// <summary>
    /// Nivel de prioridad para la cola de despacho en segundo plano.
    /// </summary>
    public enum PrioridadCarga
    {
        Baja = 0,
        Normal = 1,
        Alta = 2,
        Critica = 3
    }

    /// <summary>
    /// Categoría del activo multimedia o técnico del juego.
    /// </summary>
    public enum TipoRecursoJuego
    {
        ImagenTextura = 0,
        MallaVectorial = 1,
        ClipAudio = 2,
        DatosMision = 3
    }
}


