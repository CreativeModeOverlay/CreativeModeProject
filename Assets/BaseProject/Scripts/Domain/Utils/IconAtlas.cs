using System;
using System.Collections.Generic;
using System.Linq;
using CreativeMode;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

public class IconAtlas : IDisposable
{
    private static readonly Lazy<Material> clearMaterial = new Lazy<Material>(() => 
        new Material(Shader.Find("Hidden/ClearToTransparent")));

    private readonly RenderTexture atlasTexture;
    private readonly int atlasWidth;
    private readonly int atlasHeight;
    private readonly int iconWidth;
    private readonly int iconHeight;
    private readonly int iconXCount;
    private readonly int iconYCount;
    private readonly int maxIconCount;
    private int loadedIconCount;

    private readonly Dictionary<string, IconData> loadedIconsByUri 
        = new Dictionary<string, IconData>();
    private readonly LinkedList<IconData> loadedIconsByUsage 
        = new LinkedList<IconData>();
    private readonly Queue<int> reclaimableIndices 
        = new Queue<int>();
    private readonly List<IconData> animatedIcons 
        = new List<IconData>();
    
    private float lastAnimationTime;
    private ImageLoader loader;

    public int IconsPerX => iconXCount;
    public int IconsPerY => iconYCount;
    public Texture Texture => atlasTexture;

    public IconAtlas(ImageLoader loader, 
        int atlasWidth, int atlasHeight, 
        int iconWidth, int iconHeight)
    {
        this.loader = loader;
        this.atlasWidth = atlasHeight;
        this.atlasHeight = atlasHeight;
        this.iconWidth = iconWidth;
        this.iconHeight = iconHeight;

        atlasTexture = new RenderTexture(atlasWidth, atlasHeight, 0) { name = "IconAtlasTexture" };
        lastAnimationTime = Time.time;
        
        iconXCount = atlasWidth / iconWidth;
        iconYCount = atlasHeight / iconHeight;
        maxIconCount = iconXCount * iconYCount;

        Observable.EveryUpdate()
            .Subscribe(frame => UpdateAnimation());
    }
    
    private void UpdateAnimation()
    {
        var delta = Time.time - lastAnimationTime;
        lastAnimationTime = Time.time;
        var iconCount = animatedIcons.Count;
        
        for (var i = 0; i < iconCount; i++)
        {
            var icon = animatedIcons[i];
            icon.animationTimer -= delta;

            if (icon.animationTimer <= 0)
            {
                var image = icon.image.Asset;
                var frame = image.Frames[icon.animationFrame];
                ClearRect(icon.uvRect);
                BlitToAtlas(frame.sprite.texture, icon.blitRect);

                icon.animationTimer = frame.duration;
                icon.animationFrame++;

                if (icon.animationFrame >= image.Frames.Length)
                    icon.animationFrame = 0;
            }
        }
    }

    public Rect GetIcon(string url)
    {
        if (loadedIconsByUri.TryGetValue(url, out var data))
        {
            UpdateIconUsage(data);
            return data.uvRect;
        }

        var iconIndex = GetNextIconIndex(out var isReused);
        data = new IconData
        {
            url = url,
            index = iconIndex,
            blitRect = GetIndexPixelRect(iconIndex),
            uvRect = GetIndexUvRect(iconIndex)
        };

        if (isReused)
            ClearRect(data.blitRect);

        StoreIconData(data);
        DownloadIconData(data);
        
        return data.uvRect;
    }

    private void StoreIconData(IconData data)
    {
        loadedIconsByUri[data.url] = data;
        loadedIconsByUsage.AddFirst(data);
    }

    private void UpdateIconUsage(IconData data)
    {
        loadedIconsByUsage.Remove(data);
        loadedIconsByUsage.AddFirst(data);
    }

    private void DownloadIconData(IconData data)
    {
        data.loadingDisposable = loader.GetAsset(data.url)
            .Subscribe(image =>
            {
                if (image.Asset.IsStatic)
                {
                    OnNonAnimatedImageLoaded(image, data);
                }
                else
                {
                    OnAnimatedImageLoaded(image, data);
                }
            }, e =>
            {
                Debug.LogError(e);
                FreeIconData(data);
            });
    }

    private void FreeIconData(IconData data)
    {
        data.blitRect = Rect.zero;
        data.uvRect = Rect.zero;
        data.image.Dispose();

        loadedIconsByUri[data.url] = null;
        loadedIconsByUsage.Remove(data);
        reclaimableIndices.Enqueue(data.index);
    }

    private int GetNextIconIndex(out bool isReused)
    {
        if (loadedIconCount >= maxIconCount)
        {
            var lastValue = loadedIconsByUsage.Last.Value;
            FreeIconData(lastValue);
        }
        
        if (reclaimableIndices.Count > 0)
        {
            isReused = true;
            return reclaimableIndices.Dequeue();
        }

        isReused = false;
        return loadedIconCount++;
    }

    private Rect GetIndexPixelRect(int index)
    {
        return new Rect(
            index % iconXCount * iconWidth, 
            index / iconYCount * iconHeight, 
            iconWidth, iconHeight);
    }

    private Rect GetIndexUvRect(int index)
    {
        var xScale = 1f / atlasWidth;
        var yScale = 1f / atlasHeight;
        var pixelRect = GetIndexPixelRect(index);

        return new Rect(pixelRect.x * xScale, pixelRect.y * yScale,
            pixelRect.width * xScale, pixelRect.height * yScale);
    }

    private void OnNonAnimatedImageLoaded(SharedAsset<ImageAsset> image, IconData data)
    {
        using (image)
        {
            BlitToAtlas(image.Asset.StaticImage.texture, data.blitRect);
        }
    }

    private void OnAnimatedImageLoaded(SharedAsset<ImageAsset> image, IconData data)
    {
        data.image = image;
        animatedIcons.Add(data);
    }

    private void ClearRect(Rect uvRect)
    {
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, 1, 0, 1);
        Graphics.SetRenderTarget(atlasTexture);
        Graphics.DrawTexture(uvRect, Texture2D.blackTexture, clearMaterial.Value);
        GL.PopMatrix();
    }

    private void BlitToAtlas(Texture texture, Rect pixelRect)
    {
        var width = Mathf.Min(texture.width, pixelRect.width - 2);
        var height = Mathf.Min(texture.height, pixelRect.height - 2);
        var xOffset = (iconWidth - width) / 2f;
        var yOffset = (iconHeight - height) / 2f;
        var xScale = 1f / atlasWidth;
        var yScale = 1f / atlasHeight;
        var targetHeight = height * yScale;

        var targetRect = new Rect(
            (xOffset + pixelRect.x) * xScale, 
            (yOffset + pixelRect.y) * yScale + targetHeight, 
            width * xScale, -targetHeight);
        
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, 1, 0, 1);
        Graphics.SetRenderTarget(atlasTexture);
        Graphics.DrawTexture(targetRect, texture);
        GL.PopMatrix();
    }
    
    private class IconData
    {
        public string url;
        public int index;
        public Rect blitRect;
        public Rect uvRect;

        public float animationTimer;
        public int animationFrame;
        public IDisposable loadingDisposable;
        public SharedAsset<ImageAsset> image;
    }

    public void Dispose()
    {
        // TODO: Dispose
    }
}
