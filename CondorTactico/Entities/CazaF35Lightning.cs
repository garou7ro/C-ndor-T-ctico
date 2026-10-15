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

using System.Collections.Generic;

namespace CondorTactico
{
    /// <summary>
    /// Subclase concreta (Nivel 3, DIT=3): Caza polivalente furtivo de quinta generación Lockheed Martin F-35A Lightning II.
    /// Especializado en alta velocidad, viraje ágil y disparo de misiles supersónicos AIM-120 AMRAAM.
    /// </summary>
    public class CazaF35Lightning : AeronaveBase
    {
        public CazaF35Lightning(string idAeronave, string nombrePiloto, bool esRival, float x, float y, int vidaPersonalizada = 0)
            : base(
                idAeronave,
                nombrePiloto,
                TipoAeronave.F35_Lightning_II,
                esRival,
                x,
                y,
                ancho: 84,
                alto: 118,
                vidaMaxima: vidaPersonalizada > 0 ? vidaPersonalizada : (esRival ? 50 : 25),
                escudoMaximo: 5,
                velocidad: 12.0f)
        {
            NombreModelo = "Lockheed Martin F-35A Lightning II";
            Matricula = "AF-15-5104";
            CadenciaDisparoTicks = 8;
        }

        /// <summary>
        /// Implementación polimórfica: Dispara un misil supersónico AIM-120 AMRAAM de alta velocidad.
        /// </summary>
        public override List<Proyectil> Disparar()
        {
            var proyectiles = new List<Proyectil>(1);
            if (EnfriamientoDisparo > 0) return proyectiles;

            EnfriamientoDisparo = CadenciaDisparoTicks;
            float centroX = X + (Ancho * 0.5f);
            float salidaY = EsRival ? (Y + Alto - 6f) : (Y - 22f);
            float velY = EsRival ? 11.5f : -14.0f;

            proyectiles.Add(new Proyectil(
                IdAeronave,
                EsRival,
                TipoAeronave.F35_Lightning_II,
                centroX,
                salidaY,
                velocidadX: 0f,
                velocidadY: velY,
                danoCabina: 10,
                danoAlas: 1));

            return proyectiles;
        }

        public override string ObtenerFichaTecnica() =>
            "F-35A Lightning II | Caza Furtivo Mach 1.6 | Misil AIM-120 Supersónico | Alta Maniobrabilidad";
    }
}


