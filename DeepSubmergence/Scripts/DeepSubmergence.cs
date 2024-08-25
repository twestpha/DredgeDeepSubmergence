using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine;
using Winch.Core;
using Winch.Util;

namespace DeepSubmergence {
    public class DeepSubmergence : MonoBehaviour {
    
        public static DeepSubmergence instance;
        
        // [/] Pressure vessel art
        
        // V0.3: Underwater Base, Questline and characters
        // [x] underwater base model, texture
        // [x] Find a spot on the map to put base, fish around it
        //   - Docking? Special dock that doesn't surface the boat?
        // [x] Selling pumps and pressure vessels, level up with caught fish
        // [x] Underwater Mechanic tinker character, questline for fishes
        // [x] Re-organize harvest spots
        // [x] Cover art for game start? Just a fullscreen splash on a canvas, not 3D backgroundy
        // [x] Play it a shitload, bugtest, etc.
        
        public GameObject dredgePlayer;
        public GameObject submarinePlayer;
        public GameObject submarineUI;
        public GameObject underwaterFishableManager;
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
            
            setup = true;
        }
        
        void Update(){
            if(setup){
                if(dredgePlayer == null){
                    ShutDown();
                } else {
                    // Not sure yet...
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
    }
}
