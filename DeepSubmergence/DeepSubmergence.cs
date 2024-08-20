using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine;
using Winch.Core;
using Winch.Util;

namespace DeepSubmergence {
    public class DeepSubmergence : MonoBehaviour {
    
        public static DeepSubmergence instance;
        
        // SHORT TERM GOALS
        // [/] Get player submarine model/texture
        
        // V0.1: Submarine Visual Mod (might just stop here lol)
        // [x] Get player sub model replacing current model
        // [x] Get player diving/surfacing (purely visually, raycasting to bottom for max dive) working
        // [x] UI for diving (pick a key, toggle a sprite?)
        
        // V0.2: Submarine Specific Collectable Fish, Submarine Parts, ship layout
        
        // V0.3: Submarine specific minigame and collectable points, 
        
        // V0.4: Deep submergence (underwater map), Underwater Base
        
        // V0.5 Questline and characters
        
        public GameObject dredgePlayer;
        public GameObject submarinePlayer;
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
            
            dredgePlayer = null;
            submarinePlayer = null;
            
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
    }
}
