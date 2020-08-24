using UniRx;
using UnityEngine;

namespace CreativeMode
{
    public class DesktopCaptureRegionWidget : MonoBehaviour
    {
        private IDesktopCaptureManager CaptureManager => Instance<IDesktopCaptureManager>.Get();
        
        public RectTransform windowCaptureRect;
        public CanvasGroup windowCaptureVisibilityGroup;

        public float widgetFadeSpeed;
        
        private Rect currentRect;
        private FocusInfo focusInfo;

        private void Awake()
        {
            CaptureManager.FocusPoint
                .Subscribe(f => focusInfo = f);

            UpdatePosition(false);
        }

        private void LateUpdate()
        {
            UpdatePosition(true);
        }

        private void UpdatePosition(bool animate)
        {
            Rect windowRect;
            float targetAlpha;

            if (focusInfo.isFocused)
            {
                windowRect = focusInfo.focusRegion;
                targetAlpha = 1f;
            }
            else
            {
                windowRect = new Rect(0, 0, Screen.width, Screen.height);
                targetAlpha = 0f;
            }

            if (animate)
            {
                windowCaptureVisibilityGroup.alpha = Mathf.Lerp(
                    windowCaptureVisibilityGroup.alpha, 
                    targetAlpha, Time.deltaTime * widgetFadeSpeed);

                currentRect = RectUtils.Lerp(currentRect, windowRect, 
                    Time.deltaTime * widgetFadeSpeed);
            }
            else
            {
                windowCaptureVisibilityGroup.alpha = targetAlpha;
                currentRect = windowRect;
            }

            windowCaptureRect.gameObject.SetActive(windowCaptureVisibilityGroup.alpha > 0.01f);
            windowCaptureRect.pivot = Vector2.zero;
            windowCaptureRect.anchorMin = Vector2.zero;
            windowCaptureRect.anchorMax = Vector2.one;
            windowCaptureRect.anchoredPosition = currentRect.position;
            windowCaptureRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentRect.width);
            windowCaptureRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentRect.height);
        }
    }
}