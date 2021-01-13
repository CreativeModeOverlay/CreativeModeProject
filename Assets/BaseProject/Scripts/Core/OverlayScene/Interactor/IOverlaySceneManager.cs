namespace CreativeMode
{
    public interface IOverlaySceneManager
    {
        IOverlayTransition DefaultTransition { get; set; }
        IOverlayLayerManager GlobalLayers { get; }

        IOverlayLayerManager GetLayers(IOverlayElement element);
        
        void Show(IOverlayElement scene);
        void Show(IOverlayElement scene, IOverlayTransition transition);
    }
}