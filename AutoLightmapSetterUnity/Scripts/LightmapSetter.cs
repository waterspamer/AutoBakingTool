using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AutoLightmapper.Scripts
{
    public class LightmapSetter : MonoBehaviour
    {
        
        
        [Header("Lightmap Source Settings")] [SerializeField]
        private string lightmapSourcePath;

        [SerializeField][Range(0, 1f)] private float indirectMultiplier = 1f;
        private List<Renderer> lightMappedRenderers;


        private void OnValidate()
        {
            foreach (var renderer in lightMappedRenderers)
            {
                renderer.sharedMaterial.SetFloat("_IndirectMultiplier", indirectMultiplier);
            }
        }

        public void ResetMaterials()
        {
            var gameObjectNames = Directory.GetDirectories(lightmapSourcePath).Select(d => new DirectoryInfo(d).Name).ToList();
            
            foreach (var goName in gameObjectNames)
            {
                var go = GameObject.Find(goName);
                if (go)
                {
                    GameObject.Find(goName).GetComponent<Renderer>().sharedMaterial = null;
                    AssetDatabase.DeleteAsset($"Assets/Lightmaps/{goName}_mat.mat");
                }
                else Debug.Log($"GameObject naming mismatch : {goName}");
            }
            lightMappedRenderers.Clear();
        }
        
        public void SetTextureSets()
        {
            var gameObjectNames = Directory.GetDirectories(lightmapSourcePath).Select(d => new DirectoryInfo(d).Name).ToList();
            foreach (var goName in gameObjectNames)
            {
                if (File.Exists($"Assets/Lightmaps/{goName}_mat.mat"))
                {
                    var gObject = GameObject.Find(goName);
                    if (gObject!= null)
                    {
                        var material = AssetDatabase.LoadAssetAtPath($"Assets/Lightmaps/{goName}_mat.mat", typeof(Material)) as Material;
                        gObject.GetComponent<Renderer>().sharedMaterial = material;
                    }
                    else Debug.LogWarning($"Material {goName}_mat.mat found, but no corresponding GameObject found");
                    continue;
                }
                string directFilename = $"{lightmapSourcePath}/{goName}/{goName}_direct.jpg";
                string indirectFilename = $"{lightmapSourcePath}/{goName}/{goName}_indirect.jpg";
                var rawData = File.ReadAllBytes(directFilename);
                Texture2D texDirect = new Texture2D(2, 2); // Create an empty Texture; size doesn't matter (she said)
                texDirect.LoadImage(rawData);
                Debug.Log($"Direct texture for {goName} loaded");
                rawData = System.IO.File.ReadAllBytes(indirectFilename);
                Texture2D texIndirect = new Texture2D(2, 2); // Create an empty Texture; size doesn't matter (she said)
                texIndirect.LoadImage(rawData);
                Debug.Log($"Indirect texture for {goName} loaded");

                Material myNewMaterial = new Material(Shader.Find("Unlit/Lightmap"));
                
                AssetDatabase.CreateAsset(myNewMaterial, $"Assets/Lightmaps/{goName}_mat.mat");
                var mat = AssetDatabase.LoadAssetAtPath($"Assets/Lightmaps/{goName}_mat.mat", typeof(Material)) as Material;
                var go = GameObject.Find(goName);
                if (go)
                {
                    var rend = GameObject.Find(goName).GetComponent<Renderer>();
                    lightMappedRenderers.Add(rend);
                    rend.sharedMaterial = mat;
                    rend.sharedMaterial.SetTexture("_IndirectLightMap", texIndirect); 
                    rend.sharedMaterial.SetTexture("_DirectLightMap", texDirect); 
                }
                else Debug.Log($"GameObject naming mismatch : {goName}");
            }
        }
    }
}
