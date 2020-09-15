using CreativeMode.Generic;
using SQLite;

namespace CreativeMode.Impl
{
    public class WidgetStorage : IWidgetStorage
    {
        public IEntityStorage<string, WidgetPanelDB> PanelData { get; }
        public ICreatableEntityStorage<WidgetDataDB> WidgetData { get; }

        public WidgetStorage(SQLiteConnection connection)
        {
            PanelData = new EntityStorage<WidgetPanelTable, string, WidgetPanelDB>(connection, 
                JsonNetEntitySerializer<WidgetPanelDB>.Instance);
            
            WidgetData = new CreatableEntityStorage<WidgetDataTable, WidgetDataDB>(connection, 
                JsonNetEntitySerializer<WidgetDataDB>.Instance);
        }
    }
    
    class WidgetPanelTable : EntityDb<string, WidgetPanelDB> { }
    class WidgetDataTable : CreatableEntityDb<WidgetDataDB> { }
}