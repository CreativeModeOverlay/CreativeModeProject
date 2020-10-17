using System;
using UnityEngine;

namespace CreativeMode
{
    public struct AppWidgetInfo
    {
        public string name;
        public Sprite icon;
        public Type dataType;
        public Func<AppWidget> widgetFactory;
    }
}