using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine;
using Winch.Core;
using Winch.Util;

namespace DeepSubmergence {
    public class DeepSubmergence : MonoBehaviour {
    
        public static DeepSubmergence instance;
        
        // Helpful links
        // https://dredgemods.com/mods/
        // https://github.com/DREDGE-Mods/Winch/blob/5de432bc9657aae5a553bfd654b71853ca4a345b/Winch/Util/TextureUtil.cs#L15
        // https://github.com/Hacktix/Winch/wiki/Mod-Structure
        // https://github.com/DREDGE-Mods/Winch/blob/5de432bc9657aae5a553bfd654b71853ca4a345b/Winch/Core/AssetLoader.cs#L33
        
        // PLAN
        // V0.2: Submarine Specific Collectable Fish, Submarine Parts, ship layout
        // [x] Can dive hookups all around
        // [x] Light that turns on and off based on player light
        // [t] Test all of the powers
        // [x] Robot fish only collectable with submarine while submerged
        // [x] Dive timer, damage if stays down too long, surfacing refills
        
        // V0.X: Submarine specific fishing minigame
        
        // V0.X: Deep submergence (fully underwater map), Underwater Base
        
        // V0.X Questline and characters
        
        public GameObject dredgePlayer;
        public GameObject submarinePlayer;
        public GameObject submarineUI;
        public GameObject debugAxes;
        public List<GameObject> managedObjects = new();
        
        void Awake(){
            instance = this;
            WinchCore.Log.Debug("mod loaded");
            
            ModelUtil.Initialize();
        }
        
        IEnumerator Start(){
            // spin until we find a player
            while(dredgePlayer == null){
                yield return null;
                dredgePlayer = GameObject.Find("Player");
            }
            
            // Instantiate all the objects needed for the mod
            SetupSubmarinePlayer();
            SetupDebugAxes();
            SetupDiveUI();
        }
        
        void Update(){
            if(dredgePlayer == null){
                ShutDown();
            } else {
                // every so often, spawn some new pickups around the player?
            }
        }
        
        private void ShutDown(){
            for(int i = 0, count = managedObjects.Count; i < count; ++i){
                Destroy(managedObjects[i]);
            }
            
            // TODO kick off restart?
            // StartCoroutine(Start());
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
                new Vector2(37.5f, 300.0f)
            );
            
            submarineUI.AddComponent<SubmarineUI>();
        }
    }
}
