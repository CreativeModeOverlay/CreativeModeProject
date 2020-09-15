using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CreativeMode
{
    public interface IWidgetUI
    {
        GameObject Root { get; }
        WidgetUISize Size { get; }
        
        Type DataType { get; }
        void SetData(BaseWidget data);
    }

    public abstract class BaseWidgetUI<T> : MonoBehaviour, IWidgetUI 
        where T : BaseWidget
    {
        [FormerlySerializedAs("info")]
        public WidgetUISize size;
        
        public Type DataType => typeof(T);
        public GameObject Root => gameObject;
        public WidgetUISize Size => size;

        public void SetData(BaseWidget data) => SetData((T) data);
        protected virtual void SetData(T data) { }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public abstract class BaseGraphicWidgetUI<T> : Graphic, IWidgetUI
        where T : BaseWidget
    {
        [FormerlySerializedAs("info")]
        public WidgetUISize size;
        
        public Type DataType => typeof(T);
        public GameObject Root => gameObject;
        public WidgetUISize Size => size;

        public void SetData(BaseWidget data) => SetData((T) data);
        protected virtual void SetData(T data) { }
    }
}