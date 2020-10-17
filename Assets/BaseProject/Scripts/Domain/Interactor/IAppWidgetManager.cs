using System;
using System.Collections.Generic;

namespace CreativeMode
{
    public interface IAppWidgetManager
    {
        IObservable<AppWidgetData> WidgetUpdated { get; }
        IObservable<IReadOnlyList<AppWidgetData>> Widgets { get; }
        IObservable<IReadOnlyList<AppWidgetPanel.Widget>> WidgetsForPanel(string panelId);

        void UpdatePanel(AppWidgetPanel panel);
        AppWidgetPanel GetPanel(string panelId);

        AppWidgetData CreateWidget(Type dataType);
        AppWidgetData CreateWidget(AppWidget data);
        AppWidgetData GetWidget(int id);
        void UpdateWidget(AppWidgetData data);
        void RemoveWidget(int id);
    }
}