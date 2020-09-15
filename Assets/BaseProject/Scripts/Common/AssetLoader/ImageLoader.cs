using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CreativeMode;
using ThreeDISevenZeroR.UnityGifDecoder;
using UniRx;
using UnityEngine;
 
public class ImageLoader : AssetLoader<ImageAsset>
{
    protected override IObservable<SharedAsset<ImageAsset>.IReferenceProvider> CreateAssetProvider(string url)
    {
        return GetAssetStream(url)
            .Select(s =>
            {
                using (s)
                {
                    return GetImageBytes(s);
                }
            })
            .ObserveOnMainThread()
            .SelectMany(b => IsGifImage(b)
                ? LoadAnimatedImage(b)
                : LoadNonAnimatedImage(b))
            .Select(frames =>
            {
                var asset = new ImageAsset(new ImageMetadata {url = url}, frames);
                return SharedAsset<ImageAsset>.Create(asset,
                    a => a.Frames != null && !a.Frames.All(f => f.sprite && f.sprite.texture),
                    a =>
                    {
                        var destroyFunc = Application.isPlaying
                            ? (Action<UnityEngine.Object>) UnityEngine.Object.Destroy
                            : (Action<UnityEngine.Object>) UnityEngine.Object.DestroyImmediate;

                        foreach (var frame in a.Frames)
                        {
                            destroyFunc(frame.sprite.texture);
                            destroyFunc(frame.sprite);
                        }
                    });
            });
    }

    private byte[] GetImageBytes(Stream stream)
    {
        using (stream)
        {
            if (stream.CanSeek)
            {
                var startPosition = stream.Position;
                var reader = new BinaryReader(stream);
                var header = new string(reader.ReadChars(3));
                stream.Position = startPosition;
            
                if (header.StartsWith("ID3"))
                {
                    var tagStream = new TagLibStream("file.mp3", stream);
                    var tagFile = TagLib.File.Create(tagStream);
                    var firstPic = tagFile.Tag.Pictures.FirstOrDefault();

                    if (firstPic != null)
                        return firstPic.Data.ToArray();

                    throw new ArgumentException("No image in mp3 file");
                }
            }

            var streamCopy = new MemoryStream();
            stream.CopyTo(streamCopy);
            return streamCopy.ToArray();
        }
    }

    private IObservable<ImageFrame[]> LoadNonAnimatedImage(byte[] data)
    {
        return Observable.Start(() =>
        {
            var texture = CreateTexture(0, 0, true);
            texture.LoadImage(data);
            texture.Apply(true, true);

            return new[]
            {
                new ImageFrame
                {
                    sprite = Sprite.Create(texture,
                        new Rect(0, 0, texture.width, texture.height),
                        Vector2.zero, 100, 0, SpriteMeshType.FullRect),
                    duration = float.MaxValue,
                }
            };
        }, Scheduler.MainThread);
    }

    private IObservable<ImageFrame[]> LoadAnimatedImage(byte[] data)
    {
        var spriteList = new List<Sprite>();
        var nanoPool = new Queue<Color32[]>();
        
        return Observable.CreateSafe<GifFrameData>(o =>
            {
                var stream = new GifStream(data);

                while (true)
                {
                    switch (stream.CurrentToken)
                    {
                        case GifStream.Token.Image:
                            var img = stream.ReadImage();
                            Color32[] colors;

                            lock (nanoPool)
                                colors = nanoPool.Count > 0 ? nanoPool.Dequeue() : null;

                            if (colors == null)
                                colors = new Color32[img.colors.Length];
                            
                            Array.Copy(img.colors, colors, colors.Length);

                            o.OnNext(new GifFrameData
                            {
                                colors = colors,
                                duration = img.SafeDelaySeconds,
                                width = stream.Header.width,
                                height = stream.Header.height
                            });
                            break;

                        case GifStream.Token.EndOfFile:
                            o.OnCompleted();
                            return stream;

                        default:
                            stream.SkipToken();
                            break;
                    }
                }
            })
            .MaxItemsPerFrame(5)
            .Select(f =>
            {
                var texture = CreateTexture(f.width, f.height, false);
                var buffer = texture.GetRawTextureData<Color32>();
                buffer.CopyFrom(f.colors);
                texture.Apply(true, true);
                
                texture.wrapMode = TextureWrapMode.Clamp;

                var sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    Vector2.zero, 100, 0, SpriteMeshType.FullRect);

                lock (nanoPool)
                    nanoPool.Enqueue(f.colors);
                
                spriteList.Add(sprite);

                return new ImageFrame
                {
                    duration = f.duration,
                    sprite = sprite,
                };
            })
            .ToList()
            .DoOnCancel(() =>
            {
                for (var i = 0; i < spriteList.Count; i++)
                {
                    UnityEngine.Object.Destroy(spriteList[i].texture);
                    UnityEngine.Object.Destroy(spriteList[i]);
                }
            })
            .Select(list => list.ToArray())
            .SubscribeOn(Scheduler.ThreadPool);
    }

    private Texture2D CreateTexture(int width, int height, bool mipMaps)
    {
        return new Texture2D(width, height, TextureFormat.RGBA32, mipMaps)
        {
            anisoLevel = 16, 
            filterMode = FilterMode.Trilinear,
            wrapMode = TextureWrapMode.Clamp
        };
    }

    private bool IsGifImage(byte[] bytes)
    {
        return bytes.Length > 3 && 
               bytes[0] == 'G' && 
               bytes[1] == 'I' && 
               bytes[2] == 'F';
    }
    
    private struct GifFrameData
    {
        public int width;
        public int height;
        public float duration;
        public Color32[] colors;
    }
}
