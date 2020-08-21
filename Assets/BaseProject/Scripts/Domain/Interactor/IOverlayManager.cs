﻿namespace CreativeMode
{
    public interface IOverlayManager
    {
        IOverlayTransition DefaultTransition { get; set; }
        IOverlayLayerManager GlobalLayers { get; }

        IOverlayLayerManager GetLayers(IOverlayElement element);
        
        void Show(IOverlayElement scene);
        void Show(IOverlayElement scene, IOverlayTransition transition);
    }

    public interface IOverlayLayerManager
    {
        void AddLayer(IOverlayLayer layer);
        void RemoveLayer(IOverlayLayer layer);
    }
}