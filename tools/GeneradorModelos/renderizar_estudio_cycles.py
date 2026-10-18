# ==============================================================================
# CÓNDOR TÁCTICO 3D - ESTUDIO DE RENDER CYCLES (BLENDER 5.x, SIN INTERFAZ)
# Genera los renders oficiales a partir de los modelos GLB REALES del juego:
#   <Nombre>_Render_UltraHD.png  -> vista cenital ortográfica, proa arriba, 2048 px (sprite 2D maestro)
#   <Nombre>_Render_Game.png     -> misma vista cenital reducida a 512 px (sprite de juego)
#   <Nombre>_Render_3D_Top.png   -> plano heroico 3/4 elevado con sombra de contacto
#   F35_Render_Rival.png         -> vista cenital con pintura carmesí de escuadrón rival
#   Flota_Showcase.png           -> alineación de toda la flota del jugador
# Los GLB no se modifican: el detalle de superficie (líneas de panel, rugosidad)
# se añade solo en la escena de render.
#
# Uso:
#   blender -b --factory-startup --python renderizar_estudio_cycles.py -- [modelo ...] [--calidad final|prueba] [--salida DIR] [--sin-showcase]
# ==============================================================================

import bpy
import math
import os
import sys
import time
from mathutils import Vector

import numpy as np

RAIZ = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
DIR_MODELOS = os.path.join(RAIZ, "CondorTactico", "wwwroot", "assets", "modelos")
DIR_SALIDA = os.path.join(RAIZ, "assets", "renders")
HDRI_ESTUDIO = os.path.join(os.path.dirname(bpy.app.binary_path), f"{bpy.app.version[0]}.{bpy.app.version[1]}",
                            "datafiles", "studiolights", "world", "studio.exr")

# Archivo GLB -> (prefijo de salida, bando, genera Game, color de contorno)
CATALOGO = {
    "Condor_X":            ("Condor_X",            "jugador", True,  (0.0, 0.85, 1.0)),
    "F35_Lightning_II":    ("F35",                 "jugador", True,  (0.0, 0.85, 1.0)),
    "B2_Spirit":           ("B2_Spirit",           "jugador", True,  (0.0, 0.85, 1.0)),
    "F22_Raptor":          ("F22_Raptor",          "jugador", False, (0.0, 0.85, 1.0)),
    "Enemigo_Ligero":      ("Enemigo_Ligero",      "enemigo", False, (1.0, 0.12, 0.25)),
    "Enemigo_Ataque":      ("Enemigo_Ataque",      "enemigo", False, (1.0, 0.45, 0.05)),
    "Enemigo_Pesado":      ("Enemigo_Pesado",      "enemigo", False, (1.0, 0.15, 0.6)),
    "Valkiria_Boss":       ("Valkiria_Boss",       "enemigo", False, (1.0, 0.3, 0.05)),
    "Asteroide_Volcanico": ("Asteroide_Volcanico", "objeto",  False, (1.0, 0.4, 0.1)),
    "Item_Escudo":         ("Item_Escudo",         "objeto",  False, (0.0, 0.85, 1.0)),
    "Item_Reparacion":     ("Item_Reparacion",     "objeto",  False, (0.1, 1.0, 0.5)),
    "Misil_Tactico":       ("Misil_Tactico",       "objeto",  False, (1.0, 0.5, 0.1)),
}

FLOTA_SHOWCASE = ["B2_Spirit", "F22_Raptor", "Condor_X", "F35_Lightning_II"]


def leer_argumentos():
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    calidad = "final"
    if "--calidad" in args:
        calidad = args[args.index("--calidad") + 1]
        args = [a for a in args if a not in ("--calidad", calidad)]
    global DIR_SALIDA
    if "--salida" in args:
        DIR_SALIDA = args[args.index("--salida") + 1]
        args = [a for a in args if a not in ("--salida", DIR_SALIDA)]
    showcase = "--sin-showcase" not in args
    modelos = [a for a in args if not a.startswith("--")] or list(CATALOGO.keys())
    return modelos, calidad, showcase


# ------------------------------------------------------------------------------
# ESCENA, MOTOR Y GESTIÓN DE COLOR
# ------------------------------------------------------------------------------
def preparar_escena(calidad):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    escena = bpy.context.scene
    escena.render.engine = 'CYCLES'
    escena.cycles.device = 'CPU'
    escena.cycles.samples = 160 if calidad == "final" else 24
    escena.cycles.use_adaptive_sampling = True
    escena.cycles.adaptive_threshold = 0.015
    escena.cycles.use_denoising = True
    try:
        escena.cycles.denoiser = 'OPENIMAGEDENOISE'
    except TypeError:
        pass
    escena.cycles.max_bounces = 8
    escena.cycles.glossy_bounces = 4
    escena.cycles.transparent_max_bounces = 8
    escena.render.film_transparent = True
    escena.render.filter_size = 1.2
    escena.render.image_settings.file_format = 'PNG'
    escena.render.image_settings.color_mode = 'RGBA'
    escena.render.image_settings.color_depth = '8'
    escena.view_settings.view_transform = 'AgX'
    for look in ("AgX - Punchy", "Punchy"):
        try:
            escena.view_settings.look = look
            break
        except TypeError:
            continue
    escena.view_settings.exposure = 0.0
    return escena


def configurar_mundo(rotacion_z=0.0, intensidad=0.3):
    mundo = bpy.data.worlds.new("MundoEstudio")
    bpy.context.scene.world = mundo
    mundo.use_nodes = True
    nodos = mundo.node_tree.nodes
    enlaces = mundo.node_tree.links
    fondo = nodos["Background"]
    if os.path.exists(HDRI_ESTUDIO):
        coord = nodos.new("ShaderNodeTexCoord")
        mapeo = nodos.new("ShaderNodeMapping")
        mapeo.inputs["Rotation"].default_value[2] = rotacion_z
        env = nodos.new("ShaderNodeTexEnvironment")
        env.image = bpy.data.images.load(HDRI_ESTUDIO, check_existing=True)
        enlaces.new(coord.outputs["Generated"], mapeo.inputs["Vector"])
        enlaces.new(mapeo.outputs["Vector"], env.inputs["Vector"])
        enlaces.new(env.outputs["Color"], fondo.inputs["Color"])
    else:
        fondo.inputs["Color"].default_value = (0.08, 0.1, 0.14, 1.0)
    fondo.inputs["Strength"].default_value = intensidad


# ------------------------------------------------------------------------------
# IMPORTACIÓN, CENTRADO Y DETALLE DE SUPERFICIE PROCEDURAL
# ------------------------------------------------------------------------------
def importar_modelo(nombre_glb):
    antes = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=os.path.join(DIR_MODELOS, nombre_glb + ".glb"))
    nuevos = [o for o in bpy.data.objects if o not in antes]
    raices = [o for o in nuevos if o.parent is None]
    pivote = bpy.data.objects.new(f"Pivote_{nombre_glb}", None)
    bpy.context.collection.objects.link(pivote)
    for r in raices:
        r.parent = pivote
    bpy.context.view_layer.update()
    mn, mx = caja_limites([o for o in nuevos if o.type == 'MESH'])
    centro = (mn + mx) * 0.5
    for r in raices:
        r.location -= centro
    bpy.context.view_layer.update()
    mallas = [o for o in nuevos if o.type == 'MESH']
    for m in mallas:
        for p in m.data.polygons:
            p.use_smooth = p.use_smooth  # respetar el sombreado exportado
    return pivote, mallas


def caja_limites(mallas):
    mn = Vector((1e9, 1e9, 1e9))
    mx = Vector((-1e9, -1e9, -1e9))
    for o in mallas:
        for v in o.bound_box:
            w = o.matrix_world @ Vector(v)
            mn = Vector((min(mn.x, w.x), min(mn.y, w.y), min(mn.z, w.z)))
            mx = Vector((max(mx.x, w.x), max(mx.y, w.y), max(mx.z, w.z)))
    return mn, mx


def es_material_de_casco(bsdf):
    emision = bsdf.inputs["Emission Strength"].default_value
    rug = bsdf.inputs["Roughness"].default_value
    met = bsdf.inputs["Metallic"].default_value
    color_em = bsdf.inputs["Emission Color"].default_value
    emite = emision > 0.05 and max(color_em[:3]) > 0.05 and not (min(color_em[:3]) > 0.95 and emision <= 1.0)
    cristal = met > 0.85 and rug < 0.08
    return not emite and not cristal


def enriquecer_materiales(mallas, escala_paneles, tinte_rival=False, es_objeto=False):
    vistos = set()
    for o in mallas:
        for ranura in o.material_slots:
            mat = ranura.material
            if not mat or mat.name in vistos or not mat.use_nodes:
                continue
            vistos.add(mat.name)
            arbol = mat.node_tree
            bsdf = next((n for n in arbol.nodes if n.type == 'BSDF_PRINCIPLED'), None)
            if not bsdf:
                continue
            nodos, enlaces = arbol.nodes, arbol.links
            nombre = mat.name.lower()

            if tinte_rival:
                if any(k in nombre for k in ("ram", "intake")):
                    base = bsdf.inputs["Base Color"].default_value
                    bsdf.inputs["Base Color"].default_value = (base[0] * 1.25 + 0.06, base[1] * 0.45, base[2] * 0.5, 1.0)
                if any(k in nombre for k in ("plume", "flame")):
                    bsdf.inputs["Base Color"].default_value = (1.0, 0.08, 0.15, 1.0)
                    bsdf.inputs["Emission Color"].default_value = (1.0, 0.08, 0.15, 1.0)
                if "canopy" in nombre:
                    bsdf.inputs["Base Color"].default_value = (0.95, 0.12, 0.18, 1.0)
                    bsdf.inputs["Emission Color"].default_value = (0.6, 0.03, 0.06, 1.0)

            if "asteroide" in nombre:
                # Igual que en el juego (emisión 0): roca basáltica con cráteres en relieve y vetas de magma
                bsdf.inputs["Emission Strength"].default_value = 0.0
                bsdf.inputs["Base Color"].default_value = (0.13, 0.11, 0.10, 1.0)
                bsdf.inputs["Roughness"].default_value = 0.9
                bsdf.inputs["Metallic"].default_value = 0.0
                coord = nodos.new("ShaderNodeTexCoord")
                ruido = nodos.new("ShaderNodeTexNoise")
                ruido.inputs["Scale"].default_value = 3.5
                ruido.inputs["Detail"].default_value = 12.0
                ruido.inputs["Roughness"].default_value = 0.65
                crateres = nodos.new("ShaderNodeTexVoronoi")
                crateres.inputs["Scale"].default_value = 2.6
                suma = nodos.new("ShaderNodeMath")
                suma.operation = 'ADD'
                enlaces.new(coord.outputs["Object"], ruido.inputs["Vector"])
                enlaces.new(coord.outputs["Object"], crateres.inputs["Vector"])
                enlaces.new(ruido.outputs["Fac"], suma.inputs[0])
                enlaces.new(crateres.outputs["Distance"], suma.inputs[1])
                relieve = nodos.new("ShaderNodeBump")
                relieve.inputs["Strength"].default_value = 0.8
                relieve.inputs["Distance"].default_value = 0.08
                enlaces.new(suma.outputs["Value"], relieve.inputs["Height"])
                enlaces.new(relieve.outputs["Normal"], bsdf.inputs["Normal"])
                # Vetas incandescentes solo en las grietas profundas
                magma = nodos.new("ShaderNodeMapRange")
                magma.inputs["From Min"].default_value = 0.022
                magma.inputs["From Max"].default_value = 0.0
                magma.inputs["To Min"].default_value = 0.0
                magma.inputs["To Max"].default_value = 3.0
                vetas = nodos.new("ShaderNodeTexVoronoi")
                vetas.feature = 'DISTANCE_TO_EDGE'
                vetas.inputs["Scale"].default_value = 1.5
                enlaces.new(coord.outputs["Object"], vetas.inputs["Vector"])
                enlaces.new(vetas.outputs["Distance"], magma.inputs["Value"])
                bsdf.inputs["Emission Color"].default_value = (1.0, 0.32, 0.04, 1.0)
                enlaces.new(magma.outputs["Result"], bsdf.inputs["Emission Strength"])
                continue

            tope_emision = 1.1 if es_objeto else 2.2
            if bsdf.inputs["Emission Strength"].default_value > tope_emision:
                # AgX desatura la emisión muy intensa hacia el blanco: se limita para conservar el color neón
                bsdf.inputs["Emission Strength"].default_value = tope_emision

            if not es_material_de_casco(bsdf):
                continue

            # Pintura militar: dieléctrica y satinada (el metal desnudo reflejaba el estudio como espejo)
            if not bsdf.inputs["Metallic"].is_linked:
                bsdf.inputs["Metallic"].default_value = min(bsdf.inputs["Metallic"].default_value, 0.35)
            if not bsdf.inputs["Roughness"].is_linked:
                bsdf.inputs["Roughness"].default_value = max(bsdf.inputs["Roughness"].default_value, 0.34)

            coord = nodos.new("ShaderNodeTexCoord")

            # Líneas de panel rectilíneas (paneles escalonados tipo fuselaje) en espacio de objeto
            paneles = nodos.new("ShaderNodeTexBrick")
            paneles.inputs["Scale"].default_value = escala_paneles
            paneles.inputs["Mortar Size"].default_value = 0.008
            paneles.inputs["Mortar Smooth"].default_value = 0.4
            paneles.inputs["Brick Width"].default_value = 0.62
            paneles.inputs["Row Height"].default_value = 0.31
            paneles.offset = 0.5
            paneles.inputs["Color1"].default_value = (1, 1, 1, 1)
            paneles.inputs["Color2"].default_value = (1, 1, 1, 1)
            paneles.inputs["Mortar"].default_value = (0, 0, 0, 1)
            rango = nodos.new("ShaderNodeMapRange")
            rango.inputs["From Min"].default_value = 0.0
            rango.inputs["From Max"].default_value = 1.0
            enlaces.new(coord.outputs["Object"], paneles.inputs["Vector"])
            enlaces.new(paneles.outputs["Color"], rango.inputs["Value"])

            # Variación de rugosidad (desgaste y huellas de mantenimiento)
            ruido = nodos.new("ShaderNodeTexNoise")
            ruido.inputs["Scale"].default_value = escala_paneles * 3.2
            ruido.inputs["Detail"].default_value = 6.0
            enlaces.new(coord.outputs["Object"], ruido.inputs["Vector"])
            rug_base = bsdf.inputs["Roughness"].default_value
            rug = nodos.new("ShaderNodeMapRange")
            rug.inputs["From Min"].default_value = 0.3
            rug.inputs["From Max"].default_value = 0.7
            rug.inputs["To Min"].default_value = max(0.05, rug_base - 0.08)
            rug.inputs["To Max"].default_value = min(1.0, rug_base + 0.10)
            enlaces.new(ruido.outputs["Fac"], rug.inputs["Value"])
            if not bsdf.inputs["Roughness"].is_linked:
                enlaces.new(rug.outputs["Result"], bsdf.inputs["Roughness"])

            # Relieve de juntas
            relieve = nodos.new("ShaderNodeBump")
            relieve.inputs["Strength"].default_value = 0.22
            relieve.inputs["Distance"].default_value = 0.015
            enlaces.new(rango.outputs["Result"], relieve.inputs["Height"])
            if not bsdf.inputs["Normal"].is_linked:
                enlaces.new(relieve.outputs["Normal"], bsdf.inputs["Normal"])

            # Oscurecer ligeramente las juntas sobre el color base
            if not bsdf.inputs["Base Color"].is_linked:
                base = tuple(bsdf.inputs["Base Color"].default_value)
                junta = nodos.new("ShaderNodeMix")
                junta.data_type = 'RGBA'
                junta.inputs["Factor"].default_value = 1.0
                enlaces.new(rango.outputs["Result"], junta.inputs["Factor"])
                junta.inputs["A"].default_value = (base[0] * 0.62, base[1] * 0.62, base[2] * 0.62, 1.0)
                junta.inputs["B"].default_value = base
                enlaces.new(junta.outputs["Result"], bsdf.inputs["Base Color"])

            # Barniz sutil: reflejos nítidos de estudio sobre la pintura
            if "Coat Weight" in bsdf.inputs:
                bsdf.inputs["Coat Weight"].default_value = 0.1
                bsdf.inputs["Coat Roughness"].default_value = 0.12


# ------------------------------------------------------------------------------
# ILUMINACIÓN DE ESTUDIO (CLAVE SUAVE + DOS CONTORNOS DE COLOR + CENITAL)
# ------------------------------------------------------------------------------
def luz_area(nombre, posicion, objetivo, potencia, tamano, color):
    datos = bpy.data.lights.new(nombre, 'AREA')
    datos.energy = potencia
    datos.size = tamano
    datos.color = color
    obj = bpy.data.objects.new(nombre, datos)
    bpy.context.collection.objects.link(obj)
    obj.location = posicion
    obj.rotation_euler = (Vector(objetivo) - Vector(posicion)).to_track_quat('-Z', 'Y').to_euler()
    return obj


def iluminar(radio, color_contorno):
    k = radio * radio
    # Clave compacta: modela el volumen y deja una sombra de contacto definida (no una mancha difusa)
    luz_area("Clave", (radio * 1.6, radio * 2.2, radio * 2.6), (0, 0, 0), 70 * k, radio * 0.7, (1.0, 0.96, 0.9))
    luz_area("Relleno", (-radio * 2.4, radio * 0.8, radio * 1.4), (0, 0, 0), 14 * k, radio * 2.5, (0.75, 0.85, 1.0))
    luz_area("ContornoA", (-radio * 1.8, -radio * 2.6, radio * 1.1), (0, 0, 0), 80 * k, radio * 1.0, color_contorno)
    luz_area("ContornoB", (radio * 2.2, -radio * 2.0, radio * 0.6), (0, 0, 0), 40 * k, radio * 0.8, (1.0, 0.82, 0.55))
    luz_area("Cenital", (0, 0, radio * 3.2), (0, 0, 0), 22 * k, radio * 2.0, (0.95, 0.97, 1.0))


# ------------------------------------------------------------------------------
# CÁMARAS
# ------------------------------------------------------------------------------
def camara_cenital(mn, mx, lado_mayor_px):
    datos = bpy.data.cameras.new("CamCenital")
    datos.type = 'ORTHO'
    ancho, alto = (mx.x - mn.x), (mx.y - mn.y)
    margen = 1.06
    datos.ortho_scale = max(ancho, alto) * margen
    cam = bpy.data.objects.new("CamCenital", datos)
    bpy.context.collection.objects.link(cam)
    cam.location = (0, 0, mx.z + 20)
    cam.rotation_euler = (0, 0, 0)
    datos.clip_end = 200
    escena = bpy.context.scene
    escena.camera = cam
    if ancho >= alto:
        escena.render.resolution_x = lado_mayor_px
        escena.render.resolution_y = max(64, int(lado_mayor_px * alto / ancho))
    else:
        escena.render.resolution_y = lado_mayor_px
        escena.render.resolution_x = max(64, int(lado_mayor_px * ancho / alto))
    escena.render.resolution_percentage = 100
    return cam


def camara_heroica(radio, resolucion, azimut_grados=38, elevacion_grados=26, lente=58):
    datos = bpy.data.cameras.new("CamHeroica")
    datos.lens = lente
    cam = bpy.data.objects.new("CamHeroica", datos)
    bpy.context.collection.objects.link(cam)
    az = math.radians(azimut_grados)
    el = math.radians(elevacion_grados)
    # Ajuste del encuadre: distancia para que la esfera envolvente quepa en el campo de visión
    fov = 2 * math.atan(datos.sensor_width / (2 * lente))
    dist = radio / math.sin(fov / 2) * 1.02
    direccion = Vector((math.sin(az) * math.cos(el), math.cos(az) * math.cos(el), math.sin(el)))
    cam.location = direccion * dist
    cam.rotation_euler = (-direccion).to_track_quat('-Z', 'Y').to_euler()
    datos.clip_end = dist * 4
    escena = bpy.context.scene
    escena.camera = cam
    escena.render.resolution_x = resolucion
    escena.render.resolution_y = resolucion
    return cam


def suelo_captador_sombras(z):
    bpy.ops.mesh.primitive_plane_add(size=200, location=(0, 0, z))
    suelo = bpy.context.active_object
    suelo.name = "CaptadorSombras"
    suelo.is_shadow_catcher = True
    return suelo


# ------------------------------------------------------------------------------
# POSTPROCESO: RESPLANDOR (BLOOM) CON ALFA Y RECORTE AL CONTENIDO
# ------------------------------------------------------------------------------
def desenfoque_gaussiano(img, sigma):
    radio = max(1, int(sigma * 3))
    x = np.arange(-radio, radio + 1, dtype=np.float32)
    nucleo = np.exp(-(x * x) / (2 * sigma * sigma))
    nucleo /= nucleo.sum()
    salida = img
    for eje in (0, 1):
        relleno = [(0, 0)] * img.ndim
        relleno[eje] = (radio, radio)
        p = np.pad(salida, relleno, mode='constant')
        acumulado = np.zeros_like(salida)
        for i, w in enumerate(nucleo):
            corte = [slice(None)] * img.ndim
            corte[eje] = slice(i, i + salida.shape[eje])
            acumulado += p[tuple(corte)] * w
        salida = acumulado
    return salida


def leer_render(ruta):
    img = bpy.data.images.load(ruta, check_existing=False)
    w, h = img.size
    px = np.array(img.pixels[:], dtype=np.float32).reshape(h, w, 4)
    bpy.data.images.remove(img)
    return px[::-1].copy()  # Blender guarda de abajo hacia arriba


def escribir_png(px, ruta):
    h, w = px.shape[:2]
    img = bpy.data.images.new("salida", w, h, alpha=True)
    img.pixels[:] = np.clip(px[::-1], 0, 1).ravel()
    img.filepath_raw = ruta
    img.file_format = 'PNG'
    img.save()
    bpy.data.images.remove(img)


def aplicar_resplandor(px, intensidad=0.9):
    rgb = px[..., :3]
    a = px[..., 3:4]
    lum = rgb.max(axis=2, keepdims=True)
    # Solo los píxeles muy brillantes y saturados (plasma, toberas, cristales) generan halo
    saturacion = lum - rgb.min(axis=2, keepdims=True)
    mascara = np.clip((lum - 0.78) / 0.22, 0, 1) * np.clip(saturacion * 1.6, 0.25, 1) * a
    fuente = rgb * mascara
    h, w = px.shape[:2]
    escala = max(h, w)
    halo = desenfoque_gaussiano(fuente, escala * 0.006) * 0.6 + desenfoque_gaussiano(fuente, escala * 0.018) * 0.4
    halo *= intensidad
    alfa_halo = np.clip(halo.max(axis=2, keepdims=True) * 1.2, 0, 1)
    nuevo_a = np.clip(a + alfa_halo * (1 - a), 0, 1)
    # Composición premultiplicada: el halo se suma por encima del objeto y aporta alfa fuera de él
    rgb_pre = rgb * a + halo
    nuevo_rgb = np.where(nuevo_a > 1e-4, rgb_pre / np.maximum(nuevo_a, 1e-4), 0)
    return np.concatenate([np.clip(nuevo_rgb, 0, 1), nuevo_a], axis=2)


def recortar_contenido(px, margen_rel=0.02, umbral=0.02):
    a = px[..., 3]
    filas = np.where(a.max(axis=1) > umbral)[0]
    cols = np.where(a.max(axis=0) > umbral)[0]
    if len(filas) == 0:
        return px
    m = int(max(px.shape[:2]) * margen_rel)
    y0, y1 = max(0, filas[0] - m), min(px.shape[0], filas[-1] + 1 + m)
    x0, x1 = max(0, cols[0] - m), min(px.shape[1], cols[-1] + 1 + m)
    return px[y0:y1, x0:x1]


def reducir(px, lado_mayor):
    h, w = px.shape[:2]
    factor = lado_mayor / max(h, w)
    if factor >= 1:
        return px
    nh, nw = max(1, round(h * factor)), max(1, round(w * factor))
    # Reducción por área con alfa premultiplicado (sin halos oscuros en los bordes)
    pre = px.copy()
    pre[..., :3] *= pre[..., 3:4]
    ys = (np.arange(nh + 1) * h / nh).astype(int)
    xs = (np.arange(nw + 1) * w / nw).astype(int)
    acumulado = np.add.reduceat(np.add.reduceat(pre, ys[:-1], axis=0), xs[:-1], axis=1)
    cuenta = np.outer(np.diff(ys), np.diff(xs))[..., None]
    sal = acumulado / cuenta
    sal[..., :3] = np.where(sal[..., 3:4] > 1e-4, sal[..., :3] / np.maximum(sal[..., 3:4], 1e-4), 0)
    return sal


def renderizar_a(ruta):
    bpy.context.scene.render.filepath = ruta
    bpy.ops.render.render(write_still=True)


# ------------------------------------------------------------------------------
# FLUJOS DE RENDER
# ------------------------------------------------------------------------------
def renderizar_modelo(nombre_glb, calidad, temporal):
    prefijo, bando, genera_game, color_contorno = CATALOGO[nombre_glb]
    lado_cenital = 2048 if calidad == "final" else 512
    lado_heroico = 1600 if calidad == "final" else 400
    variantes = [("normal", False)]
    if nombre_glb == "F35_Lightning_II":
        variantes.append(("rival", True))

    for variante, rival in variantes:
        preparar_escena(calidad)
        configurar_mundo(rotacion_z=math.radians(35))
        _, mallas = importar_modelo(nombre_glb)
        mn, mx = caja_limites(mallas)
        radio = (mx - mn).length * 0.5
        escala_paneles = 1.15 / max(0.5, radio / 3.0)
        enriquecer_materiales(mallas, escala_paneles, tinte_rival=rival, es_objeto=(bando == "objeto"))
        iluminar(radio, (1.0, 0.15, 0.2) if rival else color_contorno)

        # 1) Vista cenital ortográfica (proa hacia +Y = arriba en la imagen)
        camara_cenital(mn, mx, lado_cenital)
        crudo = os.path.join(temporal, f"{prefijo}_{variante}_cenital.png")
        renderizar_a(crudo)
        px = recortar_contenido(aplicar_resplandor(leer_render(crudo), 0.75), margen_rel=0.015)
        if rival:
            escribir_png(reducir(px, 512), os.path.join(DIR_SALIDA, "F35_Render_Rival.png"))
            continue
        escribir_png(px, os.path.join(DIR_SALIDA, f"{prefijo}_Render_UltraHD.png"))
        if genera_game:
            escribir_png(reducir(px, 512), os.path.join(DIR_SALIDA, f"{prefijo}_Render_Game.png"))

        # 2) Plano heroico 3/4 con sombra de contacto
        suelo_captador_sombras(mn.z - (0.22 * radio if bando != "objeto" else 0.04 * radio))
        if nombre_glb == "Misil_Tactico":
            camara_heroica(radio, lado_heroico, azimut_grados=62, elevacion_grados=22, lente=70)
        else:
            camara_heroica(radio, lado_heroico)
        crudo = os.path.join(temporal, f"{prefijo}_heroico.png")
        renderizar_a(crudo)
        px = recortar_contenido(aplicar_resplandor(leer_render(crudo), 0.9), margen_rel=0.03)
        escribir_png(px, os.path.join(DIR_SALIDA, f"{prefijo}_Render_3D_Top.png"))


def renderizar_showcase(calidad, temporal):
    preparar_escena(calidad)
    configurar_mundo(rotacion_z=math.radians(35))
    todas = []
    separacion = 8.4
    for i, nombre in enumerate(FLOTA_SHOWCASE):
        pivote, mallas = importar_modelo(nombre)
        enriquecer_materiales(mallas, 1.15)
        # Formación en cuña escalonada: el insignia (Cóndor) al frente
        x = (i - (len(FLOTA_SHOWCASE) - 1) / 2) * separacion
        y = -abs(i - 1.8) * 3.2
        pivote.location = (x, y, 0.25 * (i % 2))
        pivote.rotation_euler = (0, 0, math.radians(-12 + i * 8))
        todas += mallas
    bpy.context.view_layer.update()
    mn, mx = caja_limites(todas)
    centro = (mn + mx) * 0.5
    radio = (mx - mn).length * 0.5
    iluminar(radio * 0.6, (0.0, 0.85, 1.0))
    suelo_captador_sombras(mn.z - 1.2)
    escena = bpy.context.scene
    datos = bpy.data.cameras.new("CamFlota")
    datos.lens = 42
    cam = bpy.data.objects.new("CamFlota", datos)
    bpy.context.collection.objects.link(cam)
    direccion = Vector((0.18, 0.86, 0.48)).normalized()
    cam.location = centro + direccion * radio * 2.25
    cam.rotation_euler = (centro - cam.location).to_track_quat('-Z', 'Y').to_euler()
    escena.camera = cam
    escena.render.resolution_x = 2400 if calidad == "final" else 600
    escena.render.resolution_y = 1200 if calidad == "final" else 300
    crudo = os.path.join(temporal, "flota.png")
    renderizar_a(crudo)
    px = recortar_contenido(aplicar_resplandor(leer_render(crudo), 0.9), margen_rel=0.02)
    escribir_png(px, os.path.join(DIR_SALIDA, "Flota_Showcase.png"))


def main():
    modelos, calidad, showcase = leer_argumentos()
    temporal = os.path.join(bpy.app.tempdir or os.environ.get("TEMP", "."), "condor_renders_crudos")
    os.makedirs(temporal, exist_ok=True)
    os.makedirs(DIR_SALIDA, exist_ok=True)
    print(f"=== ESTUDIO CYCLES · calidad={calidad} · modelos={modelos} ===")
    for nombre in modelos:
        if nombre not in CATALOGO:
            print(f"[AVISO] Modelo desconocido: {nombre}")
            continue
        t0 = time.time()
        renderizar_modelo(nombre, calidad, temporal)
        print(f"[OK] {nombre} renderizado en {time.time() - t0:.1f} s")
    if showcase:
        t0 = time.time()
        renderizar_showcase(calidad, temporal)
        print(f"[OK] Flota_Showcase renderizada en {time.time() - t0:.1f} s")
    print("=== RENDERS COMPLETADOS ===")


if __name__ == "__main__":
    main()
