using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Winch.Core;
using Winch.Util;

namespace DeepSubmergence {
    public class SeaBaseFakeDock : MonoBehaviour {
        
        private SubmarinePlayer cachedSubmarinePlayer;
        private SphereCollider sphereCollider;
        
        void Start(){
            cachedSubmarinePlayer = DeepSubmergence.instance.submarinePlayer.GetComponent<SubmarinePlayer>();
            
            sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 3.0f;
            sphereCollider.isTrigger = true;
        }
        
        void Update(){
            sphereCollider.enabled = cachedSubmarinePlayer.CompletelySubmerged();

            // TEMP
            DeepSubmergence.instance.debugAxes.transform.position = transform.position;
        }
        
        void OnTriggerEnter(Collider other){
            if(other.gameObject.name.Contains("HarvestZoneDetector")){
                WinchCore.Log.Debug("ASLKDJ");
                // kick off UI flow
            }
        }
    }
}