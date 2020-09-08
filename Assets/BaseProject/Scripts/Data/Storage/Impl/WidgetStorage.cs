using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace CreativeMode.Impl
{
    public class WidgetStorage : IWidgetStorage
    {
        private SQLiteConnection connection;

        public WidgetStorage(SQLiteConnection connection)
        {
            this.connection = connection;

            connection.CreateTable<WidgetData>();
            connection.CreateTable<PanelData>();
        }

        public void UpdatePanel(WidgetPanelDB panel)
        {
            connection.InsertOrReplace(new PanelData
            {
                Id = panel.id,
                Data = panel.data
            });
        }

        public WidgetPanelDB GetPanel(string panelId)
        {
            var data = connection.Table<PanelData>()
                .FirstOrDefault(p => p.Id == panelId);

            if (data == null)
                return null;

            return new WidgetPanelDB
            {
                id = data.Id,
                data = data.Data
            };
        }

        public WidgetDataDB CreateWidget(byte[] data)
        {
            var dbData = new WidgetData { Data = data};
            connection.Insert(dbData);

            return ToDBInstance(dbData);
        }

        public WidgetDataDB GetWidget(int id)
        {
            return ToDBInstance(connection.Table<WidgetData>()
                .FirstOrDefault(w => w.Id == id));
        }

        public void UpdateWidget(WidgetDataDB data)
        {
            connection.InsertOrReplace(new WidgetData
            {
                Id = data.id,
                Data = data.data
            });
        }

        public void RemoveWidget(int widgetId)
        {
            connection.Delete<WidgetData>(widgetId);
        }

        public List<WidgetDataDB> GetAllWidgets()
        {
            return connection.Table<WidgetData>()
                .Select(ToDBInstance)
                .ToList();
        }

        private static WidgetDataDB ToDBInstance(WidgetData data)
        {
            return new WidgetDataDB
            {
                id = data.Id,
                data = data.Data
            };
        }

        private class WidgetData
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public byte[] Data { get; set; }
        }
        
        private class PanelData
        {
            [PrimaryKey]
            public string Id { get; set; }
            public byte[] Data { get; set; }
        }
    }
}