using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine;
using Winch.Core;
using Winch.Util;

namespace DeepSubmergence {
    public class SubmarinePlayer : MonoBehaviour {
        
        private const float DISABLE_MODELS_TIME = 0.5f;
        private readonly Vector3 surfaceModelOffset = new Vector3(0.0f, 0.0f, 0.0f);
        
        private GameObject cachedDredgePlayer;
        private bool onSurface = true;
        
        private List<GameObject> boatModelProxies = new();
        private Timer disableModelProxiesTimer = new(DISABLE_MODELS_TIME);
    
        void Start(){
            cachedDredgePlayer = DeepSubmergence.instance.dredgePlayer;
            
            // cachedDredgePlayer.GetComponent<BoatModelProxy>();
            
            // iterate children of dredgePlayer, look for BoatModelProxy. SetActiveFalse all of them intermittently
            // boatModelProxies
            disableModelProxiesTimer.Start();
        }
        
        void Update(){
            // Intermittently disable all the other boat models in case they get activated in other ways
            if(disableModelProxiesTimer.Finished()){
                // boatModelProxies
            }
            
            // Copy position every frame of dredgePlayer, push downward into ocean a bit
            // give it some bob and noise when on surface
            // if button, toggle dived state
            // raycast down always to get some distance up from the bottom of the ocean
            // if distance < x, surface anyway
            // Give diving and surfacing direction-facing tweaks to make it feel more fun
        }
    }
}