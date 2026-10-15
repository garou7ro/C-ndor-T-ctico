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
using System.Windows.Forms;
using CondorTactico.UI;

namespace CondorTactico
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para el simulador Cóndor Táctico.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Desplegar la pantalla de carga e introducción táctica (60 FPS, paralaje, precalentamiento)
            using var pantallaCarga = new PantallaCarga();
            DialogResult res = pantallaCarga.ShowDialog();

            if (res != DialogResult.OK || pantallaCarga.Resultado == null)
            {
                // El usuario canceló la carga (Escape) o cerró la ventana
                return;
            }

            // Iniciar el simulador de combate táctico con los recursos precargados
            Application.Run(new VentanaCombateTactico(pantallaCarga.Resultado));
        }
    }
}

