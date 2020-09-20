using System;

namespace CreativeMode
{
    public interface IWidgetUIFactory
    {
        bool SupportsUI(Type dataType);
        
        UIContentSize GetSizeInfo(Type widgetType);
        IWidgetUI CreateWidgetUI(Type widgetType);
    }
}