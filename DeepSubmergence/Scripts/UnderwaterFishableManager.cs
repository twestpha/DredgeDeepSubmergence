using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine;
using Winch.Core;
using Winch.Util;

namespace DeepSubmergence {
    public class UnderwaterFishableManager : MonoBehaviour {
        
        public static UnderwaterFishableManager instance;
        
        private const string FISHABLE_POOL_NAME = "UnderwaterFishables";
        private const int FISHABLE_POOL_SIZE = 32;
        
        void Awake(){
            instance = this;
        }
        
        void Start(){
            GameObject fishablePrototype = new();
            PooledGameObjectManager.SetupPool(FISHABLE_POOL_NAME, FISHABLE_POOL_SIZE, fishablePrototype, gameObject);
        }
        
        void Update(){
            
        }
    }
}