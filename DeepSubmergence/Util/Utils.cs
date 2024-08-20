using UnityEngine;
using Winch.Core;

namespace DeepSubmergence {
    public static class Utils {
        
        const string DEFAULT_SHADER_NAME = "Shader Graphs/Lit_Shader";
        const string DEFAULT_TEXTURE_PROP = "Texture2D_9aa7ba2263944b48bbf43c218dc48459";
        
        public static GameObject SetupModelTextureAsGameObject(string name, Mesh mesh, Texture texture){
            GameObject newObject = new GameObject();
            newObject.name = "[DeepSubmergence] " + name;
            
            // Setup mesh, texture, material
            MeshFilter newMeshFilter = newObject.AddComponent<MeshFilter>();
            WinchCore.Log.Debug("mod loaded");
            newMeshFilter.mesh = mesh;
            
            // Setup material with texture
            Material newMaterial = new Material(Shader.Find(DEFAULT_SHADER_NAME));
            newMaterial.SetTexture(DEFAULT_TEXTURE_PROP, texture);
            
            MeshRenderer newMeshRenderer = newObject.AddComponent<MeshRenderer>();
            newMeshRenderer.material = newMaterial;
            
            // Manually manage lifetime
            GameObject.DontDestroyOnLoad(newObject);
            DeepSubmergence.instance.managedObjects.Add(newObject);
            
            return newObject;
        }
        
        public static GameObject SetupTextureAsSpriteOnCavas(string name, Texture texture){
            return null;
        }
    }
}