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
using System.Drawing.Drawing2D;

namespace CondorTactico
{
    /// <summary>
    /// Modelo vectorial fotorrealista en 3D del bombardero estratégico furtivo Northrop Grumman B-2 Spirit (AF 82-1071).
    /// Recreado a partir de la fotografía adjunta ("Spirit of Mississippi" AF 82-1071) mediante 2 matrices de vectores:
    /// 1. MatrizPuntos: Matriz de vectores de puntos (PointF[][]) en espacio normalizado [0..140] x [0..100].
    /// 2. MatrizColores: Matriz de colores (Color[]) con iluminación atmosférica 3D, recubrimiento RAM,
    ///    góndolas dorsales S-duct, cabina de 4 paneles con destello solar especular y borde de fuga en dientes de sierra.
    /// </summary>
    public static class B2SpiritModel
    {
        // =========================================================================
        // 1. MATRIZ DE VECTORES DE PUNTOS (Coordenadas normalizadas [0..140] ancho x [0..100] alto)
        // Orientación base (anguloRotar = 0): Proa apuntando hacia arriba (Y = 4), envergadura en eje X [2..138].
        // =========================================================================
        public static readonly PointF[][] MatrizPuntos = new PointF[][]
        {
            // 0. Sombra volumétrica atmosférica proyectada bajo el ala volante (Efecto 3D)
            new PointF[] {
                new PointF(70, 5), new PointF(4, 62), new PointF(14, 77),
                new PointF(39, 55), new PointF(51, 67), new PointF(59, 60),
                new PointF(70, 74), new PointF(81, 60), new PointF(89, 67),
                new PointF(101, 55), new PointF(126, 77), new PointF(136, 62)
            },

            // 1. Silueta base principal del Ala Volante (Dark Compass Ghost Gray - Acabado furtivo)
            new PointF[] {
                new PointF(70, 4),   // Punta de proa (Apex)
                new PointF(3, 60),   // Punta de ala izquierda delantera
                new PointF(13, 74),  // Punta de ala izquierda trasera
                new PointF(39, 52),  // Vértice interior ala izquierda
                new PointF(51, 64),  // Diente de sierra exterior izquierdo
                new PointF(59, 57),  // Hendidura de escape izquierda
                new PointF(70, 71),  // Punta central de cola (Aguijón trasero)
                new PointF(81, 57),  // Hendidura de escape derecha
                new PointF(89, 64),  // Diente de sierra exterior derecho
                new PointF(101, 52), // Vértice interior ala derecha
                new PointF(127, 74), // Punta de ala derecha trasera
                new PointF(137, 60)  // Punta de ala derecha delantera
            },

            // 2. Ala izquierda - Faceta superior iluminada por el cielo azulado
            new PointF[] {
                new PointF(70, 4), new PointF(3, 60), new PointF(13, 74),
                new PointF(39, 52), new PointF(54, 36), new PointF(64, 18)
            },

            // 3. Ala derecha - Faceta en sombreado volumétrico suave
            new PointF[] {
                new PointF(70, 4), new PointF(137, 60), new PointF(127, 74),
                new PointF(101, 52), new PointF(86, 36), new PointF(76, 18)
            },

            // 4. Bisel del borde de ataque izquierdo (Reflejo especular metálico intenso)
            new PointF[] {
                new PointF(70, 4), new PointF(3, 60), new PointF(5.5f, 62.5f), new PointF(70, 7.2f)
            },

            // 5. Bisel del borde de ataque derecho (Reflejo especular de luz solar)
            new PointF[] {
                new PointF(70, 4), new PointF(137, 60), new PointF(134.5f, 62.5f), new PointF(70, 7.2f)
            },

            // 6. Panel RAM exterior del ala izquierda (Recubrimiento absorbente de radar)
            new PointF[] {
                new PointF(40, 32), new PointF(10, 58), new PointF(16, 66), new PointF(45, 42)
            },

            // 7. Panel RAM exterior del ala derecha
            new PointF[] {
                new PointF(100, 32), new PointF(130, 58), new PointF(124, 66), new PointF(95, 42)
            },

            // 8. Ruddervator / Aerofreno partido de punta de ala izquierda (Split drag rudder)
            new PointF[] {
                new PointF(6, 62), new PointF(13, 74), new PointF(24, 64.5f), new PointF(17, 55)
            },

            // 9. Ruddervator / Aerofreno partido de punta de ala derecha
            new PointF[] {
                new PointF(134, 62), new PointF(127, 74), new PointF(116, 64.5f), new PointF(123, 55)
            },

            // 10. Elevón medio e interior del ala izquierda (Superficie de control en borde de fuga)
            new PointF[] {
                new PointF(24, 64.5f), new PointF(39, 52), new PointF(35, 47), new PointF(20, 59.5f)
            },

            // 11. Elevón medio e interior del ala derecha
            new PointF[] {
                new PointF(116, 64.5f), new PointF(101, 52), new PointF(105, 47), new PointF(120, 59.5f)
            },

            // 12. Cuerpo central abovedado (Fuselaje integrado / Blended Wing Body)
            new PointF[] {
                new PointF(70, 7), new PointF(53, 28), new PointF(50, 54),
                new PointF(70, 68), new PointF(90, 54), new PointF(87, 28)
            },

            // 13. Cresta dorsal central (Iluminación cenital sobre la joroba de cabina)
            new PointF[] {
                new PointF(70, 8), new PointF(62, 22), new PointF(60, 52),
                new PointF(70, 65), new PointF(80, 52), new PointF(78, 22)
            },

            // 14. Góndola del motor izquierdo (Volumen curvo S-duct izquierdo)
            new PointF[] {
                new PointF(57, 25), new PointF(47, 36), new PointF(49, 58),
                new PointF(59, 55), new PointF(61, 32)
            },

            // 15. Destello de volumen en góndola izquierda
            new PointF[] {
                new PointF(56, 28), new PointF(50, 36), new PointF(52, 52), new PointF(58, 49)
            },

            // 16. Góndola del motor derecho (Volumen curvo S-duct derecho)
            new PointF[] {
                new PointF(83, 25), new PointF(93, 36), new PointF(91, 58),
                new PointF(81, 55), new PointF(79, 32)
            },

            // 17. Destello de volumen en góndola derecha
            new PointF[] {
                new PointF(84, 28), new PointF(90, 36), new PointF(88, 52), new PointF(82, 49)
            },

            // 18. Toma de aire furtiva dentada izquierda (Labio superior en zig-zag)
            new PointF[] {
                new PointF(58, 26), new PointF(50, 32), new PointF(53, 36), new PointF(60, 31)
            },

            // 19. Cavidad oscura de admisión de turbina izquierda (F118-GE-100)
            new PointF[] {
                new PointF(57.5f, 28), new PointF(51.5f, 32.5f), new PointF(54, 36), new PointF(59.5f, 32)
            },

            // 20. Toma de aire furtiva dentada derecha
            new PointF[] {
                new PointF(82, 26), new PointF(90, 32), new PointF(87, 36), new PointF(80, 31)
            },

            // 21. Cavidad oscura de admisión de turbina derecha
            new PointF[] {
                new PointF(82.5f, 28), new PointF(88.5f, 32.5f), new PointF(86, 36), new PointF(80.5f, 32)
            },

            // 22. Difusor térmico trasero izquierdo (Escudo de carbono-titanio negro mate)
            new PointF[] {
                new PointF(50, 50), new PointF(58, 48), new PointF(59, 57), new PointF(51, 63)
            },

            // 23. Tobera oculta de escape izquierda
            new PointF[] {
                new PointF(51.5f, 49), new PointF(57, 47.5f), new PointF(57.5f, 52), new PointF(51, 54)
            },

            // 24. Difusor térmico trasero derecho (Escudo de carbono-titanio negro mate)
            new PointF[] {
                new PointF(90, 50), new PointF(82, 48), new PointF(81, 57), new PointF(89, 63)
            },

            // 25. Tobera oculta de escape derecha
            new PointF[] {
                new PointF(88.5f, 49), new PointF(83, 47.5f), new PointF(82.5f, 52), new PointF(89, 54)
            },

            // 26. Aguijón central de cola (Beaver tail / FLIR & radar stealth trailing wedge)
            new PointF[] {
                new PointF(64, 52), new PointF(76, 52), new PointF(70, 71)
            },

            // 27. Bisel iluminado del aguijón de cola
            new PointF[] {
                new PointF(67, 52), new PointF(73, 52), new PointF(70, 69)
            },

            // 28. Marco estructural de la cabina de mando (4-window Spirit Cockpit)
            new PointF[] {
                new PointF(70, 12.5f), new PointF(61.5f, 19), new PointF(63, 25),
                new PointF(70, 23.5f), new PointF(77, 25), new PointF(78.5f, 19)
            },

            // 29. Parabrisas lateral izquierdo
            new PointF[] {
                new PointF(62.2f, 19.2f), new PointF(65.2f, 16.5f), new PointF(65.5f, 21.5f), new PointF(63.2f, 23.8f)
            },

            // 30. Parabrisas frontal izquierdo (Cristal ITO antirradar)
            new PointF[] {
                new PointF(66.0f, 16.0f), new PointF(69.6f, 13.8f), new PointF(69.6f, 20.5f), new PointF(66.2f, 21.3f)
            },

            // 31. Parabrisas frontal derecho (Con reflejo celeste intenso como en la foto)
            new PointF[] {
                new PointF(70.4f, 13.8f), new PointF(74.0f, 16.0f), new PointF(73.8f, 21.3f), new PointF(70.4f, 20.5f)
            },

            // 32. Parabrisas lateral derecho
            new PointF[] {
                new PointF(74.8f, 16.5f), new PointF(77.8f, 19.2f), new PointF(76.8f, 23.8f), new PointF(74.5f, 21.5f)
            },

            // 33. Destello solar especular (Sun Glint blanco brillante en el parabrisas, idéntico a la foto)
            new PointF[] {
                new PointF(71.2f, 15.0f), new PointF(73.5f, 16.5f), new PointF(73.2f, 19.8f), new PointF(71.0f, 19.0f)
            },

            // 34. Sensores Pitot / Datos de aire furtivos en la proa
            new PointF[] {
                new PointF(67.5f, 10.5f), new PointF(68.8f, 10.0f), new PointF(68.5f, 11.8f)
            },
            new PointF[] {
                new PointF(72.5f, 10.5f), new PointF(71.2f, 10.0f), new PointF(71.5f, 11.8f)
            },

            // 36. Receptáculo dorsal de reabastecimiento en vuelo (En la espina central)
            new PointF[] {
                new PointF(68.5f, 30), new PointF(71.5f, 30), new PointF(72, 35), new PointF(68, 35)
            }
        };

        // =========================================================================
        // 2. MATRIZ DE VECTORES DE COLORES (Paleta fotorrealista B-2 Spirit AF 82-1071)
        // =========================================================================
        public static readonly Color[] MatrizColores = new Color[]
        {
            Color.FromArgb(65, 12, 20, 30),     // 0. Sombra atmosférica proyectada
            Color.FromArgb(255, 62, 76, 88),    // 1. Silueta base Dark Compass Ghost Gray
            Color.FromArgb(255, 76, 92, 106),   // 2. Ala izq iluminada por el cielo
            Color.FromArgb(255, 54, 67, 79),    // 3. Ala der sombreado volumétrico
            Color.FromArgb(255, 132, 150, 166), // 4. Bisel borde de ataque izq (Luz especular)
            Color.FromArgb(255, 110, 128, 144), // 5. Bisel borde de ataque der
            Color.FromArgb(255, 68, 83, 96),    // 6. Panel RAM exterior izq
            Color.FromArgb(255, 49, 61, 72),    // 7. Panel RAM exterior der
            Color.FromArgb(255, 55, 68, 80),    // 8. Ruddervator punta izq
            Color.FromArgb(255, 44, 55, 66),    // 9. Ruddervator punta der
            Color.FromArgb(255, 64, 79, 92),    // 10. Elevón ala izq
            Color.FromArgb(255, 48, 60, 71),    // 11. Elevón ala der
            Color.FromArgb(255, 72, 87, 100),   // 12. Cuerpo central Blended Wing Body
            Color.FromArgb(255, 88, 105, 120),  // 13. Cresta dorsal iluminada
            Color.FromArgb(255, 80, 96, 110),   // 14. Góndola motor izq
            Color.FromArgb(255, 98, 115, 130),  // 15. Brillo góndola motor izq
            Color.FromArgb(255, 58, 72, 84),    // 16. Góndola motor der
            Color.FromArgb(255, 74, 90, 104),   // 17. Brillo góndola motor der
            Color.FromArgb(255, 105, 122, 138), // 18. Labio dentado toma aire izq
            Color.FromArgb(255, 14, 18, 22),    // 19. Cavidad oscura turbina izq
            Color.FromArgb(255, 86, 102, 116),  // 20. Labio dentado toma aire der
            Color.FromArgb(255, 12, 15, 19),    // 21. Cavidad oscura turbina der
            Color.FromArgb(255, 24, 27, 32),    // 22. Difusor térmico carbono izq
            Color.FromArgb(255, 12, 14, 17),    // 23. Tobera escape izq
            Color.FromArgb(255, 22, 25, 30),    // 24. Difusor térmico carbono der
            Color.FromArgb(255, 10, 12, 15),    // 25. Tobera escape der
            Color.FromArgb(255, 58, 71, 83),    // 26. Aguijón central de cola
            Color.FromArgb(255, 78, 93, 107),   // 27. Bisel aguijón de cola
            Color.FromArgb(255, 38, 47, 56),    // 28. Marco estructural cabina
            Color.FromArgb(255, 28, 44, 58),    // 29. Parabrisas lateral izq
            Color.FromArgb(255, 42, 68, 88),    // 30. Parabrisas frontal izq
            Color.FromArgb(255, 145, 195, 225), // 31. Parabrisas frontal der (Reflejo cielo)
            Color.FromArgb(255, 55, 85, 108),   // 32. Parabrisas lateral der
            Color.FromArgb(250, 245, 252, 255), // 33. Destello solar blanco (Sun Glint)
            Color.FromArgb(255, 165, 120, 95),  // 34. Sensor Pitot proa izq
            Color.FromArgb(255, 165, 120, 95),  // 35. Sensor Pitot proa der
            Color.FromArgb(255, 62, 76, 88)     // 36. Compuerta reabastecimiento dorsal
        };

        /// <summary>
        /// Dibuja el bombardero furtivo Northrop Grumman B-2 Spirit (AF 82-1071) con supersampling 4x (SSAA),
        /// líneas de paneles absorbentes de radar (RAM), matrícula "1071", escudos USAF y destello solar en cabina.
        /// </summary>
        public static Bitmap DibujarB2Spirit(int ancho, int alto, int anguloRotar, bool esRival, bool acelerando, int ssaa = 4)
        {
            int hiW = ancho * ssaa;
            int hiH = alto * ssaa;

            Bitmap hiBmp = new Bitmap(hiW, hiH, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(hiBmp))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;

                float sx = (float)hiW / 140.0f;
                float sy = (float)hiH / 100.0f;

                // 1. Renderizar todas las facetas poligonales 3D desde MatrizPuntos y MatrizColores
                for (int i = 0; i < MatrizPuntos.Length; i++)
                {
                    PointF[] ptsOrig = MatrizPuntos[i];
                    PointF[] ptsEscalados = new PointF[ptsOrig.Length];

                    for (int j = 0; j < ptsOrig.Length; j++)
                    {
                        float x = ptsOrig[j].X * sx;
                        float y = ptsOrig[j].Y * sy;

                        if (anguloRotar == 180)
                        {
                            x = hiW - x;
                            y = hiH - y;
                        }

                        ptsEscalados[j] = new PointF(x, y);
                    }

                    Color col = MatrizColores[i];
                    if (esRival && i > 0 && i != 33)
                    {
                        // Tono sigilo nocturno / obsidiana carmesí para el B-2 rival
                        int r = Math.Min(255, (int)(col.R * 0.95f + 28));
                        int gCol = (int)(col.G * 0.78f);
                        int b = (int)(col.B * 0.82f);
                        col = Color.FromArgb(col.A, r, gCol, b);
                    }

                    if (i == 0)
                    {
                        using (SolidBrush bSombra = new SolidBrush(Color.FromArgb(40, 10, 16, 24)))
                        {
                            g.FillPolygon(bSombra, ptsEscalados);
                        }
                    }
                    else
                    {
                        using (SolidBrush brocha = new SolidBrush(col))
                        {
                            g.FillPolygon(brocha, ptsEscalados);
                        }
                    }
                }

                // 2. Líneas perimetrales claras de paneles RAM (Característica icónica del B-2 Spirit en la foto)
                Color colorLineaRAM = esRival
                    ? Color.FromArgb(120, 215, 145, 155)
                    : Color.FromArgb(135, 175, 198, 215);

                using (Pen penRAM = new Pen(colorLineaRAM, Math.Max(1.0f, 0.38f * sx)))
                {
                    PointF[][] lineasRAM = new PointF[][]
                    {
                        new PointF[] { new PointF(42, 31), new PointF(10, 58), new PointF(16, 66), new PointF(47, 40) },
                        new PointF[] { new PointF(98, 31), new PointF(130, 58), new PointF(124, 66), new PointF(93, 40) },
                        new PointF[] { new PointF(64, 26), new PointF(61, 47), new PointF(70, 55), new PointF(79, 47), new PointF(76, 26) },
                        new PointF[] { new PointF(47, 36), new PointF(35, 46), new PointF(39, 50), new PointF(50, 41) },
                        new PointF[] { new PointF(93, 36), new PointF(105, 46), new PointF(101, 50), new PointF(90, 41) }
                    };

                    foreach (PointF[] poli in lineasRAM)
                    {
                        PointF[] esc = new PointF[poli.Length];
                        for (int j = 0; j < poli.Length; j++)
                        {
                            float x = poli[j].X * sx;
                            float y = poli[j].Y * sy;
                            if (anguloRotar == 180)
                            {
                                x = hiW - x;
                                y = hiH - y;
                            }
                            esc[j] = new PointF(x, y);
                        }
                        g.DrawPolygon(penRAM, esc);
                    }
                }

                // 3. Destello solar estelar en el parabrisas (Lens flare de la foto original)
                float glintX = 72.3f * sx;
                float glintY = 17.2f * sy;
                if (anguloRotar == 180)
                {
                    glintX = hiW - glintX;
                    glintY = hiH - glintY;
                }
                using (Pen penGlint = new Pen(Color.FromArgb(235, 255, 255, 255), Math.Max(1.0f, 0.55f * sx)))
                {
                    g.DrawLine(penGlint, glintX - 3.8f * sx, glintY, glintX + 3.8f * sx, glintY);
                    g.DrawLine(penGlint, glintX, glintY - 3.8f * sy, glintX, glintY + 3.8f * sy);
                }

                // 4. Insignias ACC/USAF y matrícula "AF 82 1071" en el fuselaje (Fiel a la imagen adjunta)
                using (SolidBrush bBlanco = new SolidBrush(Color.FromArgb(215, 238, 245, 252)))
                using (Font fMatricula = new Font("Consolas", Math.Max(5.0f, 2.4f * sx), FontStyle.Bold))
                {
                    float txtX = 52.5f * sx;
                    float txtY = 38.0f * sy;
                    if (anguloRotar == 180)
                    {
                        txtX = hiW - 64.0f * sx;
                        txtY = hiH - 42.0f * sy;
                    }
                    g.DrawString("1071", fMatricula, bBlanco, new PointF(txtX, txtY));
                }

                // 5. Doble propulsión furtiva de plasma / postcombustión en las 2 toberas superiores
                if (acelerando)
                {
                    float[] toberasX = new float[] { 54.5f * sx, 85.5f * sx };
                    float nozY = (anguloRotar == 180) ? (hiH - 58.0f * sy) : (58.0f * sy);
                    float dir = (anguloRotar == 180) ? -1.0f : 1.0f;

                    foreach (float nx in toberasX)
                    {
                        float realX = (anguloRotar == 180) ? (hiW - nx) : nx;
                        PointF[] llamaExt = new PointF[] {
                            new PointF(realX - 3.2f * sx, nozY),
                            new PointF(realX + 3.2f * sx, nozY),
                            new PointF(realX, nozY + dir * 15.0f * sy)
                        };
                        using (SolidBrush bPlasmaExt = new SolidBrush(Color.FromArgb(210, 60, 190, 255)))
                        {
                            g.FillPolygon(bPlasmaExt, llamaExt);
                        }

                        PointF[] llamaInt = new PointF[] {
                            new PointF(realX - 1.5f * sx, nozY),
                            new PointF(realX + 1.5f * sx, nozY),
                            new PointF(realX, nozY + dir * 9.5f * sy)
                        };
                        using (SolidBrush bPlasmaInt = new SolidBrush(Color.FromArgb(245, 230, 255, 255)))
                        {
                            g.FillPolygon(bPlasmaInt, llamaInt);
                        }
                    }
                }
            }

            // Downsampling bicúbico 4x -> 1x para bordes ultra nítidos
            Bitmap finalBmp = new Bitmap(ancho, alto, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics gFinal = Graphics.FromImage(finalBmp))
            {
                gFinal.SmoothingMode = SmoothingMode.HighQuality;
                gFinal.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gFinal.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gFinal.CompositingQuality = CompositingQuality.HighQuality;
                gFinal.DrawImage(hiBmp, new Rectangle(0, 0, ancho, alto));
            }
            hiBmp.Dispose();
            return finalBmp;
        }

        /// <summary>
        /// Dibuja el misil de crucero furtivo AGM-158 JASSM / bomba guiada de plasma del B-2 Spirit con 4x SSAA.
        /// </summary>
        public static Bitmap DibujarMisilB2(int ancho, int alto, int anguloRotar, bool esRival, int ssaa = 4)
        {
            int hiW = ancho * ssaa;
            int hiH = alto * ssaa;

            Bitmap hiBmp = new Bitmap(hiW, hiH, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(hiBmp))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;

                float cx = hiW / 2.0f;
                float sx = hiW / 16.0f;
                float sy = hiH / 36.0f;

                Color cNucleo = esRival ? Color.FromArgb(255, 255, 70, 90) : Color.FromArgb(255, 0, 245, 255);
                Color cCuerpo = esRival ? Color.FromArgb(255, 65, 45, 52) : Color.FromArgb(255, 52, 65, 78);
                Color cEstela = esRival ? Color.FromArgb(220, 255, 50, 30) : Color.FromArgb(220, 40, 175, 255);

                // Estela de propulsión furtiva
                PointF[] estela = new PointF[] {
                    new PointF(cx - 3.5f * sx, 23 * sy),
                    new PointF(cx + 3.5f * sx, 23 * sy),
                    new PointF(cx, 35 * sy)
                };
                using (SolidBrush bEstela = new SolidBrush(cEstela))
                {
                    g.FillPolygon(bEstela, estela);
                }

                // Alas desplegables furtivas en flecha (AGM-158)
                PointF[] alas = new PointF[] {
                    new PointF(cx, 9 * sy),
                    new PointF(cx - 7.2f * sx, 19 * sy),
                    new PointF(cx - 6.0f * sx, 21.5f * sy),
                    new PointF(cx, 17 * sy),
                    new PointF(cx + 6.0f * sx, 21.5f * sy),
                    new PointF(cx + 7.2f * sx, 19 * sy)
                };
                using (SolidBrush bAlas = new SolidBrush(Color.FromArgb(255, 75, 90, 105)))
                {
                    g.FillPolygon(bAlas, alas);
                }

                // Fuselaje furtivo facetado
                PointF[] fuselaje = new PointF[] {
                    new PointF(cx, 1.5f * sy),
                    new PointF(cx + 2.8f * sx, 8 * sy),
                    new PointF(cx + 2.5f * sx, 24 * sy),
                    new PointF(cx, 26 * sy),
                    new PointF(cx - 2.5f * sx, 24 * sy),
                    new PointF(cx - 2.8f * sx, 8 * sy)
                };
                using (SolidBrush bCuerpo = new SolidBrush(cCuerpo))
                {
                    g.FillPolygon(bCuerpo, fuselaje);
                }

                // Ojiva de plasma guiada por radar
                PointF[] ojiva = new PointF[] {
                    new PointF(cx, 0.5f * sy),
                    new PointF(cx + 2.0f * sx, 6.5f * sy),
                    new PointF(cx, 5.0f * sy),
                    new PointF(cx - 2.0f * sx, 6.5f * sy)
                };
                using (SolidBrush bOjiva = new SolidBrush(cNucleo))
                {
                    g.FillPolygon(bOjiva, ojiva);
                }

                if (anguloRotar == 180)
                {
                    hiBmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                }
            }

            Bitmap finalBmp = new Bitmap(ancho, alto, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics gFinal = Graphics.FromImage(finalBmp))
            {
                gFinal.SmoothingMode = SmoothingMode.HighQuality;
                gFinal.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gFinal.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gFinal.CompositingQuality = CompositingQuality.HighQuality;
                gFinal.DrawImage(hiBmp, new Rectangle(0, 0, ancho, alto));
            }
            hiBmp.Dispose();
            return finalBmp;
        }
    }
}


