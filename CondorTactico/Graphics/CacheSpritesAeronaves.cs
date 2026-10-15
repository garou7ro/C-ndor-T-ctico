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
using System.Drawing.Drawing2D;

namespace CondorTactico
{
    /// <summary>
    /// Caché Hash O(1) de sprites vectoriales pre-renderizados con 4x SSAA Bicúbico.
    /// Elimina la rasterización intensiva por frame sustituyéndola por consulta O(1) en Dictionary.
    /// </summary>
    public static class CacheSpritesAeronaves
    {
        private readonly struct ClaveSprite : IEquatable<ClaveSprite>
        {
            public readonly TipoAeronave Tipo;
            public readonly bool EsRival;
            public readonly bool Acelerando;
            public readonly int AnguloDiscreto;

            public ClaveSprite(TipoAeronave tipo, bool esRival, bool acelerando, int anguloDiscreto)
            {
                Tipo = tipo;
                EsRival = esRival;
                Acelerando = acelerando;
                AnguloDiscreto = anguloDiscreto;
            }

            public bool Equals(ClaveSprite other) =>
                Tipo == other.Tipo && EsRival == other.EsRival &&
                Acelerando == other.Acelerando && AnguloDiscreto == other.AnguloDiscreto;

            public override bool Equals(object? obj) => obj is ClaveSprite other && Equals(other);

            public override int GetHashCode() => HashCode.Combine((int)Tipo, EsRival, Acelerando, AnguloDiscreto);
        }

        private static readonly Dictionary<ClaveSprite, Bitmap> _cacheNaves = new Dictionary<ClaveSprite, Bitmap>();
        private static readonly Dictionary<(TipoAeronave, bool), Bitmap> _cacheMisiles = new Dictionary<(TipoAeronave, bool), Bitmap>();
        private static bool _inicializado = false;

        /// <summary>
        /// Procedimiento de pre-carga que genera todos los estados visuales de F-35 y B-2 Spirit una única vez.
        /// </summary>
        public static void InicializarCache()
        {
            if (_inicializado) return;

            int[] angulos = new int[] { -15, 0, 15 };
            bool[] estadosRival = new bool[] { false, true };
            bool[] estadosAcel = new bool[] { false, true };

            foreach (TipoAeronave tipo in Enum.GetValues(typeof(TipoAeronave)))
            {
                int ancho = (tipo == TipoAeronave.B2_Spirit_Stealth) ? 132 : 84;
                int alto = (tipo == TipoAeronave.B2_Spirit_Stealth) ? 92 : 118;

                foreach (bool esRival in estadosRival)
                {
                    int rotacionBase = esRival ? 180 : 0;
                    foreach (bool acel in estadosAcel)
                    {
                        using (Bitmap bmpBase = (tipo == TipoAeronave.B2_Spirit_Stealth)
                            ? B2SpiritModel.DibujarB2Spirit(ancho, alto, rotacionBase, esRival, acel, 4)
                            : F35Model.DibujarF35(ancho, alto, rotacionBase, esRival, acel, 4))
                        {
                            foreach (int ang in angulos)
                            {
                                ClaveSprite clave = new ClaveSprite(tipo, esRival, acel, ang);
                                Bitmap bmpRotado = RotarBitmapAltaCalidad(bmpBase, ang);
                                _cacheNaves[clave] = bmpRotado;

                                // Integrar con el Gestor de Recursos Asíncrono para telemetría y ciclo de vida
                                GestorRecursosAsincrono.Instancia.RegistrarRecursoListo(
                                    new RecursoGrafico($"sprite_nave_{tipo}_{(esRival ? "rival" : "aliado")}_{(acel ? "acel" : "base")}_{ang}", bmpRotado));
                            }
                        }
                    }

                    // Cachear también el proyectil correspondiente
                    Bitmap bmpMisil = (tipo == TipoAeronave.B2_Spirit_Stealth)
                        ? B2SpiritModel.DibujarMisilB2(20, 40, rotacionBase, esRival, 4)
                        : F35Model.DibujarMisil(18, 38, rotacionBase, esRival, 4);
                    _cacheMisiles[(tipo, esRival)] = bmpMisil;

                    GestorRecursosAsincrono.Instancia.RegistrarRecursoListo(
                        new RecursoGrafico($"sprite_misil_{tipo}_{(esRival ? "rival" : "aliado")}", bmpMisil));
                }
            }

            _inicializado = true;
        }

        /// <summary>
        /// Función de complejidad constante O(1) que retorna el sprite pre-renderizado de una aeronave.
        /// </summary>
        public static Bitmap ObtenerSpriteNave(TipoAeronave tipo, bool esRival, bool acelerando, float angulo)
        {
            if (!_inicializado) InicializarCache();

            int angDiscreto = angulo < -5f ? -15 : (angulo > 5f ? 15 : 0);
            var clave = new ClaveSprite(tipo, esRival, acelerando, angDiscreto);

            if (_cacheNaves.TryGetValue(clave, out Bitmap? bmp))
            {
                return bmp;
            }
            return _cacheNaves[new ClaveSprite(tipo, esRival, false, 0)];
        }

        /// <summary>
        /// Función de complejidad constante O(1) que retorna el sprite del proyectil.
        /// </summary>
        public static Bitmap ObtenerSpriteMisil(TipoAeronave tipo, bool esRival)
        {
            if (!_inicializado) InicializarCache();
            return _cacheMisiles[(tipo, esRival)];
        }

        private static Bitmap RotarBitmapAltaCalidad(Bitmap img, float rotationAngle)
        {
            if (Math.Abs(rotationAngle) < 0.01f)
            {
                return (Bitmap)img.Clone();
            }
            Bitmap bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gfx.CompositingQuality = CompositingQuality.HighQuality;
                gfx.TranslateTransform((float)bmp.Width / 2f, (float)bmp.Height / 2f);
                gfx.RotateTransform(rotationAngle);
                gfx.TranslateTransform(-(float)bmp.Width / 2f, -(float)bmp.Height / 2f);
                gfx.DrawImage(img, new Point(0, 0));
            }
            return bmp;
        }
    }
}


