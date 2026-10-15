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
    /// Entidad proyectil supersónico AIM-120 AMRAAM o misil furtivo de crucero AGM-158 JASSM.
    /// Modela velocidad lineal, vector de traslación y daño segmentado por zona de impacto.
    /// </summary>
    public class Proyectil : EntidadEspacial
    {
        public string IdPropietario { get; }
        public bool EsRival { get; }
        public TipoAeronave TipoOrigen { get; }
        public float VelocidadX { get; }
        public float VelocidadY { get; }
        public int DanoCabina { get; }
        public int DanoAlas { get; }

        public Proyectil(
            string idPropietario,
            bool esRival,
            TipoAeronave tipoOrigen,
            float xCentro,
            float yInicial,
            float velocidadX,
            float velocidadY,
            int danoCabina = 10,
            int danoAlas = 1)
            : base(
                xCentro - ((tipoOrigen == TipoAeronave.B2_Spirit_Stealth ? 20 : 18) / 2f),
                yInicial,
                tipoOrigen == TipoAeronave.B2_Spirit_Stealth ? 20 : 18,
                tipoOrigen == TipoAeronave.B2_Spirit_Stealth ? 40 : 38)
        {
            IdPropietario = idPropietario;
            EsRival = esRival;
            TipoOrigen = tipoOrigen;
            VelocidadX = velocidadX;
            VelocidadY = velocidadY;
            DanoCabina = danoCabina;
            DanoAlas = danoAlas;
        }

        public override void ActualizarFisica(Size limitesEspacio)
        {
            X += VelocidadX;
            Y += VelocidadY;

            // Desactivación inmediata al abandonar el espacio aéreo jugable (AABB culling)
            if (Y < -Alto || Y > limitesEspacio.Height + Alto || X < -Ancho || X > limitesEspacio.Width + Ancho)
            {
                Activo = false;
            }
        }

        public override void Dibujar(Graphics g)
        {
            if (!Activo) return;
            Bitmap sprite = CacheSpritesAeronaves.ObtenerSpriteMisil(TipoOrigen, EsRival);
            g.DrawImage(sprite, X, Y, Ancho, Alto);
        }
    }
}


