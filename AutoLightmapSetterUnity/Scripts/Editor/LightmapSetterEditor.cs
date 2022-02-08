using UnityEditor;
using UnityEngine;

namespace AutoLightmapper.Scripts.Editor
{
    [CustomEditor(typeof(LightmapSetter))]
    public class LightmapSetterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Set baked textures"))
            {
                (target as LightmapSetter)?.SetTextureSets();
            }
            
            if (GUILayout.Button("Reset materials"))
            {
                (target as LightmapSetter)?.ResetMaterials();
            }
        }
    }
}
