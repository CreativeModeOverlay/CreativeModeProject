﻿using CreativeMode.Impl;
 using UnityEngine;

namespace CreativeMode
{
    public class DesktopVisibleRegionWidget : MonoBehaviour
    {
        public DesktopFocusManager focusManager;
        public RectTransform visibleRegionRect;
        public CanvasGroup widgetVisibilityGroup;

        public float widgetFadeSpeed;
        private Rect currentRect;

        private void LateUpdate()
        {
            Rect windowRect;
            float targetAlpha;

            if (focusManager.IsFocused)
            {
                windowRect = focusManager.FocusedWindow.programRect;
                targetAlpha = 1f;
            }
            else
            {
                windowRect = new Rect(0, 0, Screen.width, Screen.height);
                targetAlpha = 0f;
            }

            widgetVisibilityGroup.alpha = Mathf.Lerp(
                widgetVisibilityGroup.alpha, 
                targetAlpha, Time.deltaTime * widgetFadeSpeed);

            currentRect = RectUtils.Lerp(currentRect, windowRect, 
                Time.deltaTime * widgetFadeSpeed);

            visibleRegionRect.gameObject.SetActive(widgetVisibilityGroup.alpha > 0.01f);
            visibleRegionRect.anchoredPosition = currentRect.position;
            visibleRegionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentRect.width);
            visibleRegionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentRect.height);
        }
    }
}