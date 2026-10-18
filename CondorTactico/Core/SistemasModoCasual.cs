// ==============================================================================
// PROYECTO: CÓNDOR TÁCTICO (SIMULADOR DE COMBATE AÉREO 2D)
// AUTORES:
//   - RONAL ABELIN LARICO CCASO
//   - CLEVER NANDOR MAMANI HUALLPA
//   - JHON YOSSEP HINOJOSA APAZA
//   - JHON ALEX HUARACALLO SUCARI
//   - HECTOR CARLOS TORRES FLORES
// INSTITUCION: UNIVERSIDAD NACIONAL DE JULIACA (UNAJ)
// VERSION: 4.0.0
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CondorTactico
{
    /// <summary>
    /// Representa una cápsula de suministro o power-up recolectable en pleno vuelo.
    /// </summary>
    public struct SuministroCombate
    {
        public float X;
        public float Y;
        public float VY;
        public TipoSuministro Tipo;
        public bool Activo;
        public int TicksVida;

        public RectangleF ObtenerHitbox() => new RectangleF(X - 16f, Y - 16f, 32f, 32f);
    }

    /// <summary>
    /// Representa un obstáculo o peligro ambiental activo en el escenario (rayos, meteoritos, granizo, láser orbital).
    /// </summary>
    public struct ObstaculoAmbiental
    {
        public float X;
        public float Y;
        public float VX;
        public float VY;
        public float RadioOAncho;
        public TipoPeligroAmbiental Tipo;
        public int TicksAdvertencia; // >0 = fase de aviso visual antes de hacer daño
        public int TicksActivos;     // >0 = fase dañina activa
        public bool Activo;

        public bool IntersectaAeronave(AeronaveBase nave)
        {
            if (!Activo || TicksAdvertencia > 0) return false;

            RectangleF rectNave = new RectangleF(nave.X + 10, nave.Y + 8, nave.Ancho - 20, nave.Alto - 16);

            if (Tipo == TipoPeligroAmbiental.TormentaElectrica || Tipo == TipoPeligroAmbiental.LaserOrbitalSatelital)
            {
                // Haz vertical de ancho RadioOAncho centrado en X
                RectangleF rectHaz = new RectangleF(X - RadioOAncho * 0.5f, 0, RadioOAncho, 2000f);
                return rectHaz.IntersectsWith(rectNave);
            }
            else
            {
                // Proyectil ambiental circular (granizo o meteorito piroclástico)
                RectangleF rectObs = new RectangleF(X - RadioOAncho, Y - RadioOAncho, RadioOAncho * 2f, RadioOAncho * 2f);
                return rectObs.IntersectsWith(rectNave);
            }
        }
    }

    /// <summary>
    /// Controlador de progresión del Tutorial Interactivo paso a paso.
    /// </summary>
    public sealed class ControladorTutorial
    {
        public int PasoActual { get; private set; } = 1;
        public float DistanciaVolada { get; set; }
        public int DisparosRealizados { get; set; }
        public bool SuministroRecolectado { get; set; }
        public bool PoderEspecialUsado { get; set; }
        public bool Completado => PasoActual > 5;

        public string ObtenerInstruccionActual()
        {
            return PasoActual switch
            {
                1 => "PASO 1/5 [MANIOBRA DE VUELO]: Usa [W A S D] o las [Flechas] para desplazarte por el espacio aéreo.",
                2 => "PASO 2/5 [SISTEMA DE ARMAS]: Presiona [ESPACIO] o [ENTER] al menos 3 veces para probar tus misiles.",
                3 => "PASO 3/5 [LOGÍSTICA AÉREA]: Vuela hacia la cápsula de suministro brillante para recolectarla.",
                4 => "PASO 4/5 [PODER ESPECIAL]: Tu energía está al 100%. Presiona [Q] o [E] para activar tu Habilidad Táctica.",
                5 => "PASO 5/5 [GRADUACIÓN DE FUEGO]: Derriba la aeronave instructora rival para completar el entrenamiento.",
                _ => "¡TUTORIAL COMPLETADO CON ÉXITO! Estás listo para la Campaña y el Multijugador."
            };
        }

        public void AvanzarPasoSiCorresponde(out bool avanzo)
        {
            avanzo = false;
            if (PasoActual == 1 && DistanciaVolada >= 120f)
            {
                PasoActual = 2;
                avanzo = true;
            }
            else if (PasoActual == 2 && DisparosRealizados >= 3)
            {
                PasoActual = 3;
                avanzo = true;
            }
            else if (PasoActual == 3 && SuministroRecolectado)
            {
                PasoActual = 4;
                avanzo = true;
            }
            else if (PasoActual == 4 && PoderEspecialUsado)
            {
                PasoActual = 5;
                avanzo = true;
            }
        }

        public void Reiniciar()
        {
            PasoActual = 1;
            DistanciaVolada = 0f;
            DisparosRealizados = 0;
            SuministroRecolectado = false;
            PoderEspecialUsado = false;
        }
    }

    /// <summary>
    /// Renderizador auxiliar para los elementos del Modo Casual: Peligros Ambientales, Suministros,
    /// Barra de Jefe Final, Indicador de Poder Especial, Combos y Menú de Pausa.
    /// </summary>
    public static class RenderizadorSistemasJuego
    {
        public static void DibujarObstaculosAmbientales(Graphics g, List<ObstaculoAmbiental> obstaculos, int altoLienzo)
        {
            for (int i = 0; i < obstaculos.Count; i++)
            {
                ObstaculoAmbiental obs = obstaculos[i];
                if (!obs.Activo) continue;

                if (obs.Tipo == TipoPeligroAmbiental.TormentaElectrica || obs.Tipo == TipoPeligroAmbiental.LaserOrbitalSatelital)
                {
                    Color colorBase = obs.Tipo == TipoPeligroAmbiental.LaserOrbitalSatelital
                        ? Color.FromArgb(255, 60, 120)
                        : Color.FromArgb(120, 220, 255);

                    if (obs.TicksAdvertencia > 0)
                    {
                        // Línea de advertencia parpadeante antes de la descarga
                        int alfa = (obs.TicksAdvertencia % 6 < 3) ? 160 : 60;
                        using var penWarn = new Pen(Color.FromArgb(alfa, colorBase), 2f) { DashStyle = DashStyle.Dash };
                        g.DrawLine(penWarn, obs.X, 0, obs.X, altoLienzo);

                        using var fWarn = new Font("Consolas", 9f, FontStyle.Bold);
                        using var brWarn = new SolidBrush(Color.FromArgb(220, 255, 220, 60));
                        g.DrawString("⚠ PELIGRO", fWarn, brWarn, obs.X - 36, 95);
                    }
                    else if (obs.TicksActivos > 0)
                    {
                        // Haz activo de alta energía
                        float wHaz = obs.RadioOAncho;
                        using var brExterior = new SolidBrush(Color.FromArgb(110, colorBase));
                        using var brNucleo = new SolidBrush(Color.FromArgb(230, Color.White));
                        g.FillRectangle(brExterior, obs.X - wHaz * 0.5f, 0, wHaz, altoLienzo);
                        g.FillRectangle(brNucleo, obs.X - wHaz * 0.2f, 0, wHaz * 0.4f, altoLienzo);
                    }
                }
                else if (obs.Tipo == TipoPeligroAmbiental.MeteoritosPiroclasticos)
                {
                    float r = obs.RadioOAncho;
                    using var brHalo = new SolidBrush(Color.FromArgb(90, 255, 90, 20));
                    using var brRoca = new SolidBrush(Color.FromArgb(240, 220, 70, 25));
                    using var brCentro = new SolidBrush(Color.FromArgb(255, 255, 220, 100));
                    g.FillEllipse(brHalo, obs.X - r * 1.4f, obs.Y - r * 1.4f, r * 2.8f, r * 2.8f);
                    g.FillEllipse(brRoca, obs.X - r, obs.Y - r, r * 2f, r * 2f);
                    g.FillEllipse(brCentro, obs.X - r * 0.45f, obs.Y - r * 0.45f, r * 0.9f, r * 0.9f);
                }
                else if (obs.Tipo == TipoPeligroAmbiental.GranizoCriogenico)
                {
                    float r = obs.RadioOAncho;
                    using var brHielo = new SolidBrush(Color.FromArgb(210, 140, 240, 255));
                    using var penHielo = new Pen(Color.White, 1.5f);
                    g.FillEllipse(brHielo, obs.X - r, obs.Y - r, r * 2f, r * 2f);
                    g.DrawEllipse(penHielo, obs.X - r, obs.Y - r, r * 2f, r * 2f);
                }
            }
        }

        public static void DibujarSuministros(Graphics g, List<SuministroCombate> suministros)
        {
            using var fIcono = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            for (int i = 0; i < suministros.Count; i++)
            {
                SuministroCombate s = suministros[i];
                if (!s.Activo) continue;

                (Color color, string etiq) = s.Tipo switch
                {
                    TipoSuministro.ReparacionCasco => (Color.FromArgb(60, 240, 110), "+HP"),
                    TipoSuministro.RecargaEscudo => (Color.FromArgb(60, 190, 255), "+SHD"),
                    TipoSuministro.EnergiaHabilidad => (Color.FromArgb(190, 100, 255), "+EMP"),
                    TipoSuministro.CreditosExtra => (Color.FromArgb(255, 215, 50), "+🪙"),
                    _ => (Color.FromArgb(255, 130, 40), "2X")
                };

                RectangleF r = new RectangleF(s.X - 15f, s.Y - 15f, 30f, 30f);
                using var brFondo = new SolidBrush(Color.FromArgb(190, 12, 22, 36));
                using var brGlow = new SolidBrush(Color.FromArgb(55, color));
                using var penBorde = new Pen(color, 2f);
                using var brTexto = new SolidBrush(color);

                g.FillEllipse(brGlow, s.X - 21f, s.Y - 21f, 42f, 42f);
                g.FillEllipse(brFondo, r);
                g.DrawEllipse(penBorde, r);
                g.DrawString(etiq, fIcono, brTexto, r, sf);
            }
        }

        public static void DibujarBarraJefeFinal(Graphics g, int anchoLienzo, string nombreJefe, int vidaActual, int vidaMax)
        {
            if (vidaActual <= 0 || vidaMax <= 0) return;

            float bw = Math.Min(460f, anchoLienzo * 0.45f);
            float bx = (anchoLienzo - bw) * 0.5f;
            float by = 12f;

            using var brFondo = new SolidBrush(Color.FromArgb(215, 24, 10, 18));
            using var penBorde = new Pen(Color.FromArgb(220, 255, 70, 90), 1.8f);
            g.FillRectangle(brFondo, bx, by, bw, 44f);
            g.DrawRectangle(penBorde, bx, by, bw, 44f);

            using var fTit = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            using var brTexto = new SolidBrush(Color.FromArgb(255, 190, 200));
            g.DrawString($"☠ JEFE DE SECTOR: {nombreJefe} ({vidaActual}/{vidaMax} HP)", fTit, brTexto, bx + 10f, by + 4f);

            float ratio = Math.Clamp((float)vidaActual / vidaMax, 0f, 1f);
            RectangleF rectBarra = new RectangleF(bx + 10f, by + 24f, bw - 20f, 13f);
            using var brVacio = new SolidBrush(Color.FromArgb(40, 15, 20));
            g.FillRectangle(brVacio, rectBarra);

            if (ratio > 0f)
            {
                using var brVida = new LinearGradientBrush(
                    rectBarra,
                    Color.FromArgb(255, 50, 70),
                    Color.FromArgb(255, 165, 30),
                    LinearGradientMode.Horizontal);
                g.FillRectangle(brVida, rectBarra.X, rectBarra.Y, rectBarra.Width * ratio, rectBarra.Height);
            }
        }

        public static void DibujarPanelEstadoTacticoInferior(
            Graphics g,
            int anchoLienzo,
            int altoLienzo,
            float cargaPoder,
            int ticksPoderActivo,
            int comboActual,
            int monedasPartida,
            int vidasReserva,
            bool checkpointActivo,
            string textoOleada)
        {
            float panelW = 540f;
            float px = (anchoLienzo - panelW) * 0.5f;
            float py = altoLienzo - 54f;

            using var brPanel = new SolidBrush(Color.FromArgb(195, 10, 18, 32));
            using var penPanel = new Pen(Color.FromArgb(110, 0, 200, 255), 1f);
            g.FillRectangle(brPanel, px, py, panelW, 44f);
            g.DrawRectangle(penPanel, px, py, panelW, 44f);

            // Barra de Poder Especial [Q]
            TipoPoderEspecial poder = GestorEstadoGlobal.ObtenerPoderEspecialEquipado();
            string nomPoder = poder switch
            {
                TipoPoderEspecial.EscudoCuanticoSobrecarga => "ESCUDO CUÁNTICO",
                TipoPoderEspecial.SobrecargaVulcan_Overdrive => "HIPER-VULCAN",
                TipoPoderEspecial.SalvaTermobarica => "ONDA TERMOBÁRICA",
                _ => "PULSO EMP"
            };

            using var fChica = new Font("Consolas", 8.2f, FontStyle.Bold);
            Color cPoder = cargaPoder >= 100f ? Color.FromArgb(0, 255, 180) : Color.FromArgb(0, 180, 255);
            string estadoPoder = ticksPoderActivo > 0
                ? $"⚡ {nomPoder} [ACTIVO]"
                : (cargaPoder >= 100f ? $"⚡ [Q] {nomPoder} ¡LISTO!" : $"⚡ [Q] {nomPoder}: {(int)cargaPoder}%");

            using var brPoderTxt = new SolidBrush(cPoder);
            g.DrawString(estadoPoder, fChica, brPoderTxt, px + 10f, py + 6f);

            RectangleF rBarra = new RectangleF(px + 10f, py + 24f, 195f, 11f);
            using var brVac = new SolidBrush(Color.FromArgb(20, 32, 48));
            using var brLleno = new SolidBrush(cPoder);
            g.FillRectangle(brVac, rBarra);
            g.FillRectangle(brLleno, rBarra.X, rBarra.Y, rBarra.Width * Math.Clamp(cargaPoder / 100f, 0f, 1f), rBarra.Height);

            // Multiplicador de Combo, Monedas, Checkpoint y Oleada
            Color cCombo = comboActual >= 4 ? Color.Gold : Color.White;
            using var brCombo = new SolidBrush(cCombo);
            using var brInfo = new SolidBrush(Color.FromArgb(190, 230, 255));

            string linea1 = $"COMBO: x{Math.Max(1, comboActual)}  |  CR: +{monedasPartida}  |  VIDAS EXTRA: {vidasReserva}";
            string linea2 = $"{textoOleada}" + (checkpointActivo ? "  [✔ CHECKPOINT]" : "");

            g.DrawString(linea1, fChica, brCombo, px + 220f, py + 6f);
            g.DrawString(linea2, fChica, brInfo, px + 220f, py + 24f);
        }

        public static void DibujarMenuPausaSuperpuesto(Graphics g, int w, int h, bool checkpointDisponible)
        {
            using var brVelo = new SolidBrush(Color.FromArgb(185, 6, 10, 20));
            g.FillRectangle(brVelo, 0, 0, w, h);

            float cw = 480f;
            float ch = 330f;
            float cx = (w - cw) * 0.5f;
            float cy = (h - ch) * 0.5f;

            using var brCaja = new SolidBrush(Color.FromArgb(240, 14, 24, 42));
            using var penBorde = new Pen(Color.FromArgb(0, 210, 255), 2f);
            g.FillRectangle(brCaja, cx, cy, cw, ch);
            g.DrawRectangle(penBorde, cx, cy, cw, ch);

            using var fTit = new Font("Segoe UI", 16f, FontStyle.Bold);
            using var fOpc = new Font("Consolas", 10.5f, FontStyle.Bold);
            using var brCian = new SolidBrush(Color.FromArgb(0, 225, 255));

            g.DrawString("⏸ SIMULACIÓN EN PAUSA TÁCTICA", fTit, brCian, cx + 52f, cy + 22f);

            string[] opciones =
            {
                "[P] o [ESPACIO]  ->  Reanudar Combate",
                checkpointDisponible
                    ? "[K]              ->  Reaparecer en Último Checkpoint"
                    : "[R]              ->  Reiniciar Operación Actual",
                "[C]              ->  Abrir Configuración (Audio / Controles)",
                "[T]              ->  Abrir Tienda e Inventario",
                "[M] o [BACKSPACE]->  Guardar y Volver al Menú Principal"
            };

            for (int i = 0; i < opciones.Length; i++)
            {
                RectangleF r = new RectangleF(cx + 28f, cy + 78f + i * 44f, cw - 56f, 36f);
                using var brFila = new SolidBrush(Color.FromArgb(24, 40, 68));
                g.FillRectangle(brFila, r);
                g.DrawString(opciones[i], fOpc, Brushes.White, r.X + 12f, r.Y + 9f);
            }
        }
    }
}
