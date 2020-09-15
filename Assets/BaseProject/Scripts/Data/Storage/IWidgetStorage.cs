﻿using CreativeMode.Generic;

namespace CreativeMode
{
    public interface IWidgetStorage
    {
        IEntityStorage<string, WidgetPanelDB> PanelData { get; }
        ICreatableEntityStorage<WidgetDataDB> WidgetData { get; }
    }
}