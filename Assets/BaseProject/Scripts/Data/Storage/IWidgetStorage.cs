using System.Collections.Generic;

namespace CreativeMode
{
    public interface IWidgetStorage
    {
        void UpdatePanel(WidgetPanelDB panel);
        WidgetPanelDB GetPanel(string panelId);

        WidgetDataDB CreateWidget(byte[] data);
        WidgetDataDB GetWidget(int id);
        void UpdateWidget(WidgetDataDB data);
        void RemoveWidget(int id);

        List<WidgetDataDB> GetAllWidgets();
    }
}