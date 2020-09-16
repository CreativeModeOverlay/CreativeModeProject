﻿using CreativeMode.Generic;

namespace CreativeMode
{
    public interface IWidgetStorage
    {
        IEntityStorage<string, WidgetPanelDB> PanelData { get; }
        IEntityStorage<int, WidgetDataDB> WidgetData { get; }
    }
}