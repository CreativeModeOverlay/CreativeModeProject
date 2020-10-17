using System;

namespace CreativeMode
{
    public interface IAppWidgetUIFactory
    {
        bool SupportsUI(Type dataType);
        
        UIContentSize GetSizeInfo(Type widgetType);
        IAppWidgetUI CreateWidgetUI(Type widgetType);
        IWidgetEditorUI CreateWidgetEditorUI(Type widgetType);
    }
}