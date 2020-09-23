using UnityEngine;

namespace CreativeMode
{
    public class WidgetUIContainer : MonoBehaviour
    {
        public WidgetUIRenderer widgetRenderer;
        public CanvasGroup widgetCanvasGroup;

        public UIContentSize Size => Widget.Size;
        public IWidgetUI Widget => widgetRenderer.WidgetUI;

        public void SetData(Widget widget)
        {
            Widget.SetData(widget);
        }
    }
}