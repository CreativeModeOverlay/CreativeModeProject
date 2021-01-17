using System;
using UnityEngine;

namespace CreativeMode
{
    public interface IOverlayWidgetUI
    {
        GameObject Root { get; }
        
        ContentSize Size { get; }
        Type DataType { get; }
        
        void SetData(object data);
        void OnWidgetCreated();
        void OnWidgetDestroyed();
    }
}