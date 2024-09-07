using System.Collections.Generic;
using UnityEngine;
using System;
using Winch.Core;

namespace DeepSubmergence {
    public class UnderwaterFishableManager : MonoBehaviour {
        
        public static UnderwaterFishableManager instance;
        
        private const string HARVESTABLE_POIS_PARENT_NAME = "HarvestPOIs";
        private const string CUSTOM_HARVESTABLE_IDENTIFIER = "deepsubmergence";
        
        private const float DIVE_SIGHT_DISTANCE = 30.0f;
        
        private List<GameObject> allCustomHarvestables = new();
        private SubmarinePlayer cachedSubmarinePlayer;
        
        void Awake(){
            instance = this;
        }
        
        void Start(){
            try {
                cachedSubmarinePlayer = DeepSubmergence.instance.submarinePlayer.GetComponent<SubmarinePlayer>();
                
                GameObject parent = GameObject.Find(HARVESTABLE_POIS_PARENT_NAME);
                
                for(int i = 0, count = parent.transform.childCount; i < count; ++i){
                    GameObject child = parent.transform.GetChild(i).gameObject;
                    
                    if(child.name.Contains(CUSTOM_HARVESTABLE_IDENTIFIER)){
                        Destroy(child.GetComponent<Cullable>());
                        allCustomHarvestables.Add(child);
                    }
                }
            } catch (Exception e){
                WinchCore.Log.Error(e.ToString());
            }
        }
        
        void Update(){
            try {
                bool currentDiving = cachedSubmarinePlayer.CompletelySubmerged();
                
                // Only enabling them when close and submerged
                for(int i = 0, count = allCustomHarvestables.Count; i < count; ++i){
                    float distance = (allCustomHarvestables[i].transform.position - cachedSubmarinePlayer.transform.position).magnitude;
                    allCustomHarvestables[i].SetActive(currentDiving && distance < DIVE_SIGHT_DISTANCE);
                }
            } catch (Exception e){
                WinchCore.Log.Error(e.ToString());
            }
        }
    }
}