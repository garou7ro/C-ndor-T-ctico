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

namespace CondorTactico.GraphicsEffects
{
    /// <summary>
    /// Cámara táctica cinematográfica con sacudida de pantalla basada en trauma cuadrático (Screen Shake).
    /// Simula el retroceso de cañón de 20mm, eyección de misiles e impacto de explosiones en combate.
    /// </summary>
    public sealed class CamaraCombate
    {
        private float _trauma = 0f;
        private float _tiempo = 0f;
        private float _offsetX = 0f;
        private float _offsetY = 0f;
        private float _anguloRotacion = 0f;

        public float OffsetX => _offsetX;
        public float OffsetY => _offsetY;
        public float AnguloRotacion => _anguloRotacion;

        /// <summary>Agrega trauma entre 0.0 y 1.0 (se satura en 1.0).</summary>
        public void AgregarTrauma(float cantidad)
        {
            _trauma = Math.Clamp(_trauma + cantidad, 0f, 1f);
        }

        public void Actualizar(float dt)
        {
            _tiempo += dt;
            if (_trauma > 0.001f)
            {
                // Decaimiento exponencial suave del trauma
                _trauma = Math.Max(0f, _trauma - (dt * 1.6f));

                // Sacudida no lineal (Trauma^2) para que impactos leves apenas tiemblen pero explosiones sean intensas
                float shake = _trauma * _trauma;
                float frec1 = _tiempo * 38f;
                float frec2 = _tiempo * 47f;
                float frec3 = _tiempo * 29f;

                _offsetX = (float)(Math.Sin(frec1) + Math.Cos(frec2 * 0.7)) * 14f * shake;
                _offsetY = (float)(Math.Cos(frec1 * 0.9) - Math.Sin(frec2)) * 12f * shake;
                _anguloRotacion = (float)Math.Sin(frec3) * 1.8f * shake;
            }
            else
            {
                _trauma = 0f;
                _offsetX = 0f;
                _offsetY = 0f;
                _anguloRotacion = 0f;
            }
        }
    }

    /// <summary>Onda expansiva supersónica circular con halo refractivo y borde óptico.</summary>
    public struct OndaExpansiva
    {
        public float X;
        public float Y;
        public float Radio;
        public float RadioMaximo;
        public float VelocidadExpansion;
        public Color ColorBorde;
        public bool Activa;

        public void Actualizar(float dt)
        {
            Radio += VelocidadExpansion * dt;
            if (Radio >= RadioMaximo) Activa = false;
        }

        public void Dibujar(Graphics g, Pen plumaReutilizable)
        {
            if (!Activa || Radio <= 1f) return;

            float fraccion = Math.Clamp(Radio / RadioMaximo, 0f, 1f);
            int alpha = (int)(255f * (1f - fraccion));
            if (alpha <= 0) return;

            plumaReutilizable.Color = Color.FromArgb(alpha, ColorBorde);
            plumaReutilizable.Width = Math.Max(1.2f, 3.8f * (1f - fraccion));

            g.DrawEllipse(plumaReutilizable, X - Radio, Y - Radio, Radio * 2f, Radio * 2f);

            // Anillo interior secundario tenue
            if (Radio > 12f)
            {
                plumaReutilizable.Color = Color.FromArgb(alpha / 3, Color.White);
                plumaReutilizable.Width = 1f;
                float rSec = Radio * 0.85f;
                g.DrawEllipse(plumaReutilizable, X - rSec, Y - rSec, rSec * 2f, rSec * 2f);
            }
        }
    }

    /// <summary>Destello atmosférico lumínico que ilumina toda la escena durante detonaciones.</summary>
    public sealed class DestelloAtmosferico
    {
        private float _opacidad = 0f;
        private Color _colorDestello = Color.White;

        public void Disparar(Color color, float intensidadInicial = 0.45f)
        {
            _colorDestello = color;
            _opacidad = Math.Clamp(intensidadInicial, 0f, 0.75f);
        }

        public void Actualizar(float dt)
        {
            if (_opacidad > 0.005f)
            {
                _opacidad = Math.Max(0f, _opacidad - (dt * 3.8f));
            }
            else
            {
                _opacidad = 0f;
            }
        }

        public void Dibujar(Graphics g, int ancho, int alto, SolidBrush brochaReutilizable)
        {
            if (_opacidad <= 0.005f) return;
            int alpha = (int)(255f * _opacidad);
            brochaReutilizable.Color = Color.FromArgb(alpha, _colorDestello);
            g.FillRectangle(brochaReutilizable, 0, 0, ancho, alto);
        }
    }

    /// <summary>Casquillo de munición de 20mm eyectado físicamente con rotación y gravedad.</summary>
    public struct CasquilloMunicios
    {
        public float X;
        public float Y;
        public float VX;
        public float VY;
        public float Angulo;
        public float VelocidadGiro;
        public int VidaRestante;
        public bool Activo;

        public void Actualizar(float dt)
        {
            X += VX * dt * 60f;
            Y += VY * dt * 60f;
            VY += 0.22f * dt * 60f; // Gravedad
            Angulo += VelocidadGiro * dt * 60f;
            VidaRestante--;
            if (VidaRestante <= 0) Activo = false;
        }

        public void Dibujar(Graphics g, SolidBrush brochaReutilizable)
        {
            if (!Activo) return;
            brochaReutilizable.Color = Color.FromArgb(Math.Min(255, VidaRestante * 12), 245, 195, 60);

            var estadoPrevio = g.Save();
            g.TranslateTransform(X, Y);
            g.RotateTransform(Angulo);
            g.FillRectangle(brochaReutilizable, -2f, -1f, 4f, 2f);
            g.Restore(estadoPrevio);
        }
    }

    /// <summary>Vórtice de condensación generado en la punta de las alas al aladear o maniobrar.</summary>
    public struct VorticePuntaAla
    {
        public float X;
        public float Y;
        public float Radio;
        public int VidaRestante;
        public int VidaMaxima;
        public bool Activo;

        public void Actualizar()
        {
            VidaRestante--;
            Radio += 0.25f;
            if (VidaRestante <= 0) Activo = false;
        }

        public void Dibujar(Graphics g, SolidBrush brochaReutilizable)
        {
            if (!Activo) return;
            int alpha = Math.Clamp((int)(140f * VidaRestante / Math.Max(1, VidaMaxima)), 0, 255);
            brochaReutilizable.Color = Color.FromArgb(alpha, 215, 240, 255);
            g.FillEllipse(brochaReutilizable, X - Radio, Y - Radio, Radio * 2f, Radio * 2f);
        }
    }

    /// <summary>
    /// Gestor unificado de efectos visuales cinematográficos de combate.
    /// Actualiza y renderiza sombras pseudo-3D, ondas expansivas, destellos, casquillos y vórtices.
    /// </summary>
    public sealed class GestorEfectosVisualesCombate
    {
        public CamaraCombate Camara { get; } = new CamaraCombate();
        public DestelloAtmosferico Destello { get; } = new DestelloAtmosferico();

        private readonly List<OndaExpansiva> _ondas = new List<OndaExpansiva>(32);
        private readonly List<CasquilloMunicios> _casquillos = new List<CasquilloMunicios>(64);
        private readonly List<VorticePuntaAla> _vortices = new List<VorticePuntaAla>(128);
        private readonly Random _rng = new Random();

        public void CrearOndaExpansiva(float x, float y, float radioMax, Color colorBorde, float velocidad = 260f)
        {
            _ondas.Add(new OndaExpansiva
            {
                X = x,
                Y = y,
                Radio = 2f,
                RadioMaximo = radioMax,
                VelocidadExpansion = velocidad,
                ColorBorde = colorBorde,
                Activa = true
            });
        }

        public void EyectarCasquillo(float x, float y, bool haciaIzquierda)
        {
            float dir = haciaIzquierda ? -1f : 1f;
            _casquillos.Add(new CasquilloMunicios
            {
                X = x,
                Y = y,
                VX = (dir * (1.2f + (float)_rng.NextDouble() * 1.8f)),
                VY = -1.5f - ((float)_rng.NextDouble() * 1.5f),
                Angulo = (float)_rng.NextDouble() * 360f,
                VelocidadGiro = (float)(_rng.NextDouble() * 24.0 - 12.0),
                VidaRestante = 26,
                Activo = true
            });
        }

        public void EmitirVorticeAla(float x, float y)
        {
            _vortices.Add(new VorticePuntaAla
            {
                X = x,
                Y = y,
                Radio = 1.5f,
                VidaRestante = 18,
                VidaMaxima = 18,
                Activo = true
            });
        }

        public void Actualizar(float dt)
        {
            Camara.Actualizar(dt);
            Destello.Actualizar(dt);

            for (int i = _ondas.Count - 1; i >= 0; i--)
            {
                OndaExpansiva o = _ondas[i];
                o.Actualizar(dt);
                if (!o.Activa) _ondas.RemoveAt(i);
                else _ondas[i] = o;
            }

            for (int i = _casquillos.Count - 1; i >= 0; i--)
            {
                CasquilloMunicios c = _casquillos[i];
                c.Actualizar(dt);
                if (!c.Activo) _casquillos.RemoveAt(i);
                else _casquillos[i] = c;
            }

            for (int i = _vortices.Count - 1; i >= 0; i--)
            {
                VorticePuntaAla v = _vortices[i];
                v.Actualizar();
                if (!v.Activo) _vortices.RemoveAt(i);
                else _vortices[i] = v;
            }
        }

        /// <summary>
        /// Dibuja sombras proyectadas pseudo-3D debajo de cada aeronave sobre la capa de nubes/atmósfera.
        /// Crea una sensación de profundidad y despegue tridimensional.
        /// </summary>
        public void DibujarSombrasAeronaves(Graphics g, IReadOnlyList<AeronaveBase> aeronaves, SolidBrush brochaReutilizable)
        {
            brochaReutilizable.Color = Color.FromArgb(50, 6, 14, 28);
            for (int i = 0; i < aeronaves.Count; i++)
            {
                AeronaveBase nave = aeronaves[i];
                if (!nave.Activo) continue;

                // Desplazamiento de sombra angular según altitud simulada
                float offsetX = 22f + (nave.AnguloInclinacion * 0.8f);
                float offsetY = 32f;
                float sx = nave.X + offsetX;
                float sy = nave.Y + offsetY;

                // Sombra suave elíptica proporcional al perfil del ala
                g.FillEllipse(brochaReutilizable, sx, sy, nave.Ancho * 0.88f, nave.Alto * 0.65f);
            }
        }

        public void DibujarVorticesYCasquillos(Graphics g, SolidBrush brochaReutilizable)
        {
            for (int i = 0; i < _vortices.Count; i++)
            {
                _vortices[i].Dibujar(g, brochaReutilizable);
            }
            for (int i = 0; i < _casquillos.Count; i++)
            {
                _casquillos[i].Dibujar(g, brochaReutilizable);
            }
        }

        public void DibujarOndasExpansivas(Graphics g, Pen plumaReutilizable)
        {
            for (int i = 0; i < _ondas.Count; i++)
            {
                _ondas[i].Dibujar(g, plumaReutilizable);
            }
        }
    }
}
