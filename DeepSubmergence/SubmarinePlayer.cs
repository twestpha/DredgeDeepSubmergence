using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine;
using Winch.Core;
using Winch.Util;

namespace DeepSubmergence {
    public class SubmarinePlayer : MonoBehaviour {
        
        private const float DISABLE_MODELS_TIME = 0.5f;
        private const float DIVE_TIME = 1.2f;
        private readonly Vector3 SURFACE_MODEL_OFFSET = new Vector3(0.0f, -0.1f, 0.0f);
        
        private GameObject cachedDredgePlayer;
        private bool onSurface = true;
        private float depthParameter = 0.0f;
        private float depthVelocity = 0.0f;
                
        private List<GameObject> boatModelProxies = new();
        private Timer disableModelProxiesTimer = new(DISABLE_MODELS_TIME);
    
        void Start(){
            cachedDredgePlayer = DeepSubmergence.instance.dredgePlayer;
            
            // Hide player boat models
            boatModelProxies.Clear();
            BoatModelProxy[] allBoatModelProxies = Object.FindObjectsOfType<BoatModelProxy>();
            foreach(BoatModelProxy b in allBoatModelProxies){
                b.gameObject.SetActive(false);
                boatModelProxies.Add(b.gameObject);
            }

            disableModelProxiesTimer.Start();
        }
        
        void Update(){
            // Intermittently disable all the other boat models in case they get activated in other ways
            if(disableModelProxiesTimer.Finished()){
                disableModelProxiesTimer.Start();
                
                for(int i = 0, count = boatModelProxies.Count; i < count; ++i){
                    boatModelProxies[i].SetActive(false);
                }
            }
            
            UpdateInputs();
            UpdatePositionAndRotation();
        }
        
        private void UpdateInputs(){
            if(Input.GetKeyDown(KeyCode.V)){
                onSurface = !onSurface;
            }
        }
        
        private void UpdatePositionAndRotation(){        
            // TODO raycast down always to get some distance up from the bottom of the ocean
            // if distance < x, just be surfaced anyway
            Vector3 diveTargetPosition = SURFACE_MODEL_OFFSET;
            
            // Drive the parameter towards the target when relevant
            depthParameter = Mathf.SmoothDamp(depthParameter, onSurface ? 0.0f : 1.0f, ref depthVelocity, DIVE_TIME);
            
            // Apply the position based on parameter
            transform.position = cachedDredgePlayer.transform.position + Vector3.Lerp(
                SURFACE_MODEL_OFFSET, 
                diveTargetPosition, 
                CustomMath.EaseInOut(depthParameter)
            );
            // TODO Give diving and surfacing direction-facing tweaks to make it feel more fun
            transform.rotation = cachedDredgePlayer.transform.rotation;
        }
    }
}