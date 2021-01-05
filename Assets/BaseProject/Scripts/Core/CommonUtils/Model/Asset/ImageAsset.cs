using System.Linq;
using UnityEngine;

namespace CreativeMode
{
    public class ImageAsset
    {
        public bool IsStatic => Frames.Length == 1;
        public Sprite StaticImage => Frames.First().sprite;
        
        public ImageMetadata Meta { get; }
        public ImageFrame[] Frames { get; }

        public ImageAsset(ImageMetadata meta, ImageFrame[] frames)
        {
            Meta = meta;
            Frames = frames;
        }
    }
}