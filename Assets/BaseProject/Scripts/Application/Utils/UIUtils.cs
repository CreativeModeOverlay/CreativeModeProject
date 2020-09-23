using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public static class UIUtils
    {
        private static readonly float[] horizontalAnchor01Table =
        {
            0.0f, 0.5f, 1.0f,
            0.0f, 0.5f, 1.0f,
            0.0f, 0.5f, 1.0f
        };
        
        private static readonly float[] verticalAnchor01Table =
        {
            1.0f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f,
            0.0f, 0.0f, 0.0f
        };
        
        private static readonly float[] horizontalAnchorSignedTable =
        {
            -1.0f, 0.0f, +1.0f,
            -1.0f, 0.0f, +1.0f,
            -1.0f, 0.0f, +1.0f
        };
        
        private static readonly float[] verticalAnchorSignedTable =
        {
            1.0f, 1.0f, 1.0f,
            0.0f, 0.0f, 0.0f,
            -1.0f, -1.0f, -1.0f
        };

        public static RectTransform AsRectTransform(this Transform t) => (RectTransform) t;
        
        public static Sequence ChangeText(this Text t, string text, float duration = 1f, Action onTextChanged = null)
        {
            if (t.text != text)
            {
                DOTween.Kill(t);
                var halfDuration = duration / 2f;
                return DOTween.Sequence().SetTarget(t)
                    .Append(t.DOFade(0f, halfDuration))
                    .AppendCallback(() =>
                    {
                        t.text = text;
                        onTextChanged?.Invoke();
                    })
                    .Append(t.DOFade(1f, halfDuration));
            }

            return null;
        }
        
        public static float GetHorizontalSignedScale(this TextAnchor t) => horizontalAnchorSignedTable[(int) t];
        public static float GetVerticalSignedScale(this TextAnchor t) => verticalAnchorSignedTable[(int) t];
        public static Vector2 GetSignedScale(this TextAnchor t) => new Vector2(
            horizontalAnchorSignedTable[(int) t], 
            verticalAnchorSignedTable[(int) t]);
        
        public static float GetHorizontal01Scale(this TextAnchor t) => horizontalAnchor01Table[(int) t];
        public static float GetVertical01Scale(this TextAnchor t) => verticalAnchor01Table[(int) t];
        public static Vector2 Get01Scale(this TextAnchor t) => new Vector2(
            horizontalAnchor01Table[(int) t], 
            verticalAnchor01Table[(int) t]);
        
        public static T CreateInnerGraphic<T>(Component c) 
            where T : Component
        {
            var graphicObject = new GameObject("Graphic");
            graphicObject.transform.SetParent(c.gameObject.transform, false);
            graphicObject.AddComponent<CanvasRenderer>();
            var result = graphicObject.AddComponent<T>();

            var graphicTransform = (RectTransform) graphicObject.transform;
            graphicTransform.anchorMin = Vector2.zero;
            graphicTransform.anchorMax = Vector2.one;
            graphicTransform.pivot = Vector2.zero;
            graphicTransform.sizeDelta = Vector2.zero;

            return result;
        }
    }
}