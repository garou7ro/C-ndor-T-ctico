import bpy
import os
import math
from mathutils import Vector

OUT_DIR = r"C:\Users\kreiv\.gemini\antigravity\brain\e3dfc0d3-6b4d-4e81-844b-2e7afcb48a5a\renders_preview"
MODELS_DIR = r"C:\Users\kreiv\OneDrive\Documentos\JuegoAviones\CondorTactico\wwwroot\assets\modelos"

os.makedirs(OUT_DIR, exist_ok=True)

MODELOS = [
    "Condor_X",
    "F35_Lightning_II",
    "B2_Spirit",
    "F22_Raptor",
    "Enemigo_Ligero",
    "Enemigo_Ataque",
    "Enemigo_Pesado",
    "Valkiria_Boss"
]

def render_model(model_name):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    
    # Importar GLB
    glb_path = os.path.join(MODELS_DIR, f"{model_name}.glb")
    if not os.path.exists(glb_path):
        print(f"No existe {glb_path}")
        return
        
    bpy.ops.import_scene.gltf(filepath=glb_path)
    
    # Calcular centro y escala del modelo
    objs = [o for o in bpy.data.objects if o.type == 'MESH']
    if not objs:
        return
        
    min_co = Vector((1e9, 1e9, 1e9))
    max_co = Vector((-1e9, -1e9, -1e9))
    for obj in objs:
        for v in obj.bound_box:
            world_co = obj.matrix_world @ Vector(v)
            for i in range(3):
                min_co[i] = min(min_co[i], world_co[i])
                max_co[i] = max(max_co[i], world_co[i])
                
    center = (min_co + max_co) * 0.5
    size = max_co - min_co
    max_dim = max(size.x, size.y, size.z)
    
    # Configurar Cámara isométrica / 3/4 top-down
    cam_data = bpy.data.cameras.new("StudioCamera")
    cam_data.lens = 65
    cam_obj = bpy.data.objects.new("StudioCamera", cam_data)
    bpy.context.collection.objects.link(cam_obj)
    
    dist = max_dim * 1.85
    # Ángulo 3/4 frontal visto desde arriba a la derecha (igual que las hojas de referencia)
    cam_obj.location = Vector((center.x + dist * 0.65, center.y + dist * 0.95, center.z + dist * 0.85))
    
    # Apuntar cámara al centro
    dir_vec = center - cam_obj.location
    rot_quat = dir_vec.to_track_quat('-Z', 'Y')
    cam_obj.rotation_euler = rot_quat.to_euler()
    
    bpy.context.scene.camera = cam_obj
    
    # Configurar Iluminación de Estudio Profesional de 3 puntos
    # Luz Principal (Key Light)
    key_light_data = bpy.data.lights.new(name="KeyLight", type='SUN')
    key_light_data.energy = 4.5
    key_light_data.color = (0.95, 0.98, 1.0)
    key_light_obj = bpy.data.objects.new(name="KeyLight", object_data=key_light_data)
    key_light_obj.rotation_euler = (math.radians(50), math.radians(20), math.radians(-35))
    bpy.context.collection.objects.link(key_light_obj)
    
    # Luz de Relleno (Fill Light)
    fill_light_data = bpy.data.lights.new(name="FillLight", type='SUN')
    fill_light_data.energy = 2.0
    fill_light_data.color = (0.4, 0.55, 0.75)
    fill_light_obj = bpy.data.objects.new(name="FillLight", object_data=fill_light_data)
    fill_light_obj.rotation_euler = (math.radians(35), math.radians(-25), math.radians(130))
    bpy.context.collection.objects.link(fill_light_obj)
    
    # Luz de Contorno (Rim Light / Cyber glow)
    rim_light_data = bpy.data.lights.new(name="RimLight", type='SUN')
    rim_light_data.energy = 3.2
    rim_light_data.color = (0.3, 0.8, 1.0) if "Condor" in model_name or "B2" in model_name else (1.0, 0.3, 0.2)
    rim_light_obj = bpy.data.objects.new(name="RimLight", object_data=rim_light_data)
    rim_light_obj.rotation_euler = (math.radians(-40), math.radians(30), math.radians(45))
    bpy.context.collection.objects.link(rim_light_obj)
    
    # Fondo de estudio oscuro con rejilla sutil
    world = bpy.data.worlds.new("StudioWorld")
    world.use_nodes = True
    bg_node = world.node_tree.nodes['Background']
    bg_node.inputs['Color'].default_value = (0.05, 0.065, 0.08, 1.0)
    bg_node.inputs['Strength'].default_value = 0.5
    bpy.context.scene.world = world
    
    # Render settings
    scene = bpy.context.scene
    scene.render.resolution_x = 768
    scene.render.resolution_y = 768
    scene.render.film_transparent = False
    scene.render.image_settings.file_format = 'PNG'
    scene.render.filepath = os.path.join(OUT_DIR, f"{model_name}.png")
    
    bpy.ops.render.render(write_still=True)
    print(f"Render completado: {model_name}.png")

def main():
    print("=== INICIANDO RENDERIZADO DE VISTAS PREVIAS DE MODELOS 3D ===")
    for m in MODELOS:
        render_model(m)
    print("=== TODOS LOS RENDERS HAN SIDO COMPLETADOS CON ÉXITO ===")

if __name__ == "__main__":
    main()
