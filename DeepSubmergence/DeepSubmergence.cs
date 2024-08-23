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
        
        // [/] Screenshot with hero render or something in github readme, punch it up in general, see https://github.com/xen-42/cosmic-horror-fishing-buddies
        
        // V0.2: Submarine Specific Collectable Fish
        // [x] Scatter some harvest areas around the marrows just for now

        // V0.X: Submarine specific fishing minigame
        // [x] Dive timer, damage if stays down too long, surfacing refills. Don't run out while fishing.
        // [x] Parts to improve dive times
        // [x] Other submarine specific parts
        // [x] Make fish locations dive-only
        
        // V0.X: Underwater Base, Questline and characters
        // [x] Submarine item, when you throw it overboard, you become a submarine; dupe and fake a player model hanging out there
        // [x] Find a spot on the map to put these
        // [x] Cover art for game start?
        
        public GameObject dredgePlayer;
        public GameObject submarinePlayer;
        public GameObject submarineUI;
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
    }
}
