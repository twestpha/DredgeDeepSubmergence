using System.Collections.Generic;
﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Winch.Core;
using Winch.Util;

namespace DeepSubmergence {
    public class SubmarineUI : MonoBehaviour {
        
        private SubmarinePlayer cachedSubmarinePlayer;
        private GameObject diveIcon;
        private GameObject diveButton;
        
        private RectTransform rectTransform;
        private RectTransform diveIconRect;
        
        void Start(){
            cachedSubmarinePlayer = DeepSubmergence.instance.submarinePlayer.GetComponent<SubmarinePlayer>();
            rectTransform = GetComponent<RectTransform>();
            
            diveIcon = Utils.SetupTextureAsSpriteOnCanvas(
                "Dive Icon",
                TextureUtil.GetSprite("deepsubmergence.uidiveicon"),
                new Vector2(44.0f, 44.0f),
                new Vector2(0.0f, 0.0f)
            );
            diveIconRect = diveIcon.GetComponent<RectTransform>();
            diveIconRect.SetParent(rectTransform);
            diveIconRect.anchoredPosition = new Vector2(30.0f, 25.0f);
            
            diveButton = Utils.SetupTextureAsSpriteOnCanvas(
                "Dive Button",
                TextureUtil.GetSprite("deepsubmergence.uidivebuttonq"),
                new Vector2(33.0f, 33.0f),
                new Vector2(0.0f, 0.0f)
            );
            RectTransform diveButtonRect = diveButton.GetComponent<RectTransform>();
            diveButtonRect.SetParent(rectTransform);
            diveButtonRect.anchoredPosition = new Vector2(75.0f, 25.0f);
        }
        
        void Update(){
            bool canDive = Utils.CanDive();
            
            // Flip direction of dive icon arrow depending on surface/not
            diveIconRect.transform.localScale = new Vector3(1.0f, cachedSubmarinePlayer.OnSurface() ? 1.0f : -1.0f, 1.0f);
        }
    }
}