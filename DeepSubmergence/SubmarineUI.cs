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
        
        private Image ribbonUI;
        private RectTransform rectTransform;
        private RectTransform diveIconRect;
        
        private Timer disableUIAtStart = new Timer(2.0f);
        
        void Start(){
            cachedSubmarinePlayer = DeepSubmergence.instance.submarinePlayer.GetComponent<SubmarinePlayer>();
            ribbonUI = GetComponent<Image>();
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
            
            // Force ui to be disabled for some time at the start of gameplay
            // This lets all the other dependent UI get set up first
            disableUIAtStart.Start();
        }
        
        void Update(){
            // Show or hide UI if diving is possible
            bool showUI = Utils.CanDive() == CannotDiveReason.None && disableUIAtStart.Finished();
            ribbonUI.enabled = showUI;
            diveIcon.SetActive(showUI);
            diveButton.SetActive(showUI);
            
            // Flip direction of dive icon arrow depending on surface/not
            diveIconRect.transform.localScale = new Vector3(1.0f, cachedSubmarinePlayer.OnSurface() ? 1.0f : -1.0f, 1.0f);
        }
    }
}