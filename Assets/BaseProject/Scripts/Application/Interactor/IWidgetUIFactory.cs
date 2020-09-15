using System;

namespace CreativeMode
{
    public interface IWidgetUIFactory
    {
        bool CanCreateUI(Type dataType);
        IWidgetUI CreateWidgetUI(Type widgetType);
    }
}