// ==============================================================================
// PROYECTO: CÓNDOR TÁCTICO 3D (MOTOR WEBGL 2.0 + WEBVIEW2 GPU + RED UDP NATIVA)
// AUTORES:
//   - RONAL ABELIN LARICO CCASO
//   - CLEVER NANDOR MAMANI HUALLPA
//   - JHON YOSSEP HINOJOSA APAZA
//   - JHON ALEX HUARACALLO SUCARI
//   - HECTOR CARLOS TORRES FLORES
// INSTITUCION: UNIVERSIDAD NACIONAL DE JULIACA (UNAJ)
// VERSION: 5.1.0
// ==============================================================================

using System;
using System.Windows.Forms;
using CondorTactico.UI;

namespace CondorTactico
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para Cóndor Táctico 3D.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new VentanaJuego3D());
        }
    }
}
