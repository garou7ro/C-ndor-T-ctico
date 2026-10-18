"""
=============================================================================
GENERADOR DE TEXTURAS Y MAPAS DE NORMALES PBR PROFESIONALES (NumPy + Pillow)
CÓNDOR TÁCTICO 3D - ASSETS DE MÁXIMA FIDELIDAD VISUAL
Genera texturas PBR con albedo calibrado, mapas de normales tangenciales (RGB 128, 128, 255 base)
y mapas de rugosidad/emisión para:
1. Meteorito / Asteroide Volcánico (Basalto + Fisuras de Magma)
2. Cazas Aliados (Cóndor X, F-35, B-2, F-22)
3. Cazas y Naves Enemigas (Ligero, Ataque, Pesado, Jefe Valkiria)
=============================================================================
"""

import os
import math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

WWWROOT_TEXTURAS = os.path.abspath(r"CondorTactico\wwwroot\assets\texturas")
ASSETS_TEXTURAS = os.path.abspath(r"assets\texturas")
BIN_TEXTURAS = os.path.abspath(r"tools\CapturadorRemoto\bin\Debug\net8.0-windows\wwwroot\assets\texturas")

def asegurar_carpetas():
    for c in [WWWROOT_TEXTURAS, ASSETS_TEXTURAS, BIN_TEXTURAS]:
        os.makedirs(c, exist_ok=True)

def guardar_textura_en_todas(img, nombre):
    for c in [WWWROOT_TEXTURAS, ASSETS_TEXTURAS, BIN_TEXTURAS]:
        dest = os.path.join(c, nombre)
        img.save(dest, "PNG", optimize=True)
    print(f"  [OK] Guardado: {nombre} ({img.size})")

def generar_mapa_normal_tangencial(altura, fuerza=1.8):
    """
    Convierte un mapa de altura (0.0 a 1.0) en un auténtico mapa de normales
    en espacio tangente con vector base neutral (128, 128, 255).
    """
    h, w = altura.shape
    gx = np.zeros_like(altura)
    gy = np.zeros_like(altura)
    
    # Derivadas centrales Sobel
    gx[:, 1:-1] = (altura[:, 2:] - altura[:, :-2]) * fuerza * 2.0
    gx[:, 0] = (altura[:, 1] - altura[:, 0]) * fuerza * 2.0
    gx[:, -1] = (altura[:, -1] - altura[:, -2]) * fuerza * 2.0
    
    gy[1:-1, :] = (altura[2:, :] - altura[:-2, :]) * fuerza * 2.0
    gy[0, :] = (altura[1, :] - altura[0, :]) * fuerza * 2.0
    gy[-1, :] = (altura[-1, :] - altura[-2, :]) * fuerza * 2.0
    
    nz = np.ones_like(altura)
    longitud = np.sqrt(gx**2 + gy**2 + nz**2)
    
    nx = (-gx / longitud) * 0.5 + 0.5
    ny = (-gy / longitud) * 0.5 + 0.5
    nz = (nz / longitud) * 0.5 + 0.5
    
    norm_img = Image.merge('RGB', [
        Image.fromarray((nx * 255).astype(np.uint8)),
        Image.fromarray((ny * 255).astype(np.uint8)),
        Image.fromarray((nz * 255).astype(np.uint8))
    ])
    return norm_img

# ==============================================================================
# 1. ASTEROIDE VOLCÁNICO (Basalto rocoso poroso, cráteres y fisuras de magma)
# ==============================================================================
def generar_texturas_asteroide():
    print("-> Generando texturas PBR de Asteroide Volcánico...")
    tamano = 1024
    np.random.seed(42)
    
    # Altura base fractal armónica
    x = np.linspace(0, 10 * np.pi, tamano)
    y = np.linspace(0, 10 * np.pi, tamano)
    xv, yv = np.meshgrid(x, y)
    
    r1 = np.sin(xv * 1.4 + np.cos(yv * 1.6)) * 0.5 + 0.5
    r2 = np.sin(xv * 4.2 - yv * 3.5) * 0.25 + 0.25
    r3 = np.sin(xv * 11.0 + yv * 13.0) * 0.125 + 0.125
    grano = np.random.uniform(0.0, 0.15, (tamano, tamano))
    
    altura = r1 * 0.45 + r2 * 0.3 + r3 * 0.15 + grano
    altura = (altura - altura.min()) / (altura.max() - altura.min())
    
    # Cráteres de impacto de diversos diámetros
    for _ in range(24):
        cx = np.random.randint(60, tamano - 60)
        cy = np.random.randint(60, tamano - 60)
        radio = np.random.randint(30, 110)
        prof = np.random.uniform(0.28, 0.55)
        
        y0, y1 = max(0, cy - radio * 2), min(tamano, cy + radio * 2)
        x0, x1 = max(0, cx - radio * 2), min(tamano, cx + radio * 2)
        y_idx, x_idx = np.ogrid[y0:y1, x0:x1]
        dist = np.sqrt((x_idx - cx)**2 + (y_idx - cy)**2)
        
        mask_in = dist < radio
        d_norm = dist[mask_in] / radio
        cuenco = np.sin(d_norm * np.pi / 2) ** 2
        altura[y0:y1, x0:x1][mask_in] -= prof * (1.0 - cuenco)
        
        mask_rim = (dist >= radio) & (dist < radio * 1.35)
        d_rim = (dist[mask_rim] - radio) / (radio * 0.35)
        reborde = np.sin(d_rim * np.pi) * (prof * 0.38)
        altura[y0:y1, x0:x1][mask_rim] += reborde
        
    altura = np.clip(altura, 0.0, 1.0)
    
    # Fisuras incandescentes de magma
    grietas = np.zeros((tamano, tamano), dtype=np.float32)
    for _ in range(14):
        px, py = np.random.randint(0, tamano), np.random.randint(0, tamano)
        ang = np.random.uniform(0, 2 * np.pi)
        largo = np.random.randint(100, 300)
        for _ in range(largo):
            ang += np.random.uniform(-0.35, 0.35)
            px = int((px + np.cos(ang) * 1.8) % tamano)
            py = int((py + np.sin(ang) * 1.8) % tamano)
            grietas[py, px] = 1.0
            
    img_g = Image.fromarray((grietas * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(radius=2.2))
    grietas_s = np.array(img_g, dtype=np.float32) / 255.0
    
    # Color difuso PBR: roca basáltica oscura calibrada con fisuras de lava
    r = np.clip(70 + altura * 55 + grietas_s * 185, 0, 255).astype(np.uint8)
    g = np.clip(62 + altura * 48 + grietas_s * 65, 0, 255).astype(np.uint8)
    b = np.clip(58 + altura * 42 + grietas_s * 10, 0, 255).astype(np.uint8)
    
    img_dif = Image.merge('RGB', [Image.fromarray(r), Image.fromarray(g), Image.fromarray(b)])
    guardar_textura_en_todas(img_dif, "textura_meteoro_roca.png")
    
    img_norm = generar_mapa_normal_tangencial(altura, fuerza=2.2)
    guardar_textura_en_todas(img_norm, "textura_meteoro_roca_normal.png")
    guardar_textura_en_todas(img_norm, "textura_meteoro_normal.png")

# ==============================================================================
# 2. CAZAS ALIADOS (Cóndor X, F-35, B-2, F-22)
# ==============================================================================
def generar_texturas_cazas_aliados():
    print("-> Generando texturas PBR de Cazas Aliados...")
    tamano = 1024
    
    # A. CÓNDOR DORADO X (Titanio azulado andino, ribetes dorados y paneles angulares)
    img_condor = Image.new('RGB', (tamano, tamano), (118, 132, 150))
    d = ImageDraw.Draw(img_condor)
    # Patrones de camuflaje de camuflaje poligonal
    np.random.seed(11)
    for _ in range(16):
        cx, cy = np.random.randint(150, 874), np.random.randint(150, 874)
        rx, ry = np.random.randint(80, 180), np.random.randint(60, 140)
        d.polygon([(cx, cy - ry), (cx + rx, cy), (cx, cy + ry), (cx - rx, cy)], fill=(96, 110, 128))
    img_condor = img_condor.filter(ImageFilter.GaussianBlur(radius=8))
    d = ImageDraw.Draw(img_condor)
    # Líneas de paneles stealth
    for x in range(120, 920, 75):
        d.line([(x, 50), (x, 974)], fill=(155, 170, 190), width=2)
    for y in range(140, 900, 85):
        d.line([(50, y), (974, y)], fill=(155, 170, 190), width=2)
    # Bandas doradas ceremoniales / vectoriales
    d.polygon([(480, 80), (544, 80), (530, 940), (494, 940)], fill=(235, 175, 45))
    d.line([(200, 512), (824, 512)], fill=(245, 185, 55), width=3)
    guardar_textura_en_todas(img_condor, "textura_condor_x.png")
    
    alt_condor = np.array(img_condor.convert('L'), dtype=np.float32) / 255.0
    guardar_textura_en_todas(generar_mapa_normal_tangencial(alt_condor, fuerza=1.6), "textura_condor_x_normal.png")
    
    # B. F-35 LIGHTNING II (Gris compuesto stealth, cinta RAM y panelado serrado)
    img_f35 = Image.new('RGB', (tamano, tamano), (135, 142, 152))
    d_f35 = ImageDraw.Draw(img_f35)
    for _ in range(14):
        cx, cy = np.random.randint(100, 924), np.random.randint(100, 924)
        rx, ry = np.random.randint(70, 160), np.random.randint(70, 160)
        d_f35.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=(112, 118, 128))
    img_f35 = img_f35.filter(ImageFilter.GaussianBlur(radius=12))
    d_f35 = ImageDraw.Draw(img_f35)
    # Líneas RAM tape características del F-35
    for x in range(100, 924, 65):
        d_f35.line([(x, 60), (x, 964)], fill=(168, 176, 188), width=2)
    for y in range(120, 920, 80):
        d_f35.line([(60, y), (964, y)], fill=(168, 176, 188), width=2)
    guardar_textura_en_todas(img_f35, "textura_f35.png")
    
    alt_f35 = np.array(img_f35.convert('L'), dtype=np.float32) / 255.0
    guardar_textura_en_todas(generar_mapa_normal_tangencial(alt_f35, fuerza=1.7), "textura_f35_normal.png")
    
    # C. B-2 SPIRIT (Grafito oscuro homogéneo, bordes facetados y paneles RAM)
    img_b2 = Image.new('RGB', (tamano, tamano), (92, 98, 108))
    d_b2 = ImageDraw.Draw(img_b2)
    for _ in range(10):
        cx, cy = np.random.randint(150, 874), np.random.randint(150, 874)
        rx, ry = np.random.randint(90, 200), np.random.randint(90, 200)
        d_b2.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=(78, 84, 94))
    img_b2 = img_b2.filter(ImageFilter.GaussianBlur(radius=14))
    d_b2 = ImageDraw.Draw(img_b2)
    # Paneles facetados W en borde de ataque y salida
    for x in range(120, 904, 75):
        d_b2.line([(x, 80), (x, 944)], fill=(125, 132, 144), width=2)
    for y in range(150, 874, 90):
        d_b2.line([(80, y), (944, y)], fill=(125, 132, 144), width=2)
    guardar_textura_en_todas(img_b2, "textura_camuflaje_b2.png")
    
    alt_b2 = np.array(img_b2.convert('L'), dtype=np.float32) / 255.0
    guardar_textura_en_todas(generar_mapa_normal_tangencial(alt_b2, fuerza=1.5), "textura_camuflaje_b2_normal.png")
    
    # D. F-22 RAPTOR (Camuflaje bi-tono de superioridad aérea y bahías de armas)
    img_f22 = Image.new('RGB', (tamano, tamano), (142, 150, 160))
    d_f22 = ImageDraw.Draw(img_f22)
    np.random.seed(33)
    for _ in range(15):
        bx, by = np.random.randint(150, 874), np.random.randint(150, 874)
        rx, ry = np.random.randint(70, 170), np.random.randint(90, 220)
        d_f22.ellipse([bx - rx, by - ry, bx + rx, by + ry], fill=(116, 124, 134))
    img_f22 = img_f22.filter(ImageFilter.GaussianBlur(radius=16))
    d_f22 = ImageDraw.Draw(img_f22)
    for x in range(110, 914, 70):
        d_f22.line([(x, 70), (x, 954)], fill=(182, 190, 202), width=2)
    for y in range(130, 894, 80):
        d_f22.line([(70, y), (954, y)], fill=(182, 190, 202), width=2)
    guardar_textura_en_todas(img_f22, "textura_camuflaje_f22.png")
    
    alt_f22 = np.array(img_f22.convert('L'), dtype=np.float32) / 255.0
    guardar_textura_en_todas(generar_mapa_normal_tangencial(alt_f22, fuerza=1.7), "textura_camuflaje_f22_normal.png")

# ==============================================================================
# 3. NAVES Y CAZAS ENEMIGOS (Ligero, Ataque, Pesado, Jefe Valkiria)
# ==============================================================================
def generar_texturas_enemigos():
    print("-> Generando texturas PBR de Aeronaves Enemigas...")
    tamano = 1024
    
    # A. ENEMIGO LIGERO (Dron Interceptor: Acero oscuro con franjas carmesí agresivas)
    img_ligero = Image.new('RGB', (tamano, tamano), (124, 118, 126))
    d = ImageDraw.Draw(img_ligero)
    for _ in range(12):
        cx, cy = np.random.randint(120, 904), np.random.randint(120, 904)
        rx, ry = np.random.randint(60, 150), np.random.randint(60, 150)
        d.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=(98, 92, 102))
    img_ligero = img_ligero.filter(ImageFilter.GaussianBlur(radius=10))
    d = ImageDraw.Draw(img_ligero)
    # Paneles técnicos y avisos de advertencia carmesí
    for x in range(100, 924, 70):
        d.line([(x, 60), (x, 964)], fill=(155, 148, 160), width=2)
    # Bandas de ataque carmesí / rubí
    d.polygon([(470, 60), (554, 60), (530, 964), (494, 964)], fill=(220, 30, 65))
    d.line([(180, 300), (844, 300)], fill=(220, 30, 65), width=4)
    guardar_textura_en_todas(img_ligero, "textura_enemigo_ligero.png")
    
    alt_ligero = np.array(img_ligero.convert('L'), dtype=np.float32) / 255.0
    guardar_textura_en_todas(generar_mapa_normal_tangencial(alt_ligero, fuerza=1.8), "textura_enemigo_ligero_normal.png")
    
    # B. ENEMIGO ATAQUE (Caza Víbora Flecha Invertida: Titanio con chevrones rojos)
    img_ataque = Image.new('RGB', (tamano, tamano), (130, 124, 134))
    d_atk = ImageDraw.Draw(img_ataque)
    for _ in range(14):
        cx, cy = np.random.randint(120, 904), np.random.randint(120, 904)
        rx, ry = np.random.randint(70, 160), np.random.randint(70, 160)
        d_atk.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=(104, 98, 108))
    img_ataque = img_ataque.filter(ImageFilter.GaussianBlur(radius=10))
    d_atk = ImageDraw.Draw(img_ataque)
    for x in range(90, 934, 65):
        d_atk.line([(x, 50), (x, 974)], fill=(162, 156, 168), width=2)
    for y in range(110, 914, 75):
        d_atk.line([(50, y), (974, y)], fill=(162, 156, 168), width=2)
    # Flechas dobles carmesí
    d_atk.polygon([(512, 100), (620, 240), (590, 260), (512, 160), (434, 260), (404, 240)], fill=(235, 35, 75))
    d_atk.polygon([(512, 280), (620, 420), (590, 440), (512, 340), (434, 440), (404, 420)], fill=(235, 35, 75))
    guardar_textura_en_todas(img_ataque, "textura_enemigo_ataque.png")
    
    alt_atk = np.array(img_ataque.convert('L'), dtype=np.float32) / 255.0
    guardar_textura_en_todas(generar_mapa_normal_tangencial(alt_atk, fuerza=1.9), "textura_enemigo_ataque_normal.png")
    
    # C. ENEMIGO PESADO (Gunship Blindado: Placas de blindaje remachadas)
    img_pesado = Image.new('RGB', (tamano, tamano), (138, 144, 154))
    d_pes = ImageDraw.Draw(img_pesado)
    for _ in range(16):
        cx, cy = np.random.randint(100, 924), np.random.randint(100, 924)
        rx, ry = np.random.randint(80, 180), np.random.randint(80, 180)
        d_pes.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=(110, 116, 126))
    img_pesado = img_pesado.filter(ImageFilter.GaussianBlur(radius=12))
    d_pes = ImageDraw.Draw(img_pesado)
    # Malla de placas de blindaje con remaches
    for x in range(80, 944, 80):
        d_pes.line([(x, 40), (x, 984)], fill=(175, 182, 194), width=3)
    for y in range(90, 934, 90):
        d_pes.line([(40, y), (984, y)], fill=(175, 182, 194), width=3)
    # Bandas de peligro amarillas / negras
    for i in range(6):
        d_pes.polygon([(200 + i * 50, 800), (230 + i * 50, 800), (200 + i * 50, 850), (170 + i * 50, 850)], fill=(240, 180, 30))
    guardar_textura_en_todas(img_pesado, "textura_enemigo_pesado.png")
    
    alt_pes = np.array(img_pesado.convert('L'), dtype=np.float32) / 255.0
    guardar_textura_en_todas(generar_mapa_normal_tangencial(alt_pes, fuerza=2.2), "textura_enemigo_pesado_normal.png")
    
    # D. VALKIRIA BOSS (Dreadnought Volcánico: Casco blindado masivo, conductos de plasma y reactor)
    img_boss = Image.new('RGB', (tamano, tamano), (110, 105, 116))
    d_boss = ImageDraw.Draw(img_boss)
    for _ in range(20):
        cx, cy = np.random.randint(100, 924), np.random.randint(100, 924)
        rx, ry = np.random.randint(90, 210), np.random.randint(90, 210)
        d_boss.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=(85, 80, 90))
    img_boss = img_boss.filter(ImageFilter.GaussianBlur(radius=14))
    d_boss = ImageDraw.Draw(img_boss)
    # Estructura de mamparos reforzados
    for x in range(80, 944, 70):
        d_boss.line([(x, 40), (x, 984)], fill=(155, 150, 162), width=3)
    for y in range(90, 934, 80):
        d_boss.line([(40, y), (984, y)], fill=(155, 150, 162), width=3)
    # Conductos de energía de plasma volcánico
    d_boss.line([(512, 60), (512, 964)], fill=(245, 45, 75), width=6)
    d_boss.line([(280, 250), (744, 250)], fill=(245, 45, 75), width=4)
    d_boss.line([(280, 750), (744, 750)], fill=(245, 45, 75), width=4)
    guardar_textura_en_todas(img_boss, "textura_valkiria_boss.png")
    
    alt_boss = np.array(img_boss.convert('L'), dtype=np.float32) / 255.0
    guardar_textura_en_todas(generar_mapa_normal_tangencial(alt_boss, fuerza=2.3), "textura_valkiria_boss_normal.png")

# ==============================================================================
# 4. ITEMS Y SUMINISTROS (Escudo y Reparación)
# ==============================================================================
def generar_texturas_items():
    print("-> Generando texturas PBR de Items y Suministros...")
    tamano = 512
    # Item Escudo (Cian / Esmeralda con hexágonos)
    img_esc = Image.new('RGB', (tamano, tamano), (30, 110, 140))
    d_esc = ImageDraw.Draw(img_esc)
    for y in range(30, 480, 50):
        for x in range(30, 480, 50):
            d_esc.regular_polygon((x, y, 22), 6, fill=(50, 195, 235), outline=(180, 245, 255))
    guardar_textura_en_todas(img_esc, "textura_item_escudo.png")
    alt_esc = np.array(img_esc.convert('L'), dtype=np.float32) / 255.0
    guardar_textura_en_todas(generar_mapa_normal_tangencial(alt_esc, fuerza=2.0), "textura_item_escudo_normal.png")
    
    # Item Reparación (Cruz médica verde brillante / oro)
    img_rep = Image.new('RGB', (tamano, tamano), (35, 120, 85))
    d_rep = ImageDraw.Draw(img_rep)
    d_rep.rectangle([180, 60, 332, 452], fill=(60, 235, 140))
    d_rep.rectangle([60, 180, 452, 332], fill=(60, 235, 140))
    guardar_textura_en_todas(img_rep, "textura_item_reparacion.png")
    alt_rep = np.array(img_rep.convert('L'), dtype=np.float32) / 255.0
    guardar_textura_en_todas(generar_mapa_normal_tangencial(alt_rep, fuerza=2.0), "textura_item_reparacion_normal.png")

if __name__ == '__main__':
    asegurar_carpetas()
    generar_texturas_asteroide()
    generar_texturas_cazas_aliados()
    generar_texturas_enemigos()
    generar_texturas_items()
    print("\n[EXITO] Todas las texturas PBR profesionales y mapas normales fueron generados con exito!")
