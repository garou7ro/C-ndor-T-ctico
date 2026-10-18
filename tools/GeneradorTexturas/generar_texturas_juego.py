"""
=============================================================================
GENERADOR PROCEDIMENTAL DE TEXTURAS Y MAPAS DE NORMALES (NumPy + Pillow)
CÓNDOR TÁCTICO 3D - ASSETS DE ALTA FIDELIDAD
Genera:
1. Textura de roca basáltica con fisuras de magma incandescente para el meteorito.
2. Mapa de normales tangenciales (RGB) para relieve rocoso 3D hiperrealista.
3. Texturas de camuflaje stealth militar para F-22, F-35 y B-2 Spirit.
4. Sprites de partículas suaves (humo volumétrico y chispas de plasma) para explosiones.
=============================================================================
"""

import os
import math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

def asegurar_directorio(ruta):
    os.makedirs(ruta, exist_ok=True)

def generar_textura_y_normales_meteoro(directorio_salida):
    print("-> Generando textura y mapa de normales para el Meteorito 3D...")
    tamano = 1024
    
    # 1. Base fractal multi-octava usando combinaciones armónicas
    np.random.seed(42)
    x = np.linspace(0, 8 * np.pi, tamano)
    y = np.linspace(0, 8 * np.pi, tamano)
    xv, yv = np.meshgrid(x, y)
    
    # Ruido armónico compuesto
    ruido1 = np.sin(xv * 1.5 + np.cos(yv * 1.2)) * 0.5 + 0.5
    ruido2 = np.sin(xv * 4.2 - yv * 3.8) * 0.25 + 0.25
    ruido3 = np.sin(xv * 12.0 + yv * 14.0) * 0.125 + 0.125
    ruido_fino = np.random.uniform(0.0, 0.12, (tamano, tamano))
    
    altura = ruido1 * 0.5 + ruido2 * 0.3 + ruido3 * 0.15 + ruido_fino
    altura = (altura - altura.min()) / (altura.max() - altura.min())
    
    # 2. Generación de Cráteres de impacto realistas
    num_crateres = 28
    for _ in range(num_crateres):
        cx = np.random.randint(50, tamano - 50)
        cy = np.random.randint(50, tamano - 50)
        radio = np.random.randint(25, 95)
        profundidad = np.random.uniform(0.25, 0.55)
        
        # Coordenadas locales del cráter
        y_min = max(0, cy - radio * 2)
        y_max = min(tamano, cy + radio * 2)
        x_min = max(0, cx - radio * 2)
        x_max = min(tamano, cx + radio * 2)
        
        y_idx, x_idx = np.ogrid[y_min:y_max, x_min:x_max]
        dist = np.sqrt((x_idx - cx)**2 + (y_idx - cy)**2)
        
        # Perfil de cráter con reborde elevado (rim) y fondo plano/cóncavo
        mascara_interior = dist < radio
        d_norm = dist[mascara_interior] / radio
        cuenco = np.sin(d_norm * np.pi / 2) ** 2
        altura[y_min:y_max, x_min:x_max][mascara_interior] -= profundidad * (1.0 - cuenco)
        
        # Reborde exterior acumulado
        mascara_borde = (dist >= radio) & (dist < radio * 1.35)
        d_borde = (dist[mascara_borde] - radio) / (radio * 0.35)
        reborde = np.sin(d_borde * np.pi) * (profundidad * 0.45)
        altura[y_min:y_max, x_min:x_max][mascara_borde] += reborde

    altura = np.clip(altura, 0.0, 1.0)
    
    # 3. Grietas y fallas tectónicas incandescentes (Magma fissures)
    grietas = np.zeros((tamano, tamano), dtype=np.float32)
    for _ in range(16):
        px = np.random.randint(0, tamano)
        py = np.random.randint(0, tamano)
        angulo = np.random.uniform(0, 2 * np.pi)
        longitud = np.random.randint(120, 360)
        
        for step in range(longitud):
            angulo += np.random.uniform(-0.35, 0.35)
            px = int((px + np.cos(angulo) * 1.8) % tamano)
            py = int((py + np.sin(angulo) * 1.8) % tamano)
            grietas[py, px] = 1.0
            
    # Dilatación y suavizado de grietas para simular calor interno
    img_grietas = Image.fromarray((grietas * 255).astype(np.uint8))
    img_grietas = img_grietas.filter(ImageFilter.GaussianBlur(radius=2.5))
    grietas_suaves = np.array(img_grietas, dtype=np.float32) / 255.0
    
    # 4. Color Difuso PBR: Basalto oscuro, ceniza volcánica y venas de magma ardiente
    r_chan = np.clip(altura * 70 + grietas_suaves * 255, 0, 255).astype(np.uint8)
    g_chan = np.clip(altura * 55 + grietas_suaves * 90, 0, 255).astype(np.uint8)
    b_chan = np.clip(altura * 45 + grietas_suaves * 15, 0, 255).astype(np.uint8)
    
    img_difuso = Image.merge('RGB', [
        Image.fromarray(r_chan),
        Image.fromarray(g_chan),
        Image.fromarray(b_chan)
    ])
    ruta_difuso = os.path.join(directorio_salida, "textura_meteoro_roca.png")
    img_difuso.save(ruta_difuso, "PNG", optimize=True)
    print(f"  [OK] Guardado: {ruta_difuso}")
    
    # 5. Generación del Mapa de Normales Tangenciales (Sobel Derivatives)
    sobel_x = np.array([[-1, 0, 1], [-2, 0, 2], [-1, 0, 1]], dtype=np.float32)
    sobel_y = np.array([[-1, -2, -1], [0, 0, 0], [1, 2, 1]], dtype=np.float32)
    
    # Gradientes con NumPy para mapa de normales
    gx = np.zeros_like(altura)
    gy = np.zeros_like(altura)
    gx[:, 1:-1] = (altura[:, 2:] - altura[:, :-2]) * 2.5
    gy[1:-1, :] = (altura[2:, :] - altura[:-2, :]) * 2.5
    
    # Normalización del vector (nx, ny, nz)
    fuerza_normal = 1.0
    nz = np.ones_like(altura) * (1.0 / fuerza_normal)
    longitud_norm = np.sqrt(gx**2 + gy**2 + nz**2)
    
    nx = (-gx / longitud_norm) * 0.5 + 0.5
    ny = (-gy / longitud_norm) * 0.5 + 0.5
    nz = (nz / longitud_norm) * 0.5 + 0.5
    
    img_normal = Image.merge('RGB', [
        Image.fromarray((nx * 255).astype(np.uint8)),
        Image.fromarray((ny * 255).astype(np.uint8)),
        Image.fromarray((nz * 255).astype(np.uint8))
    ])
    ruta_normal = os.path.join(directorio_salida, "textura_meteoro_normal.png")
    img_normal.save(ruta_normal, "PNG", optimize=True)
    print(f"  [OK] Guardado: {ruta_normal}")

def generar_texturas_camuflaje_aeronaves(directorio_salida):
    print("-> Generando texturas de camuflaje y paneles stealth para F-22, F-35 y B-2...")
    tamano = 1024
    
    # --- F-22 Raptor: Camuflaje bi-tono de superioridad aérea y bordes RAM ---
    img_f22 = Image.new('RGB', (tamano, tamano), (75, 82, 92))
    draw_f22 = ImageDraw.Draw(img_f22)
    
    # Manchas de camuflaje oscuro en fuselaje y alas
    np.random.seed(101)
    for _ in range(12):
        bx = np.random.randint(200, 824)
        by = np.random.randint(200, 824)
        rx = np.random.randint(60, 160)
        ry = np.random.randint(80, 220)
        draw_f22.ellipse([bx - rx, by - ry, bx + rx, by + ry], fill=(52, 58, 66))
    
    img_f22 = img_f22.filter(ImageFilter.GaussianBlur(radius=18))
    draw_f22 = ImageDraw.Draw(img_f22)
    
    # Líneas de paneles stealth y bahías de armas (RAM tape)
    for x in range(150, 874, 80):
        draw_f22.line([(x, 100), (x, 924)], fill=(110, 118, 130), width=2)
    for y in range(200, 824, 90):
        draw_f22.line([(150, y), (874, y)], fill=(110, 118, 130), width=2)
        
    ruta_f22 = os.path.join(directorio_salida, "textura_camuflaje_f22.png")
    img_f22.save(ruta_f22, "PNG", optimize=True)
    print(f"  [OK] Guardado: {ruta_f22}")

    # --- B-2 Spirit: Revestimiento absorbente de radar carbón mate uniforme ---
    img_b2 = Image.new('RGB', (tamano, tamano), (28, 30, 34))
    draw_b2 = ImageDraw.Draw(img_b2)
    
    # Paneles facetados serrados en bordes
    for i in range(12):
        offset = i * 40
        draw_b2.polygon([(512, 120 + offset), (450 + offset // 2, 400 + offset), (574 - offset // 2, 400 + offset)], outline=(42, 45, 52), fill=(32, 34, 38))
        
    img_b2 = img_b2.filter(ImageFilter.GaussianBlur(radius=2))
    ruta_b2 = os.path.join(directorio_salida, "textura_camuflaje_b2.png")
    img_b2.save(ruta_b2, "PNG", optimize=True)
    print(f"  [OK] Guardado: {ruta_b2}")

def generar_sprites_particulas(directorio_salida):
    print("-> Generando sprites de partículas suaves (humo y chispas)...")
    tamano = 256
    
    # 1. Partícula de Humo Radial Suave
    img_humo = Image.new('RGBA', (tamano, tamano), (0, 0, 0, 0))
    centro = tamano / 2.0
    pixels_humo = img_humo.load()
    
    for y in range(tamano):
        for x in range(tamano):
            dist = math.hypot(x - centro, y - centro) / centro
            if dist < 1.0:
                # Curva cosenoidal suave hacia el borde para cero cortes cuadrados
                alfa = int(math.cos(dist * math.pi / 2) ** 2 * 230)
                pixels_humo[x, y] = (210, 220, 235, alfa)
                
    ruta_humo = os.path.join(directorio_salida, "particula_humo.png")
    img_humo.save(ruta_humo, "PNG")
    print(f"  [OK] Guardado: {ruta_humo}")
    
    # 2. Chispa de Plasma Incandescente
    img_chispa = Image.new('RGBA', (tamano, tamano), (0, 0, 0, 0))
    pixels_chispa = img_chispa.load()
    
    for y in range(tamano):
        for x in range(tamano):
            dist = math.hypot(x - centro, y - centro) / centro
            if dist < 1.0:
                intensidad = math.exp(-dist * 4.5)
                alfa = int(min(255, intensidad * 255))
                # Gradiente de núcleo blanco a borde cian/oro
                r = int(min(255, 180 + 75 * intensidad))
                g = int(min(255, 230 + 25 * intensidad))
                b = 255
                pixels_chispa[x, y] = (r, g, b, alfa)
                
    ruta_chispa = os.path.join(directorio_salida, "particula_chispa.png")
    img_chispa.save(ruta_chispa, "PNG")
    print(f"  [OK] Guardado: {ruta_chispa}")

if __name__ == '__main__':
    directorio = os.path.abspath(r"CondorTactico\wwwroot\assets\texturas")
    asegurar_directorio(directorio)
    generar_textura_y_normales_meteoro(directorio)
    generar_texturas_camuflaje_aeronaves(directorio)
    generar_sprites_particulas(directorio)
    print("\n¡Todas las texturas y recursos de alta fidelidad fueron generados exitosamente!")
