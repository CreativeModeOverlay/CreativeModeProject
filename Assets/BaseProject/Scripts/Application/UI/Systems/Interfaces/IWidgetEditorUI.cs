using System;

namespace CreativeMode
{
    public interface IWidgetEditorUI : IUIElement
    {
        Type DataType { get; }
        AppWidget Data { get; set; }
    }
}