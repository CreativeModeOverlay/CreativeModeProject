using System;

namespace CreativeMode
{
    public interface IWidgetEditorUI : IUIElement
    {
        Type DataType { get; }
        Widget Data { get; set; }
    }
}