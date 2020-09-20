﻿using UnityEngine;

namespace CreativeMode
{
    public static class TransformUtils
    {
        public static void ResetAnchors(this RectTransform t)
        {
            t.pivot = Vector2.zero;
            t.anchorMin = Vector2.zero;
            t.anchorMax = Vector2.zero;
        }

        public static Rect GetPositonRect(this RectTransform t)
        {
            return new Rect(t.localPosition, t.sizeDelta);
        }
        
        public static void SetPositionRect(this RectTransform t, Rect rect)
        {
            t.ResetAnchors();
            t.localPosition = rect.position;
            t.sizeDelta = rect.size;
        }
    }
}