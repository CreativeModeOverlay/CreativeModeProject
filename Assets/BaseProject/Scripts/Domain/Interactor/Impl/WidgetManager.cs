using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
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
            widgets = Storage.GetAllWidgets().Select(w => new WidgetData
            {
                id = w.id,
                data = DeserializeData(w.data)
            }).ToList();
            
            widgetsSubject = new BehaviorSubject<List<WidgetData>>(widgets);
            widgetUpdatedSubject = new Subject<WidgetData>();
        }
        
        public IObservable<IReadOnlyList<WidgetData>> GetWidgetsForPanel(string panelId)
        {
            return panelWidgets.EveryUpdate(panelId)
                .SelectMany(_ =>
                {
                    var panel = GetPanel(panelId);
                    var set = new HashSet<int>(panel.widgetIds);
                    return Widgets.Select(l => l.Where(w => set.Contains(w.id)));
                }).Select(w => w.ToList());
        }

        public void UpdatePanel(WidgetPanel panel)
        {
            Storage.UpdatePanel(new WidgetPanelDB
            {
                id = panel.id,
                data = SerializeWidgetIds(panel.widgetIds)
            });
            panelWidgets.NotifyUpdate(panel.id);
        }

        public WidgetPanel GetPanel(string panelId)
        {
            var panel = Storage.GetPanel(panelId);
            return new WidgetPanel
            {
                id = panel.id,
                widgetIds = DeserializeWidgetIds(panel.data)
            };
        }

        public WidgetData CreateWidget(object data)
        {
            var widget = Storage.CreateWidget(SerializeData(data));
            var widgetData = new WidgetData
            {
                id = widget.id,
                data = data
            };
            
            widgets.Add(widgetData);
            widgetsSubject.OnNext(widgets);

            return widgetData;
        }

        public WidgetData GetWidget(int id)
        {
            var dbWidget = Storage.GetWidget(id);
            return new WidgetData
            {
                id = dbWidget.id,
                data = DeserializeData(dbWidget.data)
            };
        }

        public void UpdateWidget(WidgetData data)
        {
            var widgetData = new WidgetDataDB
            {
                id = data.id,
                data = SerializeData(data.data)
            };

            widgetUpdatedSubject.OnNext(data);
            Storage.UpdateWidget(widgetData);
        }

        public void RemoveWidget(int id)
        {
            if (widgets.RemoveWhere(w => w.id == id))
                widgetsSubject.OnNext(widgets);

            Storage.RemoveWidget(id);
        }

        private byte[] SerializeData(object data)
        {
            return Encoding.Default.GetBytes(
                JsonConvert.SerializeObject(data, typeof(object), jsonSettings));
        }

        private object DeserializeData(byte[] serializedData)
        {
            return JsonConvert.DeserializeObject(
                Encoding.Default.GetString(serializedData), typeof(object), jsonSettings);
        }
        
        private byte[] SerializeWidgetIds(List<int> widgetIds)
        {
            var ids = widgetIds.ToArray();
            var data = new byte[ids.Length * sizeof(int)];
            Buffer.BlockCopy(ids, 0, data, 0, data.Length);
            
            return data;
        }

        private List<int> DeserializeWidgetIds(byte[] data)
        {
            var ids = new int[data.Length / sizeof(int)];
            Buffer.BlockCopy(data, 0, ids, 0, ids.Length);
            return new List<int>(ids);
        }
        
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
    }
}