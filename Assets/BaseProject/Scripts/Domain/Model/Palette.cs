using UnityEngine;

namespace CreativeMode
{
    public struct Palette
    {
        public Swatch[] Swatches { get; }
        public Color VibrantColor { get; }
        public Color BackgroundColor { get; }

        public Palette(Swatch[] swatches, Color vibrantColor, Color backgroundColor)
        {
            Swatches = swatches;
            VibrantColor = vibrantColor;
            BackgroundColor = backgroundColor;
        }
        
        public struct Swatch
        {
            public int count;
            public Color center;
            public Color darkest;
            public Color lightest;
        }
    }
}