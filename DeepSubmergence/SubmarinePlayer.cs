using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine;
using Winch.Core;
using Winch.Util;

namespace DeepSubmergence {
    public class SubmarinePlayer : MonoBehaviour {
        
        private const float DISABLE_MODELS_TIME = 0.1f;
        
        private const float DIVE_TIME = 1.2f;
        private const float MAX_DIVE_DISTANCE = 2.0f;
        private const float DIVE_ABOVE_FLOOR = 1.0f;
        
        private const float SHOULD_FOAM = 0.2f;
        private const float SURFACE_THRESHOLD = 0.25f;
        
        private const float PROP_SPEED = 1440.0f;
        private const float PROP_SPINUP_TIME = 0.4f;
        
        private readonly Vector3 SURFACE_MODEL_OFFSET = new Vector3(0.0f, -0.1f, 0.0f);
        
        private readonly float DIVING_ROTATION = 10.0f;
        private readonly float DIVE_ROTATION_TIME = 0.25f;
        
         // Pretty sure this is in the base game
        private const string BOAT_PROXY_NAME = "Boat4";
        
        private GameObject cachedDredgePlayer;
        private GameObject propeller;
        private ParticleSystem cachedBoatParticles;
        private bool onSurface = true;
        private bool moveKeyPressed = false;
        
        private float depthParameter = 0.0f;
        private float depthVelocity = 0.0f;
        
        private float propAmount = 0.0f;
        private float propAmountVelocity = 0.0f;
        
        private float pitch = 0.0f;
        private float pitchVelocity = 0.0f;
        
        private Vector3 diveTargetPosition;
                        
        private List<GameObject> boatModelProxies;
        private Timer disableTimer = new(DISABLE_MODELS_TIME);
    
        void Start(){
            cachedDredgePlayer = DeepSubmergence.instance.dredgePlayer;
            
            // Set up and dis/enable the right player gameobjects and components to make this all work
            ApplyAblingToDredgePlayer();
            
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
            if(disableTimer.Finished()){
                disableTimer.Start();
                ApplyAblingToDredgePlayer();
            }
            
            UpdateInputs();
            UpdatePositionAndRotation();
        }
        
        private void UpdateInputs(){
            CannotDiveReason cannotDiveReason = Utils.CanDive();
            
            if((cannotDiveReason & CannotDiveReason.InDock) != CannotDiveReason.None){
                onSurface = true;
            } else if(cannotDiveReason == CannotDiveReason.None){
                if(Input.GetKeyDown(KeyCode.Q)){
                    onSurface = !onSurface;
                }
                
                moveKeyPressed = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
            }
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
                    // We can't catch everything (partly because not everything has a collider) but this get most things
                    // TODO maybe go through all the rocks in the game and add a mesh collider to them :P maybe later
                    bool shouldCollide = allRaycastHits[i].collider.gameObject.name == "Terrain"
                                         || allRaycastHits[i].collider.gameObject.name.Contains("Rock")
                                         || allRaycastHits[i].collider.gameObject.layer == LayerMask.NameToLayer("CollidesWithPlayer");

                    if(shouldCollide){
                        Vector3 toDivePosition = (allRaycastHits[i].point + Vector3.up * DIVE_ABOVE_FLOOR) - dredgePlayerPosition;
                        diveTargetPosition = dredgePlayerPosition + toDivePosition.normalized * Mathf.Min(toDivePosition.magnitude, MAX_DIVE_DISTANCE);
                        break;
                    }
                }
            }
            
            // Always be under the player
            diveTargetPosition.x = dredgePlayerPosition.x;
            diveTargetPosition.z = dredgePlayerPosition.z;
            
            // Drive the parameter towards the target when relevant
            float previousDepthParameter = depthParameter;
            depthParameter = Mathf.SmoothDamp(depthParameter, onSurface ? 0.0f : 1.0f, ref depthVelocity, DIVE_TIME);
            bool diving = previousDepthParameter > depthParameter && depthParameter > 0.02f && depthParameter < 0.98f;
            bool surfacing = previousDepthParameter < depthParameter && depthParameter > 0.02f && depthParameter < 0.98f;
            
            // Apply the position based on parameter
            Vector3 previousPosition = transform.position;
            transform.position = Vector3.Lerp(
                dredgePlayerPosition + SURFACE_MODEL_OFFSET, 
                diveTargetPosition,
                CustomMath.EaseInOut(depthParameter)
            );
            
            //  Pitch the submarine model when it's diving/surfacing for fun visuals
            Quaternion baseRotation = Quaternion.LookRotation(cachedDredgePlayer.transform.forward);
            float distanceFromMidpoint = 1.0f - (Mathf.Abs(depthParameter - 0.5f) * 2.0f);
            float targetPitch = 0.0f;
            
            if(diving){
                targetPitch = -DIVING_ROTATION * distanceFromMidpoint;
            } else if(surfacing){
                targetPitch = DIVING_ROTATION * distanceFromMidpoint;
            }
            pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVelocity, DIVE_ROTATION_TIME);
            
            transform.rotation = baseRotation * Quaternion.Euler(pitch, 0.0f, 0.0f);
            
            // If we're high enough up anyway, just be surfaced
            bool visuallySurfacing = previousPosition.y < transform.position.y;
            if(!onSurface && visuallySurfacing && diveTargetPosition.y > dredgePlayerPosition.y - SURFACE_THRESHOLD){
                onSurface = true;
            }
            
            // Enable foam particles near surface, disable when dived far enough
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
        
        public void ApplyAblingToDredgePlayer(){
            if(boatModelProxies == null){
                boatModelProxies = new();
                
                BoatModelProxy[] allBoatModelProxies = Object.FindObjectsOfType<BoatModelProxy>();
                
                foreach(BoatModelProxy b in allBoatModelProxies){
                    b.gameObject.SetActive(false);
                    boatModelProxies.Add(b.gameObject);
                }
            }
            
            // Disable all mesh renderers, but leave one specific boat model on for it's collider.
            // But then also disable all of the children.
            for(int i = 0, icount = boatModelProxies.Count; i < icount; ++i){
                boatModelProxies[i].SetActive(boatModelProxies[i].gameObject.name == BOAT_PROXY_NAME);
                boatModelProxies[i].GetComponent<MeshRenderer>().enabled = false;
                
                for(int j = 0, jcount = boatModelProxies[i].transform.childCount; j < jcount; ++j){
                    boatModelProxies[i].transform.GetChild(j).gameObject.SetActive(false);
                }
                
            }
        }
    }
}