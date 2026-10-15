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

using System.Drawing;

namespace CondorTactico
{
    /// <summary>
    /// Estructura de partícula para efectos visuales de postcombustión, chispas y detonaciones aéreas.
    /// Diseñada como Value Type para minimizar la presión sobre el Garbage Collector.
    /// </summary>
    public struct ParticulaCombate
    {
        public float X;
        public float Y;
        public float VX;
        public float VY;
        public float Radio;
        public int VidaRestante;
        public int VidaInicial;
        public Color ColorBase;
    }
}


