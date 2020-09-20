using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreativeMode.Impl
{
    public class WidgetUIFactory : MonoBehaviour, IWidgetUIFactory
    {
        public GameObject[] widgetPrefabs;
        
        private readonly Dictionary<Type, GameObject> prefabCache 
            = new Dictionary<Type, GameObject>();

        public bool SupportsUI(Type widgetType)
        {
            return GetWidgetPrefab(widgetType) != null;
        }

        public UIContentSize GetSizeInfo(Type widgetType)
        {
            return GetWidgetPrefab(widgetType)
                .GetComponent<IWidgetUI>().Size;
        }

        public IWidgetUI CreateWidgetUI(Type widgetType)
        {
            var prefab = GetWidgetPrefab(widgetType);
            return Instantiate(prefab).GetComponent<IWidgetUI>();
        }

        private GameObject GetWidgetPrefab(Type widgetType)
        {
            if (prefabCache.TryGetValue(widgetType, out var prefab))
                return prefab;

            foreach (var w in widgetPrefabs)
            {
                var ui = w.GetComponent<IWidgetUI>();
                
                if (ui != null && ui.DataType == widgetType)
                {
                    prefabCache[widgetType] = w;
                    return w;
                }
            }

            prefabCache[widgetType] = null;
            return null;
        }
    }
}