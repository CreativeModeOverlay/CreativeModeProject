using System;
using UnityEngine;

namespace CreativeMode
{
    public interface IWidgetUI
    {
        GameObject Root { get; }
        WidgetUISize Size { get; }
        
        Type DataType { get; }
        void SetData(BaseWidget data);
    }
}