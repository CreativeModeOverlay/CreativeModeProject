using System;

namespace CreativeMode
{
    public interface IAppWidgetEditorUI : IUIElement
    {
        Type DataType { get; }
        AppWidget Data { get; set; }
    }
}