using UnityEngine;
using UnityEngine.UI;
using Winch.Core;
using System.Collections;
using Winch.Util;
using TMPro;
using System;

namespace DeepSubmergence {
    public class SeaBaseFakeDock : MonoBehaviour {
        
        private const string TEXT_NAME = "DialogueView";
        private const string TEXT_A_NAME = "Container";
        private const string TEXT_B_NAME = "DialogueTextContainer";
        private const string TEXT_C_NAME = "DialogueText";

        private const string MAIN_QUEST_KEY = "deepsubmergence.mainquest";
        private const string INTRO_QUEST_KEY = "deepsubmergence.introquest";

        private const float MAX_PROGRESS = 5;
        private const float RETRIGGER_DOCK_TIME = 0.5f;
        private const float INTRO_DELAY = 5.0f;
        
        private SubmarinePlayer cachedSubmarinePlayer;
        private SphereCollider sphereCollider;
        private Rigidbody cachedDredgePlayerRigidbody;
        
        private Image diverImage;
        private RectTransform diverImageRect;
        private GameObject dialogueBackground;
        private GameObject dialogueQuotes;
        private GameObject dialogueTitleBackground;
        private GameObject dialogueText;
        private GameObject dialogueTitleText;
        
        private TMP_Text dialogueTextT;
        private TMP_Text dialogueTitleTextT;
        
        public bool playerInFakeDock = false;
        
        private GameObject abilityCanvas;
        
        private Timer redockTimer = new Timer(RETRIGGER_DOCK_TIME); 
        private Timer introDelayTimer = new Timer(INTRO_DELAY); 
        
        void Start(){
            try {
                cachedSubmarinePlayer = DeepSubmergence.instance.submarinePlayer.GetComponent<SubmarinePlayer>();
                cachedDredgePlayerRigidbody = DeepSubmergence.instance.dredgePlayer.GetComponent<Rigidbody>();
                
                sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.radius = 0.5f;
                sphereCollider.isTrigger = true;
                
                abilityCanvas = Utils.FindInChildren(Utils.FindInChildren(GameObject.Find("GameCanvases"), "GameCanvas"), "Abilities");
                
                // Setup UI pieces
                diverImage = Utils.SetupTextureAsSpriteOnCanvas(
                    "Diver Image",
                    TextureUtil.GetSprite("deepsubmergence.uidiver0"),
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
                
                dialogueQuotes = Utils.SetupTextureAsSpriteOnCanvas(
                    "Dialogue Quotes",
                    TextureUtil.GetSprite("deepsubmergence.uitextquotes"),
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
                dialogueQuotes.SetActive(false);
                dialogueTitleBackground.SetActive(false);
                dialogueText.SetActive(false);
                dialogueTitleText.SetActive(false);
            } catch(Exception e){
                WinchCore.Log.Error(e.ToString());
            }
        }

        void Update(){
            try {
                // Debug key combo for resetting quests
                if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R)){
                    QuestManager.instance.ResetAllQuests();
                }
                
                // Debug key combo for progressing main quest
                if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T)){
                    QuestManager.instance.IncrementProgress(MAIN_QUEST_KEY);
                }
                
                // Try to play intro quest if applicable, some dialogue hinting at the location and a map item
                if(QuestManager.instance.CanProgress(INTRO_QUEST_KEY) && !playerInFakeDock){
                    CannotDiveReason cannotDiveReason = Utils.CanDive();
                    
                    if((cannotDiveReason & CannotDiveReason.InDock) > 0){
                        introDelayTimer.Start();
                    }
                    
                    if(introDelayTimer.Finished()){
                        StartCoroutine(SeabaseQuestUICoroutine(INTRO_QUEST_KEY));
                    }
                }
                
                // Able the dock collider only when diving
                sphereCollider.enabled = cachedSubmarinePlayer.CompletelySubmerged();
            } catch(Exception e){
                WinchCore.Log.Error(e.ToString());
            }
        }
        
        void OnTriggerEnter(Collider other){
            if(other.gameObject.name.Contains("HarvestZoneDetector") && redockTimer.Finished()){
                StartCoroutine(SeabaseQuestUICoroutine(MAIN_QUEST_KEY));
            }
        }

        private IEnumerator SeabaseQuestUICoroutine(string quest){
            QuestManager qm = QuestManager.instance;
            
            if(qm.CanProgress(quest)){
                SetupPlayerAtFakeDock(true);
                
                // Cache items on finish
                string[] itemsOnFinish = qm.GetItemsOnFinish(quest);
                
                // Check for progression
                string[] requiredFish = qm.GetRequiredItems(quest);
                
                // No required fish should play dialogue, but not progress the quest
                if(requiredFish != null && requiredFish.Length > 0){
                    bool hasAllRequiredFish = true;
                    for(int i = 0, count = requiredFish.Length; i < count; ++i){
                        hasAllRequiredFish &= Utils.HasItemInCargo(requiredFish[i]);
                    }
                    
                    if(hasAllRequiredFish){
                        qm.IncrementProgress(quest);

                        // Take the fish
                        for (int i = 0, count = requiredFish.Length; i < count; ++i){
                            Utils.DestroyItemInCargo(requiredFish[i]);
                        }
                    }
                }
                
                // Play dialogue
                dialogueTextT.text = "";
                dialogueTitleTextT.text = "";
                
                string speakerName = qm.GetSpeakerName(quest);
                string[] dialogues = qm.GetDialogueOnFinish(quest);

                for(int i = 0, count = dialogues.Length; i < count; ++i){
                    DialogueControlTags controlTags = qm.GetControlTags(quest, i);
                    string nextFrameSpriteName = qm.GetNextFrame(quest, i);
                    string localeCode = GameManager.Instance.LanguageManager.GetLocale().Identifier.Code;

                    yield return PlayDialogue(
                        TextureUtil.GetSprite(nextFrameSpriteName),
                        LocalizationUtil.GetLocalizedString(localeCode, speakerName),
                        LocalizationUtil.GetLocalizedString(localeCode, dialogues[i]),
                        controlTags
                    );
                    
                    // Get items and add to inventory
                    if(itemsOnFinish != null && itemsOnFinish.Length > i && !string.IsNullOrEmpty(itemsOnFinish[i])){
                        Utils.PutItemInCargo(itemsOnFinish[i], true);
                    }
                }
                
                if(qm.ShouldAutoProgress(quest)){
                    qm.IncrementProgress(quest);
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
            
            // Sort of add a safe zone
            GameManager.Instance.MonsterManager.isBanishActive = inDock;
            
            // Disable mouse-camera control while in dock
            GameManager.Instance.PlayerCamera.cinemachineCamera.enabled = !inDock;

            if(!inDock){
                cachedSubmarinePlayer.ForceSurface();
            }
        }
        
        private IEnumerator PlayDialogue(Sprite sprite, string localizedTitle, string localizedDialogue, DialogueControlTags controlTags){
            bool useAlpha = !dialogueBackground.activeSelf;
            
            dialogueTitleTextT.text = localizedTitle;
            dialogueTextT.text = localizedDialogue;
            dialogueTextT.maxVisibleCharacters = 0;
            diverImage.sprite = sprite;
            
            if(useAlpha){
                diverImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            }
            
            diverImage.enabled = sprite != null;
            dialogueBackground.SetActive(true);
            dialogueTitleBackground.SetActive(true);
            dialogueText.SetActive(true);
            dialogueTitleText.SetActive(true);
            dialogueQuotes.SetActive((controlTags & DialogueControlTags.UseQuotes) > 0);
            
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
            dialogueQuotes.SetActive(false);

            yield break;
        }
    }
}