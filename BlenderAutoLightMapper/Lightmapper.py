import bpy
import os
import sys

import io
from contextlib import redirect_stdout

stdout = io.StringIO()

clear = lambda: os.system('cls')
clear()
objects = bpy.context.selected_objects

for o in objects:
    if o.type == "MESH":
        obj = o
        while True:
            obj.select_set(True)
            obj = obj.parent
            if obj == None:
                break;

i = 0

print ('Start baking lightmaps for ' + str(len(objects))  + ' objects')
print ('')

bpy.context.scene.display_settings.display_device = 'None'

#if bpy.context.scene.cycles.device != 'GPU':
#    print('Setting render device to GPU')
#    bpy.context.scene.cycles.device = 'GPU'
    
for obj in objects:
    for item in objects:
        if item != obj:
            item.select_set(False)
    
    if obj.type == 'MESH':
        i+=1
        image_name = obj.name + '_BakedTexture'
        maxDim = 0
        for dimension in obj.dimensions:
            if dimension > maxDim:
                maxDim = dimension
        lightMapSize = 256
        if 14<= maxDim <=100:
            lightMapSize = 2048
        if 10<= maxDim <=14:
            lightMapSize = 1024
        if 5<= maxDim <=10:
            lightMapSize = 512
        if 3<= maxDim <=5:
            lightMapSize = 256
        if 0<= maxDim <=3:
            lightMapSize = 128
        img = bpy.data.images.new(image_name,lightMapSize,lightMapSize)
        img.colorspace_settings.name = 'Non-Color'
        print ('(' + str(i) + '/' + str(len(objects)) + ')')
        print ('Baking texture for object '+ obj.name + ', resolution '+ str(lightMapSize)+'x' + str(lightMapSize))
        if obj.data.materials != None:
            for mat in obj.data.materials:

                mat.use_nodes = True 
                nodes = mat.node_tree.nodes
                texture_node =nodes.new('ShaderNodeTexImage')
                texture_node.name = 'Bake_node'
                
                uvMap_node = nodes.new('ShaderNodeUVMap')
                links = bpy.context.active_object.active_material.node_tree.links
                print( obj.data.uv_layers)
                uvLayerInd = 1
                if len(obj.data.uv_layers) < 2:
                    print('No secondary uv map found, baking in uvSet_0')
                    uvLayerInd = 0
                print('Baking in uvSet_1')
                uvMap_node.uv_map = obj.data.uv_layers[uvLayerInd].name
                links.new(uvMap_node.outputs[0], texture_node.inputs[0])
                
                texture_node.select = True
                nodes.active = texture_node
                texture_node.image = img
            
            print('[debug] obj = ' + obj.name)
            bpy.context.view_layer.objects.active = obj
            path_dir = 'D:\\Demo\\21_URP_nBox\\New Unity Project\\Assets\\Lightmaps'
            filepathIndir = "{}/{}".format(path_dir,'\\' + obj.name + '\\' + obj.name + '_indirect.jpg')
            filepathDir = "{}/{}".format(path_dir, '\\' + obj.name + '\\' +obj.name + '_direct.jpg')
            
            print('[debug] selected count = ' + str(len(bpy.context.selected_objects)))
            print('Baking indirect lightmap')
            with redirect_stdout(stdout):
                bpy.ops.object.bake(type='DIFFUSE', pass_filter =  {'INDIRECT'}, save_mode='EXTERNAL')
            img.save_render(filepathIndir)
            
            print('Baking direct lightmap')
            with redirect_stdout(stdout):
                bpy.ops.object.bake(type='DIFFUSE', pass_filter =  {'DIRECT'}, save_mode='EXTERNAL')
            img.save_render(filepathDir)
            
            for mat in obj.data.materials:
                for n in mat.node_tree.nodes:
                    if n.name == 'Bake_node ':
                        mat.node_tree.nodes.remove(n)
        print('')                
        for u in objects:
            u.select_set(True)
print ('')
print ('Baking finished')