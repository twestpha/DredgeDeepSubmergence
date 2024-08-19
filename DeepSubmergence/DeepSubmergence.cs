using UnityEngine;
using Winch.Core;

namespace DeepSubmergence {
    public class DeepSubmergence : MonoBehaviour {
    
        public static DeepSubmergence instance;
        
        // SHORT TERM GOALS
        // [/] Get mod loading at all
        // [x] load a model and put it in scene at player (?)
        // [x] apply texture and material to the model (?)
        
        public void Awake(){
            instance = this;
            WinchCore.Log.Debug($"[DeepSubmergence] mod loaded");
        }
        
        void Update(){
            // Nothing yet
        }
    }
}
