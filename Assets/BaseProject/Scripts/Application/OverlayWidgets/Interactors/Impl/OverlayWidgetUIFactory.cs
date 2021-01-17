using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreativeMode.Impl
{
    internal class OverlayWidgetUIFactory : IOverlayWidgetUIFactory
    {
        private readonly Dictionary<Type, WidgetFactory> factoryDictionary
            = new Dictionary<Type, WidgetFactory>();
        
        public void RegisterUIFactory<T>(WidgetFactory factory) where T : struct
            => RegisterUIFactory(typeof(T), factory);
        
        public void RegisterUIFactory(Type widgetDataType, WidgetFactory factory)
        {
            factoryDictionary[widgetDataType] = factory;
        }

        public bool SupportsUI(Type dataType)
        {
            return factoryDictionary.ContainsKey(dataType);
        }

        public IOverlayWidgetUI CreateWidgetUI(Type widgetType, GameObject root)
        {
            return factoryDictionary[widgetType](root);
        }
    }
}