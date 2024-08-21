using UnityEngine.UI;
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
        
        const string DEFAULT_GAME_CANVAS_NAME = "GameCanvas";
        private static GameObject cachedGameCanvas;
        
        public static GameObject SetupTextureAsSpriteOnCanvas(string name, Sprite sprite, Vector2 rect, Vector2 position){
            if(cachedGameCanvas == null){
                cachedGameCanvas = GameObject.Find(DEFAULT_GAME_CANVAS_NAME);
            }
            
            if(cachedGameCanvas == null){
                WinchCore.Log.Error("Error finding game canvas");
            }
            
            GameObject newObject = new GameObject();
            newObject.name = "[DeepSubmergence] " + name;
            
            RectTransform newRectTransform = newObject.AddComponent<RectTransform>();
            
            Image newImage = newObject.AddComponent<Image>();
            newImage.sprite = sprite;
            
            newRectTransform.SetParent(cachedGameCanvas.GetComponent<RectTransform>());
            
            // Default to bottom-left corner because reasons
            newRectTransform.anchorMax = Vector2.zero;
            newRectTransform.anchorMin = Vector2.zero;
            
            newRectTransform.sizeDelta = rect;  
            newRectTransform.anchoredPosition = position;
            
            // Manually manage lifetime
            GameObject.DontDestroyOnLoad(newObject);
            DeepSubmergence.instance.managedObjects.Add(newObject);
            return newObject;
        }
        
        public static bool CanDive(){
            // find and cache a bunch of shit
            // then check those when queried
            
            // TODO return false when
            // - in dock
            // - when tab menu is open
            // - while fishing
            // - probably other times
            
            return true;
        }
    }
}