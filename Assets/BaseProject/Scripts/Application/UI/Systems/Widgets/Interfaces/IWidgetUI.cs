using System;
using UnityEngine;

namespace CreativeMode
{
    public interface IWidgetUI
    {
        GameObject Root { get; }
        UIContentSize Size { get; }
        
        Type DataType { get; }
        void SetData(Widget data);
    }
}