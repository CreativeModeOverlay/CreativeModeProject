using UnityEngine;

namespace CreativeMode
{
    public struct TextTag
    {
        public int textStartIndex;
        public int textEndIndex;
        public bool isClosing;
        public object tag;
    }

    public class SizeTag
    {
        public int size;
    }
    
    public class SizeScaleTag
    {
        public float scale;
    }

    public class IconTag
    {
        public Rect rect;
        public Texture texture;
        public bool isModifier;
    }

    public class ColorTag
    {
        public Color32 color;
    }

    public class BoldTag
    {
        public static BoldTag Instance = new BoldTag();
    }

    public class ItalicTag
    {
        public static ItalicTag Instance = new ItalicTag();
    }
}