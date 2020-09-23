using System;

namespace CreativeMode
{
    public interface IWidgetUI : IUIElement
    {
        Type DataType { get; }
        void SetData(Widget data);
    }
}