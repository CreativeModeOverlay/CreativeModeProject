using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CreativeMode
{
    public abstract class BaseWidgetUI<T> : MonoBehaviour, IWidgetUI 
        where T : Widget
    {
        public GameObject Root => gameObject;
        public UIContentSize Size => size;
        
        public Type DataType => typeof(T);
        protected T Data => widgetData;
        
        [FormerlySerializedAs("info")]
        public UIContentSize size = UIContentSize.GetDefault();

        [SerializeField]
        private T widgetData;
        private bool isSetDataCalled;

        protected virtual void Start()
        {
            if (!isSetDataCalled && widgetData != null)
                SetData(widgetData);
        }

        public void SetData(Widget data)
        {
            isSetDataCalled = true;
            var tData = (T) data;
            
            widgetData = tData;
            SetData(tData);
        }

        protected virtual void SetData(T data) { }
    }
}