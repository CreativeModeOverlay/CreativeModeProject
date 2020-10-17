using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreativeMode
{
    public interface IAppWidgetRegistry
    {
        List<AppWidgetInfo> GetWidgets();

        AppWidgetInfo GetWidgetInfo(Type type);

        void RegisterWidget<T>(string name, Sprite icon)
            where T : AppWidget, new();
        
        void RegisterWidget(Type type, string name, Sprite icon, Func<AppWidget> factory);
    }
}