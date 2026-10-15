# ==============================================================================
# SCRIPT: GenerarModeloF35.ps1
# DESCRIPCIÓN: Recrea el Lockheed Martin F-35 Lightning II fotorrealista en 3D
#              mediante 2 matrices de vectores (MatrizPuntos y MatrizColores).
#              Genera renders en alta resolución y en resolución de juego
#              con Supersampling Anti-Aliasing (SSAA 4x/8x) para máxima nitidez.
# USO: powershell -ExecutionPolicy Bypass -File GenerarModeloF35.ps1
# ==============================================================================

Add-Type -AssemblyName System.Drawing

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BaseDir = Split-Path -Parent $ScriptDir
$OutputFileHiRes = Join-Path $BaseDir "assets\renders\F35_Render_UltraHD.png"
$OutputFileGame = Join-Path $BaseDir "assets\renders\F35_Render_Game.png"
$OutputFileRival = Join-Path $BaseDir "assets\renders\F35_Render_Rival.png"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   GENERADOR VECTORIAL F-35 LIGHTNING II 3D (SSAA)" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. MATRIZ DE VECTORES DE PUNTOS ([0..100] x [0..140])
$MatrizPuntos = @(
    # 0. Sombra volumétrica proyectada en el suelo (Efecto 3D de suspensión)
    @( [System.Drawing.PointF]::new(48,4), [System.Drawing.PointF]::new(43,15), [System.Drawing.PointF]::new(38,35), [System.Drawing.PointF]::new(32,50),
       [System.Drawing.PointF]::new(2,96), [System.Drawing.PointF]::new(4,107), [System.Drawing.PointF]::new(34,100), [System.Drawing.PointF]::new(11,135),
       [System.Drawing.PointF]::new(22,139), [System.Drawing.PointF]::new(36,132), [System.Drawing.PointF]::new(44,138), [System.Drawing.PointF]::new(50,139),
       [System.Drawing.PointF]::new(56,138), [System.Drawing.PointF]::new(64,132), [System.Drawing.PointF]::new(78,139), [System.Drawing.PointF]::new(89,135),
       [System.Drawing.PointF]::new(66,100), [System.Drawing.PointF]::new(96,107), [System.Drawing.PointF]::new(98,96), [System.Drawing.PointF]::new(68,50),
       [System.Drawing.PointF]::new(62,35), [System.Drawing.PointF]::new(57,15), [System.Drawing.PointF]::new(52,4) ),

    # 1..3 Estabilizador horizontal izquierdo
    @( [System.Drawing.PointF]::new(36,108), [System.Drawing.PointF]::new(13,135), [System.Drawing.PointF]::new(16,138), [System.Drawing.PointF]::new(30,138), [System.Drawing.PointF]::new(37,126) ),
    @( [System.Drawing.PointF]::new(36,108), [System.Drawing.PointF]::new(13,135), [System.Drawing.PointF]::new(15,136), [System.Drawing.PointF]::new(36.5,110) ),
    @( [System.Drawing.PointF]::new(16,138), [System.Drawing.PointF]::new(30,138), [System.Drawing.PointF]::new(29,136), [System.Drawing.PointF]::new(17,136) ),

    # 4..6 Estabilizador horizontal derecho
    @( [System.Drawing.PointF]::new(64,108), [System.Drawing.PointF]::new(87,135), [System.Drawing.PointF]::new(84,138), [System.Drawing.PointF]::new(70,138), [System.Drawing.PointF]::new(63,126) ),
    @( [System.Drawing.PointF]::new(64,108), [System.Drawing.PointF]::new(87,135), [System.Drawing.PointF]::new(85,136), [System.Drawing.PointF]::new(63.5,110) ),
    @( [System.Drawing.PointF]::new(84,138), [System.Drawing.PointF]::new(70,138), [System.Drawing.PointF]::new(71,136), [System.Drawing.PointF]::new(83,136) ),

    # 7..11 Ala principal izquierda
    @( [System.Drawing.PointF]::new(29,59), [System.Drawing.PointF]::new(2,95), [System.Drawing.PointF]::new(3,105), [System.Drawing.PointF]::new(6,105), [System.Drawing.PointF]::new(35,98) ),
    @( [System.Drawing.PointF]::new(29,59), [System.Drawing.PointF]::new(2,95), [System.Drawing.PointF]::new(3.5,96.5), [System.Drawing.PointF]::new(30,61) ),
    @( [System.Drawing.PointF]::new(2,95), [System.Drawing.PointF]::new(0.5,96.5), [System.Drawing.PointF]::new(2,105), [System.Drawing.PointF]::new(5,105), [System.Drawing.PointF]::new(3.5,95) ),
    @( [System.Drawing.PointF]::new(0.5,96), [System.Drawing.PointF]::new(2,95), [System.Drawing.PointF]::new(2,97), [System.Drawing.PointF]::new(0.5,97.5) ),
    @( [System.Drawing.PointF]::new(6,105), [System.Drawing.PointF]::new(35,98), [System.Drawing.PointF]::new(34,96), [System.Drawing.PointF]::new(7,103) ),

    # 12..16 Ala principal derecha
    @( [System.Drawing.PointF]::new(71,59), [System.Drawing.PointF]::new(98,95), [System.Drawing.PointF]::new(97,105), [System.Drawing.PointF]::new(94,105), [System.Drawing.PointF]::new(65,98) ),
    @( [System.Drawing.PointF]::new(71,59), [System.Drawing.PointF]::new(98,95), [System.Drawing.PointF]::new(96.5,96.5), [System.Drawing.PointF]::new(70,61) ),
    @( [System.Drawing.PointF]::new(98,95), [System.Drawing.PointF]::new(99.5,96.5), [System.Drawing.PointF]::new(98,105), [System.Drawing.PointF]::new(95,105), [System.Drawing.PointF]::new(96.5,95) ),
    @( [System.Drawing.PointF]::new(99.5,96), [System.Drawing.PointF]::new(98,95), [System.Drawing.PointF]::new(98,97), [System.Drawing.PointF]::new(99.5,97.5) ),
    @( [System.Drawing.PointF]::new(94,105), [System.Drawing.PointF]::new(65,98), [System.Drawing.PointF]::new(66,96), [System.Drawing.PointF]::new(93,103) ),

    # 17..20 Paneles RAM alas
    @( [System.Drawing.PointF]::new(23,70), [System.Drawing.PointF]::new(27,69), [System.Drawing.PointF]::new(29,74), [System.Drawing.PointF]::new(26,77), [System.Drawing.PointF]::new(22,76), [System.Drawing.PointF]::new(20,72) ),
    @( [System.Drawing.PointF]::new(13,82), [System.Drawing.PointF]::new(17,80), [System.Drawing.PointF]::new(20,85), [System.Drawing.PointF]::new(17,88), [System.Drawing.PointF]::new(13,87), [System.Drawing.PointF]::new(11,83) ),
    @( [System.Drawing.PointF]::new(77,70), [System.Drawing.PointF]::new(73,69), [System.Drawing.PointF]::new(71,74), [System.Drawing.PointF]::new(74,77), [System.Drawing.PointF]::new(78,76), [System.Drawing.PointF]::new(80,72) ),
    @( [System.Drawing.PointF]::new(87,82), [System.Drawing.PointF]::new(83,80), [System.Drawing.PointF]::new(80,85), [System.Drawing.PointF]::new(83,88), [System.Drawing.PointF]::new(87,87), [System.Drawing.PointF]::new(89,83) ),

    # 21..22 Insignia USAF
    @( [System.Drawing.PointF]::new(16,72), [System.Drawing.PointF]::new(19,68), [System.Drawing.PointF]::new(22,72), [System.Drawing.PointF]::new(25,72), [System.Drawing.PointF]::new(21,75), [System.Drawing.PointF]::new(18,75) ),
    @( [System.Drawing.PointF]::new(18,71), [System.Drawing.PointF]::new(20,69), [System.Drawing.PointF]::new(22,71), [System.Drawing.PointF]::new(21,73), [System.Drawing.PointF]::new(19,73) ),

    # 23..24 Botalones de cola
    @( [System.Drawing.PointF]::new(35,96), [System.Drawing.PointF]::new(33,124), [System.Drawing.PointF]::new(43,124), [System.Drawing.PointF]::new(43,96) ),
    @( [System.Drawing.PointF]::new(65,96), [System.Drawing.PointF]::new(67,124), [System.Drawing.PointF]::new(57,124), [System.Drawing.PointF]::new(57,96) ),

    # 25..32 Tobera de escape Pratt & Whitney F135
    @( [System.Drawing.PointF]::new(44,122), [System.Drawing.PointF]::new(56,122), [System.Drawing.PointF]::new(55,126), [System.Drawing.PointF]::new(45,126) ),
    @( [System.Drawing.PointF]::new(45,126), [System.Drawing.PointF]::new(55,126), [System.Drawing.PointF]::new(54,136), [System.Drawing.PointF]::new(46,136) ),
    @( [System.Drawing.PointF]::new(44,125), [System.Drawing.PointF]::new(46,125), [System.Drawing.PointF]::new(45,133) ),
    @( [System.Drawing.PointF]::new(46,125), [System.Drawing.PointF]::new(48,125), [System.Drawing.PointF]::new(47,135) ),
    @( [System.Drawing.PointF]::new(48,125), [System.Drawing.PointF]::new(50,125), [System.Drawing.PointF]::new(49,136) ),
    @( [System.Drawing.PointF]::new(50,125), [System.Drawing.PointF]::new(52,125), [System.Drawing.PointF]::new(51,136.5) ),
    @( [System.Drawing.PointF]::new(52,125), [System.Drawing.PointF]::new(54,125), [System.Drawing.PointF]::new(53,135) ),
    @( [System.Drawing.PointF]::new(54,125), [System.Drawing.PointF]::new(56,125), [System.Drawing.PointF]::new(55,133) ),

    # 33..37 Timón vertical izquierdo (con sombra 3D proyectada)
    @( [System.Drawing.PointF]::new(36,96), [System.Drawing.PointF]::new(24,124), [System.Drawing.PointF]::new(30,131), [System.Drawing.PointF]::new(39,126), [System.Drawing.PointF]::new(38,96) ),
    @( [System.Drawing.PointF]::new(38,94), [System.Drawing.PointF]::new(23,124), [System.Drawing.PointF]::new(27,131), [System.Drawing.PointF]::new(39,127) ),
    @( [System.Drawing.PointF]::new(38,94), [System.Drawing.PointF]::new(23,124), [System.Drawing.PointF]::new(25,124), [System.Drawing.PointF]::new(39,96) ),
    @( [System.Drawing.PointF]::new(23,124), [System.Drawing.PointF]::new(25,124), [System.Drawing.PointF]::new(26,126), [System.Drawing.PointF]::new(24,126) ),
    @( [System.Drawing.PointF]::new(27,131), [System.Drawing.PointF]::new(39,127), [System.Drawing.PointF]::new(38,125), [System.Drawing.PointF]::new(28,129) ),

    # 38..42 Timón vertical derecho (con sombra 3D proyectada)
    @( [System.Drawing.PointF]::new(64,96), [System.Drawing.PointF]::new(76,124), [System.Drawing.PointF]::new(70,131), [System.Drawing.PointF]::new(61,126), [System.Drawing.PointF]::new(62,96) ),
    @( [System.Drawing.PointF]::new(62,94), [System.Drawing.PointF]::new(77,124), [System.Drawing.PointF]::new(73,131), [System.Drawing.PointF]::new(61,127) ),
    @( [System.Drawing.PointF]::new(62,94), [System.Drawing.PointF]::new(77,124), [System.Drawing.PointF]::new(75,124), [System.Drawing.PointF]::new(61,96) ),
    @( [System.Drawing.PointF]::new(77,124), [System.Drawing.PointF]::new(75,124), [System.Drawing.PointF]::new(74,126), [System.Drawing.PointF]::new(76,126) ),
    @( [System.Drawing.PointF]::new(73,131), [System.Drawing.PointF]::new(61,127), [System.Drawing.PointF]::new(62,125), [System.Drawing.PointF]::new(72,129) ),

    # 43..48 Fuselaje central y góndolas del motor
    @( [System.Drawing.PointF]::new(37,49), [System.Drawing.PointF]::new(63,49), [System.Drawing.PointF]::new(65,96), [System.Drawing.PointF]::new(35,96) ),
    @( [System.Drawing.PointF]::new(36,65), [System.Drawing.PointF]::new(46,65), [System.Drawing.PointF]::new(45,118), [System.Drawing.PointF]::new(35,118) ),
    @( [System.Drawing.PointF]::new(39,68), [System.Drawing.PointF]::new(44,68), [System.Drawing.PointF]::new(43,112), [System.Drawing.PointF]::new(38,112) ),
    @( [System.Drawing.PointF]::new(54,65), [System.Drawing.PointF]::new(64,65), [System.Drawing.PointF]::new(65,118), [System.Drawing.PointF]::new(55,118) ),
    @( [System.Drawing.PointF]::new(56,68), [System.Drawing.PointF]::new(61,68), [System.Drawing.PointF]::new(62,112), [System.Drawing.PointF]::new(57,112) ),
    @( [System.Drawing.PointF]::new(47,49), [System.Drawing.PointF]::new(53,49), [System.Drawing.PointF]::new(53,118), [System.Drawing.PointF]::new(47,118) ),

    # 49..52 Marcas dorsales de orientación y advertencia
    @( [System.Drawing.PointF]::new(50,64), [System.Drawing.PointF]::new(48,70), [System.Drawing.PointF]::new(52,70) ),
    @( [System.Drawing.PointF]::new(48,73), [System.Drawing.PointF]::new(52,73), [System.Drawing.PointF]::new(53,81), [System.Drawing.PointF]::new(47,81) ),
    @( [System.Drawing.PointF]::new(48.5,74.5), [System.Drawing.PointF]::new(51.5,74.5), [System.Drawing.PointF]::new(51.5,76), [System.Drawing.PointF]::new(48.5,76) ),
    @( [System.Drawing.PointF]::new(48,77.5), [System.Drawing.PointF]::new(52,77.5), [System.Drawing.PointF]::new(52,79), [System.Drawing.PointF]::new(48,79) ),

    # 53..56 Paneles RAM lomo dorsal
    @( [System.Drawing.PointF]::new(38,82), [System.Drawing.PointF]::new(44,82), [System.Drawing.PointF]::new(45,88), [System.Drawing.PointF]::new(43,90), [System.Drawing.PointF]::new(37,90), [System.Drawing.PointF]::new(36,85) ),
    @( [System.Drawing.PointF]::new(38,94), [System.Drawing.PointF]::new(44,94), [System.Drawing.PointF]::new(45,100), [System.Drawing.PointF]::new(43,102), [System.Drawing.PointF]::new(37,102), [System.Drawing.PointF]::new(36,97) ),
    @( [System.Drawing.PointF]::new(56,82), [System.Drawing.PointF]::new(62,82), [System.Drawing.PointF]::new(63,85), [System.Drawing.PointF]::new(62,90), [System.Drawing.PointF]::new(56,90), [System.Drawing.PointF]::new(55,88) ),
    @( [System.Drawing.PointF]::new(56,94), [System.Drawing.PointF]::new(62,94), [System.Drawing.PointF]::new(63,97), [System.Drawing.PointF]::new(62,102), [System.Drawing.PointF]::new(56,102), [System.Drawing.PointF]::new(55,100) ),

    # 57..58 Receptáculo de reabastecimiento en vuelo hexagonal
    @( [System.Drawing.PointF]::new(49,87), [System.Drawing.PointF]::new(51,87), [System.Drawing.PointF]::new(52,89.5), [System.Drawing.PointF]::new(51,92), [System.Drawing.PointF]::new(49,92), [System.Drawing.PointF]::new(48,89.5) ),
    @( [System.Drawing.PointF]::new(49.3,88), [System.Drawing.PointF]::new(50.7,88), [System.Drawing.PointF]::new(51.4,89.5), [System.Drawing.PointF]::new(50.7,91), [System.Drawing.PointF]::new(49.3,91), [System.Drawing.PointF]::new(48.6,89.5) ),

    # 59..62 Tomas de aire DSI furtivas (Diverterless Supersonic Inlets)
    @( [System.Drawing.PointF]::new(32,58), [System.Drawing.PointF]::new(37,49), [System.Drawing.PointF]::new(36,65), [System.Drawing.PointF]::new(29,59) ),
    @( [System.Drawing.PointF]::new(34,57), [System.Drawing.PointF]::new(37,50), [System.Drawing.PointF]::new(36,63), [System.Drawing.PointF]::new(32,58) ),
    @( [System.Drawing.PointF]::new(68,58), [System.Drawing.PointF]::new(63,49), [System.Drawing.PointF]::new(64,65), [System.Drawing.PointF]::new(71,59) ),
    @( [System.Drawing.PointF]::new(66,57), [System.Drawing.PointF]::new(63,50), [System.Drawing.PointF]::new(64,63), [System.Drawing.PointF]::new(68,58) ),

    # 63..65 Chine de proa y crestas de fuselaje
    @( [System.Drawing.PointF]::new(38,49), [System.Drawing.PointF]::new(42.5,35), [System.Drawing.PointF]::new(47.3,15), [System.Drawing.PointF]::new(50,15), [System.Drawing.PointF]::new(44,49) ),
    @( [System.Drawing.PointF]::new(62,49), [System.Drawing.PointF]::new(57.5,35), [System.Drawing.PointF]::new(52.7,15), [System.Drawing.PointF]::new(50,15), [System.Drawing.PointF]::new(56,49) ),
    @( [System.Drawing.PointF]::new(48.5,15), [System.Drawing.PointF]::new(51.5,15), [System.Drawing.PointF]::new(51,49), [System.Drawing.PointF]::new(49,49) ),

    # 66..69 Radomo de proa con tubo Pitot
    @( [System.Drawing.PointF]::new(50,1), [System.Drawing.PointF]::new(47.3,15), [System.Drawing.PointF]::new(52.7,15) ),
    @( [System.Drawing.PointF]::new(50,1), [System.Drawing.PointF]::new(47.3,15), [System.Drawing.PointF]::new(50,15) ),
    @( [System.Drawing.PointF]::new(50,1), [System.Drawing.PointF]::new(52.7,15), [System.Drawing.PointF]::new(50,15) ),
    @( [System.Drawing.PointF]::new(49.7,0), [System.Drawing.PointF]::new(50.3,0), [System.Drawing.PointF]::new(50.2,2), [System.Drawing.PointF]::new(49.8,2) ),

    # 70..74 Cabina interior, asiento eyectable y HUD
    @( [System.Drawing.PointF]::new(46,20), [System.Drawing.PointF]::new(54,20), [System.Drawing.PointF]::new(55,45), [System.Drawing.PointF]::new(45,45) ),
    @( [System.Drawing.PointF]::new(48.5,28), [System.Drawing.PointF]::new(51.5,28), [System.Drawing.PointF]::new(52,38), [System.Drawing.PointF]::new(48,38) ),
    @( [System.Drawing.PointF]::new(49,29), [System.Drawing.PointF]::new(51,29), [System.Drawing.PointF]::new(51,32), [System.Drawing.PointF]::new(49,32) ),
    @( [System.Drawing.PointF]::new(48.8,33), [System.Drawing.PointF]::new(51.2,33), [System.Drawing.PointF]::new(51.2,37), [System.Drawing.PointF]::new(48.8,37) ),
    @( [System.Drawing.PointF]::new(49,22), [System.Drawing.PointF]::new(51,22), [System.Drawing.PointF]::new(51,25), [System.Drawing.PointF]::new(49,25) ),

    # 75..79 Cúpula de burbuja de cristal con curvatura 3D y reflejo especular
    @( [System.Drawing.PointF]::new(46.5,19), [System.Drawing.PointF]::new(53.5,19), [System.Drawing.PointF]::new(54.5,44), [System.Drawing.PointF]::new(45.5,44) ),
    @( [System.Drawing.PointF]::new(47.5,21), [System.Drawing.PointF]::new(52.5,21), [System.Drawing.PointF]::new(53.5,42), [System.Drawing.PointF]::new(46.5,42) ),
    @( [System.Drawing.PointF]::new(46.5,19), [System.Drawing.PointF]::new(53.5,19), [System.Drawing.PointF]::new(54.5,44), [System.Drawing.PointF]::new(45.5,44),
       [System.Drawing.PointF]::new(46,44), [System.Drawing.PointF]::new(54,44), [System.Drawing.PointF]::new(53,20), [System.Drawing.PointF]::new(47,20) ),
    @( [System.Drawing.PointF]::new(49.2,21), [System.Drawing.PointF]::new(47.5,28), [System.Drawing.PointF]::new(47.2,38), [System.Drawing.PointF]::new(48.2,38), [System.Drawing.PointF]::new(48.5,28), [System.Drawing.PointF]::new(49.8,21) ),
    @( [System.Drawing.PointF]::new(48.4,38), [System.Drawing.PointF]::new(48,43), [System.Drawing.PointF]::new(49,43), [System.Drawing.PointF]::new(49.2,38) )
)

# 2. MATRIZ DE COLORES FOTORREALISTAS (ILUMINACIÓN 3D MUESTREADA)
$MatrizColores = @(
    [System.Drawing.Color]::FromArgb(65, 20, 25, 30),     # 0. Sombra en el suelo
    [System.Drawing.Color]::FromArgb(255, 96, 105, 115),  # 1. Estab horiz izq base
    [System.Drawing.Color]::FromArgb(255, 128, 139, 151), # 2. Estab horiz izq bisel luz
    [System.Drawing.Color]::FromArgb(255, 75, 83, 92),    # 3. Estab horiz izq fuga
    [System.Drawing.Color]::FromArgb(255, 78, 87, 96),    # 4. Estab horiz der base
    [System.Drawing.Color]::FromArgb(255, 105, 115, 126), # 5. Estab horiz der bisel
    [System.Drawing.Color]::FromArgb(255, 60, 68, 76),    # 6. Estab horiz der fuga
    [System.Drawing.Color]::FromArgb(255, 112, 123, 135), # 7. Ala izq base
    [System.Drawing.Color]::FromArgb(255, 146, 157, 170), # 8. Ala izq borde ataque luz
    [System.Drawing.Color]::FromArgb(255, 68, 76, 85),    # 9. Riel punta ala izq
    [System.Drawing.Color]::FromArgb(255, 175, 183, 190), # 10. Sensor óptico izq
    [System.Drawing.Color]::FromArgb(255, 86, 95, 105),   # 11. Flaperón ala izq
    [System.Drawing.Color]::FromArgb(255, 96, 106, 117),  # 12. Ala der base sombra
    [System.Drawing.Color]::FromArgb(255, 124, 135, 147), # 13. Ala der borde ataque
    [System.Drawing.Color]::FromArgb(255, 58, 66, 74),    # 14. Riel punta ala der
    [System.Drawing.Color]::FromArgb(255, 155, 163, 170), # 15. Sensor óptico der
    [System.Drawing.Color]::FromArgb(255, 72, 80, 89),    # 16. Flaperón ala der
    [System.Drawing.Color]::FromArgb(255, 136, 147, 160), # 17. RAM ala izq 1
    [System.Drawing.Color]::FromArgb(255, 140, 151, 164), # 18. RAM ala izq 2
    [System.Drawing.Color]::FromArgb(255, 116, 126, 138), # 19. RAM ala der 1
    [System.Drawing.Color]::FromArgb(255, 120, 130, 142), # 20. RAM ala der 2
    [System.Drawing.Color]::FromArgb(255, 168, 178, 189), # 21. Insignia USAF base
    [System.Drawing.Color]::FromArgb(255, 92, 102, 112),  # 22. Insignia USAF interior
    [System.Drawing.Color]::FromArgb(255, 105, 115, 126), # 23. Botalón trasero izq
    [System.Drawing.Color]::FromArgb(255, 80, 89, 99),    # 24. Botalón trasero der
    [System.Drawing.Color]::FromArgb(255, 52, 57, 63),    # 25. Cubierta tobera titanio
    [System.Drawing.Color]::FromArgb(255, 16, 17, 20),    # 26. Garganta tobera oscura
    [System.Drawing.Color]::FromArgb(255, 95, 88, 77),    # 27. Pétalo tobera 1
    [System.Drawing.Color]::FromArgb(255, 125, 116, 100), # 28. Pétalo tobera 2
    [System.Drawing.Color]::FromArgb(255, 82, 76, 68),    # 29. Pétalo tobera 3
    [System.Drawing.Color]::FromArgb(255, 130, 122, 105), # 30. Pétalo tobera 4
    [System.Drawing.Color]::FromArgb(255, 78, 73, 66),    # 31. Pétalo tobera 5
    [System.Drawing.Color]::FromArgb(255, 118, 110, 95),  # 32. Pétalo tobera 6
    [System.Drawing.Color]::FromArgb(195, 16, 20, 26),    # 33. Sombra timón izq 3D
    [System.Drawing.Color]::FromArgb(255, 104, 115, 126), # 34. Timón izq cara luz
    [System.Drawing.Color]::FromArgb(255, 175, 186, 198), # 35. Timón izq borde ataque
    [System.Drawing.Color]::FromArgb(255, 200, 209, 219), # 36. Timón izq punta
    [System.Drawing.Color]::FromArgb(255, 74, 82, 91),    # 37. Timón izq fuga
    [System.Drawing.Color]::FromArgb(195, 14, 17, 22),    # 38. Sombra timón der
    [System.Drawing.Color]::FromArgb(255, 34, 40, 48),    # 39. Timón der cara sombra
    [System.Drawing.Color]::FromArgb(255, 138, 149, 161), # 40. Timón der borde ataque
    [System.Drawing.Color]::FromArgb(255, 165, 175, 185), # 41. Timón der punta
    [System.Drawing.Color]::FromArgb(255, 24, 29, 35),    # 42. Timón der fuga
    [System.Drawing.Color]::FromArgb(255, 108, 118, 129), # 43. Fuselaje central
    [System.Drawing.Color]::FromArgb(255, 126, 137, 149), # 44. Motor izq volumen
    [System.Drawing.Color]::FromArgb(255, 148, 159, 171), # 45. Motor izq cresta luz
    [System.Drawing.Color]::FromArgb(255, 78, 87, 97),    # 46. Motor der sombra
    [System.Drawing.Color]::FromArgb(255, 98, 108, 119),  # 47. Motor der cresta
    [System.Drawing.Color]::FromArgb(255, 90, 99, 109),   # 48. Espina dorsal central
    [System.Drawing.Color]::FromArgb(255, 242, 247, 252), # 49. Chevron dorsal blanco (▲)
    [System.Drawing.Color]::FromArgb(255, 236, 242, 248), # 50. Trapezoide advertencia (▰)
    [System.Drawing.Color]::FromArgb(255, 20, 24, 28),    # 51. Franja negra advertencia 1
    [System.Drawing.Color]::FromArgb(255, 20, 24, 28),    # 52. Franja negra advertencia 2
    [System.Drawing.Color]::FromArgb(255, 132, 142, 153), # 53. RAM dorsal 1
    [System.Drawing.Color]::FromArgb(255, 136, 146, 157), # 54. RAM dorsal 2
    [System.Drawing.Color]::FromArgb(255, 104, 114, 125), # 55. RAM dorsal 3
    [System.Drawing.Color]::FromArgb(255, 108, 118, 129), # 56. RAM dorsal 4
    [System.Drawing.Color]::FromArgb(255, 155, 165, 175), # 57. Compuerta hexagonal
    [System.Drawing.Color]::FromArgb(255, 85, 94, 104),   # 58. Receptáculo hexagonal
    [System.Drawing.Color]::FromArgb(255, 138, 149, 161), # 59. Toma DSI izq rampa
    [System.Drawing.Color]::FromArgb(255, 12, 15, 19),    # 60. Toma DSI izq cavidad oscura
    [System.Drawing.Color]::FromArgb(255, 96, 106, 117),  # 61. Toma DSI der rampa
    [System.Drawing.Color]::FromArgb(255, 9, 12, 15),     # 62. Toma DSI der cavidad
    [System.Drawing.Color]::FromArgb(255, 142, 153, 165), # 63. Chine proa izq iluminado
    [System.Drawing.Color]::FromArgb(255, 42, 49, 57),    # 64. Chine proa der sombra
    [System.Drawing.Color]::FromArgb(255, 165, 176, 188), # 65. Cresta central proa
    [System.Drawing.Color]::FromArgb(255, 86, 95, 105),   # 66. Radomo proa base
    [System.Drawing.Color]::FromArgb(255, 122, 132, 144), # 67. Radomo faceta iluminada
    [System.Drawing.Color]::FromArgb(255, 48, 55, 63),    # 68. Radomo faceta sombra
    [System.Drawing.Color]::FromArgb(255, 25, 29, 34),    # 69. Tubo Pitot de proa
    [System.Drawing.Color]::FromArgb(255, 14, 16, 20),    # 70. Fondo interior cabina
    [System.Drawing.Color]::FromArgb(255, 58, 65, 55),    # 71. Asiento eyectable
    [System.Drawing.Color]::FromArgb(255, 185, 155, 110), # 72. Visor dorado piloto
    [System.Drawing.Color]::FromArgb(255, 28, 32, 36),    # 73. Cojín de asiento
    [System.Drawing.Color]::FromArgb(230, 60, 240, 120),  # 74. HUD verde fosforescente
    [System.Drawing.Color]::FromArgb(195, 26, 48, 66),   # 75. Cúpula de cristal ahumado
    [System.Drawing.Color]::FromArgb(150, 18, 35, 52),    # 76. Gradiente profundidad cristal
    [System.Drawing.Color]::FromArgb(255, 55, 63, 72),    # 77. Arco de la cúpula
    [System.Drawing.Color]::FromArgb(240, 248, 255, 255), # 78. Reflejo curvo blanco 3D
    [System.Drawing.Color]::FromArgb(170, 215, 240, 255)  # 79. Destello cian en el cristal
)

function Render-F35Vector {
    param(
        [int]$TargetWidth,
        [int]$TargetHeight,
        [int]$RotationAngle = 0,
        [bool]$IsRival = $false,
        [bool]$Afterburner = $false,
        [int]$SSAA = 4
    )

    $HiW = $TargetWidth * $SSAA
    $HiH = $TargetHeight * $SSAA

    $HiBmp = New-Object System.Drawing.Bitmap($HiW, $HiH, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $G = [System.Drawing.Graphics]::FromImage($HiBmp)
    $G.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $G.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $G.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $G.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

    $Sx = [float]$HiW / 100.0
    $Sy = [float]$HiH / 140.0

    # 1. Dibujar todas las facetas de las 2 matrices
    for ($i = 0; $i -lt $MatrizPuntos.Length; $i++) {
        $ptsOrig = $MatrizPuntos[$i]
        $ptsEscalados = New-Object System.Drawing.PointF[] $ptsOrig.Length

        for ($j = 0; $j -lt $ptsOrig.Length; $j++) {
            $x = $ptsOrig[$j].X * $Sx
            $y = $ptsOrig[$j].Y * $Sy
            if ($RotationAngle -eq 180) {
                $x = $HiW - $x
                $y = $HiH - $y
            }
            $ptsEscalados[$j] = [System.Drawing.PointF]::new($x, $y)
        }

        $col = $MatrizColores[$i]
        if ($IsRival -and $i -gt 0) {
            $r = [Math]::Min(255, [int]($col.R * 0.9 + 25))
            $gCol = [int]($col.G * 0.85)
            $b = [int]($col.B * 0.9)
            $col = [System.Drawing.Color]::FromArgb($col.A, $r, $gCol, $b)
        }

        $brush = New-Object System.Drawing.SolidBrush($col)
        $G.FillPolygon($brush, $ptsEscalados)
        $brush.Dispose()
    }

    # 2. Líneas estructurales sutiles (sin oscurecer todas las facetas para mantener acabado liso de composite)
    # Solo en uniones mecánicas (flaperones y timones)
    $penSeam = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(18, 0, 0, 0), [Math]::Max(1.0, 0.4 * $SSAA))
    foreach ($k in @(1, 4, 7, 12, 34, 39)) {
        $ptsOrig = $MatrizPuntos[$k]
        $ptsEscalados = New-Object System.Drawing.PointF[] $ptsOrig.Length
        for ($j = 0; $j -lt $ptsOrig.Length; $j++) {
            $x = $ptsOrig[$j].X * $Sx
            $y = $ptsOrig[$j].Y * $Sy
            if ($RotationAngle -eq 180) {
                $x = $HiW - $x
                $y = $HiH - $y
            }
            $ptsEscalados[$j] = [System.Drawing.PointF]::new($x, $y)
        }
        $G.DrawPolygon($penSeam, $ptsEscalados)
    }
    $penSeam.Dispose()

    # 3. Postcombustión si está acelerando
    if ($Afterburner) {
        $nozX = 50.0 * $Sx
        $nozY = if ($RotationAngle -eq 180) { $HiH - 136.0 * $Sy } else { 136.0 * $Sy }
        $dir = if ($RotationAngle -eq 180) { -1.0 } else { 1.0 }

        $llamaExterior = @(
            [System.Drawing.PointF]::new($nozX - 5.0 * $Sx, $nozY),
            [System.Drawing.PointF]::new($nozX + 5.0 * $Sx, $nozY),
            [System.Drawing.PointF]::new($nozX, $nozY + $dir * 20.0 * $Sy)
        )
        $bLlama = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(210, 255, 140, 20))
        $G.FillPolygon($bLlama, $llamaExterior)
        $bLlama.Dispose()

        $llamaInterior = @(
            [System.Drawing.PointF]::new($nozX - 2.5 * $Sx, $nozY),
            [System.Drawing.PointF]::new($nozX + 2.5 * $Sx, $nozY),
            [System.Drawing.PointF]::new($nozX, $nozY + $dir * 12.0 * $Sy)
        )
        $bNucleo = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(245, 255, 255, 180))
        $G.FillPolygon($bNucleo, $llamaInterior)
        $bNucleo.Dispose()
    }

    $G.Dispose()

    # Downsampling bicúbico a resolución objetivo
    $FinalBmp = New-Object System.Drawing.Bitmap($TargetWidth, $TargetHeight, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $GFinal = [System.Drawing.Graphics]::FromImage($FinalBmp)
    $GFinal.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $GFinal.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $GFinal.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $GFinal.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $GFinal.DrawImage($HiBmp, (New-Object System.Drawing.Rectangle(0, 0, $TargetWidth, $TargetHeight)))
    $GFinal.Dispose()
    $HiBmp.Dispose()

    return $FinalBmp
}

# Generar Render Ultra-HD (800x1120)
Write-Host "Generando render Ultra-HD (800x1120)..." -ForegroundColor Yellow
$sw = [System.Diagnostics.Stopwatch]::StartNew()
$bmpUltra = Render-F35Vector -TargetWidth 800 -TargetHeight 1120 -SSAA 4
$bmpUltra.Save($OutputFileHiRes, [System.Drawing.Imaging.ImageFormat]::Png)
$bmpUltra.Dispose()
$sw.Stop()
Write-Host " -> Guardado: $OutputFileHiRes ($($sw.ElapsedMilliseconds) ms)" -ForegroundColor Green

# Generar Render Juego (84x118) con SSAA 4x
Write-Host "Generando render en resolucion de juego (84x118)..." -ForegroundColor Yellow
$sw.Restart()
$bmpGame = Render-F35Vector -TargetWidth 84 -TargetHeight 118 -SSAA 4
$bmpGame.Save($OutputFileGame, [System.Drawing.Imaging.ImageFormat]::Png)
$bmpGame.Dispose()
$sw.Stop()
Write-Host " -> Guardado: $OutputFileGame ($($sw.ElapsedMilliseconds) ms)" -ForegroundColor Green

# Generar Render Nave Rival (84x118) con SSAA 4x
Write-Host "Generando render Nave Rival (84x118)..." -ForegroundColor Yellow
$sw.Restart()
$bmpRival = Render-F35Vector -TargetWidth 84 -TargetHeight 118 -RotationAngle 180 -IsRival $true -SSAA 4
$bmpRival.Save($OutputFileRival, [System.Drawing.Imaging.ImageFormat]::Png)
$bmpRival.Dispose()
$sw.Stop()
Write-Host " -> Guardado: $OutputFileRival ($($sw.ElapsedMilliseconds) ms)" -ForegroundColor Green

Write-Host "`nTodas las imagenes se generaron con exito y calidad maxima sin pixelado." -ForegroundColor Green
