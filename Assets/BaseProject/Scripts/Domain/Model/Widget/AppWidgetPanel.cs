using System.Collections.Generic;

namespace CreativeMode
{
    public struct AppWidgetPanel
    {
        public string id;
        public List<Widget> widgets;
        
        public struct Widget
        {
            public int widgetId;
            public WidgetLayoutParams layout;
        }

        public void AddWidget(AppWidgetData data) => widgets.Add(new Widget
        {
            widgetId = data.id
        });
        
        public void AddWidget(AppWidgetData data, WidgetLayoutParams layout) => widgets.Add(new Widget
        {
            widgetId = data.id,
            layout = layout
        });
    }
}