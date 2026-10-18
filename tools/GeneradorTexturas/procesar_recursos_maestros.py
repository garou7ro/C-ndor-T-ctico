# ==============================================================================
# PROYECTO: CÓNDOR TÁCTICO 3D - PIPELINE DE RECURSOS MAESTROS Y MAPAS FOTORREALISTAS
# Procesa las imágenes satelitales y renders maestros con matizado alfa subpíxel,
# desvanecimiento de halo, mapas de relieve bump y texturas de bioma de alta definición.
# ==============================================================================

import os
import shutil
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageOps, ImageEnhance

BRAIN_DIR = r"C:\Users\kreiv\.gemini\antigravity\brain\e3dfc0d3-6b4d-4e81-844b-2e7afcb48a5a"
PROJECT_ROOT = r"C:\Users\kreiv\OneDrive\Documentos\JuegoAviones"
ASSETS_RENDERS = os.path.join(PROJECT_ROOT, "assets", "renders")
ASSETS_SPRITES = os.path.join(PROJECT_ROOT, "assets", "sprites")
WWWROOT_TEXTURAS = os.path.join(PROJECT_ROOT, "CondorTactico", "wwwroot", "assets", "texturas")
WWWROOT_SPRITES = os.path.join(PROJECT_ROOT, "CondorTactico", "wwwroot", "assets", "sprites")
WWWROOT_REFERENCIAS = os.path.join(PROJECT_ROOT, "CondorTactico", "wwwroot", "assets", "referencias")

for d in [ASSETS_RENDERS, ASSETS_SPRITES, WWWROOT_TEXTURAS, WWWROOT_SPRITES]:
    os.makedirs(d, exist_ok=True)

def matting_subpixel(input_path, threshold=22, blur_radius=1.2, padding=12):
    """
    Extrae el objeto sobre fondo negro con anti-aliasing suave y eliminación de halo oscuro (defringing).
    """
    img = Image.open(input_path).convert('RGB')
    w, h = img.size
    gray = img.convert('L')
    
    # Mascara binaria inicial de pixeles negros del borde
    mask = gray.point(lambda p: 255 if p < threshold else 0, mode='L')
    
    # Inundacion (floodfill) desde el marco exterior para aislar solo el fondo externo
    # y preservar cabinas, toberas y paneles interiores oscuros
    step_x = max(1, w // 16)
    step_y = max(1, h // 16)
    for x in range(0, w, step_x):
        ImageDraw.floodfill(mask, (x, 0), 128, thresh=0)
        ImageDraw.floodfill(mask, (x, h - 1), 128, thresh=0)
    for y in range(0, h, step_y):
        ImageDraw.floodfill(mask, (0, y), 128, thresh=0)
        ImageDraw.floodfill(mask, (w - 1, y), 128, thresh=0)
        
    mask_arr = np.array(mask)
    is_ext_bg = (mask_arr == 128)
    
    # Mascara del primer plano (nave/objeto)
    fg_mask = Image.fromarray((~is_ext_bg).astype(np.uint8) * 255, mode='L')
    fg_smooth = fg_mask.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    alpha_arr = np.array(fg_smooth, dtype=np.float32) / 255.0
    
    # Unmultiply / defringe: quita el halo oscuro restaurando el brillo original del borde
    rgb_arr = np.array(img, dtype=np.float32)
    safe_alpha = np.maximum(alpha_arr, 0.08)[:, :, np.newaxis]
    defringed_rgb = np.clip(rgb_arr / safe_alpha, 0, 255)
    
    rgba = np.zeros((h, w, 4), dtype=np.uint8)
    rgba[:, :, :3] = defringed_rgb.astype(np.uint8)
    rgba[:, :, 3] = (alpha_arr * 255.0).astype(np.uint8)
    
    res = Image.fromarray(rgba, mode='RGBA')
    bbox = res.getbbox()
    if bbox:
        # Añadir padding sutil
        x0 = max(0, bbox[0] - padding)
        y0 = max(0, bbox[1] - padding)
        x1 = min(w, bbox[2] + padding)
        y1 = min(h, bbox[3] + padding)
        res = res.crop((x0, y0, x1, y1))
    return res

def generar_mapa_relieve_bump(img_rgb, invert=False):
    """
    Genera un mapa de relieve bump en escala de grises optimizado para sombreado PBR en Three.js.
    """
    gray = img_rgb.convert('L')
    # Aumentar contraste para resaltar crestas y fracturas geológicas
    enhancer = ImageEnhance.Contrast(gray)
    bump = enhancer.enhance(1.8)
    bump = bump.filter(ImageFilter.EDGE_ENHANCE_MORE)
    if invert:
        bump = ImageOps.invert(bump)
    return bump

def procesar_terrenos():
    print("\n--- 1. PROCESANDO MAPAS SATELITALES DE TERRENO ---")
    
    # 1. Andes Canyon
    andes_src = os.path.join(BRAIN_DIR, "mapa_terreno_andes_1792200426580.jpg")
    andes_dst = os.path.join(WWWROOT_TEXTURAS, "mapa_terreno_andes.jpg")
    andes_bump_dst = os.path.join(WWWROOT_TEXTURAS, "mapa_terreno_andes_bump.jpg")
    if os.path.exists(andes_src):
        with Image.open(andes_src) as img:
            # Redimensionar a tamaño uniforme amigable con WebGL (1024x2048 para desplazamiento longitudinal)
            img_andes = img.resize((1024, 2048), Image.Resampling.LANCZOS)
            img_andes.save(andes_dst, quality=94)
            bump = generar_mapa_relieve_bump(img_andes)
            bump.save(andes_bump_dst, quality=90)
            print(f"[OK] Mapa Terreno Andes y Bump guardados ({img_andes.size})")

    # 2. Volcán Caldera
    volcan_src = os.path.join(BRAIN_DIR, "mapa_terreno_volcan_1792200448705.jpg")
    volcan_dst = os.path.join(WWWROOT_TEXTURAS, "mapa_terreno_volcan.jpg")
    volcan_bump_dst = os.path.join(WWWROOT_TEXTURAS, "mapa_terreno_volcan_bump.jpg")
    if os.path.exists(volcan_src):
        with Image.open(volcan_src) as img:
            img_volcan = img.resize((1024, 2048), Image.Resampling.LANCZOS)
            img_volcan.save(volcan_dst, quality=94)
            bump_v = generar_mapa_relieve_bump(img_volcan)
            bump_v.save(volcan_bump_dst, quality=90)
            print(f"[OK] Mapa Terreno Volcán y Bump guardados ({img_volcan.size})")

    # 3. Glaciar Ártico (Derivado fotorealista con gradación glacial profunda)
    if os.path.exists(andes_src):
        with Image.open(andes_src) as img:
            img_base = img.resize((1024, 2048), Image.Resampling.LANCZOS)
            arr = np.array(img_base, dtype=np.float32)
            # Modulación cromática hacia hielo azul glacial y nieve perpetua
            luma = 0.299 * arr[:,:,0] + 0.587 * arr[:,:,1] + 0.114 * arr[:,:,2]
            glaciar = np.zeros_like(arr)
            # Blancos nevados en crestas altas, turquesa helado en valles
            glaciar[:,:,0] = np.clip(luma * 0.72 + 25, 0, 255)  # R
            glaciar[:,:,1] = np.clip(luma * 0.94 + 55, 0, 255)  # G
            glaciar[:,:,2] = np.clip(luma * 1.15 + 85, 0, 255)  # B fuerte azul glacial
            
            img_glaciar = Image.fromarray(glaciar.astype(np.uint8), mode='RGB')
            # Acentuar brillo de hielo
            enh = ImageEnhance.Contrast(img_glaciar)
            img_glaciar = enh.enhance(1.35)
            
            glaciar_dst = os.path.join(WWWROOT_TEXTURAS, "mapa_terreno_glaciar.jpg")
            glaciar_bump_dst = os.path.join(WWWROOT_TEXTURAS, "mapa_terreno_glaciar_bump.jpg")
            img_glaciar.save(glaciar_dst, quality=94)
            bump_g = generar_mapa_relieve_bump(img_glaciar)
            bump_g.save(glaciar_bump_dst, quality=90)
            print(f"[OK] Mapa Terreno Glaciar y Bump guardados ({img_glaciar.size})")

    # 4. Textura de Sombras de Nubes Atmosféricas Scrolling
    sombra_nubes = Image.new('RGBA', (1024, 1024), (0, 0, 0, 0))
    draw_s = ImageDraw.Draw(sombra_nubes)
    np.random.seed(42)
    for _ in range(35):
        cx = np.random.randint(50, 974)
        cy = np.random.randint(50, 974)
        rx = np.random.randint(60, 160)
        ry = int(rx * (0.4 + np.random.rand() * 0.4))
        opacity = np.random.randint(70, 140)
        draw_s.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=(10, 15, 25, opacity))
    sombra_nubes = sombra_nubes.filter(ImageFilter.GaussianBlur(radius=32))
    sombra_dst = os.path.join(WWWROOT_TEXTURAS, "textura_sombra_nubes.png")
    sombra_nubes.save(sombra_dst)
    print(f"[OK] Textura Sombra de Nubes guardada ({sombra_nubes.size})")

def procesar_aeronaves_y_recursos():
    print("\n--- 2. PROCESANDO AERONAVES, ENEMIGOS, ITEMS Y RECURSOS ---")

    items = [
        # Clave, Archivo fuente, umbral, blur, Destinos en assets/renders, Destinos en wwwroot
        (
            "NAVE_CONDOR_X",
            os.path.join(BRAIN_DIR, "v2_condor_x_1792200445231.jpg"),
            22, 1.2,
            ["Condor_X_Render_UltraHD.png", "Condor_X_Render_Game.png", "Condor_X_Render_3D_Top.png"],
            ["Condor_X_HD.png"],
            "textura_condor_x.png"
        ),
        (
            "NAVE_F35",
            os.path.join(BRAIN_DIR, "v2_f35_lightning_png_1792200450957.jpg"),
            22, 1.2,
            ["F35_Render_UltraHD.png", "F35_Render_Game.png", "F35_Render_3D_Top.png", "F35_Render_Rival.png"],
            ["F35_HD.png"],
            "textura_f35.png"
        ),
        (
            "NAVE_B2",
            os.path.join(BRAIN_DIR, "v2_b2_spirit_png_1792200473916.jpg"),
            22, 1.2,
            ["B2_Spirit_Render_UltraHD.png", "B2_Spirit_Render_Game.png", "B2_Spirit_Render_3D_Top.png"],
            ["B2_HD.png"],
            "textura_camuflaje_b2.png"
        ),
        (
            "NAVE_F22",
            os.path.join(WWWROOT_REFERENCIAS, "f22_raptor_concept_1792156600455.jpg"),
            20, 1.2,
            ["F22_Raptor_Render_UltraHD.png", "F22_Raptor_Render_3D_Top.png"],
            ["F22_HD.png"],
            "textura_camuflaje_f22.png"
        ),
        (
            "NAVE_ENEMIGO_LIGERO",
            os.path.join(BRAIN_DIR, "v2_enemigo_ligero_1792200471793.jpg"),
            22, 1.2,
            ["Enemigo_Ligero_Render_UltraHD.png", "Enemigo_Ligero_Render_3D_Top.png"],
            ["Enemigo_Ligero_HD.png"],
            "textura_enemigo_ligero.png"
        ),
        (
            "NAVE_ENEMIGO_ATAQUE",
            os.path.join(BRAIN_DIR, "enemigo_ataque_concept_1792198104714.jpg"),
            20, 1.2,
            ["Enemigo_Ataque_Render_UltraHD.png", "Enemigo_Ataque_Render_3D_Top.png"],
            ["Enemigo_Ataque_HD.png"],
            "textura_enemigo_ataque.png"
        ),
        (
            "NAVE_ENEMIGO_PESADO",
            os.path.join(BRAIN_DIR, "enemigo_pesado_concept_1792198124654.jpg"),
            20, 1.2,
            ["Enemigo_Pesado_Render_UltraHD.png", "Enemigo_Pesado_Render_3D_Top.png"],
            ["Enemigo_Pesado_HD.png"],
            "textura_enemigo_pesado.png"
        ),
        (
            "NAVE_JEFE_VALKIRIA",
            os.path.join(WWWROOT_REFERENCIAS, "valkiria_volcanica_boss_concept_1792156671867.jpg"),
            20, 1.2,
            ["Valkiria_Boss_Render_UltraHD.png", "Valkiria_Boss_Render_3D_Top.png"],
            ["Valkiria_Boss_HD.png"],
            "textura_valkiria_boss.png"
        ),
        (
            "ITEM_ESCUDO",
            os.path.join(BRAIN_DIR, "item_escudo_concept_png_1792198175274.jpg"),
            20, 1.2,
            ["Item_Escudo_Render_UltraHD.png", "Item_Escudo_Render_3D_Top.png"],
            ["Item_Escudo_HD.png"],
            "textura_item_escudo.png"
        ),
        (
            "ITEM_REPARACION",
            os.path.join(BRAIN_DIR, "item_reparacion_concept_png_1792198192354.jpg"),
            20, 1.2,
            ["Item_Reparacion_Render_UltraHD.png", "Item_Reparacion_Render_3D_Top.png"],
            ["Item_Reparacion_HD.png"],
            "textura_item_reparacion.png"
        ),
        (
            "ASTEROIDE_VOLCANICO",
            os.path.join(WWWROOT_REFERENCIAS, "solid_asteroid_meteorite_concept_1792156645213.jpg"),
            20, 1.2,
            ["Asteroide_Volcanico_Render_UltraHD.png", "Asteroide_Volcanico_Render_3D_Top.png"],
            ["Asteroide_HD.png"],
            "textura_meteoro_roca.png"
        )
    ]

    for item in items:
        clave, src_path, thresh, blur, dst_renders, dst_sprites, tex_nombre = item
        if not os.path.exists(src_path):
            print(f"[WARN] Archivo no encontrado: {src_path}")
            continue

        sprite_rgba = matting_subpixel(src_path, threshold=thresh, blur_radius=blur)
        w, h = sprite_rgba.size
        print(f"[PROCESADO] {clave} ({w}x{h}) desde {os.path.basename(src_path)}")

        # Guardar en assets/renders
        for r_name in dst_renders:
            out_p = os.path.join(ASSETS_RENDERS, r_name)
            sprite_rgba.save(out_p)

        # Guardar en CondorTactico/wwwroot/assets/sprites
        for s_name in dst_sprites:
            out_s = os.path.join(WWWROOT_SPRITES, s_name)
            sprite_rgba.save(out_s)

        # Guardar en CondorTactico/wwwroot/assets/texturas (textura cuadrada 1024x1024 para mapeo 3D PBR)
        tex_path = os.path.join(WWWROOT_TEXTURAS, tex_nombre)
        # Crear textura cuadrada con padding centrado
        max_dim = max(w, h)
        cuadrada = Image.new('RGBA', (max_dim, max_dim), (0, 0, 0, 0))
        cuadrada.paste(sprite_rgba, ((max_dim - w) // 2, (max_dim - h) // 2))
        cuadrada_1024 = cuadrada.resize((1024, 1024), Image.Resampling.LANCZOS)
        cuadrada_1024.save(tex_path)

        # Crear mapa normal aproximado a partir de la textura
        norm_nombre = tex_nombre.replace(".png", "_normal.png")
        gray_tex = cuadrada_1024.convert('L')
        bump_tex = gray_tex.filter(ImageFilter.EDGE_ENHANCE_MORE)
        bump_path = os.path.join(WWWROOT_TEXTURAS, norm_nombre)
        bump_tex.save(bump_path)

    print("\n✅ ¡Todos los recursos maestros y mapas de terreno procesados con éxito!")

if __name__ == "__main__":
    procesar_terrenos()
    procesar_aeronaves_y_recursos()
