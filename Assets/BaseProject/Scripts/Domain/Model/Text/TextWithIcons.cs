using System.Collections.Generic;
using System.Text;

namespace CreativeMode
{
    public struct TextWithIcons
    {
        public TextIcon[] icons;
        public string text;

        public bool IsIconsOnly => string.IsNullOrWhiteSpace(text) && icons != null && icons.Length > 0;

        public TextWithIcons(string text) : this(text, new TextIcon[0]) {}

        public TextWithIcons(string text, TextIcon[] icons)
        {
            this.text = text;
            this.icons = icons;
        }

        public static TextWithIcons Join(string separator, params TextWithIcons[] items)
        {
            var builder = new StringBuilder();
            var icons = new List<TextIcon>();

            foreach (var item in items)
            {
                if (item.icons != null)
                {
                    foreach (var icon in item.icons)
                    {
                        icons.Add(new TextIcon
                        {
                            position = builder.Length + icon.position,
                            rect = icon.rect,
                            atlas = icon.atlas
                        });
                    }
                }
                
                builder.Append(item.text);
            }
            
            return new TextWithIcons(builder.ToString(), icons.ToArray());
        }
    }
}