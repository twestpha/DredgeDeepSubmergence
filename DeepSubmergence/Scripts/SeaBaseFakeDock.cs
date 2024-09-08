using UnityEngine;
using UnityEngine.UI;
using Winch.Core;
using System.Collections;
using Winch.Util;
using TMPro;
using System;

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
            "deepsubmergence.fishlampcrab",
            "deepsubmergence.fishneedlefish",
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
            "deepsubmergence.uidiver3",
        };
        private string[] quest0Dialogues = {
            "deepsubmergence.quest0dialogue0",
            "deepsubmergence.quest0dialogue1",
            "deepsubmergence.quest0dialogue2",
            "deepsubmergence.quest0dialogue3",
            "deepsubmergence.quest0dialogue4",
            "deepsubmergence.quest0dialogue5",
        };
        private string[] quest1Dialogues = {
            "deepsubmergence.quest1dialogue0",
            "deepsubmergence.quest1dialogue1",
            "deepsubmergence.quest1dialogue2",
            "deepsubmergence.quest1dialogue3",
            "deepsubmergence.quest1dialogue4",
            "deepsubmergence.quest1dialogue5",
        };
        private string[] quest2Dialogues = {
            "deepsubmergence.quest2dialogue0",
            "deepsubmergence.quest2dialogue1",
            "deepsubmergence.quest2dialogue2",
            "deepsubmergence.quest2dialogue3",
            "deepsubmergence.quest2dialogue4",
            "deepsubmergence.quest2dialogue5",
        };
        private string[] quest3Dialogues = {
            "deepsubmergence.quest3dialogue0",
            "deepsubmergence.quest3dialogue1",
            "deepsubmergence.quest3dialogue2",
            "deepsubmergence.quest3dialogue3",
            "deepsubmergence.quest3dialogue4",
            "deepsubmergence.quest3dialogue5",
        };
        private string[] questDoneDialogues = {
            "deepsubmergence.questdonedialogue0",
            "deepsubmergence.questdonedialogue1",
        };
        
        private const string DIVER_TITLE = "deepsubmergence.questdivertitle";
        
        private const string TEXT_NAME = "DialogueView";
        private const string TEXT_A_NAME = "Container";
        private const string TEXT_B_NAME = "DialogueTextContainer";
        private const string TEXT_C_NAME = "DialogueText";

        private const string PROGRESSION_SAVE_KEY = "deepsubmergence.questprogress";

        private const float MAX_PROGRESS = 5;
        private const float RETRIGGER_DOCK_TIME = 0.5f;
        
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
        
        private GameObject abilityCanvas;
        
        private Timer redockTimer = new Timer(RETRIGGER_DOCK_TIME); 
        
        void Start(){
            try {
                cachedSubmarinePlayer = DeepSubmergence.instance.submarinePlayer.GetComponent<SubmarinePlayer>();
                cachedDredgePlayerRigidbody = DeepSubmergence.instance.dredgePlayer.GetComponent<Rigidbody>();
                
                sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.radius = 0.5f;
                sphereCollider.isTrigger = true;
                
                abilityCanvas = Utils.FindInChildren(Utils.FindInChildren(GameObject.Find("GameCanvases"), "GameCanvas"), "Abilities");

                // Load currentProgressLevel
                currentProgressLevel = GameManager.Instance.SaveData.GetIntVariable(PROGRESSION_SAVE_KEY, 0);
                
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
                
                // Position text
                dialogueTextRect.anchorMax = Vector2.zero;
                dialogueTextRect.anchorMin = Vector2.zero;
                dialogueTextRect.sizeDelta = new Vector2(706.0f, 146.0f);
                dialogueTextRect.anchoredPosition = new Vector2(980f, 84.0f);
                dialogueTextT.enableWordWrapping = true;
                dialogueTextT.overflowMode = TextOverflowModes.Page;

                dialogueTitleTextRect.anchorMax = Vector2.zero;
                dialogueTitleTextRect.anchorMin = Vector2.zero;
                dialogueTitleTextRect.sizeDelta = new Vector2(450.0f, 49.0f);
                dialogueTitleTextRect.anchoredPosition = new Vector2(965f, 209.0f);
                dialogueTitleTextT.verticalAlignment = VerticalAlignmentOptions.Middle;
                dialogueTitleTextT.horizontalAlignment = HorizontalAlignmentOptions.Center;

                // Set all UI inactive
                diverImage.enabled = false;
                dialogueBackground.SetActive(false);
                dialogueTitleBackground.SetActive(false);
                dialogueText.SetActive(false);
                dialogueTitleText.SetActive(false);
            } catch(Exception e){
                WinchCore.Log.Error(e.ToString());
            }
        }

        void Update(){
            try {
                sphereCollider.enabled = cachedSubmarinePlayer.CompletelySubmerged();
            } catch(Exception e){
                WinchCore.Log.Error(e.ToString());
            }
        }
        
        void OnTriggerEnter(Collider other){
            if(other.gameObject.name.Contains("HarvestZoneDetector") && redockTimer.Finished()){
                StartCoroutine(SeabaseQuestUICoroutine());
            }
        }

        private IEnumerator SeabaseQuestUICoroutine(){
            if(currentProgressLevel < MAX_PROGRESS){
                SetupPlayerAtFakeDock(true);
                
                // Check for progression
                string[] requiredFish = GetRequiredFishListForProgressLevel(currentProgressLevel);
                
                if(requiredFish != null){
                    bool hasAllRequiredFish = true;
                    for(int i = 0, count = requiredFish.Length; i < count; ++i){
                        hasAllRequiredFish &= Utils.HasItemInCargo(requiredFish[i]);
                    }
                    
                    if(hasAllRequiredFish){
                        currentProgressLevel++;
                        GameManager.Instance.SaveData.SetIntVariable(PROGRESSION_SAVE_KEY, currentProgressLevel);

                        // Take the fish
                        for (int i = 0, count = requiredFish.Length; i < count; ++i){
                            Utils.DestroyItemInCargo(requiredFish[i]);
                        }
                    }
                }
                
                // Play dialogue
                dialogueTextT.text = "";
                dialogueTitleTextT.text = "";

                string[] dialogues = GetDialogueForProgressLevel(currentProgressLevel);
                
                for(int i = 0, count = dialogues.Length; i < count; ++i){
                    int pickedSprite = UnityEngine.Random.Range(0, diverSprites.Length - 1);
                    
                    // For the last quest, always force the sprite to be the last one
                    if(dialogues == questDoneDialogues){
                        pickedSprite = diverSprites.Length - 1;
                    }
                    
                    string localeCode = GameManager.Instance.LanguageManager.GetLocale().Identifier.Code;

                    yield return PlayDialogue(
                        TextureUtil.GetSprite(diverSprites[pickedSprite]),
                        LocalizationUtil.GetLocalizedString(localeCode, DIVER_TITLE),
                        LocalizationUtil.GetLocalizedString(localeCode, dialogues[i])
                    );
                }

                // Hide ui
                yield return HideUI();
                
                // All done
                SetupPlayerAtFakeDock(false);
                redockTimer.Start();
                yield break;
            }
        }

        private void SetupPlayerAtFakeDock(bool inDock){
            playerInFakeDock = inDock;

            // Stop player from moving
            cachedDredgePlayerRigidbody.isKinematic = inDock;
            cachedDredgePlayerRigidbody.velocity = Vector3.zero;
            cachedDredgePlayerRigidbody.angularVelocity = Vector3.zero;

            // Hide other UIs
            abilityCanvas.SetActive(!inDock);
            
            // Sort add a safe zone
            GameManager.Instance.MonsterManager.isBanishActive = inDock;
            
            // Disable mouse-camera control while in dock
            GameManager.Instance.PlayerCamera.cinemachineCamera.enabled = !inDock;

            if(!inDock){
                cachedSubmarinePlayer.ForceSurface();
            }
        }
        
        private string[] GetRequiredFishListForProgressLevel(int progress){
            if(progress == 0){ return quest0requiredfish; }
            else if(progress == 1){ return quest1requiredfish; }
            else if(progress == 2){ return quest2requiredfish; }
            else if(progress == 3){ return quest3requiredfish; }
            return null;
        }
        
        private string[] GetDialogueForProgressLevel(int progress){
            if(progress == 0){ return quest0Dialogues; }
            else if(progress == 1){ return quest1Dialogues; }
            else if(progress == 2){ return quest2Dialogues; }
            else if(progress == 3){ return quest3Dialogues; }
            else { return questDoneDialogues; }
        }
        
        private IEnumerator PlayDialogue(Sprite sprite, string localizedTitle, string localizedDialogue){
            bool useAlpha = !dialogueBackground.activeSelf;
            
            dialogueTitleTextT.text = localizedTitle;
            dialogueTextT.text = localizedDialogue;
            dialogueTextT.maxVisibleCharacters = 0;
            diverImage.sprite = sprite;
            if(useAlpha){
                diverImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            }
            
            diverImage.enabled = true;
            dialogueBackground.SetActive(true);
            dialogueTitleBackground.SetActive(true);
            dialogueText.SetActive(true);
            dialogueTitleText.SetActive(true);
            
            // Show dialogue over time in various animated ways
            bool finished = false;
            
            Timer diverRevealTimer = new Timer(0.5f);
            diverRevealTimer.Start();
            Timer textRevealTimer = new Timer(1.0f);
            textRevealTimer.Start();
            
            while(!finished){
                float diverT = diverRevealTimer.Parameterized();
                diverT = Mathf.Sqrt(diverT);
                
                // Alpha and Position of diver
                if(useAlpha){
                    diverImage.color = new Color(1.0f, 1.0f, 1.0f, diverT);
                }

                diverImageRect.anchoredPosition = Vector2.Lerp(new Vector2(960f, 430.0f), new Vector2(960f, 450.0f), diverT);
                
                // Skip to end with keypress
                if(Input.GetKeyDown(KeyCode.Mouse0)){
                    textRevealTimer.SetParameterized(1.0f);
                }
                
                float textT = textRevealTimer.Parameterized();
                dialogueTextT.maxVisibleCharacters = (int) Mathf.Round(textT * ((float) localizedDialogue.Length));
                
                finished = diverRevealTimer.Finished() && textRevealTimer.Finished();
                yield return null;
            }
            
            diverImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            dialogueTextT.maxVisibleCharacters = localizedDialogue.Length;
            
            // Wait for input to next dialogue
            bool input = false;
            
            while(!input){
                yield return null;
                input = Input.GetKeyDown(KeyCode.Mouse0);
            }
            
            yield return null;
        }
        
        private IEnumerator HideUI(){
            diverImage.enabled = false;
            dialogueBackground.SetActive(false);
            dialogueTitleBackground.SetActive(false);
            dialogueText.SetActive(false);
            dialogueTitleText.SetActive(false);

            yield break;
        }
    }
}