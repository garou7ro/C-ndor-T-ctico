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
using System.Collections.Generic;
using System.Drawing;

namespace CondorTactico
{
    /// <summary>
    /// Nivel intermedio de la jerarquía de herencia OOP (Nivel 2, DIT=2).
    /// Incorpora comportamiento de vuelo, blindaje, escudos de absorción RAM,
    /// sistema de inteligencia artificial y evaluación de impactos en 3 zonas (O(1)).
    /// </summary>
    public abstract class AeronaveBase : EntidadEspacial
    {
        public string IdAeronave { get; set; }
        public string NombrePiloto { get; set; }
        public string NombreModelo { get; protected set; } = "";
        public string Matricula { get; protected set; } = "";
        public TipoAeronave Tipo { get; protected set; }
        public bool EsRival { get; set; }
        public bool ControladaPorRed { get; set; }

        public int VidaActual { get; set; }
        public int VidaMaxima { get; protected set; }
        public int EscudoActual { get; set; }
        public int EscudoMaximo { get; protected set; }

        public float VelocidadMovimiento { get; protected set; }
        public int CadenciaDisparoTicks { get; protected set; }
        public int EnfriamientoDisparo { get; set; }
        public float AnguloInclinacion { get; set; }
        public bool Acelerando { get; set; }
        public int BajasConfirmadas { get; set; }
        public int PuntajeTactico { get; set; }

        // Variables de patrulla autónoma para IA táctica rival
        private float _direccionPatrullaX = 1.0f;
        private float _faseEvasion = 0f;

        protected AeronaveBase(
            string idAeronave,
            string nombrePiloto,
            TipoAeronave tipo,
            bool esRival,
            float x,
            float y,
            int ancho,
            int alto,
            int vidaMaxima,
            int escudoMaximo,
            float velocidad)
            : base(x, y, ancho, alto)
        {
            IdAeronave = idAeronave;
            NombrePiloto = nombrePiloto;
            Tipo = tipo;
            EsRival = esRival;
            VidaMaxima = vidaMaxima;
            VidaActual = vidaMaxima;
            EscudoMaximo = escudoMaximo;
            EscudoActual = escudoMaximo;
            VelocidadMovimiento = velocidad;
        }

        /// <summary>
        /// Procedimiento que desplaza la aeronave dentro de los límites del espacio aéreo.
        /// Demuestra el uso de procedimientos (void) con argumentos tipados.
        /// </summary>
        public void Mover(float deltaX, float deltaY, Size limitesMapa)
        {
            X = Math.Clamp(X + deltaX * VelocidadMovimiento, 4f, Math.Max(4f, limitesMapa.Width - Ancho - 4f));
            Y = Math.Clamp(Y + deltaY * VelocidadMovimiento, 4f, Math.Max(4f, limitesMapa.Height - Alto - 34f));

            if (deltaX < -0.1f) AnguloInclinacion = -15f;
            else if (deltaX > 0.1f) AnguloInclinacion = 15f;
            else AnguloInclinacion = 0f;

            Acelerando = Math.Abs(deltaY) > 0.1f;
        }

        /// <summary>
        /// Procedimiento de inteligencia artificial táctica para aeronaves rivales autónomas.
        /// </summary>
        public override void ActualizarFisica(Size limitesEspacio)
        {
            if (EnfriamientoDisparo > 0) EnfriamientoDisparo--;

            if (EsRival && !ControladaPorRed)
            {
                _faseEvasion += 0.045f;
                X += _direccionPatrullaX * (VelocidadMovimiento * 0.28f);

                if (X + Ancho >= limitesEspacio.Width - 8)
                {
                    X = limitesEspacio.Width - Ancho - 8;
                    _direccionPatrullaX = -1.0f;
                }
                else if (X <= 8)
                {
                    X = 8;
                    _direccionPatrullaX = 1.0f;
                }

                // Oscilación vertical suave de combate aéreo táctico
                float yBase = 18f + (float)Math.Sin(_faseEvasion) * 14f;
                Y = Math.Clamp(yBase, 8f, 90f);
                AnguloInclinacion = _direccionPatrullaX * 15f;
            }
        }

        /// <summary>
        /// Función polimórfica abstracta: cada subclase de aeronave implementa su propio patrón de disparo y armamento.
        /// </summary>
        public abstract List<Proyectil> Disparar();

        /// <summary>
        /// Función polimórfica que retorna la ficha técnica y especificaciones de la aeronave.
        /// </summary>
        public abstract string ObtenerFichaTecnica();

        /// <summary>
        /// Función que evalúa el impacto geométrico en 3 segmentos (Ala Izquierda -1, Cabina Central -10, Ala Derecha -1) en O(1).
        /// </summary>
        public virtual ResultadoImpacto EvaluarImpactoGeometrico(Proyectil proyectil)
        {
            float segmento = Ancho / 3.0f;
            float limiteAlaIzq = X + segmento;
            float limiteAlaDer = X + (2.0f * segmento);
            float centroMisil = proyectil.X + (proyectil.Ancho / 2.0f);

            ZonaImpacto zona;
            int danoBase;
            string etiqueta;

            if (centroMisil >= limiteAlaIzq && centroMisil <= limiteAlaDer)
            {
                zona = ZonaImpacto.CabinaCentralCritica;
                danoBase = proyectil.DanoCabina; // -10 puntos por impacto directo en cabina
                etiqueta = $"impact -{danoBase} (CABINA)";
            }
            else if (centroMisil < limiteAlaIzq)
            {
                zona = ZonaImpacto.AlaIzquierda;
                danoBase = proyectil.DanoAlas;   // -1 punto en ala izquierda
                etiqueta = $"impact -{danoBase} (ALA IZQ)";
            }
            else
            {
                zona = ZonaImpacto.AlaDerecha;
                danoBase = proyectil.DanoAlas;   // -1 punto en ala derecha
                etiqueta = $"impact -{danoBase} (ALA DER)";
            }

            // Absorción parcial por escudo furtivo RAM si está disponible
            int absorbido = 0;
            int danoVida = danoBase;
            if (EscudoActual > 0 && zona != ZonaImpacto.CabinaCentralCritica)
            {
                absorbido = Math.Min(EscudoActual, danoBase);
                EscudoActual -= absorbido;
                danoVida -= absorbido;
            }

            VidaActual = Math.Max(0, VidaActual - danoVida);
            if (VidaActual <= 0)
            {
                Activo = false;
            }

            return new ResultadoImpacto(true, zona, danoBase, absorbido, danoVida, etiqueta);
        }

        public override void Dibujar(Graphics g)
        {
            if (!Activo) return;

            Bitmap sprite = CacheSpritesAeronaves.ObtenerSpriteNave(Tipo, EsRival, Acelerando, AnguloInclinacion);
            g.DrawImage(sprite, X, Y, Ancho, Alto);

            // Dibujar barra táctica de vida y etiqueta de piloto/modelo sobre la aeronave
            DibujarHudAeronave(g);
        }

        protected void DibujarHudAeronave(Graphics g)
        {
            float barW = Math.Max(64f, Ancho * 0.78f);
            float barH = 5f;
            float barX = X + (Ancho - barW) * 0.5f;
            float barY = EsRival ? (Y + Alto + 3f) : (Y - 14f);

            using (SolidBrush bFondo = new SolidBrush(Color.FromArgb(160, 15, 22, 32)))
            {
                g.FillRectangle(bFondo, barX - 1, barY - 1, barW + 2, barH + 2);
            }

            float pctVida = Math.Clamp((float)VidaActual / Math.Max(1, VidaMaxima), 0f, 1f);
            Color cVida = EsRival
                ? Color.FromArgb(235, 255, 70, 85)
                : (pctVida > 0.4f ? Color.FromArgb(235, 45, 235, 140) : Color.FromArgb(245, 255, 180, 30));

            using (SolidBrush bVida = new SolidBrush(cVida))
            {
                g.FillRectangle(bVida, barX, barY, barW * pctVida, barH);
            }

            if (EscudoMaximo > 0 && EscudoActual > 0)
            {
                float pctEsc = Math.Clamp((float)EscudoActual / EscudoMaximo, 0f, 1f);
                using (SolidBrush bEsc = new SolidBrush(Color.FromArgb(210, 60, 200, 255)))
                {
                    g.FillRectangle(bEsc, barX, barY - 3f, barW * pctEsc, 2f);
                }
            }

            using (Font fTag = new Font("Consolas", 7.2f, FontStyle.Bold))
            using (SolidBrush bTxt = new SolidBrush(EsRival ? Color.FromArgb(240, 255, 190, 190) : Color.FromArgb(240, 210, 245, 255)))
            {
                string tag = $"{NombrePiloto} [{VidaActual}HP]";
                g.DrawString(tag, fTag, bTxt, barX, EsRival ? (barY + 6f) : (barY - 13f));
            }
        }
    }
}


