using UnityEngine;

namespace CreativeMode
{
    public static class TransformUtils
    {
        public static void SetRect(this RectTransform t, Rect rect)
        {
            t.pivot = Vector2.zero;
            t.anchorMin = Vector2.zero;
            t.anchorMax = Vector2.one;
            t.anchoredPosition = rect.position;
            t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);
            t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);
        }
    }
}