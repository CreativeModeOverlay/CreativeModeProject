using UnityEngine;

namespace CreativeMode
{
    public class FocusRegionWidget : MonoBehaviour
    {
        [SerializeField]
        private float animationSpeed;
        
        [SerializeField]
        private CanvasGroup visibilityGroup;

        public bool IsVisible { get; set; }
        public Rect FocusRegion { get; set; }
        public Rect WindowRect { get; set; }
        
        private Rect currentRect;

        private void Update()
        {
            UpdateRegion(true);
        }

        private void UpdateRegion(bool animate)
        {
            Rect focusRegion;
            float targetAlpha;

            if (IsVisible)
            {
                focusRegion = FocusRegion;
                targetAlpha = 1f;
            }
            else
            {
                focusRegion = WindowRect;
                targetAlpha = 0f;
            }

            var t = animate ? Time.deltaTime * animationSpeed : 1f;
            visibilityGroup.alpha = Mathf.Lerp(visibilityGroup.alpha, targetAlpha, t);
            currentRect = RectUtils.Lerp(currentRect, focusRegion, t);

            if (visibilityGroup.alpha < 0.01f)
                visibilityGroup.alpha = 0;

            var rectTransform = (RectTransform) transform;
            rectTransform.SetPositionRect(currentRect);
        }
        
        private void OnEnable()
        {
            IsVisible = false;
            UpdateRegion(false);
        }
    }
}