# ==============================================================================
# GENERADOR DE TEXTURAS PBR HI-RES (1024x1024) PARA AERONAVES 3D Y OBJETOS
# CÓNDOR TÁCTICO 3D - ACABADO AEROESPACIAL MASTER GUIADO POR HOJAS DE REFERENCIA
# ==============================================================================

import os
import math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

OUTPUT_DIRS = [
    r"C:\Users\kreiv\OneDrive\Documentos\JuegoAviones\CondorTactico\wwwroot\assets\texturas",
    r"C:\Users\kreiv\OneDrive\Documentos\JuegoAviones\assets\texturas",
    r"C:\Users\kreiv\OneDrive\Documentos\JuegoAviones\CondorTactico\bin\Debug\net8.0-windows\wwwroot\assets\texturas",
    r"C:\Users\kreiv\OneDrive\Documentos\JuegoAviones\tools\CapturadorRemoto\bin\Debug\net8.0-windows\wwwroot\assets\texturas"
]

for d in OUTPUT_DIRS:
    os.makedirs(d, exist_ok=True)

N = 1024

def guardar_imagen(nombre, img):
    for d in OUTPUT_DIRS:
        ruta = os.path.join(d, nombre)
        img.save(ruta, "PNG", optimize=True)
    print(f"Textura guardada: {nombre} ({img.size})")

def calcular_normal_map(height_map, strength=2.2):
    """Calcula un mapa de normales tangente (RGB) a partir de un mapa de elevación."""
    h = height_map.astype(np.float32) / 255.0
    gx = np.roll(h, -1, axis=1) - np.roll(h, 1, axis=1)
    gy = np.roll(h, -1, axis=0) - np.roll(h, 1, axis=0)
    nx = -gx * strength
    ny = -gy * strength
    nz = np.ones_like(h)
    norm = np.sqrt(nx*nx + ny*ny + nz*nz)
    nx /= norm
    ny /= norm
    nz /= norm
    r = np.clip((nx * 0.5 + 0.5) * 255.0, 0, 255).astype(np.uint8)
    g = np.clip((ny * 0.5 + 0.5) * 255.0, 0, 255).astype(np.uint8)
    b = np.clip((nz * 0.5 + 0.5) * 255.0, 0, 255).astype(np.uint8)
    return Image.fromarray(np.stack([r, g, b], axis=-1), "RGB")

# ==============================================================================
# 1. CÓNDOR DORADO X (5TH-GEN SCI-FI STEALTH FIGHTER)
# Titanio azul pizarra, bordes alares y canards dorados, conductos de plasma cian
# ==============================================================================
def generar_condor_x():
    base = Image.new("RGBA", (N, N), (42, 58, 78, 255)) # Titanio azul pizarra
    draw = ImageDraw.Draw(base)
    height = Image.new("L", (N, N), 128)
    draw_h = ImageDraw.Draw(height)
    emissive = Image.new("RGBA", (N, N), (0, 0, 0, 255))
    draw_e = ImageDraw.Draw(emissive)

    # 1. Degradado y facetas de fuselaje
    for x in range(N // 2):
        factor = 1.0 - (x / (N // 2)) * 0.22
        col = (int(48 * factor), int(68 * factor), int(90 * factor), 255)
        draw.line([(x, 0), (x, N)], fill=col)
        draw.line([(N - 1 - x, 0), (N - 1 - x, N)], fill=col)

    # 2. Paneles de armadura dorada (Bordes alares y canards)
    gold_pts_izq = [(0, 720), (120, 640), (280, 520), (280, 560), (120, 720), (0, 840)]
    gold_pts_der = [(N - 1 - x, y) for (x, y) in gold_pts_izq]
    draw.polygon(gold_pts_izq, fill=(218, 175, 45, 255))
    draw.polygon(gold_pts_der, fill=(218, 175, 45, 255))
    draw_h.polygon(gold_pts_izq, fill=165)
    draw_h.polygon(gold_pts_der, fill=165)

    # Placas doradas en canards
    canard_izq = [(140, 360), (240, 310), (240, 340), (160, 410)]
    canard_der = [(N - 1 - x, y) for (x, y) in canard_izq]
    draw.polygon(canard_izq, fill=(225, 182, 50, 255))
    draw.polygon(canard_der, fill=(225, 182, 50, 255))
    draw_h.polygon(canard_izq, fill=165)
    draw_h.polygon(canard_der, fill=165)

    # 3. Líneas de paneles de blindaje táctico
    for x in [180, 280, 380, 460]:
        draw.line([(x, 150), (x, 900)], fill=(24, 34, 46, 255), width=2)
        draw.line([(N - x, 150), (N - x, 900)], fill=(24, 34, 46, 255), width=2)
        draw_h.line([(x, 150), (x, 900)], fill=95, width=2)
        draw_h.line([(N - x, 150), (N - x, 900)], fill=95, width=2)

    for y in [240, 380, 520, 680, 820]:
        draw.line([(100, y), (924, y)], fill=(24, 34, 46, 255), width=2)
        draw_h.line([(100, y), (924, y)], fill=95, width=2)

    # 4. Conductos de plasma cian resplandeciente (Líneas energéticas de la espina y alas)
    conduits = [
        [(512, 120), (512, 920)],             # Espina central
        [(512, 360), (320, 480), (180, 680)],  # Rama alar izquierda
        [(512, 360), (704, 480), (844, 680)],  # Rama alar derecha
        [(512, 580), (360, 720), (260, 860)],  # Rama cola izquierda
        [(512, 580), (664, 720), (764, 860)],  # Rama cola derecha
    ]
    for c_line in conduits:
        draw.line(c_line, fill=(0, 240, 255, 255), width=4)
        draw_e.line(c_line, fill=(0, 240, 255, 255), width=5)
        draw_h.line(c_line, fill=145, width=3)

    # 5. Cabina y HUD verde esmeralda
    cabina = [(475, 210), (549, 210), (545, 340), (479, 340)]
    draw.polygon(cabina, fill=(22, 28, 36, 255))
    draw_h.polygon(cabina, fill=180)
    # Reflejo dorado en la cúpula
    draw.polygon([(485, 220), (539, 220), (536, 325), (488, 325)], fill=(210, 165, 40, 230))
    # Retícula HUD proyectada
    draw_e.ellipse([498, 250, 526, 278], outline=(0, 255, 128, 255), width=2)
    draw_e.line([(512, 244), (512, 284)], fill=(0, 255, 128, 255), width=2)
    draw_e.line([(492, 264), (532, 264)], fill=(0, 255, 128, 255), width=2)

    # 6. Anillos de postcombustión bimotor (toberas traseras)
    for cx in [440, 584]:
        draw.ellipse([cx - 40, 870, cx + 40, 950], fill=(30, 36, 44, 255), outline=(218, 175, 45, 255), width=4)
        draw_h.ellipse([cx - 40, 870, cx + 40, 950], fill=175)
        draw_e.ellipse([cx - 28, 882, cx + 28, 938], fill=(0, 220, 255, 255))
        draw_e.ellipse([cx - 15, 895, cx + 15, 925], fill=(255, 255, 255, 255))

    guardar_imagen("tex_condor_hull.png", base)
    guardar_imagen("tex_condor_emission.png", emissive)
    guardar_imagen("tex_condor_normal.png", calcular_normal_map(np.array(height)))


# ==============================================================================
# 2. F-35 LIGHTNING II (STEALTH MULTIROLE STRIKE FIGHTER)
# Gris antracita mate, cintas RAM diente de sierra, tobera F135, misiles wingtip
# ==============================================================================
def generar_f35_lightning():
    base = Image.new("RGBA", (N, N), (50, 54, 62, 255)) # Gris furtivo RAM
    draw = ImageDraw.Draw(base)
    height = Image.new("L", (N, N), 128)
    draw_h = ImageDraw.Draw(height)
    emissive = Image.new("RGBA", (N, N), (0, 0, 0, 255))
    draw_e = ImageDraw.Draw(emissive)

    # Degradado y paneles alares
    for x in range(N // 2):
        f = 1.0 - (x / (N // 2)) * 0.16
        draw.line([(x, 0), (x, N)], fill=(int(58 * f), int(62 * f), int(72 * f), 255))
        draw.line([(N - 1 - x, 0), (N - 1 - x, N)], fill=(int(58 * f), int(62 * f), int(72 * f), 255))

    # Cintas RAM diente de sierra (Sawtooth RAM Tape) características en bordes
    ram_col = (82, 88, 100, 255)
    # Borde de ataque con dientes
    for side in [1, -1]:
        pts = []
        for i in range(16):
            t = i / 15.0
            px = 512 - side * int(80 + t * 380)
            py = int(240 + t * 440 + (8 if i % 2 == 1 else 0))
            pts.append((px, py))
        draw.line(pts, fill=ram_col, width=5)
        draw_h.line(pts, fill=155, width=4)

    # Tomas de aire DSI (Diverterless Supersonic Intake)
    for cx in [400, 624]:
        draw.polygon([(cx - 30, 310), (cx + 30, 310), (cx + 18, 430), (cx - 18, 430)], fill=(28, 30, 36, 255))
        draw_h.polygon([(cx - 30, 310), (cx + 30, 310), (cx + 18, 430), (cx - 18, 430)], fill=80)

    # Cabina de cúpula dorada (Gold Amber Stealth Canopy)
    canopy = [(478, 190), (546, 190), (542, 345), (482, 345)]
    draw.polygon(canopy, fill=(210, 160, 35, 240))
    draw_h.polygon(canopy, fill=190)

    # Escarapela USAF de baja visibilidad
    draw.ellipse([260, 610, 330, 680], outline=(70, 75, 86, 255), width=3)
    draw.ellipse([694, 610, 764, 680], outline=(70, 75, 86, 255), width=3)

    # Tobera Pratt & Whitney F135 (gran motor monomotor central)
    draw.ellipse([464, 860, 560, 956], fill=(32, 34, 40, 255), outline=(95, 100, 112, 255), width=5)
    draw_h.ellipse([464, 860, 560, 956], fill=175)
    # Flama de postcombustión naranja-azul
    draw_e.ellipse([480, 876, 544, 940], fill=(255, 110, 0, 255))
    draw_e.ellipse([494, 890, 530, 926], fill=(120, 220, 255, 255))

    guardar_imagen("tex_f35_ram.png", base)
    guardar_imagen("tex_f35_emission.png", emissive)
    guardar_imagen("tex_f35_normal.png", calcular_normal_map(np.array(height)))


# ==============================================================================
# 3. B-2 SPIRIT (ADVANCED STEALTH FLYING WING BOMBER)
# Carbón obsidiana, ventanas cuádruples, conductos S-Duct, tiras cian de borde
# ==============================================================================
def generar_b2_spirit():
    base = Image.new("RGBA", (N, N), (30, 35, 44, 255)) # Carbón compuesto obsidiana
    draw = ImageDraw.Draw(base)
    height = Image.new("L", (N, N), 128)
    draw_h = ImageDraw.Draw(height)
    emissive = Image.new("RGBA", (N, N), (0, 0, 0, 255))
    draw_e = ImageDraw.Draw(emissive)

    # Cuadrícula sutil de losetas de carbono furtivas
    for y in range(0, N, 32):
        draw.line([(0, y), (N, y)], fill=(22, 26, 34, 255), width=1)
        draw_h.line([(0, y), (N, y)], fill=112, width=1)
    for x in range(0, N, 32):
        draw.line([(x, 0), (x, N)], fill=(22, 26, 34, 255), width=1)
        draw_h.line([(x, 0), (x, N)], fill=112, width=1)

    # Tiras luminosas cian en el borde de ataque del ala volante
    tira_izq = [(512, 180), (120, 760)]
    tira_der = [(512, 180), (904, 760)]
    draw.line(tira_izq, fill=(0, 225, 255, 255), width=3)
    draw.line(tira_der, fill=(0, 225, 255, 255), width=3)
    draw_e.line(tira_izq, fill=(0, 225, 255, 255), width=4)
    draw_e.line(tira_der, fill=(0, 225, 255, 255), width=4)
    draw_h.line(tira_izq, fill=150, width=3)
    draw_h.line(tira_der, fill=150, width=3)

    # Cabina con ventanas cuádruples facetadas
    for wx, wy in [(486, 260), (502, 260), (518, 260), (534, 260)]:
        draw.rectangle([wx - 6, wy - 10, wx + 6, wy + 10], fill=(16, 20, 28, 255), outline=(70, 80, 95, 255), width=2)
        draw_h.rectangle([wx - 6, wy - 10, wx + 6, wy + 10], fill=160)

    # Tomas S-Duct festoneadas empotradas dorsalmente
    for sx in [410, 614]:
        draw.polygon([(sx - 25, 340), (sx + 25, 340), (sx + 15, 450), (sx - 15, 450)], fill=(18, 21, 28, 255))
        draw_h.polygon([(sx - 25, 340), (sx + 25, 340), (sx + 15, 450), (sx - 15, 450)], fill=75)

    # Borde de salida en W doble de sierra
    dientes_w = [(120, 760), (220, 840), (320, 760), (420, 840), (512, 780), (604, 840), (704, 760), (804, 840), (904, 760)]
    draw.line(dientes_w, fill=(18, 21, 28, 255), width=4)
    draw_h.line(dientes_w, fill=90, width=4)

    guardar_imagen("tex_b2_obsidian.png", base)
    guardar_imagen("tex_b2_emission.png", emissive)
    guardar_imagen("tex_b2_normal.png", calcular_normal_map(np.array(height)))


# ==============================================================================
# 4. F-22 RAPTOR (AIR SUPERIORITY STEALTH FIGHTER)
# Camuflaje táctico bitono plateado/pizarra, toberas 2D vectoriales, cabina ámbar
# ==============================================================================
def generar_f22_raptor():
    base = Image.new("RGBA", (N, N), (115, 128, 144, 255)) # Gris fantasma claro
    draw = ImageDraw.Draw(base)
    height = Image.new("L", (N, N), 128)
    draw_h = ImageDraw.Draw(height)
    emissive = Image.new("RGBA", (N, N), (0, 0, 0, 255))
    draw_e = ImageDraw.Draw(emissive)

    # Manchas orgánicas de camuflaje bitono gris pizarra
    camo_col = (68, 78, 92, 255)
    manchas = [
        [(450, 220), (574, 220), (590, 420), (434, 420)],
        [(280, 520), (460, 480), (420, 680), (240, 700)],
        [(564, 480), (744, 520), (784, 700), (604, 680)],
        [(360, 720), (664, 720), (640, 880), (384, 880)],
    ]
    for m in manchas:
        draw.polygon(m, fill=camo_col)
        draw_h.polygon(m, fill=135)

    # Paneles en diamante delta y bahías de armamento internas
    draw.rectangle([460, 480, 564, 740], outline=(38, 44, 54, 255), width=2)
    draw_h.rectangle([460, 480, 564, 740], fill=105)

    # Cúpula ámbar reflectante
    canopy = [(480, 180), (544, 180), (540, 320), (484, 320)]
    draw.polygon(canopy, fill=(215, 165, 45, 240))
    draw_h.polygon(canopy, fill=185)

    # Toberas vectoriales 2D rectangulares (Pratt & Whitney F119)
    for bx in [438, 550]:
        draw.rectangle([bx, 880, bx + 36, 940], fill=(32, 36, 44, 255), outline=(130, 140, 155, 255), width=3)
        draw_h.rectangle([bx, 880, bx + 36, 940], fill=170)
        draw_e.rectangle([bx + 4, 890, bx + 32, 930], fill=(255, 130, 20, 255))
        draw_e.rectangle([bx + 10, 900, bx + 26, 920], fill=(120, 220, 255, 255))

    guardar_imagen("tex_f22_camo.png", base)
    guardar_imagen("tex_f22_emission.png", emissive)
    guardar_imagen("tex_f22_normal.png", calcular_normal_map(np.array(height)))


# ==============================================================================
# 5. HALCÓN CARMESÍ (STEALTH DELTA INTERCEPTOR - ENEMIGO LIGERO)
# Obsidiana carbón, canales de energía carmesí puro, lanzas láser alares
# ==============================================================================
def generar_halcon_carmesi():
    base = Image.new("RGBA", (N, N), (24, 26, 32, 255)) # Obsidiana oscura
    draw = ImageDraw.Draw(base)
    height = Image.new("L", (N, N), 128)
    draw_h = ImageDraw.Draw(height)
    emissive = Image.new("RGBA", (N, N), (0, 0, 0, 255))
    draw_e = ImageDraw.Draw(emissive)

    # Canales de energía carmesí luminosos (Glowing Crimson Energy Conduits)
    red_lines = [
        [(512, 100), (512, 880)],            # Espina central
        [(512, 340), (280, 540), (140, 780)], # Borde alar izquierdo
        [(512, 340), (744, 540), (884, 780)], # Borde alar derecho
        [(460, 480), (340, 680)],            # Nervadura izquierda
        [(564, 480), (684, 680)],            # Nervadura derecha
    ]
    for r_line in red_lines:
        draw.line(r_line, fill=(230, 20, 40, 255), width=4)
        draw_e.line(r_line, fill=(255, 20, 40, 255), width=5)
        draw_h.line(r_line, fill=150, width=3)

    # Visor de cabina carmesí
    visor = [(484, 220), (540, 220), (536, 330), (488, 330)]
    draw.polygon(visor, fill=(220, 10, 30, 240))
    draw_e.polygon(visor, fill=(255, 20, 40, 255))
    draw_h.polygon(visor, fill=175)

    # Lanzas láser alares (agujas frontales)
    for lx in [130, 894]:
        draw.rectangle([lx - 4, 380, lx + 4, 780], fill=(45, 48, 56, 255))
        draw_e.rectangle([lx - 2, 360, lx + 2, 440], fill=(255, 30, 50, 255))
        draw_h.rectangle([lx - 4, 380, lx + 4, 780], fill=165)

    # Toberas de plasma carmesí
    for tx in [460, 564]:
        draw.ellipse([tx - 25, 850, tx + 25, 930], fill=(30, 32, 38, 255), outline=(220, 20, 40, 255), width=3)
        draw_e.ellipse([tx - 18, 862, tx + 18, 918], fill=(255, 30, 50, 255))

    guardar_imagen("tex_halcon_hull.png", base)
    guardar_imagen("tex_halcon_emission.png", emissive)
    guardar_imagen("tex_halcon_normal.png", calcular_normal_map(np.array(height)))


# ==============================================================================
# 6. VÍBORA IMPERIAL (STRIKE FIGHTER - ENEMIGO ATAQUE)
# Blindaje púrpura imperial metálico, ribetes ámbar/oro, cañones rotativos alares
# ==============================================================================
def generar_vibora_imperial():
    base = Image.new("RGBA", (N, N), (62, 42, 92, 255)) # Púrpura imperial
    draw = ImageDraw.Draw(base)
    height = Image.new("L", (N, N), 128)
    draw_h = ImageDraw.Draw(height)
    emissive = Image.new("RGBA", (N, N), (0, 0, 0, 255))
    draw_e = ImageDraw.Draw(emissive)

    # Ribetes dorados/ámbar incandescentes (Golden Amber Trim)
    amber_lines = [
        [(512, 140), (450, 280), (450, 780)], # Contorno babor
        [(512, 140), (574, 280), (574, 780)], # Contorno estribor
        [(450, 440), (220, 360), (100, 420)], # Borde de ala en flecha invertida izq
        [(574, 440), (804, 360), (924, 420)], # Borde de ala en flecha invertida der
    ]
    for a_line in amber_lines:
        draw.line(a_line, fill=(255, 175, 20, 255), width=4)
        draw_e.line(a_line, fill=(255, 160, 10, 255), width=4)
        draw_h.line(a_line, fill=155, width=3)

    # Visor rojo sangre
    draw.polygon([(480, 240), (544, 240), (538, 350), (486, 350)], fill=(180, 10, 25, 240))
    draw_e.polygon([(484, 245), (540, 245), (534, 345), (490, 345)], fill=(240, 15, 30, 255))

    # Cañones rotativos dobles en raíz alar (rotary gatling cannons)
    for gx in [340, 684]:
        draw.rectangle([gx - 12, 280, gx + 12, 440], fill=(30, 28, 35, 255), outline=(255, 175, 20, 255), width=2)
        draw_h.rectangle([gx - 12, 280, gx + 12, 440], fill=175)

    guardar_imagen("tex_vibora_hull.png", base)
    guardar_imagen("tex_vibora_emission.png", emissive)
    guardar_imagen("tex_vibora_normal.png", calcular_normal_map(np.array(height)))


# ==============================================================================
# 7. GÓLEM PESADO (FLYING FORTRESS GUNSHIP - ENEMIGO PESADO)
# Catamarán bimotor acorazado, cañones frontales pesados cuádruples, celdas misiles
# ==============================================================================
def generar_golem_pesado():
    base = Image.new("RGBA", (N, N), (48, 54, 68, 255)) # Blindaje gunmetal
    draw = ImageDraw.Draw(base)
    height = Image.new("L", (N, N), 128)
    draw_h = ImageDraw.Draw(height)
    emissive = Image.new("RGBA", (N, N), (0, 0, 0, 255))
    draw_e = ImageDraw.Draw(emissive)

    # Góndolas de casco catamarán lateral (izq y der)
    for gx in [260, 764]:
        draw.rectangle([gx - 80, 160, gx + 80, 860], fill=(40, 46, 58, 255), outline=(24, 28, 36, 255), width=4)
        draw_h.rectangle([gx - 80, 160, gx + 80, 860], fill=160)

        # Celdas honeycomb lanzamisiles laterales
        for r in range(4):
            for c in range(3):
                cx = gx - 45 + c * 40
                cy = 620 + r * 35
                draw.rectangle([cx - 10, cy - 10, cx + 10, cy + 10], fill=(20, 22, 28, 255))
                draw_e.ellipse([cx - 5, cy - 5, cx + 5, cy + 5], fill=(255, 40, 120, 255))

        # Cuatro cañones rotativos frontales pesados
        draw.rectangle([gx - 20, 80, gx + 20, 220], fill=(25, 28, 34, 255), outline=(80, 90, 110, 255), width=3)
        draw_h.rectangle([gx - 20, 80, gx + 20, 220], fill=180)

    # Puente blindado central con ranuras de visión magenta
    draw.polygon([(460, 240), (564, 240), (580, 480), (444, 480)], fill=(32, 38, 48, 255))
    draw_h.polygon([(460, 240), (564, 240), (580, 480), (444, 480)], fill=170)
    for my in [280, 340]:
        draw.rectangle([480, my, 544, my + 8], fill=(255, 0, 128, 255))
        draw_e.rectangle([480, my, 544, my + 8], fill=(255, 0, 128, 255))

    guardar_imagen("tex_golem_hull.png", base)
    guardar_imagen("tex_golem_emission.png", emissive)
    guardar_imagen("tex_golem_normal.png", calcular_normal_map(np.array(height)))


# ==============================================================================
# 8. VALKIRIA VOLCÁNICA X-9 (COLOSSAL TRIMARAN BOSS DREADNOUGHT)
# Trimarán colosal, núcleo de reactor de magma ardiente, 4 torretas de artillería, 6 motores
# ==============================================================================
def generar_valkiria_volcanica():
    base = Image.new("RGBA", (N, N), (36, 40, 50, 255)) # Acero volcánico
    draw = ImageDraw.Draw(base)
    height = Image.new("L", (N, N), 128)
    draw_h = ImageDraw.Draw(height)
    emissive = Image.new("RGBA", (N, N), (0, 0, 0, 255))
    draw_e = ImageDraw.Draw(emissive)

    # 1. Pontones laterales del trimarán
    for px in [190, 834]:
        draw.polygon([(px - 60, 260), (px + 60, 260), (px + 80, 880), (px - 80, 880)], fill=(30, 34, 42, 255), outline=(18, 20, 26, 255), width=5)
        draw_h.polygon([(px - 60, 260), (px + 60, 260), (px + 80, 880), (px - 80, 880)], fill=155)

    # 2. Celosías de interconexión alar reforzadas
    draw.polygon([(250, 460), (774, 460), (750, 560), (270, 560)], fill=(45, 50, 62, 255))
    draw_h.polygon([(250, 460), (774, 460), (750, 560), (270, 560)], fill=145)

    # 3. Barbetas de las 4 torretas de artillería
    for tx, ty in [(512, 310), (512, 690), (190, 480), (834, 480)]:
        draw.ellipse([tx - 40, ty - 40, tx + 40, ty + 40], fill=(25, 28, 34, 255), outline=(218, 175, 45, 255), width=3)
        draw_h.ellipse([tx - 40, ty - 40, tx + 40, ty + 40], fill=180)
        # Cañones dobles
        draw.rectangle([tx - 12, ty - 70, tx - 4, ty], fill=(55, 60, 72, 255))
        draw.rectangle([tx + 4, ty - 70, tx + 12, ty], fill=(55, 60, 72, 255))
        draw_e.rectangle([tx - 12, ty - 75, tx - 4, ty - 65], fill=(255, 140, 20, 255))
        draw_e.rectangle([tx + 4, ty - 75, tx + 12, ty - 65], fill=(255, 140, 20, 255))

    # 4. Núcleo de reactor de magma ardiente central (Magma Core con anillos de contención de oro)
    # Anillos de oro
    draw.ellipse([512 - 75, 490 - 75, 512 + 75, 490 + 75], fill=(220, 160, 20, 255), outline=(255, 215, 0, 255), width=6)
    draw_h.ellipse([512 - 75, 490 - 75, 512 + 75, 490 + 75], fill=195)
    # Núcleo de lava ardiente
    draw.ellipse([512 - 50, 490 - 50, 512 + 50, 490 + 50], fill=(255, 60, 0, 255))
    draw_e.ellipse([512 - 55, 490 - 55, 512 + 55, 490 + 55], fill=(255, 80, 0, 255))
    draw_e.ellipse([512 - 30, 490 - 30, 512 + 30, 490 + 30], fill=(255, 220, 80, 255))

    # 5. Seis motores cohete de propulsión trasera (6 Thrusters)
    for ex in [170, 210, 470, 554, 814, 854]:
        draw.rectangle([ex - 15, 880, ex + 15, 940], fill=(28, 30, 36, 255), outline=(180, 90, 20, 255), width=3)
        draw_h.rectangle([ex - 15, 880, ex + 15, 940], fill=170)
        draw_e.ellipse([ex - 10, 890, ex + 10, 935], fill=(255, 120, 0, 255))
        draw_e.ellipse([ex - 5, 900, ex + 5, 925], fill=(255, 240, 120, 255))

    guardar_imagen("tex_valkiria_hull.png", base)
    guardar_imagen("tex_valkiria_emission.png", emissive)
    guardar_imagen("tex_valkiria_normal.png", calcular_normal_map(np.array(height)))


# ==============================================================================
# 9. ASTEROIDE VOLCÁNICO (VOLCANIC ASTEROID GLB TEXTURE)
# Roca basáltica oscura, fracturas y venas incandescentes de lava
# ==============================================================================
def generar_asteroide_volcanico():
    base = Image.new("RGBA", (N, N), (45, 40, 36, 255)) # Basalto oscuro
    draw = ImageDraw.Draw(base)
    height = Image.new("L", (N, N), 128)
    draw_h = ImageDraw.Draw(height)
    emissive = Image.new("RGBA", (N, N), (0, 0, 0, 255))
    draw_e = ImageDraw.Draw(emissive)

    # Textura rocosa con ruido por facetas
    np.random.seed(42)
    noise = (np.random.rand(N, N) * 45).astype(np.uint8)
    h_arr = np.clip(128 + (np.random.rand(N, N) * 60 - 30), 0, 255).astype(np.uint8)
    height = Image.fromarray(h_arr, "L")
    draw_h = ImageDraw.Draw(height)

    # Venas volcánicas incandescentes
    for i in range(16):
        pts = []
        cur_x = int(np.random.rand() * N)
        cur_y = 0
        for _ in range(12):
            pts.append((cur_x, cur_y))
            cur_x += int((np.random.rand() - 0.5) * 120)
            cur_y += N // 11
        draw.line(pts, fill=(255, 80, 10, 255), width=4)
        draw_e.line(pts, fill=(255, 90, 10, 255), width=5)
        draw_h.line(pts, fill=180, width=4)

    guardar_imagen("tex_asteroide_volcanico.png", base)
    guardar_imagen("tex_asteroide_emission.png", emissive)
    guardar_imagen("tex_asteroide_normal.png", calcular_normal_map(np.array(height), strength=3.0))


if __name__ == "__main__":
    print("Iniciando generación de texturas PBR Hi-Res...")
    generar_condor_x()
    generar_f35_lightning()
    generar_b2_spirit()
    generar_f22_raptor()
    generar_halcon_carmesi()
    generar_vibora_imperial()
    generar_golem_pesado()
    generar_valkiria_volcanica()
    generar_asteroide_volcanico()
    print("¡Todas las texturas PBR maestras 1024x1024 fueron generadas exitosamente!")
