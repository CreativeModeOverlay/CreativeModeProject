using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UniRx;

namespace CreativeMode.Impl
{
    public class AppWidgetManager : IAppWidgetManager
    {
        private IAppWidgetRegistry WidgetRegistry => Instance<IAppWidgetRegistry>.Get();
        
        public IObservable<IReadOnlyList<AppWidgetData>> Widgets => widgetsSubject;
        public IObservable<AppWidgetData> WidgetUpdated => widgetUpdatedSubject;
        
        private readonly ItemWatcher<string> panelWidgets = new ItemWatcher<string>();
        
        private readonly List<AppWidgetData> widgets;
        private readonly BehaviorSubject<List<AppWidgetData>> widgetsSubject;
        private readonly Subject<AppWidgetData> widgetUpdatedSubject;

        public AppWidgetManager()
        {
            widgetsSubject = new BehaviorSubject<List<AppWidgetData>>(widgets);
            widgetUpdatedSubject = new Subject<AppWidgetData>();
        }
        
        public IObservable<IReadOnlyList<AppWidgetPanel.Widget>> WidgetsForPanel(string panelId)
        {
            return panelWidgets.EveryUpdate(panelId)
                .Select(_ => GetPanel(panelId).widgets);
        }

        public void UpdatePanel(AppWidgetPanel panel)
        {
            panelWidgets.NotifyUpdate(panel.id);
        }

        public AppWidgetPanel GetPanel(string panelId)
        {
            return new AppWidgetPanel();
            // return FromDB(Storage.PanelData.Get(panelId), panelId);
        }

        public AppWidgetData CreateWidget(Type dataType)
        {
            return CreateWidget(WidgetRegistry
                .GetWidgetInfo(dataType)
                .widgetFactory());
        }

        public AppWidgetData CreateWidget(AppWidget data)
        {
            if(data == null)
                throw new ArgumentException("Cannot add null widget");

            var dataType = data.GetType();
            var info = WidgetRegistry.GetWidgetInfo(dataType);
            var existingWidgetCount = widgets.Count(w => w.type == dataType);
            var postfix = existingWidgetCount >= 1 ? $" ({existingWidgetCount + 1})" : "";

            var widgetData = new AppWidgetData
            {
                type = data.GetType(),
                name = info.name + postfix,
                data = data
            };
            
            //widgetData.id = Storage.WidgetData.Insert(ToDB(widgetData));

            widgets.Add(widgetData);
            widgetsSubject.OnNext(widgets);
            
            return widgetData;
        }

        public AppWidgetData GetWidget(int id)
        {
            return null;
            /*var widgetData = Storage.WidgetData.Get(id);

            if (widgetData.data == null)
                return null;

            return FromDB(widgetData, id);*/
        }

        public void UpdateWidget(AppWidgetData data)
        {
            if(data?.data == null)
                throw new ArgumentException("Cannot update to null data");
            
            // Storage.WidgetData.Put(data.id, ToDB(data));
            widgetUpdatedSubject.OnNext(data);
        }

        public void RemoveWidget(int id)
        {
            if (widgets.RemoveWhere(w => w.id == id))
            {
                widgetsSubject.OnNext(widgets);
            }
            
            // Storage.WidgetData.Delete(id);
        }
        
        private WidgetDataDB ToDB(AppWidgetData data)
        {
            return new WidgetDataDB
            {
                name = data.name,
                data = data.data
            };
        }

        private WidgetPanelDB ToDB(AppWidgetPanel panel)
        {
            return new WidgetPanelDB
            {
                widgets = panel.widgets.Select(w => new PanelWidgetDB
                {
                    widgetId = w.widgetId,
                    width = w.layout.width,
                    height = w.layout.height
                }).ToList()
            };
        }

        private AppWidgetData FromDB(WidgetDataDB data, int id)
        {
            return new AppWidgetData
            {
                id = id,
                name = data.name,
                type = data.data.GetType(),
                data = (AppWidget) data.data
            };
        }

        private AppWidgetPanel FromDB(WidgetPanelDB data, string id)
        {
            return new AppWidgetPanel
            {
                id = id,
                widgets = data.widgets?.Select(w => new AppWidgetPanel.Widget()
                {
                    widgetId = w.widgetId,
                    layout = new WidgetLayoutParams
                    {
                        width = w.width,
                        height = w.height
                    }
                }).ToList() ?? new List<AppWidgetPanel.Widget>()
            };
        }
    }
}