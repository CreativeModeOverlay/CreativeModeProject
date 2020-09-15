using UnityEngine;

namespace CreativeMode
{
    public class WidgetUIContainer : MonoBehaviour
    {
        public RectTransform widgetRoot;
        public CanvasGroup widgetCanvasGroup;

        public IWidgetUI Widget { get; private set; }

        public void PutWidget(IWidgetUI widget)
        {
            var rectTransform = (RectTransform) widget.Root.transform;
            rectTransform.SetParent(widgetRoot, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.pivot = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            Widget = widget;
        }
    }
}