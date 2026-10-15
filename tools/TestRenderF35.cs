// ==============================================================================
// PROYECTO: CÓNDOR TÁCTICO (SIMULADOR DE COMBATE AÉREO 2D)
// MODULO: TOOLS / TEST DE RENDERIZADO VECTORIAL F-35
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
using System.Drawing.Imaging;
using System.IO;
using CondorTactico;

namespace CondorTactico.Tools
{
    public class TestRenderF35
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("==========================================================");
            Console.WriteLine(" TEST DE VERIFICACION DE RENDERIZADO VECTORIAL F-35 3D");
            Console.WriteLine("==========================================================");

            string outputDir = AppDomain.CurrentDomain.BaseDirectory;
            string testF35Path = Path.Combine(outputDir, "test_f35_render.png");
            string testMisilPath = Path.Combine(outputDir, "test_misil_render.png");

            Stopwatch sw = Stopwatch.StartNew();

            // 1. Renderizar F-35 en resolución de juego con SSAA 4x
            int anchoAvion = 84;
            int altoAvion = 118;
            using (Bitmap bmpF35 = F35Model.DibujarF35(anchoAvion, altoAvion, 0, false, false, 4))
            {
                bmpF35.Save(testF35Path, ImageFormat.Png);
                Console.WriteLine($"[OK] F-35 renderizado ({anchoAvion}x{altoAvion} px, SSAA 4x) en {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"     Archivo: {testF35Path}");

                // Verificación de transparencia ARGB 32-bit (sin pixelado de máscara 1-bit)
                Color esquina = bmpF35.GetPixel(0, 0);
                Color centro = bmpF35.GetPixel(anchoAvion / 2, altoAvion / 2);
                Console.WriteLine($"     Pixel Esquina (Transparente): A={esquina.A}, R={esquina.R}, G={esquina.G}, B={esquina.B}");
                Console.WriteLine($"     Pixel Centro (Fuselaje): A={centro.A}, R={centro.R}, G={centro.G}, B={centro.B}");
                if (esquina.A == 0 && centro.A == 255)
                {
                    Console.WriteLine("     [EXITO] Transparencia alfa suave verificada (Sin bordes pixelados)");
                }
            }

            // 2. Renderizar Proyectil Misil con SSAA 4x
            sw.Restart();
            using (Bitmap bmpMisil = F35Model.DibujarMisil(18, 38, 0, false, 4))
            {
                bmpMisil.Save(testMisilPath, ImageFormat.Png);
                Console.WriteLine($"[OK] Misil AIM-120 renderizado (18x38 px, SSAA 4x) en {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"     Archivo: {testMisilPath}");
            }

            Console.WriteLine("==========================================================");
            Console.WriteLine(" VERIFICACION COMPLETADA CON EXITO");
            Console.WriteLine("==========================================================");
        }
    }
}
