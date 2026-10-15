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
using System.Drawing;

namespace CondorTactico
{
    /// <summary>
    /// Raíz de la jerarquía de herencia OOP (Nivel 1, DIT=1).
    /// Modela cualquier objeto físico que coexiste en el plano 2D del simulador táctico.
    /// </summary>
    public abstract class EntidadEspacial
    {
        // Atributos encapsulados mediante propiedades con validación de estado
        public float X { get; set; }
        public float Y { get; set; }
        public int Ancho { get; protected set; }
        public int Alto { get; protected set; }
        public bool Activo { get; set; } = true;

        protected EntidadEspacial(float x, float y, int ancho, int alto)
        {
            X = x;
            Y = y;
            Ancho = ancho;
            Alto = alto;
        }

        /// <summary>
        /// Función que calcula la caja envolvente alineada a los ejes (AABB) en O(1).
        /// </summary>
        public RectangleF ObtenerRectanguloColision() => new RectangleF(X, Y, Ancho, Alto);

        /// <summary>
        /// Obtiene el baricentro geométrico de la entidad espacial.
        /// </summary>
        public PointF ObtenerCentro() => new PointF(X + Ancho * 0.5f, Y + Alto * 0.5f);

        /// <summary>
        /// Procedimiento abstracto polimórfico para actualizar la física y cinemática de la entidad.
        /// </summary>
        public abstract void ActualizarFisica(Size limitesEspacio);

        /// <summary>
        /// Procedimiento abstracto polimórfico para renderizar la entidad en el lienzo GDI+.
        /// </summary>
        public abstract void Dibujar(Graphics g);
    }
}


