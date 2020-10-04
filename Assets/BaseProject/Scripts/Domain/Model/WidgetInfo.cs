using System;
using UnityEngine;

namespace CreativeMode
{
    public struct WidgetInfo
    {
        public string name;
        public Sprite icon;
        public Type dataType;
        public Func<Widget> widgetFactory;
    }
}