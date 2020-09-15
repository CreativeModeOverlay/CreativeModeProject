﻿namespace CreativeMode
{
    public interface IOverlayLayerManager
    {
        void AddLayer(IOverlayLayer layer);
        void RemoveLayer(IOverlayLayer layer);
    }
}