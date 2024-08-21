using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine;
using Winch.Core;
using Winch.Util;

namespace DeepSubmergence {
    public class SubmarinePlayer : MonoBehaviour {
        
        private const float DISABLE_MODELS_TIME = 0.5f;
        
        private const float DIVE_TIME = 1.2f;
        private const float MAX_DIVE_DISTANCE = 2.0f;
        private const float DIVE_ABOVE_FLOOR = 1.0f;
        
        private const float SHOULD_FOAM = 0.2f;
        
        private const float PROP_SPEED = 1440.0f;
        private const float PROP_SPINUP_TIME = 1.6f;
        private readonly Vector3 SURFACE_MODEL_OFFSET = new Vector3(0.0f, -0.1f, 0.0f);
        
        private GameObject cachedDredgePlayer;
        private GameObject propeller;
        private ParticleSystem cachedBoatParticles;
        private bool onSurface = true;
        private bool moveKeyPressed = false;
        
        private float depthParameter = 0.0f;
        private float depthVelocity = 0.0f;
        
        private float propAmount = 0.0f;
        private float propAmountVelocity = 0.0f;
        
        private Vector3 diveTargetPosition;
                        
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
            
            // Find and cache boat particles
            cachedBoatParticles = GameObject.Find("BoatTrailParticles").GetComponent<ParticleSystem>();
            
            // Create and position propeller
            propeller = Utils.SetupModelTextureAsGameObject(
                "Submarine Propeller",
                ModelUtil.GetModel("deepsubmergence.submarinepropeller"),
                TextureUtil.GetTexture("deepsubmergence.propellertexture")
            );
            propeller.transform.parent = transform;
            propeller.transform.localPosition = Vector3.zero;
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
            // TODO disable switching, moveKeyPressed when
            // - in dock
            // - when tab menu is open
            // - while fishing
            // - probably other times
            
            if(Input.GetKeyDown(KeyCode.Q)){
                onSurface = !onSurface;
            }
            
            moveKeyPressed = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        }
        
        private void UpdatePositionAndRotation(){
            // Racyast to find seafloor for diving
            Vector3 dredgePlayerPosition = cachedDredgePlayer.transform.position;
            
            RaycastHit[] allRaycastHits = Physics.RaycastAll(
                new Ray(dredgePlayerPosition, -Vector3.up), 
                20.0f, 
                Physics.DefaultRaycastLayers, 
                QueryTriggerInteraction.UseGlobal
            );
            
            if(allRaycastHits != null && allRaycastHits.Length > 0){
                for(int i = 0, count = allRaycastHits.Length; i < count; ++i){
                    if(allRaycastHits[i].collider.gameObject.name == "Terrain"){
                        Vector3 toDivePosition = (allRaycastHits[i].point + Vector3.up * DIVE_ABOVE_FLOOR) - dredgePlayerPosition;
                        diveTargetPosition = dredgePlayerPosition + toDivePosition.normalized * Mathf.Min(toDivePosition.magnitude, MAX_DIVE_DISTANCE);
                        break;
                    }
                }
            }
            
            // Always be under the player
            diveTargetPosition.x = dredgePlayerPosition.x;
            diveTargetPosition.z = dredgePlayerPosition.z;

            // If we're high enough up anyway, just be surfaced
            // if(!onSurface && atSurface){
            //     onSurface = true;
            // }
            
            // Drive the parameter towards the target when relevant
            depthParameter = Mathf.SmoothDamp(depthParameter, onSurface ? 0.0f : 1.0f, ref depthVelocity, DIVE_TIME);
            
            // Apply the position based on parameter
            transform.position = Vector3.Lerp(
                dredgePlayerPosition + SURFACE_MODEL_OFFSET, 
                diveTargetPosition,
                CustomMath.EaseInOut(depthParameter)
            );
            // TODO Give diving and surfacing direction-facing tweaks to make it feel more fun
            transform.rotation = cachedDredgePlayer.transform.rotation;
            
            // Disable and enable foam near surface
            if(cachedBoatParticles.isPlaying && depthParameter >= SHOULD_FOAM){
                cachedBoatParticles.Stop();
            } else if(!cachedBoatParticles.isPlaying && depthParameter <= SHOULD_FOAM){
                cachedBoatParticles.Play();
            }
            
            // Propeller goes brrrr
            propAmount = Mathf.SmoothDamp(propAmount, moveKeyPressed ? PROP_SPEED : 0.0f, ref propAmountVelocity, PROP_SPINUP_TIME);
            propeller.transform.localRotation *= Quaternion.Euler(0.0f, 0.0f, propAmount * Time.deltaTime);
        }
        
        public bool OnSurface(){
            return onSurface;
        }
    }
}