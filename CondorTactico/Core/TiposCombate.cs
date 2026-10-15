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

namespace CondorTactico
{
    /// <summary>
    /// Enumeración de los modelos de aeronaves disponibles para desplegar en el espacio aéreo.
    /// </summary>
    public enum TipoAeronave
    {
        F35_Lightning_II = 0,
        B2_Spirit_Stealth = 1
    }

    /// <summary>
    /// Zona geométrica de impacto evaluada en O(1) mediante partición de hitbox en 3 segmentos transversales.
    /// </summary>
    public enum ZonaImpacto
    {
        Ninguna = 0,
        AlaIzquierda = 1,
        CabinaCentralCritica = 2,
        AlaDerecha = 3
    }

    /// <summary>
    /// Estructura inmutable (Value Type) que encapsula el resultado físico y numérico de una evaluación de impacto geométrico.
    /// </summary>
    public readonly struct ResultadoImpacto
    {
        public bool HuboImpacto { get; }
        public ZonaImpacto Zona { get; }
        public int DanoTotal { get; }
        public int DanoAbsorbidoEscudo { get; }
        public int DanoRealVida { get; }
        public string EtiquetaCombate { get; }

        public ResultadoImpacto(bool huboImpacto, ZonaImpacto zona, int danoTotal, int danoEscudo, int danoVida, string etiqueta)
        {
            HuboImpacto = huboImpacto;
            Zona = zona;
            DanoTotal = danoTotal;
            DanoAbsorbidoEscudo = danoEscudo;
            DanoRealVida = danoVida;
            EtiquetaCombate = etiqueta;
        }
    }
}


