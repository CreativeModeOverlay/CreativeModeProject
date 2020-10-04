using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreativeMode
{
    public interface IWidgetRegistry
    {
        List<WidgetInfo> GetWidgets();

        WidgetInfo GetWidgetInfo(Type type);

        void RegisterWidget<T>(string name, Sprite icon)
            where T : Widget, new();
        
        void RegisterWidget(Type type, string name, Sprite icon, Func<Widget> factory);
    }
}