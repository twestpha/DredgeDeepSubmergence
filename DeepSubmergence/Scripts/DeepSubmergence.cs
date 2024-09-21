using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Winch.Core;
using Winch.Util;
using System;

namespace DeepSubmergence {
    public class DeepSubmergence : MonoBehaviour {
        
        private const string MOD_ADDRESSABLE_NAME = "twestpha.deepsubmergence_assets_all_95c9b4f47ece71963eb15e8256aaac4f";

        public static DeepSubmergence instance;
        
        // V0.5: More Cool Things
        // [x] Switch over to using addressables for assets, get that pipeline working
        //   - switch models and textures over, leave materials being stolen from things
        //
        // [x] Final long-term reward for completing quest chain
        //   - cool eldritch object that does a cool thing
        // [x] Sonar ping system? Echoes in distance, plays particles on fish? using/replacing horn?
        //   - ping particles needed from addressable
        //   - sound effect needed from addressable
        //
        // [x] Torpedo bait
        //
        // [x] Coupla more fish why not
        //
        // V0.6: Post-tech improvements
        // [x] Replace fake dock with real dock
        //   - rework that whole quest chain :P
        // [x] Selling pumps and pressure vessels, level up with caught fish
        //    - unlock from progression levels
        // [x] Balance cost, efficacy of new parts too
        //
        // [x] Update readme
        // [x] Play it a shitload, bugtest, etc.
        
        public GameObject dredgePlayer;
        public GameObject submarinePlayer;
        public GameObject submarineUI;
        public GameObject underwaterFishableManager;
        public SeaBaseFakeDock seaBaseFakeDock;
        public GameObject questManager;
        public GameObject splashCanvasMenu;
        public GameObject splashArt;
        public GameObject debugAxes;

        public List<GameObject> managedObjects = new();
        
        private bool setup;
        
        void Awake(){
            instance = this;
            WinchCore.Log.Debug("mod loaded");
            
            ModelUtil.Initialize();
        }
        
        IEnumerator Start(){
            
            setup = false;

            yield return Utils.LoadAddressable(MOD_ADDRESSABLE_NAME);
            WinchCore.Log.Debug("Finished loading mod addressable");
            
            // Spin until we find the main menu canvas, then setup splash art
            splashCanvasMenu = null;
            while(splashCanvasMenu == null){
                yield return null;
                splashCanvasMenu = Utils.FindInChildren(GameObject.Find("Canvases"), "MenuCanvas");
            }
            
            SetupAndShowSplashArt();
            
            // Spin until we find a player
            while(dredgePlayer == null){
                yield return null;
                dredgePlayer = GameObject.Find("Player");
            }

            try {
                TeardownSplashArt();
                
                // Instantiate all the objects needed for the mod
                SetupDebugAxes();
                
                SetupQuestManager();
                SetupFishableManager();
                
                SetupSubmarinePlayer();
                SetupDiveUI();
                
                SetupSeaBase();
                
                setup = true;
            } catch (Exception e){
                WinchCore.Log.Error(e.ToString());
            }
        }
        
        void Update(){
            try {
                if(setup){
                    if(dredgePlayer == null){
                        ShutDown();
                    } else {
                        // Constantly reset the freshness of deepsubmergence fish to prevent them from rotting
                        // This is to facilitate quests (i.e. you can always keep them around) but also a bit spooky
                        List<SpatialItemInstance> inventoryItems = GameManager.Instance.SaveData.Inventory.GetAllItemsOfType<SpatialItemInstance>(ItemType.GENERAL);

                        for(int i = 0, count = inventoryItems.Count; i < count; ++i){
                            if(inventoryItems[i].id.Contains("deepsubmergence") && inventoryItems[i] is FishItemInstance fishy){
                                fishy.freshness = 3.0f; // Max freshness value in game
                            }
                        }

                        List<SpatialItemInstance> storageItems = GameManager.Instance.SaveData.Storage.GetAllItemsOfType<SpatialItemInstance>(ItemType.GENERAL);

                        for(int i = 0, count = storageItems.Count; i < count; ++i){
                            if(storageItems[i].id.Contains("deepsubmergence") && storageItems[i] is FishItemInstance fishy){
                                fishy.freshness = 3.0f; // Max freshness value in game
                            }
                        }
                    }
                }
            } catch (Exception e){
                WinchCore.Log.Error(e.ToString());
            }
        }

        private void ShutDown(){
            setup = false;
            
            WinchCore.Log.Debug("Resetting");
            
            for(int i = 0, count = managedObjects.Count; i < count; ++i){
                if(managedObjects != null){
                    Destroy(managedObjects[i]);
                }
            }
            
            // Kick off a restart waiting for player
            StartCoroutine(Start());
        }
        
        private void SetupSubmarinePlayer(){
            submarinePlayer = Utils.SetupModelTextureAsGameObject(
                "SubmarinePlayer",
                Utils.GetAssetFromAddressables<Mesh>("deepsubmergence.submarine"),
                TextureUtil.GetTexture("deepsubmergence.submarinetexture")
            );

            submarinePlayer.AddComponent<SubmarinePlayer>();
        }
        
        private void SetupDebugAxes(){
            debugAxes = Utils.SetupModelTextureAsGameObject(
                "Debug Axes",
                ModelUtil.GetModel("deepsubmergence.debugaxes"),
                null
            );
            
            debugAxes.transform.position = new Vector3(0.0f, -1000.0f, 0.0f);
        }
        
        private void SetupDiveUI(){
            submarineUI = Utils.SetupTextureAsSpriteOnCanvas(
                "Submarine UI",
                TextureUtil.GetSprite("deepsubmergence.uiribbon"),
                new Vector2(75.0f, 50.0f),
                new Vector2(37.5f, 320.0f)
            );
            
            submarineUI.AddComponent<SubmarineUI>();
        }
        
        private void SetupFishableManager(){
            underwaterFishableManager = Utils.SetupGameObject("Underwater Fishable Manager");
            underwaterFishableManager.AddComponent<UnderwaterFishableManager>();
        }

        private void SetupSeaBase(){
            GameObject seaBase = Utils.SetupModelTextureAsGameObject(
                "Sea Base",
                ModelUtil.GetModel("deepsubmergence.seabase"),
                TextureUtil.GetTexture("deepsubmergence.seabasetexture")
            );
            GameObject seaBaseWindows = Utils.SetupModelTextureAsGameObject(
                "Sea Base Windows",
                ModelUtil.GetModel("deepsubmergence.seabasewindows"),
                TextureUtil.GetTexture("deepsubmergence.seabaseemittexture")
            );
            
            seaBaseWindows.transform.parent = seaBase.transform;
            seaBaseWindows.transform.localPosition = Vector3.zero;
            
            // Position the sea base model
            seaBase.transform.position = new Vector3(735.0f, -5.7f, -272.0f);
            seaBase.transform.rotation = Quaternion.Euler(0.0f, 125.0f, 0.0f);

            // Setup sea base fake dock
            GameObject fakeDockObject = Utils.SetupGameObject("Sea Base Fake Dock");
            fakeDockObject.transform.position = new Vector3(730.89f, 0.0f, -276.46f);
            seaBaseFakeDock = fakeDockObject.AddComponent<SeaBaseFakeDock>();
        }
        
        private void SetupQuestManager(){
            questManager = Utils.SetupGameObject("Quest Manager");
            questManager.AddComponent<QuestManager>();
        }
        
        private void SetupAndShowSplashArt(){
            
            
            splashArt = new GameObject();
            splashArt.name = "[DeepSubmergence] Menu Splash Art";
            
            RectTransform newRectTransform = splashArt.AddComponent<RectTransform>();
            
            Image splashArtImage = splashArt.AddComponent<Image>();
            splashArtImage.sprite = TextureUtil.GetSprite("deepsubmergence.uisplashart");
            
            newRectTransform.SetParent(splashCanvasMenu.transform);
            newRectTransform.SetSiblingIndex(0);
            
            // Default to bottom-left corner because reasons
            newRectTransform.anchorMax = Vector2.zero;
            newRectTransform.anchorMin = Vector2.zero;
            
            newRectTransform.sizeDelta = new Vector2(1920.0f, 1080.0f);  
            newRectTransform.anchoredPosition = new Vector2(960.0f, 540.0f);
            
            // Manually manage lifetime
            managedObjects.Add(splashArt);
        }
        
        private void TeardownSplashArt(){
            Destroy(splashArt);
        }
    }
}
