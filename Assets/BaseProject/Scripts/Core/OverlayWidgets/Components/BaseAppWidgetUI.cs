using System;
using UnityEngine;

namespace CreativeMode
{
    public abstract class BaseAppWidgetUI<T> : MonoBehaviour, IOverlayWidgetUI 
        where T : struct
    {
        public GameObject Root => gameObject;
        public ContentSize Size => contentSize;
        
        public Type DataType => typeof(T);
        protected T Data => widgetData;
        
        [SerializeField]
        private ContentSize contentSize;

        [SerializeField]
        private T widgetData;
        
        private bool isSetDataCalled;

        protected virtual void Start()
        {
            if (!isSetDataCalled)
                SetData(widgetData);
        }
        
        public void SetData(object data)
        {
            isSetDataCalled = true;
            var tData = (T) data;
            
            widgetData = tData;
            SetData(tData);
        }
        
        protected virtual void SetData(T data) { }
        
        public virtual void OnWidgetCreated() { }
        public virtual void OnWidgetDestroyed() { }
    }
}