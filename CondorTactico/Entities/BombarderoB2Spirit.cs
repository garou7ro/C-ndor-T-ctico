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
    /// Subclase concreta (Nivel 3, DIT=3): Bombardero furtivo de ala volante Northrop Grumman B-2 Spirit.
    /// Recrea la aeronave táctica AF 82-1071 ("Spirit of Mississippi").
    /// Incorpora doble bahía de armamento con disparo simultáneo en paralelo (AGM-158 JASSM) y blindaje RAM reforzado.
    /// </summary>
    public class BombarderoB2Spirit : AeronaveBase
    {
        public BombarderoB2Spirit(string idAeronave, string nombrePiloto, bool esRival, float x, float y, int vidaPersonalizada = 0)
            : base(
                idAeronave,
                nombrePiloto,
                TipoAeronave.B2_Spirit_Stealth,
                esRival,
                x,
                y,
                ancho: 132,
                alto: 92,
                vidaMaxima: vidaPersonalizada > 0 ? vidaPersonalizada : (esRival ? 60 : 35),
                escudoMaximo: 12,
                velocidad: 10.0f)
        {
            NombreModelo = "Northrop Grumman B-2A Spirit";
            Matricula = "AF-82-1071";
            CadenciaDisparoTicks = 12;
        }

        /// <summary>
        /// Implementación polimórfica: El B-2 Spirit abre sus dos bahías internas y despliega una
        /// DOBLE SALVA TÁCTICA de misiles furtivos AGM-158 JASSM en paralelo.
        /// </summary>
        public override List<Proyectil> Disparar()
        {
            var proyectiles = new List<Proyectil>(2);
            if (EnfriamientoDisparo > 0) return proyectiles;

            EnfriamientoDisparo = CadenciaDisparoTicks;
            float centroX = X + (Ancho * 0.5f);
            float separacionBahias = 16.0f;
            float salidaY = EsRival ? (Y + Alto - 10f) : (Y - 18f);
            float velY = EsRival ? 10.5f : -12.8f;

            // Bahía de armamento izquierda
            proyectiles.Add(new Proyectil(
                IdAeronave,
                EsRival,
                TipoAeronave.B2_Spirit_Stealth,
                centroX - separacionBahias,
                salidaY,
                velocidadX: -0.25f,
                velocidadY: velY,
                danoCabina: 10,
                danoAlas: 2));

            // Bahía de armamento derecha
            proyectiles.Add(new Proyectil(
                IdAeronave,
                EsRival,
                TipoAeronave.B2_Spirit_Stealth,
                centroX + separacionBahias,
                salidaY,
                velocidadX: 0.25f,
                velocidadY: velY,
                danoCabina: 10,
                danoAlas: 2));

            return proyectiles;
        }

        public override string ObtenerFichaTecnica() =>
            "B-2A Spirit (AF 82-1071) | Ala Volante Furtiva | Doble Salva AGM-158 | Escudo RAM Reforzado";
    }
}


