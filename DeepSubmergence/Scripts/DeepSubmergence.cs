using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine;
using Winch.Core;
using Winch.Util;

namespace DeepSubmergence {
    public class DeepSubmergence : MonoBehaviour {

        public static DeepSubmergence instance;
        
        // [/] balance sell, qty
        
        // V0.3: Underwater Base, Questline and characters
        // [x] Put fish around and leading to new base, 
        // [x] Play it a shitload, bugtest, etc.
        
        // V0.4: Post-tech improvements
        // [x] Selling pumps and pressure vessels, level up with caught fish
        //    - unlock from progression levels 
        //    - sizing objects PITA? How to give them in a way that doesn't suck
        // [x] Balance cost of new parts too
        // [x] Add to readme
        // - Upgrades and Equipment for improving your Submarine
        // and remove the WIP, replace with something like "dev is done but further improvements/bugfixes/expansion are welcome"
        // [x] Cover art for game start? Just a fullscreen splash on a canvas, not 3D backgroundy
        // [x] Play it a shitload, bugtest, etc.
        
        public GameObject dredgePlayer;
        public GameObject submarinePlayer;
        public GameObject submarineUI;
        public GameObject underwaterFishableManager;
        public SeaBaseFakeDock seaBaseFakeDock;
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
            
            // spin until we find a player
            while(dredgePlayer == null){
                yield return null;
                dredgePlayer = GameObject.Find("Player");
            }

            // Instantiate all the objects needed for the mod
            SetupSubmarinePlayer();
            SetupDebugAxes();
            SetupDiveUI();
            SetupFishableManager();
            SetupSeaBase();
            
            setup = true;
        }
        
        void Update(){
            if(setup){
                if(dredgePlayer == null){
                    ShutDown();
                } else {
                    // Constantly reset the freshness of deepsubmergence fish to prevent them from rotting
                    // This is to facilitate quests (i.e. you can always keep them around) but also a bit spooky
                    List<SpatialItemInstance> inventoryItems = GameManager.Instance.SaveData.Inventory.GetAllItemsOfType<SpatialItemInstance>(ItemType.GENERAL);

                    for (int i = 0, count = inventoryItems.Count; i < count; ++i)
                    {
                        if(inventoryItems[i].id.Contains("deepsubmergence") && inventoryItems[i] is FishItemInstance fishy){
                            fishy.freshness = 3.0f; // Max freshness value in game
                        }
                    }
                }
            }
        }

        private void ShutDown(){
            setup = false;
            
            WinchCore.Log.Debug("Resetting");
            
            for(int i = 0, count = managedObjects.Count; i < count; ++i){
                Destroy(managedObjects[i]);
            }
            
            // Kick off a restart waiting for player
            StartCoroutine(Start());
        }
        
        private void SetupSubmarinePlayer(){
            submarinePlayer = Utils.SetupModelTextureAsGameObject(
                "SubmarinePlayer",
                ModelUtil.GetModel("deepsubmergence.submarine"),
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
    }
}
