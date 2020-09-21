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

        public static Rect GetPositonRect(this RectTransform t)
        {
            return new Rect(t.localPosition, t.sizeDelta);
        }
        
        public static void SetPositionRect(this RectTransform t, Rect rect)
        {
            t.localPosition = rect.position;
            t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);
            t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);
        }
    }
}