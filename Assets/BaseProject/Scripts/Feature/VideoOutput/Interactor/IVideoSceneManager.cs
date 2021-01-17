namespace CreativeMode
{
    public interface IVideoSceneManager
    {
        IVideoTransition DefaultTransition { get; set; }
        IVideoLayerManager GlobalLayers { get; }

        IVideoLayerManager GetLayers(IVideoElement element);
        
        void Show(IVideoElement scene);
        void Show(IVideoElement scene, IVideoTransition transition);
    }
}