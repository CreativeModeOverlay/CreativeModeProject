namespace CreativeMode
{
    public interface IVideoLayerManager
    {
        void AddLayer(IVideoLayer layer);
        void RemoveLayer(IVideoLayer layer);
    }
}