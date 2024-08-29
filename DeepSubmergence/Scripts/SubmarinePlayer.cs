using System.Collections.Generic;
using UnityEngine;
using Winch.Util;

namespace DeepSubmergence {
    public class SubmarinePlayer : MonoBehaviour {
        
        private const float DISABLE_MODELS_TIME = 0.1f;
        private const float DIVE_TIME = 1.2f;
        private const float MAX_DIVE_DISTANCE = 3.0f;
        private const float DIVE_ABOVE_FLOOR = 1.0f;
        private const float SHOULD_FOAM = 0.2f;
        private const float SURFACE_THRESHOLD = 0.25f;
        private const float PROP_SPEED = 1440.0f;
        private const float PROP_SPINUP_TIME = 0.4f;
        private const float DIVING_ROTATION = 10.0f;
        private const float DIVE_ROTATION_TIME = 0.25f;
        private const float LIGHT_INTENSITY = 1.0f;
        private const float LIGHT_ANGLE = 140.0f;
        private const float LIGHT_RANGE = 10.0f;
        private const float SURFACE_FILL_RATE = 4.0f;
        private const float DONE_FISHING_TIME = 1.0f;
        private const float TELEPORT_TIME = 2.0f;
        
        private readonly Vector3 SURFACE_MODEL_OFFSET = new Vector3(0.0f, -0.1f, 0.0f);
        private readonly Vector3 LIGHT_POSITION_OFFSET = new Vector3(0.0f, 0.0f, 1.5f);
        private readonly Vector2 WATER_DAMAGE_RANGE = new Vector2(4, 10);

        private const string BOAT_PROXY_NAME = "Boat4";
        private const string FLOOD_WATER_NAME = "deepsubmergence.floodwater";
        private const string ANY_PUMP_NAME = "pumptier";
        private const string ANY_PRESSUREVESSEL_NAME = "pressurevessel";
        private const string BUBBLES_NAME = "SurfaceBubbles";
        
        private class PumpData {
            public string item;
            public Timer timeRemaining;
            
            public PumpData(string item_){
                item = item_;
                timeRemaining = new(Utils.PumpTime(item));
                timeRemaining.Start();
            }
            public bool ShouldDrain(){
                if(timeRemaining.Finished()){
                    timeRemaining.Start();
                    return true;
                }
                return false;
            }
        }
        
        private GameObject cachedDredgePlayer;
        private Player cachedDredgePlayerPlayer;
        private GameObject propeller;
        
        private MeshRenderer submarineMesh;
        private Light playerLight;
        private ParticleSystem cachedBoatParticles;
        private ParticleSystem cachedBubbleParticlesCopy;
        
        private bool onSurface = true;
        private bool moveKeyPressed = false;
        private bool previouslyTeleporting = false;
        private bool previousInDock = false;
        
        private float depthParameter = 0.0f;
        private float depthVelocity = 0.0f;
        private float propAmount = 0.0f;
        private float propAmountVelocity = 0.0f;
        private float pitch = 0.0f;
        private float pitchVelocity = 0.0f;
        private float currentDiveTime = 0.0f;
        private float cachedDiveTimeMax = 0.0f;
        
        private List<PumpData> activePumps = new();
        private Vector3 diveTargetPosition;
        private List<GameObject> boatModelProxies;
        
        private Timer disableTimer = new(DISABLE_MODELS_TIME);
        private Timer teleportTimer = new(TELEPORT_TIME);
        private Timer doneFishingTimer = new(DONE_FISHING_TIME);
        
        void Start(){
            cachedDredgePlayer = DeepSubmergence.instance.dredgePlayer;
            cachedDredgePlayerPlayer = cachedDredgePlayer.GetComponent<Player>();
            
            submarineMesh = GetComponent<MeshRenderer>();
            
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
            
            // Create and setup ship light
            GameObject newLight = new GameObject();
            newLight.name = "[DeepSubmergence] Player light";
            playerLight = newLight.AddComponent<Light>();
            playerLight.type = LightType.Spot;
            playerLight.intensity = LIGHT_INTENSITY;
            playerLight.spotAngle = LIGHT_ANGLE;
            playerLight.range = LIGHT_RANGE;
            
            newLight.transform.parent = transform;
            newLight.transform.localPosition = LIGHT_POSITION_OFFSET;
            
            // Find and copy bubble particles
            GameObject foundBubbles = GameObject.Find(BUBBLES_NAME);
            GameObject bubbleCopy = GameObject.Instantiate(foundBubbles);
            cachedBubbleParticlesCopy = bubbleCopy.GetComponent<ParticleSystem>();
            cachedBubbleParticlesCopy.Stop();
        }
        
        void Update(){
            // Intermittently disable all the other boat models in case they get activated in other ways
            if(disableTimer.Finished()){
                disableTimer.Start();
                ApplyAblingToDredgePlayer();
            }

            // Enable lights
            playerLight.enabled = Utils.IsLightOn();
            
            // Enable model
            bool teleporting = Utils.IsTeleporting();
            if(!previouslyTeleporting && teleporting){
                teleportTimer.Start();
            }
            
            submarineMesh.enabled = teleportTimer.Finished();
            propeller.SetActive(teleportTimer.Finished());
            
            previouslyTeleporting = teleporting;
            
            // Hotkey
            if(Input.GetKeyDown(KeyCode.T)){
                Utils.PutItemInCargo("deepsubmergence.fishboltfish", true);
            }
            
            // Update inputs, movement, position
            UpdateInputs();
            UpdatePositionAndRotation();
            
            UpdateDiveTime();
            UpdateFloodWater();
        }
        
        private void UpdateInputs(){
            CannotDiveReason cannotDiveReason = Utils.CanDive();
            bool inFakeDock = DeepSubmergence.instance.seaBaseFakeDock.playerInFakeDock;
            
            if((cannotDiveReason & CannotDiveReason.InDock) != CannotDiveReason.None){
                onSurface = true;
            } else if(cannotDiveReason == CannotDiveReason.None && !inFakeDock){
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
            
            if(allRaycastHits != null){
                if(allRaycastHits.Length > 0){
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
                } else {
                    diveTargetPosition = dredgePlayerPosition + (Vector3.up * -1.0f) * MAX_DIVE_DISTANCE;
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
        
        private const float baseDiveTime = 10.0f;
        private void UpdateDiveTime(){
            // Recompute dive time any time cargo changes
            if(Utils.CargoChanged()){
                cachedDiveTimeMax = baseDiveTime;
                
                List<SpatialItemInstance> generalItems = GameManager.Instance.SaveData.Inventory.GetAllItemsOfType<SpatialItemInstance>(ItemType.GENERAL);
                
                for(int i = 0, count = generalItems.Count; i < count; ++i){
                    if(generalItems[i].id.Contains(ANY_PRESSUREVESSEL_NAME)){
                        cachedDiveTimeMax += Utils.DiveTime(generalItems[i].id);
                    }
                }
            }
            
            // Set a timer after it's cleared so you don't get back from fishing at 0 time left
            bool currentlyFishing = Utils.CanDive() != CannotDiveReason.None;
            bool inFakeDock = DeepSubmergence.instance.seaBaseFakeDock.playerInFakeDock;
            
            if(currentlyFishing || inFakeDock){
                doneFishingTimer.Start();
            }
            
            if(onSurface){
                currentDiveTime = Mathf.Max(currentDiveTime - (Time.deltaTime * SURFACE_FILL_RATE), 0.0f);
            } else if(doneFishingTimer.Finished() && !inFakeDock){
                currentDiveTime += Time.deltaTime;
            }
            
            // Stayed down too long
            if(currentDiveTime > cachedDiveTimeMax){
                cachedDredgePlayerPlayer.OnCollision();
                
                cachedBubbleParticlesCopy.transform.position = transform.position;
                cachedBubbleParticlesCopy.Emit(50);

                // Force surfacing
                onSurface = true;
                
                // Give player several floodwaters
                for(int i = 0, count = (int) UnityEngine.Random.Range(WATER_DAMAGE_RANGE.x, WATER_DAMAGE_RANGE.y); i < count; ++i){
                    Utils.PutItemInCargo(FLOOD_WATER_NAME, true);
                }
            }
        }
        
        private void UpdateFloodWater(){
            bool inDock = (Utils.CanDive() & CannotDiveReason.InDock) != CannotDiveReason.None;
            
            // Clear all floodwater once arrived at dock
            if(inDock && !previousInDock){
                while(Utils.HasItemInCargo(FLOOD_WATER_NAME)){
                    Utils.DestroyItemInCargo(FLOOD_WATER_NAME);
                }
            }
            
            // Update active pumps if cargo inventory changed
            if(Utils.CargoChanged()){
                activePumps.Clear();

                List<SpatialItemInstance> generalItems = GameManager.Instance.SaveData.Inventory.GetAllItemsOfType<SpatialItemInstance>(ItemType.GENERAL);
                
                for(int i = 0, count = generalItems.Count; i < count; ++i){
                    if(generalItems[i].id.Contains(ANY_PUMP_NAME)){
                        activePumps.Add(new PumpData(generalItems[i].id));
                    }
                }
            }
            
            // If any pump should drain, remove water from inventory
            for(int i = 0, count = activePumps.Count; i < count; ++i){
                if(activePumps[i].ShouldDrain() && Utils.HasItemInCargo(FLOOD_WATER_NAME)){
                    Utils.DestroyItemInCargo(FLOOD_WATER_NAME);
                }
            }
            
            previousInDock = inDock;
        }
        
        public bool OnSurface(){
            return onSurface;
        }
        
        public void ForceSurface(){
            onSurface = true;
        }
        
        public bool CompletelySubmerged(){
            return depthParameter > 0.8f;
        }
        
        public float DiveTimerPercentRemaining(){
            return Mathf.Clamp(1.0f - (currentDiveTime / cachedDiveTimeMax), 0.0f, 1.0f);
        }
        
        public void ApplyAblingToDredgePlayer(){
            if(boatModelProxies == null){
                boatModelProxies = new();
                
                BoatModelProxy[] allBoatModelProxies = UnityEngine.Object.FindObjectsOfType<BoatModelProxy>(true);
                
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