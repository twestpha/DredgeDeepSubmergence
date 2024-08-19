using UnityEngine;
using Winch.Core;

namespace DeepSubmergence {
    public static class Utils {    
        public static GameObject SetupModelTextureAsGameObject(string name, Mesh mesh, Texture texture){
            // find and steal material from somewhere (?)
            // cache and re-instantiate a copy each time
            
            GameObject newObject = new GameObject();
            newObject.name = "[DeepSubmergence] " + name;
            
            // Setup mesh, texture, material
            MeshFilter newMeshFilter = newObject.AddComponent<MeshFilter>();
            newMeshFilter.sharedMesh = mesh;
            
            Material newMaterial = null; // TODO copy from existing, but put our texture in it
            
            MeshRenderer newMeshRenderer = newObject.AddComponent<MeshRenderer>();
            newMeshRenderer.material = newMaterial;
            
            // Manually manage lifetime
            GameObject.DontDestroyOnLoad(newObject);
            
            return newObject;
        }
    }
}