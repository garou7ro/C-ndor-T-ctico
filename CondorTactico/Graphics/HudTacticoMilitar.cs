// ==============================================================================
// PROYECTO: CÓNDOR TÁCTICO (SIMULADOR DE COMBATE AÉREO 2D)
// AUTORES:
//   - RONAL ABELIN LARICO CCASO
//   - CLEVER NANDOR MAMANI HUALLPA
//   - JHON YOSSEP HINOJOSA APAZA
//   - JHON ALEX HUARACALLO SUCARI
//   - HECTOR CARLOS TORRES FLORES
// INSTITUCION: UNIVERSIDAD NACIONAL DE JULIACA (UNAJ)
// VERSION: 3.4.0
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using CondorTactico.Audio;

namespace CondorTactico.GraphicsEffects
{
    public enum ModoHudMilitar
    {
        CabinaVidrioCompleta,
        RetículaTacticoMinima,
        Desactivado
    }

    /// <summary>
    /// HUD holográfico militar de combate de 5ª generación (estilo F-35 Gen 5 / B-2 Spirit Glass Cockpit).
    /// Proyecta horizonte artificial, escala de cabeceo/alabeo, vector de velocidad (Flight Path Marker),
    /// cinta de rumbo/brújula, cintas de velocidad/altitud, barrido de radar AESA y retícula predictiva de disparo (PIPER).
    /// </summary>
    public sealed class HudTacticoMilitar
    {
        public ModoHudMilitar ModoActual { get; set; } = ModoHudMilitar.CabinaVidrioCompleta;

        private float _anguloRadarBarrido = 0f;
        private float _tiempoHud = 0f;
        private bool _objetivoFijado = false;
        private float _distanciaObjetivo = 0f;
        private AeronaveBase? _objetivoCercano = null;

        // Colores militares fósforo cian/verde táctico de alta visibilidad
        private static readonly Color ColorVerdeTactico = Color.FromArgb(215, 60, 245, 170);
        private static readonly Color ColorCianHud = Color.FromArgb(200, 70, 230, 255);
        private static readonly Color ColorAlertaRojo = Color.FromArgb(240, 255, 65, 65);
        private static readonly Color ColorAmbarAlerta = Color.FromArgb(235, 255, 185, 45);

        private static Font? _fuenteHud;
        private static Font? _fuenteHudPequena;
        private static Font? _fuenteHudBold;

        public static void AsegurarFuentes()
        {
            _fuenteHud ??= new Font("Consolas", 8.5f, FontStyle.Bold);
            _fuenteHudPequena ??= new Font("Consolas", 7.2f, FontStyle.Regular);
            _fuenteHudBold ??= new Font("Consolas", 10.5f, FontStyle.Bold);
        }

        public void AlternarModo()
        {
            ModoActual = ModoActual switch
            {
                ModoHudMilitar.CabinaVidrioCompleta => ModoHudMilitar.RetículaTacticoMinima,
                ModoHudMilitar.RetículaTacticoMinima => ModoHudMilitar.Desactivado,
                _ => ModoHudMilitar.CabinaVidrioCompleta
            };
        }

        public void Actualizar(float dt, AeronaveBase jugador, IReadOnlyList<AeronaveBase> aeronaves)
        {
            _tiempoHud += dt;
            _anguloRadarBarrido = (_anguloRadarBarrido + (dt * 130f)) % 360f;

            // Búsqueda del objetivo rival más cercano en el cono frontal
            _objetivoCercano = null;
            float mejorDistanciaSqr = float.MaxValue;
            bool habiaFijacion = _objetivoFijado;
            _objetivoFijado = false;

            float jcx = jugador.X + jugador.Ancho * 0.5f;
            float jcy = jugador.Y + jugador.Alto * 0.5f;

            for (int i = 0; i < aeronaves.Count; i++)
            {
                AeronaveBase rival = aeronaves[i];
                if (!rival.Activo || !rival.EsRival) continue;

                float rcx = rival.X + rival.Ancho * 0.5f;
                float rcy = rival.Y + rival.Alto * 0.5f;

                float dx = rcx - jcx;
                float dy = rcy - jcy;
                float distSqr = dx * dx + dy * dy;

                if (distSqr < mejorDistanciaSqr)
                {
                    mejorDistanciaSqr = distSqr;
                    _objetivoCercano = rival;
                    _distanciaObjetivo = (float)Math.Sqrt(distSqr);
                }
            }

            if (_objetivoCercano != null && _distanciaObjetivo < 550f)
            {
                _objetivoFijado = true;
                if (!habiaFijacion)
                {
                    SintetizadorAudioTactico.ReproducirAlertaLock();
                }
            }
        }

        public void Dibujar(Graphics g, int ancho, int alto, AeronaveBase jugador, SolidBrush brocha, Pen pluma)
        {
            if (ModoActual == ModoHudMilitar.Desactivado || !jugador.Activo) return;
            AsegurarFuentes();

            float jcx = jugador.X + jugador.Ancho * 0.5f;
            float jcy = jugador.Y + jugador.Alto * 0.5f;

            // Retícula mínima siempre visible en ambos modos
            DibujarSimboloProaYVectorVelocidad(g, jcx, jcy, jugador, brocha, pluma);
            DibujarObjetivoYRetículaPredictiva(g, jcx, jcy, jugador, brocha, pluma);

            if (ModoActual == ModoHudMilitar.CabinaVidrioCompleta)
            {
                DibujarEscalaCabeceoYAlabeo(g, jcx, jcy, jugador, pluma);
                DibujarCintaRumbo(g, ancho, pluma, brocha);
                DibujarCintasVelocidadYAltitud(g, ancho, alto, jugador, pluma, brocha);
                DibujarBarridoRadarAesa(g, jcx, jcy, pluma, brocha);
                DibujarAlertasYEstadoArmamento(g, ancho, alto, jugador, brocha, pluma);
            }
        }

        // =====================================================================
        // COMPONENTES INDIVIDUALES DEL HUD MILITAR
        // =====================================================================

        private void DibujarSimboloProaYVectorVelocidad(Graphics g, float cx, float cy, AeronaveBase jugador, SolidBrush brocha, Pen pluma)
        {
            pluma.Color = ColorVerdeTactico;
            pluma.Width = 1.4f;

            // 1. Símbolo de proa (Waterline W) centrado en el eje de la nave
            float wy = cy - 42f;
            g.DrawLine(pluma, cx - 18f, wy, cx - 6f, wy);
            g.DrawLine(pluma, cx - 6f, wy, cx, wy + 5f);
            g.DrawLine(pluma, cx, wy + 5f, cx + 6f, wy);
            g.DrawLine(pluma, cx + 6f, wy, cx + 18f, wy);

            // 2. Vector de velocidad (Flight Path Marker - FPM: círculo con alas y aleta)
            float fpmX = cx + (jugador.AnguloInclinacion * 1.6f);
            float fpmY = wy - 14f;
            g.DrawEllipse(pluma, fpmX - 5f, fpmY - 5f, 10f, 10f);
            g.DrawLine(pluma, fpmX - 11f, fpmY, fpmX - 5f, fpmY); // Ala izq
            g.DrawLine(pluma, fpmX + 5f, fpmY, fpmX + 11f, fpmY); // Ala der
            g.DrawLine(pluma, fpmX, fpmY - 5f, fpmX, fpmY - 10f); // Aleta vertical
        }

        private void DibujarEscalaCabeceoYAlabeo(Graphics g, float cx, float cy, AeronaveBase jugador, Pen pluma)
        {
            float wy = cy - 42f;
            float roll = jugador.AnguloInclinacion;

            var estadoPrevio = g.Save();
            g.TranslateTransform(cx, wy);
            g.RotateTransform(roll);

            pluma.Color = Color.FromArgb(140, ColorVerdeTactico);
            pluma.Width = 1f;

            // Línea de horizonte artificial con espacio central
            g.DrawLine(pluma, -110f, 0f, -24f, 0f);
            g.DrawLine(pluma, 24f, 0f, 110f, 0f);

            // Barras de cabeceo (+10°, +5°, -5°, -10°)
            DibujarBarraCabeceo(g, pluma, -36f, "+10", esAscenso: true);
            DibujarBarraCabeceo(g, pluma, -18f, "+5", esAscenso: true);
            DibujarBarraCabeceo(g, pluma, 18f, "-5", esAscenso: false);
            DibujarBarraCabeceo(g, pluma, 36f, "-10", esAscenso: false);

            // Arco de alabeo en la parte superior
            g.DrawArc(pluma, -65f, -65f, 130f, 130f, 220f, 100f);

            g.Restore(estadoPrevio);
        }

        private static void DibujarBarraCabeceo(Graphics g, Pen pluma, float y, string etiqueta, bool esAscenso)
        {
            float mitadAncho = 42f;
            if (esAscenso)
            {
                // Barras sólidas continuas
                g.DrawLine(pluma, -mitadAncho, y, -16f, y);
                g.DrawLine(pluma, -mitadAncho, y, -mitadAncho, y + 4f);
                g.DrawLine(pluma, 16f, y, mitadAncho, y);
                g.DrawLine(pluma, mitadAncho, y, mitadAncho, y + 4f);
            }
            else
            {
                // Barras punteadas para descenso
                using var pDash = new Pen(pluma.Color, pluma.Width) { DashStyle = DashStyle.Dash };
                g.DrawLine(pDash, -mitadAncho, y, -16f, y);
                g.DrawLine(pDash, 16f, y, mitadAncho, y);
            }
        }

        private void DibujarObjetivoYRetículaPredictiva(Graphics g, float cx, float cy, AeronaveBase jugador, SolidBrush brocha, Pen pluma)
        {
            if (_objetivoCercano == null || !_objetivoCercano.Activo) return;

            float rcx = _objetivoCercano.X + _objetivoCercano.Ancho * 0.5f;
            float rcy = _objetivoCercano.Y + _objetivoCercano.Alto * 0.5f;

            Color cObj = _objetivoFijado ? ColorAlertaRojo : ColorCianHud;
            pluma.Color = cObj;
            pluma.Width = 1.5f;

            // 1. Corchetes militares de encuadre [ ]
            float rw = _objetivoCercano.Ancho * 0.58f;
            float rh = _objetivoCercano.Alto * 0.58f;
            float bracketLen = 8f;

            // Esquina superior izquierda
            g.DrawLine(pluma, rcx - rw, rcy - rh + bracketLen, rcx - rw, rcy - rh);
            g.DrawLine(pluma, rcx - rw, rcy - rh, rcx - rw + bracketLen, rcy - rh);

            // Esquina superior derecha
            g.DrawLine(pluma, rcx + rw - bracketLen, rcy - rh, rcx + rw, rcy - rh);
            g.DrawLine(pluma, rcx + rw, rcy - rh, rcx + rw, rcy - rh + bracketLen);

            // Esquina inferior izquierda
            g.DrawLine(pluma, rcx - rw, rcy + rh - bracketLen, rcx - rw, rcy + rh);
            g.DrawLine(pluma, rcx - rw, rcy + rh, rcx - rw + bracketLen, rcy + rh);

            // Esquina inferior derecha
            g.DrawLine(pluma, rcx + rw - bracketLen, rcy + rh, rcx + rw, rcy + rh);
            g.DrawLine(pluma, rcx + rw, rcy + rh, rcx + rw, rcy + rh - bracketLen);

            // 2. Anillo giratorio de radar lock si está enganchado
            if (_objetivoFijado)
            {
                var st = g.Save();
                g.TranslateTransform(rcx, rcy);
                g.RotateTransform(_tiempoHud * 180f);
                using var pLock = new Pen(ColorAlertaRojo, 1.4f) { DashPattern = new[] { 4f, 4f } };
                g.DrawEllipse(pLock, -rw - 6f, -rh - 6f, (rw + 6f) * 2f, (rh + 6f) * 2f);
                g.Restore(st);
            }

            // 3. Retícula predictiva de disparo continuo (PIPER)
            // Estima el tiempo de vuelo del proyectil a la velocidad de ~11 px/tick
            float tiempoVuelo = _distanciaObjetivo / 11.5f;
            float velRivalX = (float)Math.Sin(_tiempoHud * 2.2) * 2.5f;
            float piperX = rcx + (velRivalX * tiempoVuelo * 0.5f);
            float piperY = rcy;

            pluma.Color = Color.FromArgb(200, ColorAmbarAlerta);
            pluma.Width = 1.2f;
            g.DrawEllipse(pluma, piperX - 7f, piperY - 7f, 14f, 14f);
            g.DrawLine(pluma, piperX, piperY - 10f, piperX, piperY + 10f);
            g.DrawLine(pluma, piperX - 10f, piperY, piperX + 10f, piperY);

            // Línea de remolque punteada desde la proa del jugador hasta la retícula
            using var pGuia = new Pen(Color.FromArgb(90, ColorCianHud), 1f) { DashPattern = new[] { 3f, 3f } };
            g.DrawLine(pGuia, cx, cy - 42f, piperX, piperY);

            // Telemetría de blanco
            brocha.Color = cObj;
            string txtInfo = $"TGT: {_objetivoCercano.NombreModelo}\nRNG: {(_distanciaObjetivo * 3.2f):0} M\nHP:  {_objetivoCercano.VidaActual}";
            g.DrawString(txtInfo, _fuenteHudPequena!, brocha, rcx + rw + 5f, rcy - rh);
        }

        private void DibujarCintaRumbo(Graphics g, int ancho, Pen pluma, SolidBrush brocha)
        {
            int cintaY = 12;
            int centroX = ancho / 2;
            int cintaW = 280;

            pluma.Color = Color.FromArgb(160, ColorVerdeTactico);
            pluma.Width = 1.2f;

            // Línea base horizontal de rumbo
            g.DrawLine(pluma, centroX - (cintaW / 2), cintaY + 16, centroX + (cintaW / 2), cintaY + 16);

            // Marcas cardinales y grados
            float rumboBase = 360f; // Norte hacia adelante
            for (int r = -40; r <= 40; r += 10)
            {
                float rx = centroX + (r * 3.2f);
                bool esCardinal = (r % 30 == 0);
                float hMarca = esCardinal ? 10f : 5f;

                g.DrawLine(pluma, rx, cintaY + 16, rx, cintaY + 16 - hMarca);

                if (esCardinal)
                {
                    string rotulo = r switch
                    {
                        -30 => "33",
                        0 => "N",
                        30 => "03",
                        _ => $"{((rumboBase + r) % 360):00}"
                    };
                    brocha.Color = ColorVerdeTactico;
                    g.DrawString(rotulo, _fuenteHudPequena!, brocha, rx - 6f, cintaY - 2f);
                }
            }

            // Indicador central de rumbo (caja con número exacto)
            g.DrawRectangle(pluma, centroX - 18, cintaY + 16, 36, 14);
            brocha.Color = Color.FromArgb(180, 10, 24, 20);
            g.FillRectangle(brocha, centroX - 17, cintaY + 17, 34, 12);
            brocha.Color = ColorVerdeTactico;
            g.DrawString("360°", _fuenteHudPequena!, brocha, centroX - 14, cintaY + 16);
        }

        private void DibujarCintasVelocidadYAltitud(Graphics g, int ancho, int alto, AeronaveBase jugador, Pen pluma, SolidBrush brocha)
        {
            pluma.Color = Color.FromArgb(140, ColorVerdeTactico);
            pluma.Width = 1.2f;

            int cintaH = 140;
            int midY = alto / 2;

            // 1. Cinta de Velocidad (Izquierda)
            int vx = 40;
            g.DrawLine(pluma, vx + 35, midY - (cintaH / 2), vx + 35, midY + (cintaH / 2));
            for (int dy = -60; dy <= 60; dy += 20)
            {
                g.DrawLine(pluma, vx + 28, midY + dy, vx + 35, midY + dy);
            }
            // Caja digital de velocidad (KTS)
            int kts = jugador.Acelerando ? 580 : 420;
            g.DrawRectangle(pluma, vx, midY - 10, 35, 20);
            brocha.Color = Color.FromArgb(180, 10, 24, 20);
            g.FillRectangle(brocha, vx + 1, midY - 9, 33, 18);
            brocha.Color = ColorVerdeTactico;
            g.DrawString($"{kts}", _fuenteHud!, brocha, vx + 4, midY - 7);
            g.DrawString("KTS", _fuenteHudPequena!, brocha, vx + 4, midY - 22);

            // 2. Cinta de Altitud (Derecha)
            int ax = ancho - 85;
            g.DrawLine(pluma, ax, midY - (cintaH / 2), ax, midY + (cintaH / 2));
            for (int dy = -60; dy <= 60; dy += 20)
            {
                g.DrawLine(pluma, ax, midY + dy, ax + 7, midY + dy);
            }
            // Caja digital de altitud (FT)
            int altFt = 34500 - (int)(jugador.Y * 18);
            g.DrawRectangle(pluma, ax + 7, midY - 10, 48, 20);
            brocha.Color = Color.FromArgb(180, 10, 24, 20);
            g.FillRectangle(brocha, ax + 8, midY - 9, 46, 18);
            brocha.Color = ColorVerdeTactico;
            g.DrawString($"{altFt}", _fuenteHud!, brocha, ax + 9, midY - 7);
            g.DrawString("ALT", _fuenteHudPequena!, brocha, ax + 9, midY - 22);
        }

        private void DibujarBarridoRadarAesa(Graphics g, float cx, float cy, Pen pluma, SolidBrush brocha)
        {
            // Cono frontal de radar AESA de 55° hacia el frente
            float conoLong = 220f;
            float mitadAngulo = 26f;

            var st = g.Save();
            g.TranslateTransform(cx, cy - 20f);

            // Relleno tenue del cono
            brocha.Color = Color.FromArgb(8, 60, 240, 180);
            PointF[] pts = new[]
            {
                new PointF(0, 0),
                new PointF((float)(-Math.Tan(mitadAngulo * Math.PI / 180.0) * conoLong), -conoLong),
                new PointF((float)(Math.Tan(mitadAngulo * Math.PI / 180.0) * conoLong), -conoLong)
            };
            g.FillPolygon(brocha, pts);

            // Borde del cono
            pluma.Color = Color.FromArgb(40, ColorVerdeTactico);
            pluma.Width = 1f;
            g.DrawLine(pluma, 0, 0, pts[1].X, pts[1].Y);
            g.DrawLine(pluma, 0, 0, pts[2].X, pts[2].Y);
            g.DrawArc(pluma, -conoLong * 0.45f, -conoLong, conoLong * 0.9f, conoLong * 0.9f, 240f, 60f);

            // Haz de barrido oscilante
            float anguloHaz = (float)Math.Sin(_tiempoHud * 3.4) * mitadAngulo;
            float hx = (float)(Math.Sin(anguloHaz * Math.PI / 180.0) * conoLong);
            float hy = (float)(-Math.Cos(anguloHaz * Math.PI / 180.0) * conoLong);
            pluma.Color = Color.FromArgb(90, ColorVerdeTactico);
            g.DrawLine(pluma, 0, 0, hx, hy);

            g.Restore(st);
        }

        private void DibujarAlertasYEstadoArmamento(Graphics g, int ancho, int alto, AeronaveBase jugador, SolidBrush brocha, Pen pluma)
        {
            // Banner de Alerta de Lock
            if (_objetivoFijado)
            {
                int bw = 240;
                int bx = (ancho - bw) / 2;
                int by = alto - 120;

                pluma.Color = ColorAlertaRojo;
                pluma.Width = 1.6f;
                g.DrawRectangle(pluma, bx, by, bw, 22);

                brocha.Color = Color.FromArgb(160, 45, 10, 15);
                g.FillRectangle(brocha, bx + 1, by + 1, bw - 2, 20);

                brocha.Color = ColorAlertaRojo;
                g.DrawString("⚠ RADAR LOCK DETECTADO ⚠", _fuenteHudBold!, brocha, bx + 14, by + 3);
            }

            // Alerta de daño crítico si vida < 20
            if (jugador.VidaActual < 20)
            {
                int bw = 260;
                int bx = (ancho - bw) / 2;
                int by = 60;

                bool parpadeo = ((int)(_tiempoHud * 4.0) % 2 == 0);
                if (parpadeo)
                {
                    pluma.Color = ColorAlertaRojo;
                    pluma.Width = 1.6f;
                    g.DrawRectangle(pluma, bx, by, bw, 22);

                    brocha.Color = Color.FromArgb(170, 50, 10, 10);
                    g.FillRectangle(brocha, bx + 1, by + 1, bw - 2, 20);

                    brocha.Color = Color.White;
                    g.DrawString("¡ADVERTENCIA: DAÑO CRÍTICO!", _fuenteHudBold!, brocha, bx + 12, by + 3);
                }
            }

            // Indicador de modo HUD en esquina inferior derecha
            brocha.Color = Color.FromArgb(140, ColorVerdeTactico);
            g.DrawString($"HUD: {ModoActual} [H] | AUDIO [M]", _fuenteHudPequena!, brocha, ancho - 180, alto - 24);
        }
    }
}
