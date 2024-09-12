using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine;
using Winch.Core;
using System;
using System.IO;

namespace DeepSubmergence {
    public class QuestManager : MonoBehaviour {
        
        public static QuestManager instance;
        
        public Dictionary<string, QuestDialogue> allQuestDialogues = new();
        
        void Awake(){
            instance = this;
        }
        
        void Start(){
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
            // currentProgressLevel = GameManager.Instance.SaveData.GetIntVariable(PROGRESSION_SAVE_KEY, 0);
        }

        public static QuestDialogue LoadQuestDialogue(string filename){
            string fileContents = File.ReadAllText(filename);
            QuestDialogue result = Newtonsoft.Json.JsonConvert.DeserializeObject<QuestDialogue>(fileContents);
            return result;
        }
        
        public void IncrementProgress(string saveId){
            allQuestDialogues[saveId].progress++;
        }
        
        public int GetProgress(string saveId){
            return allQuestDialogues[saveId].progress;
        }
        
        public string[] GetRequiredItems(string saveId){
            return allQuestDialogues[saveId].requiredItems;
        }
        
        public string[] GetDialogueOnFinish(string saveId){
            return allQuestDialogues[saveId].dialogueOnFinish;
        }
        
        // enum tags;
        public int GetControlTags(string saveId, int index){
            return 0;
        }
        
        public string GetNextFrame(string saveId, int index){
            return "";
        }
    }   
}