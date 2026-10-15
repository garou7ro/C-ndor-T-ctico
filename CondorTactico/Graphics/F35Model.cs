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
    /// Modelo vectorial fotorrealista en 3D del caza furtivo Lockheed Martin F-35 Lightning II.
    /// Recreado a partir de la imagen "F-35 Lightning II" mediante 2 matrices de vectores:
    /// 1. MatrizPuntos: Matriz de vectores de puntos (PointF[][]) para cada faceta y sección 3D.
    /// 2. MatrizColores: Matriz de colores (Color[]) con iluminación y sombreado 3D realista.
    /// </summary>
    public static class F35Model
    {
        // =========================================================================
        // 1. MATRIZ DE VECTORES DE PUNTOS (Coordenadas normalizadas [0..100] x [0..140])
        // Cada fila es un vector de puntos (PointF[]) que define una faceta poligonal 3D.
        // =========================================================================
        public static readonly PointF[][] MatrizPuntos = new PointF[][]
        {
            // 0. Sombra volumétrica proyectada en el suelo (Efecto 3D de suspensión en el aire)
            new PointF[] {
                new PointF(48,4), new PointF(43,15), new PointF(38,35), new PointF(32,50),
                new PointF(2,96), new PointF(4,107), new PointF(34,100), new PointF(11,135),
                new PointF(22,139), new PointF(36,132), new PointF(44,138), new PointF(50,139),
                new PointF(56,138), new PointF(64,132), new PointF(78,139), new PointF(89,135),
                new PointF(66,100), new PointF(96,107), new PointF(98,96), new PointF(68,50),
                new PointF(62,35), new PointF(57,15), new PointF(52,4)
            },

            // 1. Estabilizador horizontal izquierdo (Superficie base de titanio y composite)
            new PointF[] { new PointF(36,108), new PointF(13,135), new PointF(16,138), new PointF(30,138), new PointF(37,126) },
            // 2. Estabilizador horizontal izq (Bisel del borde de ataque - Iluminación solar directa)
            new PointF[] { new PointF(36,108), new PointF(13,135), new PointF(15,136), new PointF(36.5f,110) },
            // 3. Estabilizador horizontal izq (Borde de fuga en penumbra)
            new PointF[] { new PointF(16,138), new PointF(30,138), new PointF(29,136), new PointF(17,136) },

            // 4. Estabilizador horizontal derecho (Superficie base en lado de sombra)
            new PointF[] { new PointF(64,108), new PointF(87,135), new PointF(84,138), new PointF(70,138), new PointF(63,126) },
            // 5. Estabilizador horizontal der (Bisel de borde de ataque)
            new PointF[] { new PointF(64,108), new PointF(87,135), new PointF(85,136), new PointF(63.5f,110) },
            // 6. Estabilizador horizontal der (Borde de fuga)
            new PointF[] { new PointF(84,138), new PointF(70,138), new PointF(71,136), new PointF(83,136) },

            // 7. Ala principal izquierda (Plano delta furtivo con flecha de 35° - Gris sigilo)
            new PointF[] { new PointF(29,59), new PointF(2,95), new PointF(3,105), new PointF(6,105), new PointF(35,98) },
            // 8. Ala izq borde de ataque (Reflejo metálico especular)
            new PointF[] { new PointF(29,59), new PointF(2,95), new PointF(3.5f,96.5f), new PointF(30,61) },
            // 9. Riel lanzador de punta de ala izq (Riel de misil AIM-120/AIM-9X)
            new PointF[] { new PointF(2,95), new PointF(0.5f,96.5f), new PointF(2,105), new PointF(5,105), new PointF(3.5f,95) },
            // 10. Sensor electro-óptico EOTS de punta de ala izq
            new PointF[] { new PointF(0.5f,96), new PointF(2,95), new PointF(2,97), new PointF(0.5f,97.5f) },
            // 11. Flaperón de ala izq (Borde de fuga móvil)
            new PointF[] { new PointF(6,105), new PointF(35,98), new PointF(34,96), new PointF(7,103) },

            // 12. Ala principal derecha (Plano delta en sombreado ambiental)
            new PointF[] { new PointF(71,59), new PointF(98,95), new PointF(97,105), new PointF(94,105), new PointF(65,98) },
            // 13. Ala der borde de ataque
            new PointF[] { new PointF(71,59), new PointF(98,95), new PointF(96.5f,96.5f), new PointF(70,61) },
            // 14. Riel lanzador punta de ala der
            new PointF[] { new PointF(98,95), new PointF(99.5f,96.5f), new PointF(98,105), new PointF(95,105), new PointF(96.5f,95) },
            // 15. Sensor óptico punta de ala der
            new PointF[] { new PointF(99.5f,96), new PointF(98,95), new PointF(98,97), new PointF(99.5f,97.5f) },
            // 16. Flaperón de ala der
            new PointF[] { new PointF(94,105), new PointF(65,98), new PointF(66,96), new PointF(93,103) },

            // 17. Paneles RAM de ala izq (Cinta absorbente de radar en dientes de sierra)
            new PointF[] { new PointF(23,70), new PointF(27,69), new PointF(29,74), new PointF(26,77), new PointF(22,76), new PointF(20,72) },
            // 18. Paneles RAM exteriores ala izq
            new PointF[] { new PointF(13,82), new PointF(17,80), new PointF(20,85), new PointF(17,88), new PointF(13,87), new PointF(11,83) },
            // 19. Paneles RAM de ala der
            new PointF[] { new PointF(77,70), new PointF(73,69), new PointF(71,74), new PointF(74,77), new PointF(78,76), new PointF(80,72) },
            // 20. Paneles RAM exteriores ala der
            new PointF[] { new PointF(87,82), new PointF(83,80), new PointF(80,85), new PointF(83,88), new PointF(87,87), new PointF(89,83) },

            // 21. Insignia estelar y barras USAF en el ala izquierda (Identica a la foto)
            new PointF[] { new PointF(16,72), new PointF(19,68), new PointF(22,72), new PointF(25,72), new PointF(21,75), new PointF(18,75) },
            new PointF[] { new PointF(18,71), new PointF(20,69), new PointF(22,71), new PointF(21,73), new PointF(19,73) },

            // 23. Botalón / Extensión de fuselaje trasero izq
            new PointF[] { new PointF(35,96), new PointF(33,124), new PointF(43,124), new PointF(43,96) },
            // 24. Botalón / Extensión de fuselaje trasero der
            new PointF[] { new PointF(65,96), new PointF(67,124), new PointF(57,124), new PointF(57,96) },

            // 25. Escudo térmico de titanio de la tobera de escape
            new PointF[] { new PointF(44,122), new PointF(56,122), new PointF(55,126), new PointF(45,126) },
            // 26. Garganta interior de la turbina F135
            new PointF[] { new PointF(45,126), new PointF(55,126), new PointF(54,136), new PointF(46,136) },

            // 27..32. Pétalos cerámicos radiales y serrados de la tobera (Pratt & Whitney F135)
            new PointF[] { new PointF(44,125), new PointF(46,125), new PointF(45,133) },
            new PointF[] { new PointF(46,125), new PointF(48,125), new PointF(47,135) },
            new PointF[] { new PointF(48,125), new PointF(50,125), new PointF(49,136) },
            new PointF[] { new PointF(50,125), new PointF(52,125), new PointF(51,136.5f) },
            new PointF[] { new PointF(52,125), new PointF(54,125), new PointF(53,135) },
            new PointF[] { new PointF(54,125), new PointF(56,125), new PointF(55,133) },

            // 33. Sombra 3D proyectada por el timón vertical izquierdo
            new PointF[] { new PointF(36,96), new PointF(24,124), new PointF(30,131), new PointF(39,126), new PointF(38,96) },

            // 34. Timón vertical izquierdo (Cara principal splay hacia afuera)
            new PointF[] { new PointF(38,94), new PointF(23,124), new PointF(27,131), new PointF(39,127) },
            // 35. Timón vertical izq (Borde de ataque iluminado por el sol)
            new PointF[] { new PointF(38,94), new PointF(23,124), new PointF(25,124), new PointF(39,96) },
            // 36. Timón vertical izq (Punta biselada del timón)
            new PointF[] { new PointF(23,124), new PointF(25,124), new PointF(26,126), new PointF(24,126) },
            // 37. Timón vertical izq (Superficie de timón de dirección móvil)
            new PointF[] { new PointF(27,131), new PointF(39,127), new PointF(38,125), new PointF(28,129) },

            // 38. Sombra proyectada por el timón vertical derecho
            new PointF[] { new PointF(64,96), new PointF(76,124), new PointF(70,131), new PointF(61,126), new PointF(62,96) },

            // 39. Timón vertical derecho (Cara principal en sombra profunda)
            new PointF[] { new PointF(62,94), new PointF(77,124), new PointF(73,131), new PointF(61,127) },
            // 40. Timón vertical der (Borde de ataque reflejo de luz)
            new PointF[] { new PointF(62,94), new PointF(77,124), new PointF(75,124), new PointF(61,96) },
            // 41. Timón vertical der (Punta biselada)
            new PointF[] { new PointF(77,124), new PointF(75,124), new PointF(74,126), new PointF(76,126) },
            // 42. Timón vertical der (Superficie de dirección en sombra)
            new PointF[] { new PointF(73,131), new PointF(61,127), new PointF(62,125), new PointF(72,129) },

            // 43. Cubierta y plataforma de fuselaje central
            new PointF[] { new PointF(37,49), new PointF(63,49), new PointF(65,96), new PointF(35,96) },

            // 44. Góndola y abultamiento del motor izquierdo (Efecto de volumen curvo)
            new PointF[] { new PointF(36,65), new PointF(46,65), new PointF(45,118), new PointF(35,118) },
            new PointF[] { new PointF(39,68), new PointF(44,68), new PointF(43,112), new PointF(38,112) },

            // 46. Góndola y abultamiento del motor derecho (Volumen en penumbra)
            new PointF[] { new PointF(54,65), new PointF(64,65), new PointF(65,118), new PointF(55,118) },
            new PointF[] { new PointF(56,68), new PointF(61,68), new PointF(62,112), new PointF(57,112) },

            // 48. Depresión central de la espina dorsal
            new PointF[] { new PointF(47,49), new PointF(53,49), new PointF(53,118), new PointF(47,118) },

            // 49. Chevron dorsal blanco de orientación (▲)
            new PointF[] { new PointF(50,64), new PointF(48,70), new PointF(52,70) },
            // 50. Trapezoide dorsal blanco con marcas de advertencia (▰)
            new PointF[] { new PointF(48,73), new PointF(52,73), new PointF(53,81), new PointF(47,81) },
            new PointF[] { new PointF(48.5f,74.5f), new PointF(51.5f,74.5f), new PointF(51.5f,76), new PointF(48.5f,76) },
            new PointF[] { new PointF(48,77.5f), new PointF(52,77.5f), new PointF(52,79), new PointF(48,79) },

            // 53. Paneles RAM de lomo dorsal (Patrones en escalera)
            new PointF[] { new PointF(38,82), new PointF(44,82), new PointF(45,88), new PointF(43,90), new PointF(37,90), new PointF(36,85) },
            new PointF[] { new PointF(38,94), new PointF(44,94), new PointF(45,100), new PointF(43,102), new PointF(37,102), new PointF(36,97) },
            new PointF[] { new PointF(56,82), new PointF(62,82), new PointF(63,85), new PointF(62,90), new PointF(56,90), new PointF(55,88) },
            new PointF[] { new PointF(56,94), new PointF(62,94), new PointF(63,97), new PointF(62,102), new PointF(56,102), new PointF(55,100) },

            // 57. Compuerta hexagonal de reabastecimiento en vuelo
            new PointF[] { new PointF(49,87), new PointF(51,87), new PointF(52,89.5f), new PointF(51,92), new PointF(49,92), new PointF(48,89.5f) },
            new PointF[] { new PointF(49.5f,88), new PointF(50.5f,88), new PointF(51,89.5f), new PointF(50.5f,91), new PointF(49.5f,91), new PointF(49,89.5f) },

            // 59. Toma de aire supersónica DSI izquierda (Rampa de compresión)
            new PointF[] { new PointF(38,49), new PointF(32,58), new PointF(35,62), new PointF(41,53) },
            // 60. Cavidad oscura de entrada DSI izq
            new PointF[] { new PointF(37,51), new PointF(34,58), new PointF(37,60), new PointF(39,53) },

            // 61. Toma de aire supersónica DSI derecha
            new PointF[] { new PointF(62,49), new PointF(68,58), new PointF(65,62), new PointF(59,53) },
            // 62. Cavidad oscura de entrada DSI der
            new PointF[] { new PointF(63,51), new PointF(66,58), new PointF(63,60), new PointF(61,53) },

            // 63. Borde chine proa izquierdo (Iluminado)
            new PointF[] { new PointF(47.3f,15), new PointF(42.5f,35), new PointF(41.5f,49), new PointF(47,49), new PointF(48,35), new PointF(49,15) },
            // 64. Borde chine proa derecho (Sombra)
            new PointF[] { new PointF(52.7f,15), new PointF(57.5f,35), new PointF(58.5f,49), new PointF(53,49), new PointF(52,35), new PointF(51,15) },
            // 65. Cresta central de proa
            new PointF[] { new PointF(49,15), new PointF(51,15), new PointF(50.5f,35), new PointF(49.5f,35) },

            // 66. Radomo cónico base (Gris composite sigilo)
            new PointF[] { new PointF(47.3f,15), new PointF(52.7f,15), new PointF(51.5f,8), new PointF(48.5f,8) },
            // 67. Faceta iluminada radomo
            new PointF[] { new PointF(48.5f,8), new PointF(50,2), new PointF(50,8) },
            // 68. Faceta sombra radomo
            new PointF[] { new PointF(51.5f,8), new PointF(50,2), new PointF(50,8) },
            // 69. Tubo Pitot / Sensor de punta
            new PointF[] { new PointF(49.6f,2), new PointF(50.4f,2), new PointF(50,0.5f) },

            // 70. Interior profundo de bañera de cabina (Negro carbón)
            new PointF[] { new PointF(50,20), new PointF(45.5f,33), new PointF(46.5f,46), new PointF(50,47.5f), new PointF(53.5f,46), new PointF(54.5f,33) },

            // 71. Asiento eyectable Martin-Baker US16E
            new PointF[] { new PointF(49,32), new PointF(51,32), new PointF(51.5f,39), new PointF(48.5f,39) },
            // 72. Casco y visor dorado del piloto
            new PointF[] { new PointF(49.3f,33), new PointF(50.7f,33), new PointF(50.7f,35.5f), new PointF(49.3f,35.5f) },
            // 73. Cojín inferior de cabina
            new PointF[] { new PointF(48.5f,39), new PointF(51.5f,39), new PointF(52,43), new PointF(48,43) },

            // 74. Pantalla HUD verde fosforescente
            new PointF[] { new PointF(49.5f,23), new PointF(50.5f,23), new PointF(50.7f,25.5f), new PointF(49.3f,25.5f) },

            // 75. Cúpula de cristal tintado con recubrimiento dorado ITO
            new PointF[] { new PointF(50,20), new PointF(45.5f,33), new PointF(46.5f,46), new PointF(50,47.5f), new PointF(53.5f,46), new PointF(54.5f,33) },
            new PointF[] { new PointF(50,21.5f), new PointF(46.5f,33), new PointF(47.5f,44.5f), new PointF(50,46), new PointF(52.5f,44.5f), new PointF(53.5f,33) },

            // 77. Arco estructural de la cúpula (Bow frame)
            new PointF[] { new PointF(46,30), new PointF(54,30), new PointF(54,31.5f), new PointF(46,31.5f) },

            // 78. Reflejo especular curvo en el cristal de la cabina (Efecto 3D de cúpula de burbuja)
            new PointF[] { new PointF(49.2f,21), new PointF(47.5f,28), new PointF(47.2f,38), new PointF(48.2f,38), new PointF(48.5f,28), new PointF(49.8f,21) },
            new PointF[] { new PointF(48.4f,38), new PointF(48,43), new PointF(49,43), new PointF(49.2f,38) }
        };

        // =========================================================================
        // 2. MATRIZ DE VECTORES DE COLORES (Colores fotorrealistas con iluminación 3D)
        // Corresponde uno a uno con cada faceta de MatrizPuntos.
        // =========================================================================
        public static readonly Color[] MatrizColores = new Color[]
        {
            Color.FromArgb(65, 20, 25, 30),     // 0. Sombra en el suelo
            Color.FromArgb(255, 96, 105, 115),  // 1. Estab horiz izq base
            Color.FromArgb(255, 128, 139, 151), // 2. Estab horiz izq bisel luz
            Color.FromArgb(255, 75, 83, 92),    // 3. Estab horiz izq fuga
            Color.FromArgb(255, 78, 87, 96),    // 4. Estab horiz der base
            Color.FromArgb(255, 105, 115, 126), // 5. Estab horiz der bisel
            Color.FromArgb(255, 60, 68, 76),    // 6. Estab horiz der fuga
            Color.FromArgb(255, 112, 123, 135), // 7. Ala izq base
            Color.FromArgb(255, 146, 157, 170), // 8. Ala izq borde ataque luz
            Color.FromArgb(255, 68, 76, 85),    // 9. Riel punta ala izq
            Color.FromArgb(255, 175, 183, 190), // 10. Sensor óptico izq
            Color.FromArgb(255, 86, 95, 105),   // 11. Flaperón ala izq
            Color.FromArgb(255, 96, 106, 117),  // 12. Ala der base sombra
            Color.FromArgb(255, 124, 135, 147), // 13. Ala der borde ataque
            Color.FromArgb(255, 58, 66, 74),    // 14. Riel punta ala der
            Color.FromArgb(255, 155, 163, 170), // 15. Sensor óptico der
            Color.FromArgb(255, 72, 80, 89),    // 16. Flaperón ala der
            Color.FromArgb(255, 136, 147, 160), // 17. RAM ala izq 1
            Color.FromArgb(255, 140, 151, 164), // 18. RAM ala izq 2
            Color.FromArgb(255, 116, 126, 138), // 19. RAM ala der 1
            Color.FromArgb(255, 120, 130, 142), // 20. RAM ala der 2
            Color.FromArgb(255, 168, 178, 189), // 21. Insignia USAF base
            Color.FromArgb(255, 92, 102, 112),  // 22. Insignia USAF interior
            Color.FromArgb(255, 105, 115, 126), // 23. Botalón trasero izq
            Color.FromArgb(255, 80, 89, 99),    // 24. Botalón trasero der
            Color.FromArgb(255, 52, 57, 63),    // 25. Cubierta tobera titanio
            Color.FromArgb(255, 16, 17, 20),    // 26. Garganta tobera oscura
            Color.FromArgb(255, 95, 88, 77),    // 27. Pétalo tobera 1
            Color.FromArgb(255, 125, 116, 100), // 28. Pétalo tobera 2
            Color.FromArgb(255, 82, 76, 68),    // 29. Pétalo tobera 3
            Color.FromArgb(255, 130, 122, 105), // 30. Pétalo tobera 4
            Color.FromArgb(255, 78, 73, 66),    // 31. Pétalo tobera 5
            Color.FromArgb(255, 118, 110, 95),  // 32. Pétalo tobera 6
            Color.FromArgb(195, 16, 20, 26),    // 33. Sombra timón izq 3D
            Color.FromArgb(255, 104, 115, 126), // 34. Timón izq cara luz
            Color.FromArgb(255, 175, 186, 198), // 35. Timón izq borde ataque
            Color.FromArgb(255, 200, 209, 219), // 36. Timón izq punta
            Color.FromArgb(255, 74, 82, 91),    // 37. Timón izq fuga
            Color.FromArgb(195, 14, 17, 22),    // 38. Sombra timón der
            Color.FromArgb(255, 34, 40, 48),    // 39. Timón der cara sombra
            Color.FromArgb(255, 138, 149, 161), // 40. Timón der borde ataque
            Color.FromArgb(255, 165, 175, 185), // 41. Timón der punta
            Color.FromArgb(255, 24, 29, 35),    // 42. Timón der fuga
            Color.FromArgb(255, 108, 118, 129), // 43. Fuselaje central
            Color.FromArgb(255, 126, 137, 149), // 44. Motor izq volumen
            Color.FromArgb(255, 148, 159, 171), // 45. Motor izq cresta luz
            Color.FromArgb(255, 78, 87, 97),    // 46. Motor der sombra
            Color.FromArgb(255, 98, 108, 119),  // 47. Motor der cresta
            Color.FromArgb(255, 90, 99, 109),   // 48. Espina dorsal central
            Color.FromArgb(255, 242, 247, 252), // 49. Chevron dorsal blanco (▲)
            Color.FromArgb(255, 236, 242, 248), // 50. Trapezoide advertencia (▰)
            Color.FromArgb(255, 20, 24, 28),    // 51. Franja negra advertencia 1
            Color.FromArgb(255, 20, 24, 28),    // 52. Franja negra advertencia 2
            Color.FromArgb(255, 132, 142, 153), // 53. RAM dorsal 1
            Color.FromArgb(255, 136, 146, 157), // 54. RAM dorsal 2
            Color.FromArgb(255, 104, 114, 125), // 55. RAM dorsal 3
            Color.FromArgb(255, 108, 118, 129), // 56. RAM dorsal 4
            Color.FromArgb(255, 155, 165, 175), // 57. Compuerta hexagonal
            Color.FromArgb(255, 85, 94, 104),   // 58. Receptáculo hexagonal
            Color.FromArgb(255, 138, 149, 161), // 59. Toma DSI izq rampa
            Color.FromArgb(255, 12, 15, 19),    // 60. Toma DSI izq cavidad oscura
            Color.FromArgb(255, 96, 106, 117),  // 61. Toma DSI der rampa
            Color.FromArgb(255, 9, 12, 15),     // 62. Toma DSI der cavidad
            Color.FromArgb(255, 142, 153, 165), // 63. Chine proa izq iluminado
            Color.FromArgb(255, 42, 49, 57),    // 64. Chine proa der sombra
            Color.FromArgb(255, 165, 176, 188), // 65. Cresta central proa
            Color.FromArgb(255, 86, 95, 105),   // 66. Radomo proa base
            Color.FromArgb(255, 122, 132, 144), // 67. Radomo faceta iluminada
            Color.FromArgb(255, 48, 55, 63),    // 68. Radomo faceta sombra
            Color.FromArgb(255, 25, 29, 34),    // 69. Tubo Pitot de proa
            Color.FromArgb(255, 14, 16, 20),    // 70. Fondo interior cabina
            Color.FromArgb(255, 58, 65, 55),    // 71. Asiento eyectable
            Color.FromArgb(255, 185, 155, 110), // 72. Visor dorado piloto
            Color.FromArgb(255, 28, 32, 36),    // 73. Cojín de asiento
            Color.FromArgb(230, 60, 240, 120),  // 74. HUD verde fosforescente
            Color.FromArgb(195, 26, 48, 66),   // 75. Cúpula de cristal ahumado
            Color.FromArgb(150, 18, 35, 52),    // 76. Gradiente profundidad cristal
            Color.FromArgb(255, 55, 63, 72),    // 77. Arco de la cúpula
            Color.FromArgb(240, 248, 255, 255), // 78. Reflejo curvo blanco 3D
            Color.FromArgb(170, 215, 240, 255)  // 79. Destello cian en el cristal
        };

        /// <summary>
        /// Obtiene el contorno de silueta exterior del F-35 para delimitar el área de colisión y región.
        /// </summary>
        public static GraphicsPath ObtenerSiluetaF35(int ancho, int alto, int anguloRotar)
        {
            PointF[] siluetaNorm = new PointF[] {
                new PointF(50, 1),
                new PointF(47.3f, 15),
                new PointF(42.5f, 35),
                new PointF(38, 49),
                new PointF(32, 58),
                new PointF(29, 59),
                new PointF(2, 95),
                new PointF(0.5f, 96.5f),
                new PointF(2, 105),
                new PointF(6, 105),
                new PointF(35, 98),
                new PointF(33, 124),
                new PointF(23, 124),
                new PointF(27, 131),
                new PointF(13, 135),
                new PointF(16, 138),
                new PointF(30, 138),
                new PointF(44, 126),
                new PointF(45, 136),
                new PointF(55, 136),
                new PointF(56, 126),
                new PointF(70, 138),
                new PointF(84, 138),
                new PointF(87, 135),
                new PointF(73, 131),
                new PointF(77, 124),
                new PointF(67, 124),
                new PointF(65, 98),
                new PointF(94, 105),
                new PointF(98, 105),
                new PointF(99.5f, 96.5f),
                new PointF(98, 95),
                new PointF(71, 59),
                new PointF(68, 58),
                new PointF(62, 49),
                new PointF(57.5f, 35),
                new PointF(52.7f, 15),
                new PointF(50, 1)
            };

            float sx = (float)ancho / 100.0f;
            float sy = (float)alto / 140.0f;
            Point[] pts = new Point[siluetaNorm.Length];

            for (int i = 0; i < siluetaNorm.Length; i++)
            {
                float x = siluetaNorm[i].X * sx;
                float y = siluetaNorm[i].Y * sy;

                if (anguloRotar == 180)
                {
                    x = ancho - x;
                    y = alto - y;
                }

                pts[i] = new Point((int)Math.Round(x), (int)Math.Round(y));
            }

            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(pts);
            return path;
        }

        /// <summary>
        /// Dibuja el F-35 Lightning II fotorrealista usando las 2 matrices de vectores con supersampling 4x (SSAA)
        /// para eliminar cualquier pixelado y lograr bordes perfectamente suaves y definidos.
        /// </summary>
        public static Bitmap DibujarF35(int ancho, int alto, int anguloRotar, bool esRival, bool acelerando, int ssaa = 4)
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

                float sx = (float)hiW / 100.0f;
                float sy = (float)hiH / 140.0f;

                // 1. Dibujar todas las facetas 3D recorriendo las 2 matrices de vectores
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
                    if (esRival && i > 0)
                    {
                        // Camuflaje táctico carmesí / sigilo nocturno para la nave rival
                        int r = Math.Min(255, (int)(col.R * 0.9f + 25));
                        int gCol = (int)(col.G * 0.85f);
                        int b = (int)(col.B * 0.9f);
                        col = Color.FromArgb(col.A, r, gCol, b);
                    }

                    // Sombra 3D volumétrica difusa (faceta 0) con efecto de desenfoque suave
                    if (i == 0)
                    {
                        using (SolidBrush bSombraSuave = new SolidBrush(Color.FromArgb(35, 15, 20, 25)))
                        {
                            g.FillPolygon(bSombraSuave, ptsEscalados);
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

                // 2. Líneas de unión mecánicas y de control de vuelo de ultra-alta precisión
                // Solo en superficies móviles y articulaciones reales (flaperones y timones) para mantener el acabado liso
                using (Pen penSeam = new Pen(Color.FromArgb(20, 0, 0, 0), Math.Max(1.0f, 0.4f * ssaa)))
                {
                    int[] articulaciones = new int[] { 1, 4, 7, 11, 12, 16, 34, 37, 39, 42 };
                    foreach (int k in articulaciones)
                    {
                        if (k >= MatrizPuntos.Length) continue;
                        PointF[] ptsOrig = MatrizPuntos[k];
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
                        g.DrawPolygon(penSeam, ptsEscalados);
                    }
                }

                // 3. Efecto de postcombustión / llama de turbina Pratt & Whitney F135
                if (acelerando)
                {
                    float nozX = 50.0f * sx;
                    float nozY = (anguloRotar == 180) ? (hiH - 136.0f * sy) : (136.0f * sy);
                    float dir = (anguloRotar == 180) ? -1.0f : 1.0f;

                    PointF[] llamaExterior = new PointF[] {
                        new PointF(nozX - 5.0f * sx, nozY),
                        new PointF(nozX + 5.0f * sx, nozY),
                        new PointF(nozX, nozY + dir * 20.0f * sy)
                    };
                    using (SolidBrush bLlama = new SolidBrush(Color.FromArgb(210, 255, 140, 20)))
                    {
                        g.FillPolygon(bLlama, llamaExterior);
                    }

                    PointF[] llamaInterior = new PointF[] {
                        new PointF(nozX - 2.5f * sx, nozY),
                        new PointF(nozX + 2.5f * sx, nozY),
                        new PointF(nozX, nozY + dir * 12.0f * sy)
                    };
                    using (SolidBrush bNucleo = new SolidBrush(Color.FromArgb(245, 255, 255, 180)))
                    {
                        g.FillPolygon(bNucleo, llamaInterior);
                    }
                }
            }

            // Downsampling bicúbico de alta calidad para máxima nitidez y cero pixelado
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
        /// Genera un sprite vectorial supersónico AIM-120 con estela de plasma y supersampling 4x para cero pixelado.
        /// </summary>
        public static Bitmap DibujarMisil(int ancho, int alto, int anguloRotar, bool esRival, int ssaa = 4)
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
                float sx = hiW / 14.0f;
                float sy = hiH / 32.0f;

                Color cPunta = esRival ? Color.FromArgb(255, 255, 80, 80) : Color.FromArgb(255, 80, 240, 255);
                Color cCuerpo = esRival ? Color.FromArgb(255, 75, 60, 65) : Color.FromArgb(255, 85, 95, 105);
                Color cFuego = esRival ? Color.FromArgb(230, 255, 60, 20) : Color.FromArgb(230, 255, 170, 30);
                Color cNucleo = Color.FromArgb(250, 255, 255, 220);

                // 1. Estela de fuego / propulsión cohete en la cola
                PointF[] llamaCola = new PointF[] {
                    new PointF(cx - 3.5f * sx, 20.0f * sy),
                    new PointF(cx + 3.5f * sx, 20.0f * sy),
                    new PointF(cx + 2.0f * sx, 26.0f * sy),
                    new PointF(cx, 31.0f * sy),
                    new PointF(cx - 2.0f * sy, 26.0f * sy)
                };
                using (SolidBrush bFuego = new SolidBrush(cFuego))
                {
                    g.FillPolygon(bFuego, llamaCola);
                }

                PointF[] llamaNucleo = new PointF[] {
                    new PointF(cx - 1.5f * sx, 20.0f * sy),
                    new PointF(cx + 1.5f * sx, 20.0f * sy),
                    new PointF(cx, 25.0f * sy)
                };
                using (SolidBrush bNucleo = new SolidBrush(cNucleo))
                {
                    g.FillPolygon(bNucleo, llamaNucleo);
                }

                // 2. Aletas de estabilización de cola
                PointF[] aletasCola = new PointF[] {
                    new PointF(cx - 6.0f * sx, 21.0f * sy),
                    new PointF(cx - 2.0f * sx, 17.0f * sy),
                    new PointF(cx + 2.0f * sx, 17.0f * sy),
                    new PointF(cx + 6.0f * sx, 21.0f * sy),
                    new PointF(cx + 4.5f * sx, 22.0f * sy),
                    new PointF(cx - 4.5f * sx, 22.0f * sy)
                };
                using (SolidBrush bAletas = new SolidBrush(Color.FromArgb(255, 55, 65, 75)))
                {
                    g.FillPolygon(bAletas, aletasCola);
                }

                // 3. Fuselaje aerodinámico cilíndrico del misil
                RectangleF cuerpoMisil = new RectangleF(cx - 2.2f * sx, 7.0f * sy, 4.4f * sx, 14.0f * sy);
                using (SolidBrush bCuerpo = new SolidBrush(cCuerpo))
                {
                    g.FillRectangle(bCuerpo, cuerpoMisil);
                }

                // Brillo metálico longitudinal en el misil
                using (Pen penLuz = new Pen(Color.FromArgb(180, 230, 240, 250), 1.0f * ssaa))
                {
                    g.DrawLine(penLuz, cx - 1.0f * sx, 7.0f * sy, cx - 1.0f * sx, 21.0f * sy);
                }

                // 4. Aletas delanteras de maniobra canard
                PointF[] aletasProa = new PointF[] {
                    new PointF(cx - 5.0f * sx, 11.0f * sy),
                    new PointF(cx - 2.0f * sx, 8.5f * sy),
                    new PointF(cx + 2.0f * sx, 8.5f * sy),
                    new PointF(cx + 5.0f * sx, 11.0f * sy),
                    new PointF(cx + 4.0f * sx, 12.0f * sy),
                    new PointF(cx - 4.0f * sx, 12.0f * sy)
                };
                using (SolidBrush bCanard = new SolidBrush(Color.FromArgb(255, 65, 75, 85)))
                {
                    g.FillPolygon(bCanard, aletasProa);
                }

                // 5. Ojiva de proa ojival
                PointF[] ojiva = new PointF[] {
                    new PointF(cx, 1.0f * sy),
                    new PointF(cx + 2.2f * sx, 7.0f * sy),
                    new PointF(cx - 2.2f * sx, 7.0f * sy)
                };
                using (SolidBrush bOjiva = new SolidBrush(Color.FromArgb(255, 120, 130, 140)))
                {
                    g.FillPolygon(bOjiva, ojiva);
                }

                // 6. Núcleo de energía de plasma brillante en la punta
                PointF[] puntaPlasma = new PointF[] {
                    new PointF(cx, 0.0f * sy),
                    new PointF(cx + 1.6f * sx, 4.0f * sy),
                    new PointF(cx, 3.0f * sy),
                    new PointF(cx - 1.6f * sx, 4.0f * sy)
                };
                using (SolidBrush bPlasma = new SolidBrush(cPunta))
                {
                    g.FillPolygon(bPlasma, puntaPlasma);
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


