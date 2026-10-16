// ==============================================================================
// PROYECTO: CÓNDOR TÁCTICO - HERRAMIENTA AISLADA DE CAPTURA Y TELEMETRÍA REMOTA
// DESCRIPCIÓN: Script/aplicación independiente para generar capturas y clips de
//              avance en segundo plano SIN alterar el código fuente del juego.
// ==============================================================================

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CondorTactico;
using CondorTactico.UI;

namespace CapturadorRemoto
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            string rutaSalida = args.Length > 0 ? args[0] : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "capturas");
            Directory.CreateDirectory(rutaSalida);

            Console.WriteLine("==================================================================");
            Console.WriteLine("🦅 CÓNDOR TÁCTICO - GENERADOR AISLADO DE TELEMETRÍA Y CAPTURAS");
            Console.WriteLine($"📂 Destino: {rutaSalida}");
            Console.WriteLine("==================================================================");

            // 1. Inicializar recursos gráficos de solo lectura
            CacheSpritesAeronaves.InicializarCache();
            RecursosGraficosHud.Inicializar();

            // 2. Captura: Pantalla de Carga Andina (55% de progreso)
            GenerarCapturaPantallaCarga(
                rutaSalida,
                "01_pantalla_carga_andina.png",
                progreso: 0.55,
                etapa: "Rasterizando flota vectorial 4x SSAA",
                detalle: "50/50 sprites en caché");

            // 3. Captura: Pantalla de Carga Despegue Listo (100% de progreso)
            GenerarCapturaPantallaCarga(
                rutaSalida,
                "02_pantalla_carga_despegue.png",
                progreso: 1.0,
                etapa: "Sistemas en línea",
                detalle: "Listo para despegue · Presione cualquier tecla");

            // 4. Captura: Combate Aéreo B-2 Spirit con HUD Militar
            GenerarCapturaCombate(
                rutaSalida,
                "03_combate_b2_spirit_hud.png",
                tipo: TipoAeronave.B2_Spirit_Stealth,
                posX: 430,
                posY: 380,
                ticksIniciales: 12,
                disparar: true,
                ticksPosteriores: 4);

            // 5. Captura: Combate Aéreo F-35A Lightning II (Dogfight & retícula PIPER)
            GenerarCapturaCombate(
                rutaSalida,
                "04_combate_f35_dogfight.png",
                tipo: TipoAeronave.F35_Lightning_II,
                posX: 450,
                posY: 350,
                ticksIniciales: 20,
                disparar: true,
                ticksPosteriores: 8);

            // 6. Secuencia de 60 fotogramas para animación de video/GIF
            string dirFrames = Path.Combine(rutaSalida, "frames_temp");
            Directory.CreateDirectory(dirFrames);
            GenerarSecuenciaFotogramas(dirFrames, totalFrames: 60);

            Console.WriteLine("\n✅ [CapturadorRemoto] Todas las capturas y fotogramas fueron generados exitosamente.");
        }

        private static void GenerarCapturaPantallaCarga(string rutaSalida, string nombreArchivo, double progreso, string etapa, string detalle)
        {
            using var carga = new PantallaCarga();
            carga.ClientSize = new Size(960, 540);

            // Inyectar estado por reflexión sin tocar el código fuente del juego
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            typeof(Form).GetMethod("OnLoad", flags)?.Invoke(carga, new object[] { EventArgs.Empty });

            typeof(PantallaCarga).GetField("_progresoReal", flags)?.SetValue(carga, progreso);
            typeof(PantallaCarga).GetField("_progresoVisual", flags)?.SetValue(carga, progreso);
            typeof(PantallaCarga).GetField("_etapa", flags)?.SetValue(carga, etapa);
            typeof(PantallaCarga).GetField("_detalle", flags)?.SetValue(carga, detalle);
            if (progreso >= 1.0)
            {
                typeof(PantallaCarga).GetField("_listo", flags)?.SetValue(carga, true);
                typeof(PantallaCarga).GetField("_cargaTerminada", flags)?.SetValue(carga, true);
            }

            using var bmp = new Bitmap(960, 540, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                typeof(Control).GetMethod("OnPaint", flags)?.Invoke(carga, new object[] { new PaintEventArgs(g, new Rectangle(0, 0, 960, 540)) });
            }

            string rutaFinal = Path.Combine(rutaSalida, nombreArchivo);
            bmp.Save(rutaFinal, ImageFormat.Png);
            Console.WriteLine($"-> Guardado: {rutaFinal}");
        }

        private static void GenerarCapturaCombate(
            string rutaSalida,
            string nombreArchivo,
            TipoAeronave tipo,
            float posX,
            float posY,
            int ticksIniciales,
            bool disparar,
            int ticksPosteriores)
        {
            using var combate = new VentanaCombateTactico();
            combate.ClientSize = new Size(1020, 680);

            AjustarTamanoLienzo(combate, 1020, 680);
            combate.CambiarAeronaveJugadorDirecto(tipo);

            AeronaveBase? nave = ObtenerNaveJugador(combate);
            if (nave != null)
            {
                nave.X = posX;
                nave.Y = posY;
            }

            SimularTicks(combate, ticksIniciales);
            if (disparar)
            {
                DispararArmamento(combate);
            }
            SimularTicks(combate, ticksPosteriores);

            using var bmp = RenderizarLienzoCombate(combate, 1020, 680);
            string rutaFinal = Path.Combine(rutaSalida, nombreArchivo);
            bmp.Save(rutaFinal, ImageFormat.Png);
            Console.WriteLine($"-> Guardado: {rutaFinal}");
        }

        private static void GenerarSecuenciaFotogramas(string dirFrames, int totalFrames)
        {
            Console.WriteLine($"-> Generando {totalFrames} fotogramas de combate táctico para animación...");
            using var combate = new VentanaCombateTactico();
            combate.ClientSize = new Size(860, 560);
            AjustarTamanoLienzo(combate, 860, 560);
            combate.CambiarAeronaveJugadorDirecto(TipoAeronave.F35_Lightning_II);

            AeronaveBase? nave = ObtenerNaveJugador(combate);
            if (nave != null)
            {
                nave.X = 380;
                nave.Y = 320;
            }

            for (int f = 0; f < totalFrames; f++)
            {
                if (f % 5 == 0) DispararArmamento(combate);
                SimularTicks(combate, 1);

                using var bmp = RenderizarLienzoCombate(combate, 860, 560);
                string rutaFrame = Path.Combine(dirFrames, $"frame_{f:D3}.png");
                bmp.Save(rutaFrame, ImageFormat.Png);
            }
            Console.WriteLine($"-> {totalFrames} fotogramas generados en: {dirFrames}");
        }

        // =========================================================================
        // ACCESO REFLEXIVO TOTALMENTE AISLADO (CERO MODIFICACIÓN AL CÓDIGO DEL JUEGO)
        // =========================================================================
        private static void AjustarTamanoLienzo(VentanaCombateTactico form, int ancho, int alto)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            if (typeof(VentanaCombateTactico).GetField("contiene", flags)?.GetValue(form) is Control lienzo)
            {
                lienzo.Size = new Size(ancho, alto);
            }
        }

        private static AeronaveBase? ObtenerNaveJugador(VentanaCombateTactico form)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            return typeof(VentanaCombateTactico).GetField("_naveJugador", flags)?.GetValue(form) as AeronaveBase;
        }

        private static void DispararArmamento(VentanaCombateTactico form)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            typeof(VentanaCombateTactico).GetMethod("EjecutarDisparoJugador", flags)?.Invoke(form, null);
        }

        private static void SimularTicks(VentanaCombateTactico form, int cantidad)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var metodo = typeof(VentanaCombateTactico).GetMethod("ImpactarTick", flags);
            for (int i = 0; i < cantidad; i++)
            {
                metodo?.Invoke(form, new object?[] { form, EventArgs.Empty });
            }
        }

        private static Bitmap RenderizarLienzoCombate(VentanaCombateTactico form, int ancho, int alto)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var bmp = new Bitmap(ancho, alto, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                var metodoRender = typeof(VentanaCombateTactico).GetMethod("RenderizarEscenaCompleta", flags);
                metodoRender?.Invoke(form, new object?[] { form, new PaintEventArgs(g, new Rectangle(0, 0, ancho, alto)) });
            }
            return bmp;
        }
    }
}
