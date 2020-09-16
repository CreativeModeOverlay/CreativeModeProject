using CreativeMode.Generic;
using SQLite;

namespace CreativeMode.Impl
{
    public class WidgetStorage : IWidgetStorage
    {
        public IEntityStorage<string, WidgetPanelDB> PanelData { get; }
        public IEntityStorage<int, WidgetDataDB> WidgetData { get; }

        public WidgetStorage(SQLiteConnection connection)
        {
            PanelData = new EntityStorage<WidgetPanelTable, string, WidgetPanelDB>(connection, 
                JsonNetEntitySerializer<WidgetPanelDB>.Instance);
            
            WidgetData = new EntityStorage<WidgetDataTable, int, WidgetDataDB>(connection, 
                JsonNetEntitySerializer<WidgetDataDB>.Instance);
        }
    }
    
    class WidgetPanelTable : EntityTableDb<string> { }
    class WidgetDataTable : EntityTableDb<int> { }
}