# ==============================================================================
# PROYECTO: CÓNDOR TÁCTICO 3D - GENERADOR DE MODELOS 3D MASTER GLB (BLENDER 5.2 LTS)
# MODELADO AEROESPACIAL DE ALTA FIDELIDAD GUIADO POR HOJAS DE REFERENCIA DE ESTUDIO:
# - CÓNDOR DORADO X (ALAS EN FLECHA INVERTIDA EMPLUMADAS, CANARDS Y BORDES ORO)
# - F-35 LIGHTNING II (FUSELAJE STEALTH DSI, CINTAS RAM Y TOBERA F135 MONOMOTOR)
# - B-2 SPIRIT (ALA VOLANTE PURA CON BORDE DE SALIDA EN W DOBLE Y TOMAS S-DUCT)
# - F-22 RAPTOR (ALAS DIAMANTE, TOMAS CARET, CAMUFLAJE BITONO Y TOBERAS 2D)
# - HALCÓN CARMESÍ (RADOMO EN AGUJA, DELTA PURA Y PODS LÁSER ALAR CON AGUJAS)
# - VÍBORA IMPERIAL (ALAS FLECHA INVERTIDA ORO/ÁMBAR, CAÑONES DE RAÍZ Y BOTALONES)
# - GÓLEM PESADO (CAÑONERO CATAMARÁN INDUSTRIAL, CAÑONES CUÁDRUPLES Y REJILLAS)
# - VALKIRIA VOLCÁNICA X-9 (ACORAZADO TRIMARÁN, 4 TORRETAS Y NÚCLEO MAGMA CALDERA)
# ==============================================================================

import bpy
import bmesh
import math
import os
from mathutils import Vector, Matrix

PROJECT_ROOT = r"C:\Users\kreiv\OneDrive\Documentos\JuegoAviones"
WWWROOT_MODELOS = os.path.join(PROJECT_ROOT, "CondorTactico", "wwwroot", "assets", "modelos")
ASSETS_MODELOS = os.path.join(PROJECT_ROOT, "assets", "modelos")
WWWROOT_TEXTURAS = os.path.join(PROJECT_ROOT, "CondorTactico", "wwwroot", "assets", "texturas")
BIN_DEBUG_MODELOS = os.path.join(PROJECT_ROOT, "tools", "CapturadorRemoto", "bin", "Debug", "net8.0-windows", "wwwroot", "assets", "modelos")
CONDOR_BIN_MODELOS = os.path.join(PROJECT_ROOT, "CondorTactico", "bin", "Debug", "net8.0-windows", "wwwroot", "assets", "modelos")

for d in [WWWROOT_MODELOS, ASSETS_MODELOS, WWWROOT_TEXTURAS, BIN_DEBUG_MODELOS, CONDOR_BIN_MODELOS]:
    os.makedirs(d, exist_ok=True)


def limpiar_escena():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    for obj in list(bpy.data.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    for mesh in list(bpy.data.meshes):
        bpy.data.meshes.remove(mesh, do_unlink=True)
    for mat in list(bpy.data.materials):
        bpy.data.materials.remove(mat, do_unlink=True)
    for img in list(bpy.data.images):
        bpy.data.images.remove(img, do_unlink=True)


# ==============================================================================
# GESTIÓN DE MATERIALES PBR PROFESIONALES (BLENDER 5.2 PRINCIPLED BSDF)
# ==============================================================================
def crear_mat(nombre, color_base, metalico=0.5, rugosidad=0.3, emision_color=(0, 0, 0, 1), emision_fuerza=0.0):
    mat = bpy.data.materials.new(name=nombre)
    mat.use_nodes = True
    p = mat.node_tree.nodes['Principled BSDF']
    p.inputs['Base Color'].default_value = color_base
    p.inputs['Metallic'].default_value = metalico
    p.inputs['Roughness'].default_value = rugosidad
    if 'Emission Color' in p.inputs:
        p.inputs['Emission Color'].default_value = emision_color
        p.inputs['Emission Strength'].default_value = emision_fuerza
    return mat


# ==============================================================================
# UTILIDADES GEOMÉTRICAS B-MESH DE ALTA PRECISIÓN
# ==============================================================================
def loft_anillos(nombre_obj, lista_anillos, mat_principal, mats_extra=None, suavizar=True):
    bm = bmesh.new()
    anillos_bm = []
    for ring in lista_anillos:
        anillos_bm.append([bm.verts.new(v) for v in ring])

    num_est = len(anillos_bm)
    num_pts = len(lista_anillos[0])

    for i in range(num_est - 1):
        r1 = anillos_bm[i]
        r2 = anillos_bm[i + 1]
        for j in range(num_pts):
            jn = (j + 1) % num_pts
            f = bm.faces.new([r1[j], r1[jn], r2[jn], r2[j]])
            f.material_index = 0
            if mats_extra and (i in mats_extra):
                f.material_index = mats_extra[i]

    bm.faces.new(reversed(anillos_bm[0])).material_index = 0
    bm.faces.new(anillos_bm[-1]).material_index = 0

    if suavizar:
        for f in bm.faces:
            f.smooth = True

    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new(nombre_obj + "_Mesh")
    bm.to_mesh(mesh)
    bm.free()

    obj = bpy.data.objects.new(nombre_obj, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mat_principal)
    return obj


def crear_ala_trapezoidal(nombre, x_in, y_le_in, y_te_in, z_in, t_in,
                           x_out, y_le_out, y_te_out, z_out, t_out,
                           mat_cuerpo, mat_borde=None, simetrico_x=True):
    lados = [-1.0, 1.0] if simetrico_x else [1.0]
    for sx in lados:
        bm = bmesh.new()
        r_xin = x_in * sx
        r_xout = x_out * sx
        
        v_in = [
            bm.verts.new(Vector((r_xin, y_le_in, z_in))),
            bm.verts.new(Vector((r_xin, y_le_in * 0.7 + y_te_in * 0.3, z_in + t_in * 0.5))),
            bm.verts.new(Vector((r_xin, y_te_in, z_in))),
            bm.verts.new(Vector((r_xin, y_le_in * 0.7 + y_te_in * 0.3, z_in - t_in * 0.35)))
        ]
        v_out = [
            bm.verts.new(Vector((r_xout, y_le_out, z_out))),
            bm.verts.new(Vector((r_xout, y_le_out * 0.7 + y_te_out * 0.3, z_out + t_out * 0.5))),
            bm.verts.new(Vector((r_xout, y_te_out, z_out))),
            bm.verts.new(Vector((r_xout, y_le_out * 0.7 + y_te_out * 0.3, z_out - t_out * 0.35)))
        ]

        for j in range(4):
            jn = (j + 1) % 4
            if sx > 0:
                f = bm.faces.new([v_in[j], v_in[jn], v_out[jn], v_out[j]])
            else:
                f = bm.faces.new([v_out[j], v_out[jn], v_in[jn], v_in[j]])
            f.material_index = 1 if (mat_borde and j == 0) else 0

        if sx > 0:
            bm.faces.new(reversed(v_in)).material_index = 0
            bm.faces.new(v_out).material_index = 1 if mat_borde else 0
        else:
            bm.faces.new(v_in).material_index = 0
            bm.faces.new(reversed(v_out)).material_index = 1 if mat_borde else 0

        for f in bm.faces:
            f.smooth = True

        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        mesh = bpy.data.meshes.new(f"{nombre}_{sx}_Mesh")
        bm.to_mesh(mesh)
        bm.free()

        obj = bpy.data.objects.new(f"{nombre}_{sx}", mesh)
        bpy.context.collection.objects.link(obj)
        obj.data.materials.append(mat_cuerpo)
        if mat_borde:
            obj.data.materials.append(mat_borde)


def crear_cupula_cabina_stealth(nombre, y_pos, z_pos, largo, ancho, alto, mat_cristal, mat_marco=None):
    """Crea una cúpula de cabina de burbuja/facetada orientada horizontalmente a lo largo del eje Y."""
    bm = bmesh.new()
    estaciones = [
        (y_pos + largo * 0.50, ancho * 0.15, alto * 0.15, z_pos),
        (y_pos + largo * 0.25, ancho * 0.75, alto * 0.70, z_pos + alto * 0.15),
        (y_pos + 0.00, ancho * 1.00, alto * 1.00, z_pos + alto * 0.20),
        (y_pos - largo * 0.25, ancho * 0.85, alto * 0.80, z_pos + alto * 0.15),
        (y_pos - largo * 0.50, ancho * 0.25, alto * 0.25, z_pos)
    ]
    anillos = []
    for ey, ew, eh, ez in estaciones:
        pts = [
            Vector((0.0, ey, ez + eh)),
            Vector((ew * 0.55, ey, ez + eh * 0.72)),
            Vector((ew, ey, ez)),
            Vector((ew * 0.65, ey, ez - eh * 0.3)),
            Vector((0.0, ey, ez - eh * 0.35)),
            Vector((-ew * 0.65, ey, ez - eh * 0.3)),
            Vector((-ew, ey, ez)),
            Vector((-ew * 0.55, ey, ez + eh * 0.72))
        ]
        anillos.append([bm.verts.new(p) for p in pts])

    for i in range(len(anillos) - 1):
        r1, r2 = anillos[i], anillos[i + 1]
        for j in range(8):
            jn = (j + 1) % 8
            bm.faces.new([r1[j], r1[jn], r2[jn], r2[j]])
    bm.faces.new(reversed(anillos[0]))
    bm.faces.new(anillos[-1])
    for f in bm.faces:
        f.smooth = True
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new(f"{nombre}_Mesh")
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(nombre, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mat_cristal)
    return obj


def crear_conducto_3d(nombre, puntos, ancho, alto, mat_emision, simetrico_x=True):
    lados = [-1.0, 1.0] if simetrico_x else [1.0]
    for sx in lados:
        bm = bmesh.new()
        rings = []
        for (x, y, z) in puntos:
            px = x * sx
            pts = [
                Vector((px - ancho * 0.5, y, z + alto)),
                Vector((px + ancho * 0.5, y, z + alto)),
                Vector((px + ancho * 0.6, y, z - alto * 0.2)),
                Vector((px - ancho * 0.6, y, z - alto * 0.2))
            ]
            rings.append([bm.verts.new(p) for p in pts])

        for i in range(len(rings) - 1):
            r1, r2 = rings[i], rings[i + 1]
            for j in range(4):
                jn = (j + 1) % 4
                bm.faces.new([r1[j], r1[jn], r2[jn], r2[j]])
        bm.faces.new(reversed(rings[0]))
        bm.faces.new(rings[-1])
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        mesh = bpy.data.meshes.new(f"{nombre}_{sx}_Mesh")
        bm.to_mesh(mesh)
        bm.free()
        obj = bpy.data.objects.new(f"{nombre}_{sx}", mesh)
        bpy.context.collection.objects.link(obj)
        obj.data.materials.append(mat_emision)


def crear_tobera_con_flama(nombre, x, y, z, radio, largo, radio_llama, largo_llama, mat_shroud, mat_plasma, mat_ring=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=16, radius=radio, depth=largo, location=(x, y, z))
    shroud = bpy.context.active_object
    shroud.name = f"{nombre}_Shroud"
    shroud.rotation_euler = (math.radians(90), 0, 0)
    bpy.ops.object.transform_apply(rotation=True)
    shroud.data.materials.append(mat_shroud)

    if mat_ring:
        bpy.ops.mesh.primitive_torus_add(major_radius=radio * 0.98, minor_radius=radio * 0.12, location=(x, y - largo * 0.38, z))
        ring = bpy.context.active_object
        ring.rotation_euler = (math.radians(90), 0, 0)
        bpy.ops.object.transform_apply(rotation=True)
        ring.data.materials.append(mat_ring)

    bpy.ops.mesh.primitive_cone_add(vertices=16, radius1=radio_llama, radius2=0.015, depth=largo_llama, location=(x, y - largo * 0.5 - largo_llama * 0.5, z))
    llama = bpy.context.active_object
    llama.name = "propulsor"
    llama.rotation_euler = (math.radians(90), 0, 0)
    bpy.ops.object.transform_apply(rotation=True)
    llama.data.materials.append(mat_plasma)


def exportar_glb(nombre_base):
    for carpeta in [WWWROOT_MODELOS, ASSETS_MODELOS, BIN_DEBUG_MODELOS, CONDOR_BIN_MODELOS]:
        out_path = os.path.join(carpeta, f"{nombre_base}.glb")
        bpy.ops.export_scene.gltf(
            filepath=out_path,
            export_format='GLB',
            use_selection=False,
            export_apply=True,
            export_materials='EXPORT'
        )
    tam = os.path.getsize(os.path.join(WWWROOT_MODELOS, f"{nombre_base}.glb"))
    print(f"[EXPORTADO GLB MASTER] {nombre_base}.glb -> {tam:,} bytes")


# ==============================================================================
# 1. CÓNDOR DORADO X (CAZA INSIGNIA ANDINO 5.ª GEN STEALTH)
# Referencia: ref_flota_jugador_v3.jpg (Top-Left)
# ==============================================================================
def construir_condor_x():
    limpiar_escena()
    mat_casco = crear_mat("Mat_Condor_Hull", (0.22, 0.30, 0.42, 1.0), metalico=0.78, rugosidad=0.24)
    mat_paneles = crear_mat("Mat_Condor_Slate", (0.15, 0.20, 0.28, 1.0), metalico=0.82, rugosidad=0.20)
    mat_oro = crear_mat("Mat_Condor_Gold", (1.0, 0.78, 0.16, 1.0), metalico=0.96, rugosidad=0.12,
                        emision_color=(0.90, 0.60, 0.05, 1.0), emision_fuerza=0.6)
    mat_cabina = crear_mat("Mat_Condor_Canopy", (1.0, 0.75, 0.18, 1.0), metalico=0.92, rugosidad=0.04,
                           emision_color=(0.85, 0.50, 0.05, 1.0), emision_fuerza=0.7)
    mat_plasma = crear_mat("Mat_Condor_Plasma", (0.0, 0.95, 1.0, 1.0), metalico=0.1, rugosidad=0.1,
                           emision_color=(0.0, 0.95, 1.0, 1.0), emision_fuerza=5.5)
    mat_misil = crear_mat("Mat_AIM_White", (0.92, 0.94, 0.96, 1.0), metalico=0.4, rugosidad=0.25)

    estaciones_fus = [
        (3.80, 0.01, 0.01, 0.01, 0.0),
        (3.40, 0.18, 0.10, 0.08, 0.0),
        (2.40, 0.38, 0.18, 0.12, 0.02),
        (1.20, 0.65, 0.28, 0.18, 0.04),
        (0.00, 0.76, 0.32, 0.20, 0.04),
        (-1.20, 0.72, 0.28, 0.18, 0.02),
        (-2.20, 0.55, 0.22, 0.15, 0.0)
    ]
    anillos = []
    for y, w, ht, hb, z0 in estaciones_fus:
        pts = [
            Vector((0.0, y, z0 + ht)),
            Vector((w * 0.45, y, z0 + ht * 0.75)),
            Vector((w, y, z0)),
            Vector((w * 0.65, y, z0 - hb * 0.7)),
            Vector((0.0, y, z0 - hb)),
            Vector((-w * 0.65, y, z0 - hb * 0.7)),
            Vector((-w, y, z0)),
            Vector((-w * 0.45, y, z0 + ht * 0.75))
        ]
        anillos.append(pts)
    loft_anillos("Fuselaje_Condor", anillos, mat_casco)

    # Cúpula de cabina dorada horizontal
    crear_cupula_cabina_stealth("Cabina_Condor", 1.35, 0.20, 1.75, 0.28, 0.22, mat_cabina, mat_paneles)

    # Canards delanteros con filo de oro puro pulido
    crear_ala_trapezoidal("Canard_Condor", 0.42, 2.05, 1.30, 0.05, 0.06,
                          1.65, 1.62, 1.25, 0.08, 0.025,
                          mat_casco, mat_borde=mat_oro, simetrico_x=True)

    # Tomas de aire laterales DSI
    for sx in [-1.0, 1.0]:
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(sx * 0.72, 0.65, -0.02))
        toma = bpy.context.active_object
        toma.scale = (0.26, 0.95, 0.22)
        toma.rotation_euler = (math.radians(-6), 0, math.radians(-sx * 10))
        bpy.ops.object.transform_apply(rotation=True, scale=True)
        toma.data.materials.append(mat_oro)

    # Ala Principal en flecha invertida (Forward-Swept Wing)
    crear_ala_trapezoidal("Ala_Principal_Condor", 0.62, 0.95, -1.60, 0.04, 0.13,
                          2.35, 0.25, -0.90, 0.03, 0.065,
                          mat_casco, mat_borde=mat_oro, simetrico_x=True)

    # Plumas aerodinámicas escalonadas en puntas alares (Feathered Wingtips simétricos)
    crear_ala_trapezoidal("Pluma_1", 2.25, 0.32, -0.85, 0.03, 0.055,
                          3.45, -0.25, -0.95, 0.01, 0.025,
                          mat_casco, mat_borde=mat_oro, simetrico_x=True)

    crear_ala_trapezoidal("Pluma_2", 3.35, -0.20, -0.92, 0.01, 0.025,
                          4.05, -0.75, -1.15, -0.02, 0.015,
                          mat_oro, mat_borde=mat_oro, simetrico_x=True)

    # Estabilizadores horizontales traseros
    crear_ala_trapezoidal("Stabilator_Condor", 0.58, -1.45, -2.25, 0.02, 0.06,
                          1.75, -1.85, -2.42, 0.02, 0.02,
                          mat_casco, mat_borde=mat_oro, simetrico_x=True)

    # Derivas en V (Twin V-Tails) inclinadas 68°
    for sx in [-1.0, 1.0]:
        bm = bmesh.new()
        v_base = [
            bm.verts.new(Vector((sx * 0.52, -0.85, 0.12))),
            bm.verts.new(Vector((sx * 0.52, -1.90, 0.12))),
            bm.verts.new(Vector((sx * 0.52, -1.85, 0.18))),
            bm.verts.new(Vector((sx * 0.52, -0.92, 0.18)))
        ]
        v_top = [
            bm.verts.new(Vector((sx * 1.05, -1.45, 0.95))),
            bm.verts.new(Vector((sx * 1.05, -2.15, 0.95))),
            bm.verts.new(Vector((sx * 1.02, -2.12, 0.96))),
            bm.verts.new(Vector((sx * 1.02, -1.48, 0.96)))
        ]
        for j in range(4):
            jn = (j + 1) % 4
            f = bm.faces.new([v_base[j], v_base[jn], v_top[jn], v_top[j]] if sx > 0 else [v_top[j], v_top[jn], v_base[jn], v_base[j]])
            f.material_index = 1 if j == 0 else 0
        bm.faces.new(reversed(v_base) if sx > 0 else v_base).material_index = 0
        bm.faces.new(v_top if sx > 0 else reversed(v_top)).material_index = 1
        for f in bm.faces: f.smooth = True
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        mesh = bpy.data.meshes.new(f"VTail_{sx}_Mesh")
        bm.to_mesh(mesh)
        bm.free()
        obj = bpy.data.objects.new(f"VTail_{sx}", mesh)
        bpy.context.collection.objects.link(obj)
        obj.data.materials.append(mat_casco)
        obj.data.materials.append(mat_oro)

    # Conductos de energía cian 3D resplandecientes sobre la espina y las alas
    crear_conducto_3d("Conducto_Espina", [(0.18, 1.95, 0.22), (0.34, 0.85, 0.27), (0.32, -0.45, 0.28), (0.30, -1.75, 0.22)], 0.035, 0.016, mat_plasma, True)
    crear_conducto_3d("Conducto_Alas", [(0.42, 0.80, 0.12), (1.45, 0.35, 0.08), (2.45, -0.15, 0.06), (3.45, -0.55, 0.03)], 0.038, 0.016, mat_plasma, True)

    # Góndolas bimotor y toberas con collares de oro
    for sx in [-0.34, 0.34]:
        crear_tobera_con_flama(f"Tobera_Condor_{sx}", sx, -2.15, 0.04, 0.22, 0.40, 0.17, 0.75, mat_paneles, mat_plasma, mat_ring=mat_oro)

    # Misiles subalares AIM-9X
    for sx in [-1.0, 1.0]:
        for mx, my in [(1.85, 0.05), (2.65, -0.35)]:
            bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=0.045, depth=1.1, location=(sx * mx, my, -0.10))
            misil = bpy.context.active_object
            misil.rotation_euler = (math.radians(90), 0, 0)
            bpy.ops.object.transform_apply(rotation=True)
            misil.data.materials.append(mat_misil)

    exportar_glb("Condor_X")


# ==============================================================================
# 2. F-35 LIGHTNING II (CAZA STEALTH MULTIPROPÓSITO MONOMOTOR)
# Referencia: ref_flota_jugador_v3.jpg (Top-Right)
# ==============================================================================
def construir_f35():
    limpiar_escena()
    mat_ram = crear_mat("Mat_F35_RAM", (0.34, 0.37, 0.44, 1.0), metalico=0.55, rugosidad=0.35)
    mat_borde_ram = crear_mat("Mat_F35_EdgeRAM", (0.48, 0.52, 0.58, 1.0), metalico=0.60, rugosidad=0.30)
    mat_oscuro = crear_mat("Mat_F35_DarkIntake", (0.12, 0.14, 0.18, 1.0), metalico=0.70, rugosidad=0.25)
    mat_cabina = crear_mat("Mat_F35_GoldCanopy", (0.98, 0.76, 0.18, 1.0), metalico=0.90, rugosidad=0.04,
                           emision_color=(0.80, 0.48, 0.05, 1.0), emision_fuerza=0.6)
    mat_llama = crear_mat("Mat_F135_Plume", (1.0, 0.50, 0.08, 1.0), metalico=0.1, rugosidad=0.1,
                          emision_color=(1.0, 0.50, 0.08, 1.0), emision_fuerza=5.5)
    mat_misil = crear_mat("Mat_AIM9X_White", (0.92, 0.94, 0.96, 1.0), metalico=0.4, rugosidad=0.25)

    estaciones_f35 = [
        (3.40, 0.01, 0.01, 0.01, 0.0),
        (2.80, 0.22, 0.12, 0.10, 0.0),
        (1.80, 0.45, 0.22, 0.16, 0.02),
        (0.60, 0.72, 0.35, 0.22, 0.03),
        (-0.60, 0.78, 0.38, 0.24, 0.03),
        (-1.70, 0.54, 0.28, 0.20, 0.01),
        (-2.20, 0.38, 0.26, 0.22, 0.0)
    ]
    anillos = []
    for y, w, ht, hb, z0 in estaciones_f35:
        pts = [
            Vector((0.0, y, z0 + ht)),
            Vector((w * 0.48, y, z0 + ht * 0.78)),
            Vector((w, y, z0)),
            Vector((w * 0.70, y, z0 - hb * 0.7)),
            Vector((0.0, y, z0 - hb)),
            Vector((-w * 0.70, y, z0 - hb * 0.7)),
            Vector((-w, y, z0)),
            Vector((-w * 0.48, y, z0 + ht * 0.78))
        ]
        anillos.append(pts)
    loft_anillos("Fuselaje_F35", anillos, mat_ram)

    # Cúpula ámbar horizontal
    crear_cupula_cabina_stealth("Cabina_F35", 1.25, 0.22, 1.65, 0.27, 0.20, mat_cabina, mat_borde_ram)

    # Tomas DSI con carenado supersónico
    for sx in [-1.0, 1.0]:
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(sx * 0.68, 0.55, -0.01))
        toma = bpy.context.active_object
        toma.scale = (0.28, 1.05, 0.26)
        toma.rotation_euler = (math.radians(-4), 0, math.radians(-sx * 8))
        bpy.ops.object.transform_apply(rotation=True, scale=True)
        toma.data.materials.append(mat_oscuro)

    # Alas trapezoidales con borde RAM diente de sierra
    crear_ala_trapezoidal("Alas_F35", 0.62, 0.85, -1.35, 0.03, 0.12,
                          2.85, -0.32, -1.15, 0.01, 0.035,
                          mat_ram, mat_borde=mat_borde_ram, simetrico_x=True)

    # Rieles lanzadores en puntas alares con misiles AIM-9X
    for sx in [-1.0, 1.0]:
        bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=0.048, depth=1.35, location=(sx * 2.92, -0.55, 0.01))
        mis = bpy.context.active_object
        mis.rotation_euler = (math.radians(90), 0, 0)
        bpy.ops.object.transform_apply(rotation=True)
        mis.data.materials.append(mat_misil)

    # Estabilizadores horizontales
    crear_ala_trapezoidal("Tailerons_F35", 0.48, -1.45, -2.18, 0.0, 0.06,
                          1.75, -1.88, -2.38, 0.0, 0.02,
                          mat_ram, mat_borde=mat_borde_ram, simetrico_x=True)

    # Derivas dobles cantadas 65°
    for sx in [-1.0, 1.0]:
        bm = bmesh.new()
        v_base = [
            bm.verts.new(Vector((sx * 0.48, -0.95, 0.12))),
            bm.verts.new(Vector((sx * 0.48, -1.98, 0.12))),
            bm.verts.new(Vector((sx * 0.48, -1.92, 0.18))),
            bm.verts.new(Vector((sx * 0.48, -1.02, 0.18)))
        ]
        v_top = [
            bm.verts.new(Vector((sx * 0.95, -1.52, 0.92))),
            bm.verts.new(Vector((sx * 0.95, -2.25, 0.92))),
            bm.verts.new(Vector((sx * 0.92, -2.22, 0.93))),
            bm.verts.new(Vector((sx * 0.92, -1.55, 0.93)))
        ]
        for j in range(4):
            jn = (j + 1) % 4
            f = bm.faces.new([v_base[j], v_base[jn], v_top[jn], v_top[j]] if sx > 0 else [v_top[j], v_top[jn], v_base[jn], v_base[j]])
            f.material_index = 1 if j == 0 else 0
        bm.faces.new(reversed(v_base) if sx > 0 else v_base).material_index = 0
        bm.faces.new(v_top if sx > 0 else reversed(v_top)).material_index = 1
        for f in bm.faces: f.smooth = True
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        mesh = bpy.data.meshes.new(f"Deriva_F35_{sx}_Mesh")
        bm.to_mesh(mesh)
        bm.free()
        obj = bpy.data.objects.new(f"Deriva_F35_{sx}", mesh)
        bpy.context.collection.objects.link(obj)
        obj.data.materials.append(mat_ram)
        obj.data.materials.append(mat_borde_ram)

    # Tobera Pratt & Whitney F135 masiva central con postcombustión
    crear_tobera_con_flama("Tobera_F135", 0.0, -2.18, 0.01, 0.33, 0.45, 0.28, 0.88, mat_oscuro, mat_llama, mat_ring=mat_borde_ram)

    exportar_glb("F35_Lightning_II")


# ==============================================================================
# 3. B-2 SPIRIT (BOMBARDERO STEALTH DE ALA VOLANTE PURA)
# Referencia: ref_flota_jugador_v3.jpg (Bottom-Left)
# ==============================================================================
def construir_b2():
    limpiar_escena()
    mat_obsidian = crear_mat("Mat_B2_Obsidian", (0.20, 0.24, 0.30, 1.0), metalico=0.35, rugosidad=0.50)
    mat_panel = crear_mat("Mat_B2_Panel", (0.26, 0.31, 0.38, 1.0), metalico=0.40, rugosidad=0.45)
    mat_cavidad = crear_mat("Mat_B2_DarkCavity", (0.08, 0.10, 0.14, 1.0), metalico=0.70, rugosidad=0.20)
    mat_cristal = crear_mat("Mat_B2_Windows", (0.10, 0.70, 0.95, 1.0), metalico=0.85, rugosidad=0.04,
                            emision_color=(0.05, 0.50, 0.80, 1.0), emision_fuerza=0.8)
    mat_cyan = crear_mat("Mat_B2_CyanTrim", (0.0, 0.90, 1.0, 1.0), metalico=0.1, rugosidad=0.1,
                         emision_color=(0.0, 0.90, 1.0, 1.0), emision_fuerza=4.8)
    mat_flama = crear_mat("Mat_B2_Exhaust", (0.0, 0.85, 1.0, 1.0), metalico=0.1, rugosidad=0.1,
                          emision_color=(0.0, 0.85, 1.0, 1.0), emision_fuerza=5.0)

    # Contorno de Ala Volante B-2 con borde de salida en W Doble auténtico
    coords_contorno_sup = [
        Vector((0.0, 2.05, 0.16)),
        Vector((4.60, -1.05, 0.02)),
        Vector((3.60, -0.48, 0.03)),
        Vector((2.70, -1.08, 0.04)),
        Vector((1.75, -0.48, 0.05)),
        Vector((0.88, -1.15, 0.06)),
        Vector((0.0, -0.42, 0.08)),
        Vector((-0.88, -1.15, 0.06)),
        Vector((-1.75, -0.48, 0.05)),
        Vector((-2.70, -1.08, 0.04)),
        Vector((-3.60, -0.48, 0.03)),
        Vector((-4.60, -1.05, 0.02))
    ]
    bm = bmesh.new()
    v_top = [bm.verts.new(v) for v in coords_contorno_sup]
    v_bot = [bm.verts.new(Vector((v.x, v.y, -v.z * 0.65))) for v in coords_contorno_sup]

    bm.faces.new(v_top).material_index = 0
    bm.faces.new(reversed(v_bot)).material_index = 0

    n_pts = len(v_top)
    for i in range(n_pts):
        inxt = (i + 1) % n_pts
        f = bm.faces.new([v_top[i], v_top[inxt], v_bot[inxt], v_bot[i]])
        f.material_index = 1 if i in [0, 11] else 0

    for f in bm.faces: f.smooth = True
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new("Ala_B2_Mesh")
    bm.to_mesh(mesh)
    bm.free()
    ala_obj = bpy.data.objects.new("B2_Spirit", mesh)
    bpy.context.collection.objects.link(ala_obj)
    ala_obj.data.materials.append(mat_obsidian)
    ala_obj.data.materials.append(mat_panel)

    # Cúpula central dorsal de cabina
    est_cab = [
        (1.95, 0.02, 0.02, 0.01, 0.05),
        (1.20, 0.62, 0.28, 0.03, 0.10),
        (0.20, 0.88, 0.32, 0.04, 0.11),
        (-0.55, 0.58, 0.18, 0.03, 0.06),
        (-1.05, 0.08, 0.03, 0.01, 0.02)
    ]
    anillos_cab = []
    for y, w, ht, hb, z0 in est_cab:
        pts = [
            Vector((0.0, y, z0 + ht)),
            Vector((w * 0.5, y, z0 + ht * 0.75)),
            Vector((w, y, z0)),
            Vector((w * 0.6, y, z0 - hb)),
            Vector((0.0, y, z0 - hb)),
            Vector((-w * 0.6, y, z0 - hb)),
            Vector((-w, y, z0)),
            Vector((-w * 0.5, y, z0 + ht * 0.75))
        ]
        anillos_cab.append(pts)
    loft_anillos("Domo_Cabina_B2", anillos_cab, mat_panel)

    # 4 Ventanas de cabina con cristal facetado
    for sx in [-0.28, -0.095, 0.095, 0.28]:
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(sx, 1.12 - abs(sx) * 0.40, 0.28))
        win = bpy.context.active_object
        win.scale = (0.13, 0.22, 0.075)
        win.rotation_euler = (math.radians(18), 0, math.radians(-sx * 35))
        bpy.ops.object.transform_apply(rotation=True, scale=True)
        win.data.materials.append(mat_cristal)

    # Tomas S-Duct empotradas dorsalmente
    for sx in [-1.0, 1.0]:
        cx = sx * 0.98
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(cx, 0.45, 0.14))
        sduct = bpy.context.active_object
        sduct.scale = (0.34, 0.75, 0.14)
        bpy.ops.object.transform_apply(scale=True)
        sduct.data.materials.append(mat_cavidad)

    # Tiras luminosas de borde cian resplandeciente
    crear_conducto_3d("Tira_Borde_B2", [(0.15, 1.85, 0.16), (1.45, 0.95, 0.10), (2.95, 0.02, 0.06), (4.45, -0.92, 0.03)], 0.035, 0.015, mat_cyan, True)
    crear_conducto_3d("Tira_Panel_B2", [(0.68, 0.75, 0.14), (0.72, -0.25, 0.12), (1.65, -0.45, 0.06)], 0.030, 0.015, mat_cyan, True)

    # Toberas de escape planas integradas
    for sx in [-0.98, 0.98]:
        crear_tobera_con_flama(f"Tobera_B2_{sx}", sx, -0.98, 0.07, 0.16, 0.30, 0.13, 0.60, mat_cavidad, mat_flama, mat_ring=mat_panel)

    exportar_glb("B2_Spirit")


# ==============================================================================
# 4. F-22 RAPTOR (DOMINANCIA AÉREA STEALTH DE 5.ª GENERACIÓN)
# Referencia: ref_flota_jugador_v3.jpg (Bottom-Right)
# ==============================================================================
def construir_f22():
    limpiar_escena()
    mat_f22 = crear_mat("Mat_F22_Silver", (0.48, 0.54, 0.62, 1.0), metalico=0.82, rugosidad=0.22)
    mat_camo = crear_mat("Mat_F22_SlateCamo", (0.32, 0.37, 0.44, 1.0), metalico=0.80, rugosidad=0.24)
    mat_oscuro = crear_mat("Mat_F22_IntakeDark", (0.12, 0.15, 0.19, 1.0), metalico=0.75, rugosidad=0.20)
    mat_cabina = crear_mat("Mat_F22_AmberCanopy", (1.0, 0.76, 0.18, 1.0), metalico=0.92, rugosidad=0.04,
                           emision_color=(0.82, 0.50, 0.06, 1.0), emision_fuerza=0.6)
    mat_llama = crear_mat("Mat_F22_Plasma2D", (0.2, 0.85, 1.0, 1.0), metalico=0.1, rugosidad=0.1,
                          emision_color=(0.2, 0.85, 1.0, 1.0), emision_fuerza=5.5)

    estaciones_f22 = [
        (3.55, 0.01, 0.01, 0.01, 0.0),
        (2.95, 0.20, 0.10, 0.08, 0.0),
        (1.95, 0.42, 0.19, 0.13, 0.02),
        (0.85, 0.72, 0.30, 0.19, 0.03),
        (-0.55, 0.80, 0.33, 0.20, 0.03),
        (-1.85, 0.58, 0.24, 0.16, 0.0)
    ]
    anillos = []
    for y, w, ht, hb, z0 in estaciones_f22:
        pts = [
            Vector((0.0, y, z0 + ht)),
            Vector((w * 0.48, y, z0 + ht * 0.75)),
            Vector((w, y, z0)),
            Vector((w * 0.68, y, z0 - hb * 0.7)),
            Vector((0.0, y, z0 - hb)),
            Vector((-w * 0.68, y, z0 - hb * 0.7)),
            Vector((-w, y, z0)),
            Vector((-w * 0.48, y, z0 + ht * 0.75))
        ]
        anillos.append(pts)
    loft_anillos("Fuselaje_F22", anillos, mat_f22)

    # Cúpula ámbar horizontal
    crear_cupula_cabina_stealth("Cabina_F22", 1.25, 0.22, 1.70, 0.26, 0.20, mat_cabina, mat_oscuro)

    # Tomas de aire supersónicas estilo Caret (2D biseladas)
    for sx in [-1.0, 1.0]:
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(sx * 0.72, 0.70, -0.01))
        toma = bpy.context.active_object
        toma.scale = (0.28, 1.10, 0.24)
        toma.rotation_euler = (math.radians(-4), 0, math.radians(-sx * 10))
        bpy.ops.object.transform_apply(rotation=True, scale=True)
        toma.data.materials.append(mat_oscuro)

    # Alas en diamante características del Raptor
    crear_ala_trapezoidal("Alas_F22", 0.65, 1.15, -1.48, 0.03, 0.11,
                          3.05, -0.42, -1.28, 0.01, 0.03,
                          mat_f22, mat_borde=mat_camo, simetrico_x=True)

    # Estabilizadores horizontales
    crear_ala_trapezoidal("Stabilators_F22", 0.52, -1.45, -2.25, 0.0, 0.06,
                          1.82, -1.90, -2.42, 0.0, 0.02,
                          mat_f22, mat_borde=mat_camo, simetrico_x=True)

    # Derivas dobles cantadas 62°
    for sx in [-1.0, 1.0]:
        bm = bmesh.new()
        v_base = [
            bm.verts.new(Vector((sx * 0.55, -0.85, 0.10))),
            bm.verts.new(Vector((sx * 0.55, -1.96, 0.10))),
            bm.verts.new(Vector((sx * 0.55, -1.90, 0.16))),
            bm.verts.new(Vector((sx * 0.55, -0.92, 0.16)))
        ]
        v_top = [
            bm.verts.new(Vector((sx * 1.08, -1.42, 0.94))),
            bm.verts.new(Vector((sx * 1.08, -2.20, 0.94))),
            bm.verts.new(Vector((sx * 1.05, -2.18, 0.95))),
            bm.verts.new(Vector((sx * 1.05, -1.45, 0.95)))
        ]
        for j in range(4):
            jn = (j + 1) % 4
            f = bm.faces.new([v_base[j], v_base[jn], v_top[jn], v_top[j]] if sx > 0 else [v_top[j], v_top[jn], v_base[jn], v_base[j]])
            f.material_index = 1 if j == 0 else 0
        bm.faces.new(reversed(v_base) if sx > 0 else v_base).material_index = 0
        bm.faces.new(v_top if sx > 0 else reversed(v_top)).material_index = 1
        for f in bm.faces: f.smooth = True
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        mesh = bpy.data.meshes.new(f"Deriva_F22_{sx}_Mesh")
        bm.to_mesh(mesh)
        bm.free()
        obj = bpy.data.objects.new(f"Deriva_F22_{sx}", mesh)
        bpy.context.collection.objects.link(obj)
        obj.data.materials.append(mat_f22)
        obj.data.materials.append(mat_camo)

    # Toberas vectoriales 2D rectangulares con pétalos cerámicos
    for sx in [-0.30, 0.30]:
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(sx, -1.95, 0.02))
        tob2d = bpy.context.active_object
        tob2d.scale = (0.24, 0.38, 0.18)
        bpy.ops.object.transform_apply(scale=True)
        tob2d.data.materials.append(mat_oscuro)

        bpy.ops.mesh.primitive_cone_add(vertices=12, radius1=0.15, radius2=0.015, depth=0.75, location=(sx, -2.35, 0.02))
        llama = bpy.context.active_object
        llama.name = "propulsor"
        llama.rotation_euler = (math.radians(90), 0, 0)
        bpy.ops.object.transform_apply(rotation=True)
        llama.data.materials.append(mat_llama)

    exportar_glb("F22_Raptor")


# ==============================================================================
# 5. HALCÓN CARMESÍ (INTERCEPTOR STEALTH LIGERO ENEMIGO)
# Referencia: ref_flota_enemiga_v3.jpg (Top-Left)
# ==============================================================================
def construir_enemigo_ligero():
    limpiar_escena()
    mat_obsidian = crear_mat("Mat_Halcon_Obsidian", (0.16, 0.18, 0.24, 1.0), metalico=0.82, rugosidad=0.20)
    mat_armadura = crear_mat("Mat_Halcon_Slate", (0.26, 0.29, 0.38, 1.0), metalico=0.78, rugosidad=0.22)
    mat_carmesi = crear_mat("Mat_Halcon_Crimson", (1.0, 0.08, 0.22, 1.0), metalico=0.2, rugosidad=0.15,
                            emision_color=(1.0, 0.08, 0.22, 1.0), emision_fuerza=5.2)
    mat_plasma = crear_mat("Mat_Halcon_Flame", (1.0, 0.15, 0.35, 1.0), metalico=0.1, rugosidad=0.1,
                           emision_color=(1.0, 0.15, 0.35, 1.0), emision_fuerza=5.5)

    estaciones = [
        (3.15, 0.01, 0.01, 0.01, 0.0),
        (2.45, 0.18, 0.10, 0.08, 0.0),
        (1.55, 0.36, 0.18, 0.12, 0.01),
        (0.35, 0.55, 0.28, 0.16, 0.02),
        (-1.45, 0.42, 0.22, 0.14, 0.0)
    ]
    anillos = []
    for y, w, ht, hb, z0 in estaciones:
        pts = [
            Vector((0.0, y, z0 + ht)),
            Vector((w * 0.48, y, z0 + ht * 0.75)),
            Vector((w, y, z0)),
            Vector((w * 0.65, y, z0 - hb * 0.7)),
            Vector((0.0, y, z0 - hb)),
            Vector((-w * 0.65, y, z0 - hb * 0.7)),
            Vector((-w, y, z0)),
            Vector((-w * 0.48, y, z0 + ht * 0.75))
        ]
        anillos.append(pts)
    loft_anillos("Fuselaje_Halcon", anillos, mat_obsidian)

    # Visor rojo sangre horizontal
    crear_cupula_cabina_stealth("Visor_Halcon", 1.15, 0.18, 1.45, 0.20, 0.16, mat_carmesi, mat_obsidian)

    # Ala delta pura agresiva
    crear_ala_trapezoidal("Alas_Halcon", 0.42, 1.25, -1.35, 0.02, 0.11,
                          2.45, -0.65, -1.45, -0.01, 0.03,
                          mat_obsidian, mat_borde=mat_armadura, simetrico_x=True)

    # Canales de circuito energético carmesí trazados sobre el ala delta
    crear_conducto_3d("Canal_Delta_Halcon", [(0.52, 0.85, 0.08), (2.10, -0.58, 0.02), (0.58, -1.05, 0.05), (0.52, 0.85, 0.08)], 0.032, 0.015, mat_carmesi, True)

    # Pods de lanzas láser alares con agujas forward y aft
    for sx in [-1.0, 1.0]:
        cx = sx * 2.45
        bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=0.08, depth=0.85, location=(cx, -0.95, -0.01))
        pod = bpy.context.active_object
        pod.rotation_euler = (math.radians(90), 0, 0)
        bpy.ops.object.transform_apply(rotation=True)
        pod.data.materials.append(mat_armadura)

        bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=0.025, depth=1.20, location=(cx, -0.15, -0.01))
        lanza = bpy.context.active_object
        lanza.rotation_euler = (math.radians(90), 0, 0)
        bpy.ops.object.transform_apply(rotation=True)
        lanza.data.materials.append(mat_carmesi)

    # Derivas verticales dobles inclinadas hacia adentro (Inward-canted)
    for sx in [-1.0, 1.0]:
        bm = bmesh.new()
        v_base = [
            bm.verts.new(Vector((sx * 0.48, -0.45, 0.12))),
            bm.verts.new(Vector((sx * 0.48, -1.42, 0.12))),
            bm.verts.new(Vector((sx * 0.48, -1.38, 0.18))),
            bm.verts.new(Vector((sx * 0.48, -0.50, 0.18)))
        ]
        v_top = [
            bm.verts.new(Vector((sx * 0.38, -0.95, 0.82))),
            bm.verts.new(Vector((sx * 0.38, -1.55, 0.82))),
            bm.verts.new(Vector((sx * 0.36, -1.52, 0.83))),
            bm.verts.new(Vector((sx * 0.36, -0.98, 0.83)))
        ]
        for j in range(4):
            jn = (j + 1) % 4
            f = bm.faces.new([v_base[j], v_base[jn], v_top[jn], v_top[j]] if sx > 0 else [v_top[j], v_top[jn], v_base[jn], v_base[j]])
            f.material_index = 0
        bm.faces.new(reversed(v_base) if sx > 0 else v_base).material_index = 0
        bm.faces.new(v_top if sx > 0 else reversed(v_top)).material_index = 0
        for f in bm.faces: f.smooth = True
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        mesh = bpy.data.meshes.new(f"Deriva_Halcon_{sx}_Mesh")
        bm.to_mesh(mesh)
        bm.free()
        obj = bpy.data.objects.new(f"Deriva_Halcon_{sx}", mesh)
        bpy.context.collection.objects.link(obj)
        obj.data.materials.append(mat_armadura)

    # Toberas gemelas carmesí
    for sx in [-0.25, 0.25]:
        crear_tobera_con_flama(f"Tobera_Halcon_{sx}", sx, -1.45, 0.02, 0.18, 0.30, 0.14, 0.65, mat_armadura, mat_plasma, mat_ring=mat_carmesi)

    exportar_glb("Enemigo_Ligero")


# ==============================================================================
# 6. VÍBORA IMPERIAL (CAZA TÁCTICO DE ASALTO ENEMIGO)
# Referencia: ref_flota_enemiga_v3.jpg (Top-Right)
# ==============================================================================
def construir_enemigo_ataque():
    limpiar_escena()
    mat_purpura = crear_mat("Mat_Vibora_Hull", (0.28, 0.22, 0.40, 1.0), metalico=0.82, rugosidad=0.20)
    mat_acero = crear_mat("Mat_Vibora_Steel", (0.36, 0.30, 0.48, 1.0), metalico=0.85, rugosidad=0.18)
    mat_ambar = crear_mat("Mat_Vibora_Amber", (1.0, 0.58, 0.04, 1.0), metalico=0.2, rugosidad=0.15,
                          emision_color=(1.0, 0.52, 0.0, 1.0), emision_fuerza=5.0)
    mat_visor = crear_mat("Mat_Vibora_Visor", (0.95, 0.08, 0.16, 1.0), metalico=0.88, rugosidad=0.04,
                          emision_color=(0.90, 0.05, 0.12, 1.0), emision_fuerza=0.8)
    mat_plasma = crear_mat("Mat_Vibora_Flame", (1.0, 0.45, 0.05, 1.0), metalico=0.1, rugosidad=0.1,
                           emision_color=(1.0, 0.45, 0.05, 1.0), emision_fuerza=5.5)

    estaciones = [
        (2.95, 0.12, 0.08, 0.08, 0.0),
        (2.35, 0.26, 0.14, 0.11, 0.0),
        (1.60, 0.42, 0.22, 0.15, 0.01),
        (0.25, 0.65, 0.32, 0.20, 0.02),
        (-1.55, 0.52, 0.24, 0.18, 0.0)
    ]
    anillos = []
    for y, w, ht, hb, z0 in estaciones:
        pts = [
            Vector((0.0, y, z0 + ht)),
            Vector((w * 0.48, y, z0 + ht * 0.75)),
            Vector((w, y, z0)),
            Vector((w * 0.68, y, z0 - hb * 0.7)),
            Vector((0.0, y, z0 - hb)),
            Vector((-w * 0.68, y, z0 - hb * 0.7)),
            Vector((-w, y, z0)),
            Vector((-w * 0.48, y, z0 + ht * 0.75))
        ]
        anillos.append(pts)
    loft_anillos("Fuselaje_Vibora", anillos, mat_purpura)

    # Visor rojo rubí horizontal
    crear_cupula_cabina_stealth("Visor_Vibora", 1.15, 0.20, 1.55, 0.22, 0.18, mat_visor, mat_acero)

    # Cañón de morro que sobresale
    bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=0.035, depth=1.1, location=(0, 3.15, -0.05))
    can_morro = bpy.context.active_object
    can_morro.rotation_euler = (math.radians(90), 0, 0)
    bpy.ops.object.transform_apply(rotation=True)
    can_morro.data.materials.append(mat_acero)

    # Canards con borde ámbar
    crear_ala_trapezoidal("Canard_Vibora", 0.38, 1.85, 1.25, 0.04, 0.055,
                          1.45, 1.55, 1.20, 0.06, 0.025,
                          mat_purpura, mat_borde=mat_ambar, simetrico_x=True)

    # Alas principales en flecha invertida con filo brillante ámbar
    crear_ala_trapezoidal("Alas_Vibora", 0.55, 0.52, -1.15, 0.03, 0.12,
                          2.95, 1.02, 0.15, 0.06, 0.038,
                          mat_purpura, mat_borde=mat_ambar, simetrico_x=True)

    # Cañones rotatorios pesados de plasma en la raíz alar
    for sx in [-1.0, 1.0]:
        cx = sx * 1.35
        bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=0.10, depth=1.65, location=(cx, 0.95, -0.05))
        can_alar = bpy.context.active_object
        can_alar.rotation_euler = (math.radians(90), 0, 0)
        bpy.ops.object.transform_apply(rotation=True)
        can_alar.data.materials.append(mat_acero)

        bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=0.075, depth=0.25, location=(cx, 1.85, -0.05))
        boca = bpy.context.active_object
        boca.rotation_euler = (math.radians(90), 0, 0)
        bpy.ops.object.transform_apply(rotation=True)
        boca.data.materials.append(mat_ambar)

    # Botalones gemelos traseros con derivas dobles
    for sx in [-1.0, 1.0]:
        cx = sx * 0.62
        bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=0.18, depth=1.75, location=(cx, -0.85, 0.08))
        bot = bpy.context.active_object
        bot.rotation_euler = (math.radians(90), 0, 0)
        bpy.ops.object.transform_apply(rotation=True)
        bot.data.materials.append(mat_acero)

        bm = bmesh.new()
        v_base = [
            bm.verts.new(Vector((cx, -0.45, 0.18))),
            bm.verts.new(Vector((cx, -1.55, 0.18))),
            bm.verts.new(Vector((cx, -1.50, 0.24))),
            bm.verts.new(Vector((cx, -0.50, 0.24)))
        ]
        v_top = [
            bm.verts.new(Vector((cx + sx * 0.28, -0.98, 0.82))),
            bm.verts.new(Vector((cx + sx * 0.28, -1.72, 0.82))),
            bm.verts.new(Vector((cx + sx * 0.26, -1.70, 0.83))),
            bm.verts.new(Vector((cx + sx * 0.26, -1.00, 0.83)))
        ]
        for j in range(4):
            jn = (j + 1) % 4
            f = bm.faces.new([v_base[j], v_base[jn], v_top[jn], v_top[j]] if sx > 0 else [v_top[j], v_top[jn], v_base[jn], v_base[j]])
            f.material_index = 1 if j == 0 else 0
        bm.faces.new(reversed(v_base) if sx > 0 else v_base).material_index = 0
        bm.faces.new(v_top if sx > 0 else reversed(v_top)).material_index = 1
        for f in bm.faces: f.smooth = True
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        mesh = bpy.data.meshes.new(f"Deriva_Vibora_{sx}_Mesh")
        bm.to_mesh(mesh)
        bm.free()
        obj = bpy.data.objects.new(f"Deriva_Vibora_{sx}", mesh)
        bpy.context.collection.objects.link(obj)
        obj.data.materials.append(mat_purpura)
        obj.data.materials.append(mat_ambar)

        crear_tobera_con_flama(f"Tobera_Vibora_{sx}", cx, -1.68, 0.08, 0.19, 0.32, 0.15, 0.65, mat_acero, mat_plasma, mat_ring=mat_ambar)

    exportar_glb("Enemigo_Ataque")


# ==============================================================================
# 7. GÓLEM PESADO (CAÑONERO CATAMARÁN INDUSTRIAL PESADO)
# Referencia: ref_flota_enemiga_v3.jpg (Bottom-Left)
# ==============================================================================
def construir_enemigo_pesado():
    limpiar_escena()
    mat_metal = crear_mat("Mat_Golem_Gunmetal", (0.24, 0.27, 0.34, 1.0), metalico=0.82, rugosidad=0.28)
    mat_blindaje = crear_mat("Mat_Golem_ArmorPlate", (0.16, 0.19, 0.25, 1.0), metalico=0.85, rugosidad=0.26)
    mat_magenta = crear_mat("Mat_Golem_MagentaHeat", (1.0, 0.12, 0.65, 1.0), metalico=0.2, rugosidad=0.15,
                            emision_color=(1.0, 0.10, 0.62, 1.0), emision_fuerza=5.2)
    mat_flama = crear_mat("Mat_Golem_Flame", (1.0, 0.20, 0.55, 1.0), metalico=0.1, rugosidad=0.1,
                          emision_color=(1.0, 0.20, 0.55, 1.0), emision_fuerza=5.5)

    # 1. Casco central de mando con puente blindado biselado
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0.15, 0.12))
    puente = bpy.context.active_object
    puente.scale = (0.75, 2.6, 0.48)
    bpy.ops.object.transform_apply(scale=True)
    puente.data.materials.append(mat_metal)

    # Visor horizontal del puente de mando
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0.95, 0.28))
    vis_golem = bpy.context.active_object
    vis_golem.scale = (0.58, 0.35, 0.12)
    bpy.ops.object.transform_apply(scale=True)
    vis_golem.data.materials.append(mat_magenta)

    # 2. Puente transversal masivo que une sólidamente el casco central con los botalones
    crear_ala_trapezoidal("Puente_Transversal_Golem", 0.35, 0.85, -1.15, 0.06, 0.35,
                          1.35, 0.70, -0.95, 0.04, 0.30,
                          mat_blindaje, mat_borde=mat_metal, simetrico_x=True)

    # 3. Sponsons alares exteriores simétricos que sostienen la colmena de misiles
    crear_ala_trapezoidal("Sponson_Alar_Golem", 1.35, 0.35, -0.95, 0.02, 0.28,
                          2.15, 0.15, -0.85, 0.0, 0.22,
                          mat_metal, mat_borde=mat_blindaje, simetrico_x=True)

    # 4. Botalones laterales masivos del catamarán y colmenas de misiles
    for sx in [-1.0, 1.0]:
        cx = sx * 1.35
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(cx, 0.25, 0.05))
        casco_lat = bpy.context.active_object
        casco_lat.scale = (0.85, 3.4, 0.56)
        bpy.ops.object.transform_apply(scale=True)
        casco_lat.data.materials.append(mat_blindaje)

        # Toma de aire / morro biselado frontal
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(cx, 1.95, 0.05))
        morro_lat = bpy.context.active_object
        morro_lat.scale = (0.80, 0.50, 0.48)
        morro_lat.rotation_euler = (math.radians(-18), 0, 0)
        bpy.ops.object.transform_apply(rotation=True, scale=True)
        morro_lat.data.materials.append(mat_metal)

        # Cañones frontales rotatorios dobles pesados
        for ox in [-0.22, 0.22]:
            bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=0.08, depth=1.65, location=(cx + ox, 2.35, -0.05))
            tubo = bpy.context.active_object
            tubo.rotation_euler = (math.radians(90), 0, 0)
            bpy.ops.object.transform_apply(rotation=True)
            tubo.data.materials.append(mat_metal)

            bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=0.065, depth=0.25, location=(cx + ox, 3.20, -0.05))
            boca = bpy.context.active_object
            boca.rotation_euler = (math.radians(90), 0, 0)
            bpy.ops.object.transform_apply(rotation=True)
            boca.data.materials.append(mat_magenta)

        # Rejillas de radiador dorsal con louvers magenta
        for ry in [0.75, -0.45]:
            bpy.ops.mesh.primitive_cube_add(size=1.0, location=(cx, ry, 0.35))
            rej = bpy.context.active_object
            rej.scale = (0.55, 0.65, 0.08)
            bpy.ops.object.transform_apply(scale=True)
            rej.data.materials.append(mat_magenta)

        # Baterías de misiles en colmena en el flanco exterior (ambos lados sx)
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(sx * 2.15, -0.35, 0.02))
        pod = bpy.context.active_object
        pod.scale = (0.40, 0.95, 0.40)
        bpy.ops.object.transform_apply(scale=True)
        pod.data.materials.append(mat_metal)

        for my in [-0.15, 0.05, 0.25]:
            for mz in [-0.08, 0.08]:
                bpy.ops.mesh.primitive_cube_add(size=1.0, location=(sx * 2.38, my - 0.35, mz + 0.02))
                tubito = bpy.context.active_object
                tubito.scale = (0.05, 0.12, 0.10)
                bpy.ops.object.transform_apply(scale=True)
                tubito.data.materials.append(mat_magenta)

        # Toberas de cohete de los botalones laterales
        for tz in [-0.12, 0.18]:
            crear_tobera_con_flama(f"Tobera_Golem_{sx}_{tz}", cx, -1.55, tz, 0.20, 0.35, 0.16, 0.75, mat_blindaje, mat_flama, mat_ring=mat_magenta)

    # Toberas centrales
    for sx in [-0.24, 0.24]:
        crear_tobera_con_flama(f"Tobera_Golem_Cent_{sx}", sx, -1.25, 0.12, 0.18, 0.32, 0.14, 0.70, mat_blindaje, mat_flama, mat_ring=mat_magenta)

    exportar_glb("Enemigo_Pesado")


# ==============================================================================
# 8. JEFE FINAL: VALKIRIA VOLCÁNICA X-9 (ACORAZADO AÉREO DREADNOUGHT TRIMARÁN)
# Referencia: ref_flota_enemiga_v3.jpg (Bottom-Right)
# ==============================================================================
def construir_valkiria_boss():
    limpiar_escena()
    mat_obsidian = crear_mat("Mat_Valkiria_Hull", (0.20, 0.23, 0.30, 1.0), metalico=0.82, rugosidad=0.28)
    mat_blindaje = crear_mat("Mat_Valkiria_Armor", (0.28, 0.32, 0.40, 1.0), metalico=0.84, rugosidad=0.26)
    mat_oro = crear_mat("Mat_Valkiria_Gold", (1.0, 0.78, 0.16, 1.0), metalico=0.96, rugosidad=0.12,
                        emision_color=(0.95, 0.65, 0.05, 1.0), emision_fuerza=0.8)
    mat_carmesi = crear_mat("Mat_Valkiria_Crimson", (1.0, 0.20, 0.04, 1.0), metalico=0.2, rugosidad=0.15,
                            emision_color=(1.0, 0.18, 0.02, 1.0), emision_fuerza=4.8)
    mat_magma = crear_mat("Mat_Valkiria_MagmaSun", (1.0, 0.45, 0.05, 1.0), metalico=0.1, rugosidad=0.1,
                          emision_color=(1.0, 0.42, 0.02, 1.0), emision_fuerza=6.5)

    def crear_torreta_naval_doble(nombre, x, y, z, escala=1.0):
        bpy.ops.mesh.primitive_cylinder_add(vertices=16, radius=0.45 * escala, depth=0.22 * escala, location=(x, y, z))
        barbette = bpy.context.active_object
        barbette.data.materials.append(mat_blindaje)

        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(x, y, z + 0.18 * escala))
        casa = bpy.context.active_object
        casa.scale = (0.62 * escala, 0.75 * escala, 0.24 * escala)
        bpy.ops.object.transform_apply(scale=True)
        casa.data.materials.append(mat_obsidian)

        for bx in [-0.16 * escala, 0.16 * escala]:
            bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.075 * escala, depth=1.75 * escala, location=(x + bx, y + 0.85 * escala, z + 0.18 * escala))
            canon = bpy.context.active_object
            canon.rotation_euler = (math.radians(90), 0, 0)
            bpy.ops.object.transform_apply(rotation=True)
            canon.data.materials.append(mat_blindaje)

            bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=0.11 * escala, depth=0.45 * escala, location=(x + bx, y + 0.35 * escala, z + 0.18 * escala))
            manguito = bpy.context.active_object
            manguito.rotation_euler = (math.radians(90), 0, 0)
            bpy.ops.object.transform_apply(rotation=True)
            manguito.data.materials.append(mat_oro)

    # 1. Casco Central del Acorazado (Dreadnought Hull)
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0.2, 0.1))
    casco_cent = bpy.context.active_object
    casco_cent.scale = (1.55, 6.2, 0.85)
    bpy.ops.object.transform_apply(scale=True)
    casco_cent.data.materials.append(mat_obsidian)

    # Proa afilada tipo ariete térmico con hendidura de magma
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 3.6, 0.1))
    proa = bpy.context.active_object
    proa.scale = (1.1, 1.2, 0.75)
    proa.rotation_euler = (math.radians(-14), 0, 0)
    bpy.ops.object.transform_apply(rotation=True, scale=True)
    proa.data.materials.append(mat_blindaje)

    # Fisura vertical de magma en la proa
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 4.18, 0.12))
    fisura = bpy.context.active_object
    fisura.scale = (0.12, 0.08, 0.55)
    bpy.ops.object.transform_apply(scale=True)
    fisura.data.materials.append(mat_carmesi)

    # Torretas del Casco Central: 1 delantera y 1 central
    crear_torreta_naval_doble("Torreta_Central_Proa", 0.0, 2.35, 0.58, escala=1.15)
    crear_torreta_naval_doble("Torreta_Central_Dorsal", 0.0, 0.95, 0.62, escala=1.20)

    # 2. LA CALDERA DEL NÚCLEO DE MAGMA
    bpy.ops.mesh.primitive_cylinder_add(vertices=20, radius=0.82, depth=2.2, location=(0, -1.15, 0.52))
    nucleo = bpy.context.active_object
    nucleo.name = "nucleoJefe"
    nucleo.rotation_euler = (math.radians(90), 0, 0)
    bpy.ops.object.transform_apply(rotation=True)
    nucleo.data.materials.append(mat_magma)

    # 3 Arcos magnéticos pesados de oro de confinamiento sobre el magma
    for ay in [-0.35, -1.15, -1.95]:
        bpy.ops.mesh.primitive_torus_add(major_radius=0.98, minor_radius=0.12, location=(0, ay, 0.52))
        arco = bpy.context.active_object
        arco.rotation_euler = (math.radians(90), 0, 0)
        bpy.ops.object.transform_apply(rotation=True)
        arco.data.materials.append(mat_oro)

    # 3. Trusses dobles de interconexión alar (Delantero y Trasero, dejando el núcleo visible en medio)
    crear_ala_trapezoidal("Truss_Delantero_Valkiria", 0.75, 1.95, 0.75, 0.15, 0.32,
                          3.85, 1.45, 0.55, 0.05, 0.25,
                          mat_blindaje, mat_borde=mat_oro, simetrico_x=True)

    crear_ala_trapezoidal("Truss_Trasero_Valkiria", 0.75, -2.15, -2.95, 0.15, 0.32,
                          3.85, -1.85, -2.75, 0.05, 0.25,
                          mat_blindaje, mat_borde=mat_oro, simetrico_x=True)

    # 4. Dos pontones exteriores (Outrigger pontoons)
    for sx in [-1.0, 1.0]:
        cx = sx * 3.85
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(cx, 0.0, 0.02))
        ponton = bpy.context.active_object
        ponton.scale = (0.95, 5.2, 0.65)
        bpy.ops.object.transform_apply(scale=True)
        ponton.data.materials.append(mat_obsidian)

        # Morro biselado del pontón
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(cx, 2.85, 0.02))
        morro_p = bpy.context.active_object
        morro_p.scale = (0.85, 0.95, 0.55)
        morro_p.rotation_euler = (math.radians(-16), 0, 0)
        bpy.ops.object.transform_apply(rotation=True, scale=True)
        morro_p.data.materials.append(mat_blindaje)

        # Cola biselada del pontón
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(cx, -2.85, 0.02))
        cola_p = bpy.context.active_object
        cola_p.scale = (0.85, 0.75, 0.55)
        cola_p.rotation_euler = (math.radians(16), 0, 0)
        bpy.ops.object.transform_apply(rotation=True, scale=True)
        cola_p.data.materials.append(mat_blindaje)

        # Torretas de los pontones
        crear_torreta_naval_doble(f"Torreta_Ponton_{sx}", cx, 1.15, 0.42, escala=1.0)

        # Tobera del pontón
        crear_tobera_con_flama(f"Tobera_Ponton_{sx}", cx, -2.95, 0.02, 0.34, 0.55, 0.28, 1.15, mat_blindaje, mat_magma, mat_ring=mat_oro)

    # 5. Cuatro toberas Titan centrales del casco dreadnought
    for tx, tz in [(-0.55, 0.22), (0.55, 0.22), (-0.55, -0.15), (0.55, -0.15)]:
        crear_tobera_con_flama(f"Tobera_Titan_{tx}_{tz}", tx, -2.95, tz, 0.30, 0.50, 0.25, 1.10, mat_blindaje, mat_magma, mat_ring=mat_carmesi)

    # Deriva dorsal vertical central del acorazado
    bm = bmesh.new()
    v_base = [
        bm.verts.new(Vector((0.0, -1.85, 0.65))),
        bm.verts.new(Vector((0.0, -2.95, 0.65))),
        bm.verts.new(Vector((0.0, -2.85, 0.75))),
        bm.verts.new(Vector((0.0, -1.95, 0.75)))
    ]
    v_top = [
        bm.verts.new(Vector((0.0, -2.45, 1.65))),
        bm.verts.new(Vector((0.0, -3.15, 1.65))),
        bm.verts.new(Vector((0.0, -3.12, 1.68))),
        bm.verts.new(Vector((0.0, -2.48, 1.68)))
    ]
    for j in range(4):
        jn = (j + 1) % 4
        bm.faces.new([v_base[j], v_base[jn], v_top[jn], v_top[j]])
    bm.faces.new(reversed(v_base)).material_index = 0
    bm.faces.new(v_top).material_index = 0
    for f in bm.faces: f.smooth = True
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new("Deriva_Valkiria_Mesh")
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new("Deriva_Valkiria", mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mat_blindaje)

    exportar_glb("Valkiria_Boss")


# ==============================================================================
# 9. ASTEROIDE VOLCÁNICO Y 10. RECURSOS/ITEMS
# ==============================================================================
def construir_asteroide_volcanico():
    limpiar_escena()
    mat_ast = crear_mat("Mat_Asteroide", (0.35, 0.30, 0.28, 1.0), metalico=0.15, rugosidad=0.85,
                        emision_color=(1.0, 0.35, 0.05, 1.0), emision_fuerza=2.0)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=4, radius=1.0)
    ast = bpy.context.active_object
    mesh = ast.data
    for v in mesh.vertices:
        p = v.co.copy()
        l = p.length
        if l > 0.001:
            u = p / l
            deform = 0.22 * math.sin(u.x * 3.0) * math.cos(u.y * 3.5) + 0.12 * math.sin(u.z * 5.0)
            v.co = p * (1.0 + deform)
    mesh.update()
    ast.scale = (1.3, 1.1, 0.95)
    bpy.ops.object.transform_apply(scale=True)
    ast.data.materials.append(mat_ast)
    bpy.ops.object.shade_smooth()
    exportar_glb("Asteroide_Volcanico")


def construir_items_y_recursos():
    # Item Escudo
    limpiar_escena()
    mat_shield = crear_mat("Mat_Shield_Core", (0.0, 0.88, 1.0, 1.0), metalico=0.2, rugosidad=0.15,
                           emision_color=(0.0, 0.85, 1.0, 1.0), emision_fuerza=3.5)
    mat_ring = crear_mat("Mat_Shield_Ring", (0.8, 0.95, 1.0, 1.0), metalico=0.6, rugosidad=0.2,
                         emision_color=(0.0, 0.65, 0.95, 1.0), emision_fuerza=2.0)
    bpy.ops.mesh.primitive_cylinder_add(vertices=6, radius=0.75, depth=0.35)
    core_s = bpy.context.active_object
    core_s.data.materials.append(mat_shield)
    bpy.ops.mesh.primitive_torus_add(major_radius=1.1, minor_radius=0.11)
    ring1 = bpy.context.active_object
    ring1.data.materials.append(mat_ring)
    bpy.ops.mesh.primitive_torus_add(major_radius=0.95, minor_radius=0.07)
    ring2 = bpy.context.active_object
    ring2.rotation_euler = (math.radians(90), 0, 0)
    bpy.ops.object.transform_apply(rotation=True)
    ring2.data.materials.append(mat_ring)
    exportar_glb("Item_Escudo")

    # Item Reparación
    limpiar_escena()
    mat_rep = crear_mat("Mat_Rep_Core", (0.05, 0.95, 0.42, 1.0), metalico=0.2, rugosidad=0.18,
                        emision_color=(0.05, 0.95, 0.42, 1.0), emision_fuerza=3.5)
    mat_rep_frame = crear_mat("Mat_Rep_Frame", (0.85, 1.0, 0.90, 1.0), metalico=0.5, rugosidad=0.22,
                              emision_color=(0.05, 0.75, 0.35, 1.0), emision_fuerza=1.5)
    bpy.ops.mesh.primitive_cube_add(size=0.9)
    c1 = bpy.context.active_object
    c1.scale = (1.5, 0.45, 0.45)
    bpy.ops.object.transform_apply(scale=True)
    c1.data.materials.append(mat_rep)
    bpy.ops.mesh.primitive_cube_add(size=0.9)
    c2 = bpy.context.active_object
    c2.scale = (0.45, 1.5, 0.45)
    bpy.ops.object.transform_apply(scale=True)
    c2.data.materials.append(mat_rep)
    bpy.ops.mesh.primitive_torus_add(major_radius=1.1, minor_radius=0.08)
    halo = bpy.context.active_object
    halo.data.materials.append(mat_rep_frame)
    exportar_glb("Item_Reparacion")

    # Misil Táctico
    limpiar_escena()
    mat_misil = crear_mat("Mat_Misil", (0.85, 0.88, 0.92, 1.0), metalico=0.6, rugosidad=0.25)
    mat_ojiva = crear_mat("Mat_Ojiva", (1.0, 0.18, 0.22, 1.0), metalico=0.3, rugosidad=0.2,
                          emision_color=(0.95, 0.12, 0.15, 1.0), emision_fuerza=1.5)
    mat_llama = crear_mat("Mat_Llama", (1.0, 0.52, 0.06, 1.0), metalico=0.1, rugosidad=0.1,
                          emision_color=(1.0, 0.52, 0.06, 1.0), emision_fuerza=5.0)
    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.12, depth=1.6)
    misil = bpy.context.active_object
    misil.rotation_euler = (math.radians(90), 0, 0)
    bpy.ops.object.transform_apply(rotation=True)
    misil.data.materials.append(mat_misil)

    bpy.ops.mesh.primitive_cone_add(vertices=12, radius1=0.12, depth=0.48, location=(0, 1.04, 0))
    punta = bpy.context.active_object
    punta.rotation_euler = (math.radians(-90), 0, 0)
    bpy.ops.object.transform_apply(rotation=True)
    punta.data.materials.append(mat_ojiva)

    bpy.ops.mesh.primitive_cone_add(vertices=12, radius1=0.10, depth=0.55, location=(0, -1.05, 0))
    ll = bpy.context.active_object
    ll.name = "propulsor"
    ll.rotation_euler = (math.radians(90), 0, 0)
    bpy.ops.object.transform_apply(rotation=True)
    ll.data.materials.append(mat_llama)
    exportar_glb("Misil_Tactico")


def main():
    print("=== INICIANDO CONSTRUCCIÓN MASTER DE MODELOS 3D (BLENDER 5.2 LTS) ===")
    construir_condor_x()
    construir_f35()
    construir_b2()
    construir_f22()
    construir_enemigo_ligero()
    construir_enemigo_ataque()
    construir_enemigo_pesado()
    construir_valkiria_boss()
    construir_asteroide_volcanico()
    construir_items_y_recursos()
    print("=== TODOS LOS MODELOS 3D MASTER HAN SIDO EXPORTADOS CON ÉXITO ===")


if __name__ == "__main__":
    main()
