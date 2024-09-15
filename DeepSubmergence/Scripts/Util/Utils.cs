using UnityEngine.UI;
using UnityEngine;
using Winch.Core;
using System.Collections.Generic;
using System.Linq;
using System;
using Sirenix.OdinInspector;
using Winch.Serialization.Item;

namespace DeepSubmergence {
    
    public enum CannotDiveReason : int {
        None        = 0,
        TabMenuOpen = 1 << 1,
        InDock      = 1 << 2,
        Fishing     = 1 << 3,
        PauseMenu   = 1 << 4,
        InFakeDock  = 1 << 5,
        // Probably more things
    }
    
    public static class Utils {
        
        //######################################################################
        // Helper function for getting pump amount based on tier
        //######################################################################
        public static float PumpTime(string tier){
            if(tier == "deepsubmergence.pumptier1"){
                return 40.0f;
            } else if(tier == "deepsubmergence.pumptier2"){
                return 15.0f;
            } else if(tier == "deepsubmergence.pumptier3"){
                return 5.0f;
            }
            return 0.0f;
        }
        //######################################################################
        
        //######################################################################
        // Helper function for getting additional dive time for pressure 
        // vessels based on tier
        //######################################################################
        public static float DiveTime(string tier){
            if(tier == "deepsubmergence.pressurevesseltier1"){
                return 7.0f;
            } else if(tier == "deepsubmergence.pressurevesseltier2"){
                return 28.0f;
            } else if(tier == "deepsubmergence.pressurevesseltier3"){
                return 60.0f;
            }
            return 0.0f;
        }
        //######################################################################
        
        //######################################################################
        // Helper function for finding gameobjects by name in children
        //######################################################################
        public static GameObject FindInChildren(GameObject parent, string gameObjectName){
            if(parent == null){ return null; }
            
            for(int i = 0, count = parent.transform.childCount; i < count; ++i){
                if(parent.transform.GetChild(i).gameObject.name.Contains(gameObjectName)){
                    return parent.transform.GetChild(i).gameObject;
                }
            }
            
            return null;
        }
        //######################################################################
        
        //######################################################################
        // Helper function for quick-setting up an empty gameobject
        //######################################################################
        public static GameObject SetupGameObject(string name){
            GameObject newObject = new GameObject();
            newObject.name = "[DeepSubmergence] " + name;
            
            // Manually manage lifetime
            GameObject.DontDestroyOnLoad(newObject);
            DeepSubmergence.instance.managedObjects.Add(newObject);
            return newObject;
        }
        
        //######################################################################
        
        //######################################################################
        // Helper function for quick-setting up a copied gameobject
        //######################################################################
        public static GameObject SetupGameObject(string name, GameObject original){
            GameObject newObject = GameObject.Instantiate(original);
            newObject.name = "[DeepSubmergence] " + name;
            
            // Manually manage lifetime
            GameObject.DontDestroyOnLoad(newObject);
            DeepSubmergence.instance.managedObjects.Add(newObject);
            return newObject;
        }
        //######################################################################
        
        //######################################################################
        // Helper function for quick-setting up a gameobject with a model and
        // default material
        //######################################################################
        private const string DEFAULT_SHADER_NAME = "Shader Graphs/Lit_Shader";
        private const string DEFAULT_TEXTURE_PROP = "Texture2D_9aa7ba2263944b48bbf43c218dc48459";
        
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
        public static GameObject cachedGameCanvas;
        
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
        private static GameObject cachedInnerSettingsPanel = null;
        
        private const string PLAYER_NAME = "Player";
        private const string PLAYER_SLIDE_PANEL_NAME = "PlayerSlidePanel";
        private const string SETTINGS_PANEL_NAME = "SettingsDialog";
        private const string INNER_SETTINGS_PANEL_NAME = "TabbedPanelContainer";

        public static CannotDiveReason CanDive(){
            if(cachedPlayerSlidePanel == null || cachedPlayer == null){
                cachedPlayerSlidePanel = GameObject.Find(PLAYER_SLIDE_PANEL_NAME).GetComponent<SlidePanel>();
                cachedPlayer = GameObject.Find(PLAYER_NAME).GetComponent<Player>();
                
                cachedInnerSettingsPanel = FindInChildren(GameObject.Find(SETTINGS_PANEL_NAME), INNER_SETTINGS_PANEL_NAME);
            }
            
            CannotDiveReason reason = CannotDiveReason.None;
            if(cachedPlayerSlidePanel.isShowing || cachedPlayerSlidePanel.willShow){ reason |= CannotDiveReason.TabMenuOpen; }
            if(cachedPlayer.IsDocked) { reason |= CannotDiveReason.InDock; }
            if(cachedPlayer.IsFishing){ reason |= CannotDiveReason.Fishing; }
            if(cachedInnerSettingsPanel.activeSelf){ reason |= CannotDiveReason.PauseMenu; }
            if(DeepSubmergence.instance.seaBaseFakeDock.playerInFakeDock){ reason |= CannotDiveReason.InFakeDock; }
            

            return reason;
        }
        //######################################################################
        
        //######################################################################
        // Helper function for detecting using the manifest power
        //######################################################################
        
        private static ParticleSystem cachedManifestEffectA = null;
        private static ParticleSystem cachedManifestEffectB = null;
        
        private const string PLAYER_TELEPORT_NAME = "PlayerTeleport";
        private const string TELEPORT_EFFECT_NAME = "TeleportEffect";
        private const string MANIFEST_A_NAME = "ReassembleShards";
        private const string MANIFEST_B_NAME = "blackSmokeCloud";
        
        public static bool IsTeleporting(){        
            if(cachedManifestEffectA == null || cachedManifestEffectB == null){
                GameObject playerTeleport = GameObject.Find(PLAYER_TELEPORT_NAME);
                GameObject teleportEffect = FindInChildren(playerTeleport, TELEPORT_EFFECT_NAME);
                
                GameObject manifestA = FindInChildren(teleportEffect, MANIFEST_A_NAME);
                cachedManifestEffectA = manifestA?.GetComponent<ParticleSystem>();
                GameObject manifestB = FindInChildren(teleportEffect, MANIFEST_B_NAME);
                cachedManifestEffectB = manifestB?.GetComponent<ParticleSystem>();
            }
            
            return (cachedManifestEffectA != null && cachedManifestEffectB != null)
              && (cachedManifestEffectA.isPlaying || cachedManifestEffectB.isPlaying);
        }
        //######################################################################
        
        //######################################################################
        
        //######################################################################
        // Helper function for detecting using the manifest power
        //######################################################################
        private static GameObject cachedPersonalSanityModifier;
        
        private const string PERSONAL_SANITY_MODIFIER_NAME = "PersonalSanityModifier";
        
        public static bool IsLightOn(){
            if(cachedPersonalSanityModifier == null){
                // For some reason, this enables/disables when lights are used. So, we'll use that
                cachedPersonalSanityModifier = GameObject.Find(PERSONAL_SANITY_MODIFIER_NAME);
            }

            return cachedPersonalSanityModifier != null && cachedPersonalSanityModifier.activeSelf;
        }
        //######################################################################

        //######################################################################
        // Helper function for finding an item
        //######################################################################
        private static List<ItemData> allItems;
        
        public static ItemData FindItemByName(string name){
            if(allItems == null){
                allItems = GameManager.Instance.ItemManager.allItems;
            }
            
            for(int i = 0, count = allItems.Count; i < count; i++){
                if(allItems[i].name.Contains(name)){
                    return allItems[i];
                }
            }
            
            return null;
        }
        //######################################################################
    
        //######################################################################
        // Helper function for putting an item into the player's cargo
        //######################################################################
        public static void PutItemInCargo(string item, bool notify = false){
            WinchCore.Log.Debug("putting " + item);
            if (notify || true){
                GameManager.Instance.DialogueRunner.AddItemById(item);
            } else {
                SpatialItemData foundItem = null;
                List<ItemData> allItems = GameManager.Instance.ItemManager.allItems;

                for(int i = 0, count = allItems.Count; i < count; ++i){
                    if(allItems[i].id.Contains(item) && allItems[i] is SpatialItemData spacialItemData)
                    {
                        foundItem = spacialItemData;
                        break;
                    }
                }

                if(foundItem != null){
                    SpatialItemInstance newItem = new SpatialItemInstance();
                    newItem._itemData = foundItem;

                     WinchCore.Log.Debug("newItem: " + newItem);

                    //GameManager.Instance.SaveData.Inventory as 

                    // None of these work, with 'not implemented' errors...
                    //GameManager.Instance.SaveData.Inventory.ForceTriggerItemAddEvent(newItem);
                    //GameManager.Instance.SaveData.Inventory.AddObjectToGridData(newItem, new Vector3Int(-1, -1, 0), false);
                    //GameManager.Instance.SaveData.Inventory.FindSpaceAndAddObjectToGridData(foundItem, false);
                    GameManager.Instance.SaveData.Inventory.TriggerRefreshEvent();
                }
            }
        }

        //######################################################################
        
        //######################################################################
        // Helper function for testing if an item is present in the player's cargo
        //######################################################################
        public static bool HasItemInCargo(string item){
            return GameManager.Instance.SaveData.HasAnyOfTheseItemsInInventory(new string[]{ item }, true);
        }
        //######################################################################
        
        //######################################################################
        // Helper function for testing if an item is present in the player's cargo
        //######################################################################
        public static void DestroyItemInCargo(string item, bool notify = false){
            if(HasItemInCargo(item)){
                if(notify){
                    GameManager.Instance.DialogueRunner.RemoveItemById(item);
                } else {
                    List<SpatialItemInstance> inventoryItems = GameManager.Instance.SaveData.Inventory.GetAllItemsOfType<SpatialItemInstance>(ItemType.GENERAL);

                    SpatialItemInstance foundInstance = null;
                    for (int i = 0, count = inventoryItems.Count; i < count; ++i)
                    {
                        if (inventoryItems[i].id.Contains(item))
                        {
                            foundInstance = inventoryItems[i];
                            break;
                        }
                    }

                    if (foundInstance != null)
                    {
                        GameManager.Instance.SaveData.Inventory.RemoveObjectFromGridData(foundInstance, false);
                        GameManager.Instance.SaveData.Inventory.TriggerRefreshEvent();
                    }
                }
            }
        }
        //######################################################################
        private static int previousFilledCells;
        public static bool CargoChanged()
        {
            int filledCells = GameManager.Instance.SaveData.Inventory.GetFilledCells(ItemSubtype.ALL);
            bool changed = filledCells != previousFilledCells;
            previousFilledCells = filledCells;
            return changed;
        }
    }
}