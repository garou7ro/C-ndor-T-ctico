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
    /// Estructura de texto flotante táctico para feedback de daño, impacto en cabina crítica o absorción por escudo RAM.
    /// </summary>
    public struct TextoFlotanteImpacto
    {
        public float X;
        public float Y;
        public string Texto;
        public Color ColorTexto;
        public int TicksRestantes;
        public bool EsCritico;
    }
}


