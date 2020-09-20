using UnityEngine;

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
        
        public static void SetRect(this RectTransform t, Rect rect)
        {
            t.ResetAnchors();
            t.localPosition = rect.position;
            t.sizeDelta = rect.size;
        }
    }
}