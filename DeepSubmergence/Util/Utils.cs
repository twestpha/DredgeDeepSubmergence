using UnityEngine.UI;
using UnityEngine;
using Winch.Core;

namespace DeepSubmergence {
    
    public enum CannotDiveReason : int {
        None        = 0,
        TabMenuOpen = 1 << 1,
        InDock      = 1 << 2,
        Fishing     = 1 << 3,
        // Probably more things
    }
    
    public static class Utils {
        
        //######################################################################
        // Helper function for finding gameobjects by name in children
        //######################################################################
        public static GameObject FindInChildren(GameObject parent, string gameObjectName){
            for(int i = 0, count = parent.transform.childCount; i < count; ++i){
                if(parent.transform.GetChild(i).gameObject.name.Contains(gameObjectName)){
                    return parent.transform.GetChild(i).gameObject;
                }
            }
            
            return null;
        }
        //######################################################################
        
        //######################################################################
        // Helper function for quick-setting up a gameobject with a model and
        // default material
        //######################################################################
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
        //######################################################################
        
        //######################################################################
        // Helper function for setting up a gameobject with a sprite and putting
        // it in the game canvas
        //######################################################################
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
        //######################################################################
        
        //######################################################################
        // Helper function for detecting our UI state and if we're allowed to
        // dive or we're in other menus
        //######################################################################
        private static Player cachedPlayer = null;
        private static SlidePanel cachedPlayerSlidePanel = null;
        
        private const string PLAYER_NAME = "Player";
        private const string PLAYER_SLIDE_PANEL_NAME = "PlayerSlidePanel";

        public static CannotDiveReason CanDive(){
            // find and cache a bunch of shit
            // then check those when queried
            if(cachedPlayerSlidePanel == null || cachedPlayer == null){
                cachedPlayerSlidePanel = GameObject.Find(PLAYER_SLIDE_PANEL_NAME).GetComponent<SlidePanel>();
                cachedPlayer = GameObject.Find(PLAYER_NAME).GetComponent<Player>();
            }
            
            CannotDiveReason reason = CannotDiveReason.None;
            if(cachedPlayerSlidePanel.isShowing || cachedPlayerSlidePanel.willShow){ reason |= CannotDiveReason.TabMenuOpen; }
            if(cachedPlayer.IsDocked) { reason |= CannotDiveReason.InDock; }
            if(cachedPlayer.IsFishing){ reason |= CannotDiveReason.Fishing; }

            return reason;
        }
        //######################################################################
    }
}