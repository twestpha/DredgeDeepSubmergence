using UnityEngine;
using Winch.Core;

namespace DeepSubmergence {
    public static class Utils {
        
        const string DEFAULT_SHADER_NAME = "Shader Graphs/Lit_Shader";
        
        public static GameObject SetupModelTextureAsGameObject(string name, Mesh mesh, Texture texture){
            // find and copy material from somewhere (?)
            // cache and re-instantiate a copy each time
            
            GameObject newObject = new GameObject();
            newObject.name = "[DeepSubmergence] " + name;
            
            // Setup mesh, texture, material
            MeshFilter newMeshFilter = newObject.AddComponent<MeshFilter>();
            WinchCore.Log.Debug("mod loaded");
            newMeshFilter.mesh = mesh;
            
            Material newMaterial = null; // TODO copy from existing, but put our texture in it
            // There seems to be a per-boat material, so I don't think that's the answer...
            // Consider using "Shader Graphs/Lit_Shader" using https://docs.unity3d.com/ScriptReference/Shader.Find.html
            
            MeshRenderer newMeshRenderer = newObject.AddComponent<MeshRenderer>();
            newMeshRenderer.material = newMaterial;
            
            // Manually manage lifetime
            GameObject.DontDestroyOnLoad(newObject);
            
            return newObject;
        }
    }
}