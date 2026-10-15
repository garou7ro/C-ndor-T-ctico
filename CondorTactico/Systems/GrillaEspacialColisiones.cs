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
    /// Motor de Particionamiento Espacial en Grilla Hash O(N + K).
    /// Sustituye la detección exhaustiva cuadrática O(N * M) por agrupamiento en celdas espaciales
    /// con reutilización de listas (Object Pooling) para 0 recolecciones GC.
    /// </summary>
    public class GrillaEspacialColisiones
    {
        private readonly int _tamanoCelda;
        private readonly Dictionary<int, List<AeronaveBase>> _celdasAeronaves = new Dictionary<int, List<AeronaveBase>>(64);
        private readonly Stack<List<AeronaveBase>> _poolListas = new Stack<List<AeronaveBase>>(64);

        public int UltimasComparacionesReales { get; private set; }
        public int UltimasComparacionesFuerzaBruta { get; private set; }

        public GrillaEspacialColisiones(int tamanoCelda = 72)
        {
            _tamanoCelda = Math.Max(32, tamanoCelda);
        }

        private int CalcularHashCelda(int cx, int cy) => (cx * 73856093) ^ (cy * 19349663);

        public void Limpiar()
        {
            foreach (var lista in _celdasAeronaves.Values)
            {
                lista.Clear();
                _poolListas.Push(lista);
            }
            _celdasAeronaves.Clear();
        }

        /// <summary>
        /// Inserta una aeronave en las celdas espaciales que intersecta su hitbox en O(1).
        /// </summary>
        public void InsertarAeronave(AeronaveBase nave)
        {
            if (!nave.Activo) return;

            int minX = (int)(nave.X / _tamanoCelda);
            int maxX = (int)((nave.X + nave.Ancho) / _tamanoCelda);
            int minY = (int)(nave.Y / _tamanoCelda);
            int maxY = (int)((nave.Y + nave.Alto) / _tamanoCelda);

            for (int cx = minX; cx <= maxX; cx++)
            {
                for (int cy = minY; cy <= maxY; cy++)
                {
                    int hash = CalcularHashCelda(cx, cy);
                    if (!_celdasAeronaves.TryGetValue(hash, out List<AeronaveBase>? lista))
                    {
                        lista = _poolListas.Count > 0 ? _poolListas.Pop() : new List<AeronaveBase>(4);
                        _celdasAeronaves[hash] = lista;
                    }
                    if (!lista.Contains(nave))
                    {
                        lista.Add(nave);
                    }
                }
            }
        }

        /// <summary>
        /// Evalúa colisiones proyectil-aeronave usando sólo las celdas vecinas activas en O(N + K).
        /// </summary>
        public void ProcesarColisiones(
            List<Proyectil> proyectiles,
            List<AeronaveBase> aeronaves,
            Action<AeronaveBase, Proyectil, ResultadoImpacto> alImpactar)
        {
            Limpiar();
            int navesActivas = 0;
            for (int i = 0; i < aeronaves.Count; i++)
            {
                if (aeronaves[i].Activo)
                {
                    InsertarAeronave(aeronaves[i]);
                    navesActivas++;
                }
            }

            UltimasComparacionesFuerzaBruta = proyectiles.Count * navesActivas;
            int comparacionesReales = 0;

            for (int i = 0; i < proyectiles.Count; i++)
            {
                Proyectil p = proyectiles[i];
                if (!p.Activo) continue;

                PointF centroP = p.ObtenerCentro();
                int cx = (int)(centroP.X / _tamanoCelda);
                int cy = (int)(centroP.Y / _tamanoCelda);
                int hash = CalcularHashCelda(cx, cy);

                if (_celdasAeronaves.TryGetValue(hash, out List<AeronaveBase>? candidatas))
                {
                    for (int j = 0; j < candidatas.Count; j++)
                    {
                        AeronaveBase nave = candidatas[j];
                        if (!nave.Activo || nave.EsRival == p.EsRival || nave.IdAeronave == p.IdPropietario)
                            continue;

                        comparacionesReales++;

                        // Verificación geométrica exacta AABB
                        if (nave.X < p.X + p.Ancho && p.X < nave.X + nave.Ancho &&
                            nave.Y < p.Y + p.Alto && p.Y < nave.Y + nave.Alto)
                        {
                            p.Activo = false;
                            ResultadoImpacto resultado = nave.EvaluarImpactoGeometrico(p);
                            alImpactar(nave, p, resultado);
                            break;
                        }
                    }
                }
            }

            UltimasComparacionesReales = comparacionesReales;
        }
    }
}


