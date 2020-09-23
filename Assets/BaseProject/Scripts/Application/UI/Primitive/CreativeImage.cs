using System;
using CreativeMode;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CreativeImage : Image
{
    private ImageLoader ImageLoader => Instance<ImageLoader>.Get();

    private SharedAsset<ImageAsset> currentImage;
    
    public void PreloadImage(string url)
    {
        ImageLoader.PreloadAsset(url);
    }
    
    public void SetImage(string url, Sprite noImageDummy, Action<ImageAsset> onImageSet = null)
    {
        ImageLoader.GetAsset(url)
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
