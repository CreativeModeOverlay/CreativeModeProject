using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace CreativeMode.Impl
{
    public class WidgetManager : IWidgetManager
    {
        public IObservable<IReadOnlyList<WidgetData>> Widgets => widgetsSubject;
        public IObservable<WidgetData> WidgetUpdated => widgetUpdatedSubject;

        private IWidgetStorage Storage => Instance<IWidgetStorage>.Get();
        private readonly ItemWatcher<string> panelWidgets = new ItemWatcher<string>();
        
        private readonly List<WidgetData> widgets;
        private readonly BehaviorSubject<List<WidgetData>> widgetsSubject;
        private readonly Subject<WidgetData> widgetUpdatedSubject;

        public WidgetManager()
        {
            widgetsSubject = new BehaviorSubject<List<WidgetData>>(widgets);
            widgetUpdatedSubject = new Subject<WidgetData>();
        }
        
        public IObservable<IReadOnlyList<WidgetPanel.Widget>> WidgetsForPanel(string panelId)
        {
            return panelWidgets.EveryUpdate(panelId)
                .Select(_ => GetPanel(panelId).widgets);
        }

        public void UpdatePanel(WidgetPanel panel)
        {
            Storage.PanelData.Put(panel.id, ToDB(panel));
            panelWidgets.NotifyUpdate(panel.id);
        }

        public WidgetPanel GetPanel(string panelId)
        {
            return FromDB(Storage.PanelData.Get(panelId), panelId);
        }

        public WidgetData CreateWidget(Widget data)
        {
            if(data == null)
                throw new ArgumentException("Cannot add null widget");
            
            return new WidgetData
            {
                id = Storage.WidgetData.Insert(ToDB(new WidgetData { data = data })),
                type = data.GetType(),
                data = data
            };
        }

        public WidgetData GetWidget(int id)
        {
            var widgetData = Storage.WidgetData.Get(id);

            if (widgetData.data == null)
                return null;

            return FromDB(widgetData, id);
        }

        public void UpdateWidget(WidgetData data)
        {
            Storage.WidgetData.Put(data.id, ToDB(data));
        }

        public void RemoveWidget(int id)
        {
            if (widgets.RemoveWhere(w => w.id == id))
            {
                widgetsSubject.OnNext(widgets);
            }
            
            Storage.WidgetData.Delete(id);
        }
        
        private WidgetDataDB ToDB(WidgetData data)
        {
            return new WidgetDataDB
            {
                name = data.name,
                data = data
            };
        }

        private WidgetPanelDB ToDB(WidgetPanel panel)
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

        private WidgetData FromDB(WidgetDataDB data, int id)
        {
            return new WidgetData
            {
                id = id,
                name = data.name,
                type = data.data.GetType(),
                data = (Widget) data.data
            };
        }

        private WidgetPanel FromDB(WidgetPanelDB data, string id)
        {
            return new WidgetPanel
            {
                id = id,
                widgets = data.widgets?.Select(w => new WidgetPanel.Widget()
                {
                    widgetId = w.widgetId,
                    layout = new WidgetLayoutParams
                    {
                        width = w.width,
                        height = w.height
                    }
                }).ToList() ?? new List<WidgetPanel.Widget>()
            };
        }
    }
}