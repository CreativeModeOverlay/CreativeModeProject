using System;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    [Serializable]
    public class GraphicStyleData
    {
        [Header("Visual")] 
        public StyleProperty<Color> color = Color.white;
        public StyleProperty<Material> material;

        [Header("Shadow")] 
        public StyleProperty<ShadowType> shadowType = ShadowType.None;
        public StyleProperty<Vector2> shadowDistance = new Vector2(1f, -1f);
        public StyleProperty<Color> shadowColor = new Color(0f, 0f, 0f, 0.5f);
        
        public enum ShadowType
        {
            None, Shadow, Outline
        }
    }

    [Serializable]
    public class SelectableStyleData
    {
        private static readonly ColorBlock defaultColorBlock = ColorBlock.defaultColorBlock;
    
        [Header("Color block")] 
        public StyleProperty<Color> colorNormal = defaultColorBlock.normalColor;
        public StyleProperty<Color> colorHighlighted = defaultColorBlock.highlightedColor;
        public StyleProperty<Color> colorPressed = defaultColorBlock.pressedColor;
        public StyleProperty<Color> colorSelected = defaultColorBlock.selectedColor;
        public StyleProperty<Color> colorDisabled = defaultColorBlock.disabledColor;
        public StyleProperty<float> colorMultiplier = defaultColorBlock.colorMultiplier;
        public StyleProperty<float> colorFadeDuration = defaultColorBlock.fadeDuration;
    }

    [Serializable]
    public class TextStyleData : GraphicStyleData
    {
        private static readonly FontData defaultFontData = FontData.defaultFontData;
        
        [Header("Character")] 
        public StyleProperty<Font> font = defaultFontData.font;
        public StyleProperty<FontStyle> fontStyle = defaultFontData.fontStyle;
        public StyleProperty<int> fontSize = defaultFontData.fontSize;
        public StyleProperty<float> lineSpacing = defaultFontData.lineSpacing;
        public StyleProperty<bool> richText = defaultFontData.richText;

        [Header("Paragraph")] 
        public StyleProperty<TextAnchor> alignment = TextAnchor.MiddleCenter;
        public StyleProperty<bool> alignByGeometry = defaultFontData.alignByGeometry;
        public StyleProperty<HorizontalWrapMode> horizontalOverflow = defaultFontData.horizontalOverflow;
        public StyleProperty<VerticalWrapMode> verticalOverflow = defaultFontData.verticalOverflow;
        public StyleProperty<bool> bestFit = defaultFontData.bestFit;
        public StyleProperty<int> bestFitMinSize = defaultFontData.minSize;
        public StyleProperty<int> bestFitMaxSize = defaultFontData.maxSize;
    }

    [Serializable]
    public class ImageStyleData : GraphicStyleData
    {
        [Header("Image")] 
        public StyleProperty<Sprite> sprite;
    }

    [Serializable]
    public class ButtonStyleData : SelectableStyleData
    {
        [Header("Button")]
        public StyleReference<ImageStyleData> backgroundStyle;
        
        public StyleReference<TextStyleData> innerText;
        public StyleReference<GraphicStyleData> innerGraphic;
    }
}