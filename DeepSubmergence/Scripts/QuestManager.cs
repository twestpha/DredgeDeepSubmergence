using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine;
using Winch.Core;
using System;
using System.IO;

namespace DeepSubmergence {
    public enum DialogueControlTags : int {
        None      = 0,
        UseQuotes = 1 << 0,
    }
    
    public class QuestManager : MonoBehaviour {
        
        public static QuestManager instance;
        
        public Dictionary<string, QuestDialogue> allQuestDialogues = new();
        
        void Awake(){
            instance = this;
        }
        
        void Start(){
            try {
                WinchCore.Log.Debug("Loading quest assets...");

                // Load all quest assets' data
                string[] modDirs = Directory.GetDirectories("Mods");
                foreach (string modDir in modDirs)
                {
                    if(modDir.Contains("DeepSubmergence")){
                        string assetFolderPath = Path.Combine(Path.Combine(modDir, "Assets"), "Quests");
                        
                        if (!Directory.Exists(assetFolderPath)){
                            continue;
                        }
                        
                        string[] questFiles = Directory.GetFiles(assetFolderPath);
                        foreach(string file in questFiles){
                            try {
                                QuestDialogue newQuestDialogue = LoadQuestDialogue(file);
                                allQuestDialogues.Add(newQuestDialogue.saveId, newQuestDialogue);
                            } catch(Exception ex){
                                WinchCore.Log.Error($"Failed to load quest file {file}: {ex}");
                            }
                        }
                    }
                }
                
                WinchCore.Log.Debug("Loaded " + allQuestDialogues.Keys.Count + " quest assets");
                
                // Load progress levels for those quests
                foreach(QuestDialogue quest in allQuestDialogues.Values){
                    int loadedProgress = GameManager.Instance.SaveData.GetIntVariable(quest.saveId, -1);
                    
                    if(loadedProgress != -1){
                        quest.progress = loadedProgress;
                    }
                }
            } catch(Exception e){
                WinchCore.Log.Error(e.ToString());
            }
        }

        public static QuestDialogue LoadQuestDialogue(string filename){
            string fileContents = File.ReadAllText(filename);
            QuestDialogue result = Newtonsoft.Json.JsonConvert.DeserializeObject<QuestDialogue>(fileContents);
            return result;
        }
        
        public void ResetAllQuests(){
            WinchCore.Log.Debug("Resetting all quests");
            
            foreach(QuestDialogue quest in allQuestDialogues.Values){
                quest.progress = 0;
            }
        }
        
        public void IncrementProgress(string saveId){
            allQuestDialogues[saveId].progress++;
            GameManager.Instance.SaveData.SetIntVariable(saveId, allQuestDialogues[saveId].progress);
        }
        
        public int GetProgress(string saveId){
            return allQuestDialogues[saveId].progress;
        }
        
        public bool CanProgress(string saveId){
            return allQuestDialogues[saveId].progress < allQuestDialogues[saveId].chunks.Length;
        }
        
        public string[] GetRequiredItems(string saveId){
            return allQuestDialogues[saveId].chunks[GetProgress(saveId)].requiredItems;
        }
        
        public string GetSpeakerName(string saveId){
            return allQuestDialogues[saveId].speakerName;
        }
        
        public string[] GetDialogueOnFinish(string saveId){
            return allQuestDialogues[saveId].chunks[GetProgress(saveId)].dialogueOnFinish;
        }
        
        public DialogueControlTags GetControlTags(string saveId, int dialogueIndex){
            DialogueControlTags tags = DialogueControlTags.None;
            
            string tagString = allQuestDialogues[saveId].chunks[GetProgress(saveId)].controlTags[dialogueIndex];
            if(tagString.Contains("q")){
                tags |= DialogueControlTags.UseQuotes;
            }
            
            return tags;
        }
        
        public string GetNextFrame(string saveId, int dialogueIndex)
        {
            return allQuestDialogues[saveId].chunks[GetProgress(saveId)].frames[dialogueIndex];
        }
        
        public bool ShouldAutoProgress(string saveId){
            return allQuestDialogues[saveId].chunks[GetProgress(saveId)].autoProgress;
        }

        public string[] GetItemsOnFinish(string saveId){
            return allQuestDialogues[saveId].chunks[GetProgress(saveId)].itemsOnFinish;
        }
    }   
}