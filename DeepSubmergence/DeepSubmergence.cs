using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine;
using Winch.Core;
using Winch.Util;

namespace DeepSubmergence {
    public class DeepSubmergence : MonoBehaviour {
    
        public static DeepSubmergence instance;
        
        // SHORT TERM GOALS
        // [/] Get mod loading at all
        
        // V0.1: Submarine Visual Mod (might just stop here lol)
        // [x] Get data baking and formatting into baked assets, models and images *only* for now
        //     - from python script baking into c# code
        // [x] Get player submarine model in and replacing current model
        // [x] Get player diving/surfacing (purely visually, raycasting to bottom for max dive) working
        // [x] UI for diving (pick a key, toggle a sprite?)
        
        // V0.2: Submarine Specific Collectable Fish, Submarine Parts
        
        // V0.3: Submarine specific minigame and collectable points, 
        
        // V0.4: Deep submergence (underwater map), Underwater Base
        
        // V0.5 Questline and characters
        
        public GameObject dredgePlayer;
        public GameObject submarinePlayer;
        private List<GameObject> managedObjects = new();
        
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
        }
        
        void Update(){
            if(dredgePlayer == null){
                // shut ourselves down
                // Cleanup managedObjects
                
                // dredgePlayer = null;
                // submarinePlayer = null;
            } else {
                // every so often, spawn some new pickups around the player
            }
        }
        
        private void SetupSubmarinePlayer(){
            submarinePlayer = Utils.SetupModelTextureAsGameObject(
                "SubmarinePlayer",
                ModelUtil.GetModel("deepsubmergence.testmodel"),
                TextureUtil.GetTexture("deepsubmergence.testtexture")
            );
            
            // player.AddComponent<SubmarinePlayer>();
            
            // iteracte children of dredgePlayer, look for BoatModelProxy. SetActiveFalse all of them intermittently
            // Copy position every frame of dredgePlayer, push downward into ocean

            managedObjects.Add(submarinePlayer);
        }
    }
}
