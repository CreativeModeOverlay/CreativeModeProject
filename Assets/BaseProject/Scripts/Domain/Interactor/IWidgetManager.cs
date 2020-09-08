using System;
using System.Collections.Generic;

namespace CreativeMode
{
    public interface IWidgetManager
    {
        IObservable<WidgetData> WidgetUpdated { get; }
        
        IObservable<IReadOnlyList<WidgetData>> Widgets { get; }
        IObservable<IReadOnlyList<WidgetData>> GetWidgetsForPanel(string panelId);

        void UpdatePanel(WidgetPanel panel);
        WidgetPanel GetPanel(string panelId);

        WidgetData CreateWidget(object data);
        WidgetData GetWidget(int id);
        void UpdateWidget(WidgetData data);
        void RemoveWidget(int id);
    }
}