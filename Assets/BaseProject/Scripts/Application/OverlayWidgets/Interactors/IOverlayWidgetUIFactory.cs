using System;
using UnityEngine;

namespace CreativeMode
{
    public delegate IOverlayWidgetUI WidgetFactory(GameObject root);
    
    public interface IOverlayWidgetUIFactory
    {
        void RegisterUIFactory<T>(WidgetFactory factory) where T : struct;
        void RegisterUIFactory(Type widgetDataType, WidgetFactory factory);
        
        bool SupportsUI(Type dataType);
        IOverlayWidgetUI CreateWidgetUI(Type widgetType, GameObject root);
    }
}