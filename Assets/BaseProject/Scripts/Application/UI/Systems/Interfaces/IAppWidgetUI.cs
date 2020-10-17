using System;

namespace CreativeMode
{
    public interface IAppWidgetUI : IUIElement
    {
        Type DataType { get; }
        void SetData(AppWidget data);
    }
}