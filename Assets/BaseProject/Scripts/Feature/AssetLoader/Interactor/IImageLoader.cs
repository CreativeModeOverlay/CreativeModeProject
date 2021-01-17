using System;

namespace CreativeMode
{
    public interface IImageLoader
    {
        void Prefetch(string url);
        
        IObservable<SharedAsset<ImageAsset>> GetImage(string url);
    }
}