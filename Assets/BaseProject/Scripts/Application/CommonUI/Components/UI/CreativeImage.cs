using System;
using CreativeMode;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CreativeImage : Image
{
    private IImageLoader ImageLoader => Instance<IImageLoader>.Get();

    private SharedAsset<ImageAsset> currentImage;
    
    public void PrefetchImage(string url)
    {
        ImageLoader.Prefetch(url);
    }
    
    public void SetImage(string url, Sprite noImageDummy, Action<ImageAsset> onImageSet = null)
    {
        ImageLoader.GetImage(url)
            .Subscribe(img =>
            {
                currentImage.Dispose();
                currentImage = img;
                sprite = img.Asset.StaticImage;
                onImageSet?.Invoke(img.Asset);
            }, error =>
            {
                currentImage.Dispose();
                sprite = noImageDummy;
            })
            .AddTo(gameObject);
    }
}
