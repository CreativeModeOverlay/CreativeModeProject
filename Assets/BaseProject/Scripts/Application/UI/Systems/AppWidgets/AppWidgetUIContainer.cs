using UnityEngine;

namespace CreativeMode
{
    public class AppWidgetUIContainer : MonoBehaviour
    {
        public AppWidgetUIRenderer widgetRenderer;
        public CanvasGroup widgetCanvasGroup;

        public UIContentSize Size => Widget.Size;
        public IAppWidgetUI Widget => widgetRenderer.WidgetUI;

        public void SetData(AppWidget widget)
        {
            Widget.SetData(widget);
        }
    }
}