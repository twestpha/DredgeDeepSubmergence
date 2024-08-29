using UnityEngine;
using UnityEngine.UI;
using Winch.Core;
using System.Collections;
using Winch.Util;
using TMPro;

namespace DeepSubmergence {
    public class SeaBaseFakeDock : MonoBehaviour {
        
        private string[] quest0requiredfish = {
            "deepsubmergence.fishboltfish",
            "deepsubmergence.fishnetsquid",
            "deepsubmergence.fishshoal",
        };
        private string[] quest1requiredfish = {
            "deepsubmergence.fishelectriceel",
            "deepsubmergence.fishironlungfish",
            "deepsubmergence.lampcrab",
            "deepsubmergence.needlefish",
        };
        private string[] quest2requiredfish = {
            "deepsubmergence.fishsteelhead",
            "deepsubmergence.fishtorpedofish",
            "deepsubmergence.fishshreddershark",
        };
        private string[] quest3requiredfish = {
            "deepsubmergence.fishtrenchwhale",
        };
        
        private string[] diverSprites = {
            "deepsubmergence.uidiver0",
            "deepsubmergence.uidiver1",
            "deepsubmergence.uidiver2",
        };
        
        private const string TEXT_NAME = "DialogueView";
        private const string TEXT_A_NAME = "Container";
        private const string TEXT_B_NAME = "DialogueTextContainer";
        private const string TEXT_C_NAME = "DialogueText";
        
        private SubmarinePlayer cachedSubmarinePlayer;
        private SphereCollider sphereCollider;
        private Rigidbody cachedDredgePlayerRigidbody;
        
        private Image diverImage;
        private RectTransform diverImageRect;
        private GameObject dialogueBackground;
        private GameObject dialogueTitleBackground;
        private GameObject dialogueText;
        private GameObject dialogueTitleText;
        
        private TMP_Text dialogueTextT;
        private TMP_Text dialogueTitleTextT;
        
        public bool playerInFakeDock = false;
        private int currentProgressLevel = 0;
        
        void Start(){
            cachedSubmarinePlayer = DeepSubmergence.instance.submarinePlayer.GetComponent<SubmarinePlayer>();
            cachedDredgePlayerRigidbody = DeepSubmergence.instance.dredgePlayer.GetComponent<Rigidbody>();
            
            sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 0.5f;
            sphereCollider.isTrigger = true;
            
            // Load currentProgressLevel!
            
            // Setup UI pieces
            diverImage = Utils.SetupTextureAsSpriteOnCanvas(
                "Diver Image",
                TextureUtil.GetSprite(diverSprites[0]),
                new Vector2(460.0f, 900.0f),
                new Vector2(960f, 450.0f)
            ).GetComponent<Image>();
            diverImageRect = diverImage.GetComponent<RectTransform>();
            
            dialogueBackground = Utils.SetupTextureAsSpriteOnCanvas(
                "Dialogue Background",
                TextureUtil.GetSprite("deepsubmergence.uitextbackground"),
                new Vector2(746.0f, 186.0f),
                new Vector2(960f, 104.0f)
            );
            
            dialogueTitleBackground = Utils.SetupTextureAsSpriteOnCanvas(
                "Dialogue Title Background",
                TextureUtil.GetSprite("deepsubmergence.uititlebackground"),
                new Vector2(450.0f, 49.0f),
                new Vector2(960f, 209.0f)
            );
            
            // Setup texts
            GameObject foundText = Utils.FindInChildren(
                Utils.FindInChildren(
                    Utils.FindInChildren(
                        GameObject.Find(TEXT_NAME), TEXT_A_NAME
                    ), TEXT_B_NAME
                ), TEXT_C_NAME
            );
            
            dialogueText = Utils.SetupGameObject("Dialogue Text", foundText);
            dialogueTextT = dialogueText.GetComponent<TMP_Text>();
            RectTransform dialogueTextRect = dialogueText.GetComponent<RectTransform>();
            dialogueTextRect.SetParent(Utils.cachedGameCanvas.GetComponent<RectTransform>());
            
            dialogueTitleText = Utils.SetupGameObject("Dialogue Title Text", foundText);
            dialogueTitleTextT = dialogueTitleText.GetComponent<TMP_Text>();
            RectTransform dialogueTitleTextRect = dialogueTitleText.GetComponent<RectTransform>();
            dialogueTitleTextRect.SetParent(Utils.cachedGameCanvas.GetComponent<RectTransform>());
            
            // TEMP
            dialogueTextT.text = "Hey Cool Words";
            dialogueTitleTextT.text = "Hey Cool Words";
            dialogueText.SetActive(true);
            dialogueTitleText.SetActive(true);
            
            // Position text
            dialogueTextRect.anchorMax = Vector2.zero;
            dialogueTextRect.anchorMin = Vector2.zero;
            dialogueTextRect.sizeDelta = new Vector2(746.0f, 186.0f);
            dialogueTextRect.anchoredPosition = new Vector2(960f, 104.0f);
            
            dialogueTitleTextRect.anchorMax = Vector2.zero;
            dialogueTitleTextRect.anchorMin = Vector2.zero;
            dialogueTitleTextRect.sizeDelta = new Vector2(450.0f, 49.0f);
            dialogueTitleTextRect.anchoredPosition = new Vector2(965f, 209.0f);
            
            // TODO set all UI inactive
        }
        
        void Update(){
            sphereCollider.enabled = cachedSubmarinePlayer.CompletelySubmerged();

            // TEMP
            DeepSubmergence.instance.debugAxes.transform.position = transform.position;
        }
        
        void OnTriggerEnter(Collider other){
            if(other.gameObject.name.Contains("HarvestZoneDetector")){
                StartCoroutine(SeabaseQuestUICoroutine());
            }
        }

        private IEnumerator SeabaseQuestUICoroutine(){
            
            SetupPlayerAtFakeDock(true);
            
            // Check for progression
            string[] requiredFish = GetRequiredFishListForProgressLevel(currentProgressLevel);
            
            if(requiredFish != null){
                bool hasAllRequiredFish = true;
                for(int i = 0, count = requiredFish.Length; i < count; ++i){
                    hasAllRequiredFish &= Utils.HasItemInCargo(requiredFish[i]);
                }
                WinchCore.Log.Debug("hasAllRequiredFish: " + hasAllRequiredFish);
                
                if(hasAllRequiredFish){
                    currentProgressLevel++;
                    // PUT THIS IN SAVE DATA!!
                    
                    // Take the fish
                    for(int i = 0, count = requiredFish.Length; i < count; ++i){
                        Utils.DestroyItemInCargo(requiredFish[i]);
                    }
                }
            } else {
                // Nothing? No cutscene? Iunno.
            }
            
            // Get conversation sequence for progress level
            // currentProgressLevel
            
            
            
            
            
            
            
            
            // Setup base ui
            // write text and shuffle the character sprite
            // play sequence depending on progress
            // check if you have the fish we're looking for, if so progress things then play


            yield return new WaitForSeconds(5);
            
            SetupPlayerAtFakeDock(false);
            yield break;
        }

        private void SetupPlayerAtFakeDock(bool inDock){
            playerInFakeDock = inDock;

            // Stop player from moving
            cachedDredgePlayerRigidbody.isKinematic = inDock;
            cachedDredgePlayerRigidbody.velocity = Vector3.zero;
            cachedDredgePlayerRigidbody.angularVelocity = Vector3.zero;

            // hide other uis?
            // Set invincible, protected, safe zone-d?
            // Disable camera movement

            if(!inDock){
                cachedSubmarinePlayer.ForceSurface();
            }
        }
        
        private string[] GetRequiredFishListForProgressLevel(int progress){
            if(progress == 0){ return quest0requiredfish; }
            if(progress == 1){ return quest1requiredfish; }
            if(progress == 2){ return quest2requiredfish; }
            if(progress == 3){ return quest3requiredfish; }
            return null;
        }
    }
}