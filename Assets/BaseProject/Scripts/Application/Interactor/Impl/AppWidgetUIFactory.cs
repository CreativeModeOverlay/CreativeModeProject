using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreativeMode.Impl
{
    public class AppWidgetUIFactory : MonoBehaviour, IAppWidgetUIFactory
    {
        public GameObject[] widgetPrefabs;
        public GameObject[] widgetEditorPrefabs;
        public GameObject defaultWidgetEditorPrefab;
        
        private readonly Dictionary<Type, GameObject> prefabCache 
            = new Dictionary<Type, GameObject>();

        public bool SupportsUI(Type widgetType)
        {
            return GetWidgetPrefab(widgetType) != null;
        }

        public UIContentSize GetSizeInfo(Type widgetType)
        {
            return GetWidgetPrefab(widgetType)
                .GetComponent<IAppWidgetUI>().Size;
        }

        public IAppWidgetUI CreateWidgetUI(Type widgetType)
        {
            var prefab = GetWidgetPrefab(widgetType);
            return Instantiate(prefab).GetComponent<IAppWidgetUI>();
        }

        public IWidgetEditorUI CreateWidgetEditorUI(Type widgetType)
        {
            foreach (var prefab in widgetEditorPrefabs)
            {
                if (prefab.GetComponent<IWidgetEditorUI>().DataType == widgetType)
                {
                    return Instantiate(prefab).GetComponent<IWidgetEditorUI>();
                }
            }
            
            return Instantiate(defaultWidgetEditorPrefab).GetComponent<IWidgetEditorUI>();
        }

        private GameObject GetWidgetPrefab(Type widgetType)
        {
            if (prefabCache.TryGetValue(widgetType, out var prefab))
                return prefab;

            foreach (var w in widgetPrefabs)
            {
                var ui = w.GetComponent<IAppWidgetUI>();
                
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