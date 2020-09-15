using System;
using System.Collections.Generic;
using System.Text;
using CreativeMode;
using CreativeMode.Impl;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extended TextView with support for project-related stuff
/// </summary>
public class CreativeText : Text
{
    private const string iconPlaceholderText = " "; // Height space
    
    public float iconScale = 1.2f;
    public float modifierIconScale = 1.2f;

    private SpannedText currentSpannedText;

    private List<IconDisplayLayer> displayLayers 
        = new List<IconDisplayLayer>();
    
    private List<PlaceholderIcon> placeholderIcons 
        = new List<PlaceholderIcon>();

    private CreativeUIManager uiManager;

    protected override void Awake()
    {
        base.Awake();
        uiManager = CreativeUIManager.Instance;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        uiManager.RegisterText(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        uiManager.UnregisterText(this);
    }

    public void SetSpannedText(SpannedText spannedText)
    {
        if (!Application.isPlaying)
            return;

        supportRichText = true;
        currentSpannedText = spannedText;
        UpdateText();
    }

    private void UpdateText()
    {
        placeholderIcons.Clear();
        
        var builder = new StringBuilder();
        var previousIcon = new PlaceholderIcon();
        var colorStack = new Stack<Color32>(2);
        colorStack.Push(Color.white);

        void AppendText(string text)
        {
            // TODO: Very hacky way to escape brackets by inserting empty tag
            // Seems like no official solution for escaping characters
            builder.Append(text.Replace("<", "<<i></i>"));
        }

        void AppendTag(object tag, bool isClosing)
        {
            switch (tag)
            {
                case BoldTag _: builder.Append(isClosing ? "</b>" : "<b>"); return;
                case ItalicTag _: builder.Append(isClosing ? "</i>" : "<i>"); return;
                case SizeTag s: builder.Append(isClosing ? "</size>" : $"<size={s.size}>"); return;
                
                case SizeScaleTag s:
                    if (isClosing)
                    {
                        builder.Append("</size>");
                    }
                    else
                    {
                        var scaledSize = Mathf.RoundToInt(fontSize * s.scale);
                        builder.Append($"<size={scaledSize}>");
                    }
                    
                    return;
                
                case ColorTag c:
                    var colorHex = ColorUtility.ToHtmlStringRGBA(c.color);
                    builder.Append(isClosing ? "</color>" : $"<color=#{colorHex}>");

                    if (isClosing)
                    {
                        if(colorStack.Count > 1) 
                            colorStack.Pop();
                    }
                    else
                    {
                        colorStack.Push(c.color);
                    }
                    
                    return;

                case UrlIconTag icon:
                    
                    GetOrCreateLayer(uiManager.AtlasTexture); // just create layer for later if it not exists
                    var newIcon = new PlaceholderIcon
                    {
                        atlas = uiManager.AtlasTexture,
                        rect = uiManager.GetIcon(icon.url),
                        isModifier = icon.isModifier,
                        color = colorStack.Peek()
                    };

                    if (icon.isModifier)
                    {
                        newIcon.charIndex = previousIcon.charIndex;
                    }
                    else
                    {
                        newIcon.charIndex = builder.Length;
                        builder.Append(iconPlaceholderText);
                    }

                    placeholderIcons.Add(newIcon);
                    previousIcon = newIcon;
                    break;
            }
        }

        SpannedTextUtils.Enumerate(currentSpannedText, AppendText, AppendTag);
        text = builder.ToString();
    }

    private IconDisplayLayer GetOrCreateLayer(Texture texture)
    {
        foreach (var o in displayLayers)
        {
            if (o.AtlasTexture == texture)
                return o;
        }
        
        var overlayObject = new GameObject($"IconLayer ({texture.name})");
        overlayObject.transform.parent = transform;
        overlayObject.AddComponent<CanvasRenderer>();

        var iconLayer = overlayObject.AddComponent<IconDisplayLayer>();
        iconLayer.material = Canvas.GetDefaultCanvasMaterial();
        iconLayer.AtlasTexture = texture;

        var t = iconLayer.rectTransform;
        t.anchorMin = Vector2.zero;
        t.anchorMax = Vector2.one;
        t.sizeDelta = Vector2.zero;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
        t.pivot = rectTransform.pivot;
        
        displayLayers.Add(iconLayer);
        return iconLayer;
    }

    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
        for(var i = 0; i < displayLayers.Count; i++)
            displayLayers[i].SetVerticesDirty();
    }

    public override void SetLayoutDirty()
    {
        base.SetLayoutDirty();
        for(var i = 0; i < displayLayers.Count; i++)
            displayLayers[i].SetLayoutDirty();
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);
        OnPopulateIcons();
    }

    private void OnPopulateIcons()
    {
        foreach (var layer in displayLayers)
            layer.ClearIcons();

        var generator = cachedTextGeneratorForLayout;

        foreach (var icon in placeholderIcons)
        {
            if(icon.charIndex >= generator.characterCount)
                continue;
            
            var character = generator.characters[icon.charIndex];
            var layer = GetOrCreateLayer(icon.atlas);

            layer.AddIcon(new DisplayIcon
            {
                position = character.cursorPos,
                size = character.charWidth,
                rect = icon.rect,
                scale = icon.isModifier ? modifierIconScale * iconScale : iconScale,
                color = icon.color
            });
        }
    }
    
    private class IconDisplayLayer : Graphic
    {
        public Texture AtlasTexture { get; set; }
        
        public override Texture mainTexture => AtlasTexture;
        
        private List<DisplayIcon> icons = new List<DisplayIcon>();
        private bool isUpdated;

        public void ClearIcons()
        {
            icons.Clear();
        }

        public void AddIcon(DisplayIcon icon)
        {
            icons.Add(icon);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var vertexOffset = 0;
            
            for (var i = 0; i < icons.Count; i++)
            {
                var icon = icons[i];
                var center = icon.position;
                var rect = icon.rect;
                var size = icon.size * icon.scale;
                var scaledSizeOffset = (size - icon.size) / 2f;
                var offset = new Vector3(-scaledSizeOffset, scaledSizeOffset, 0);
                
                vh.AddVert(offset + new Vector3(center.x, center.y - size),
                    icon.color, new Vector2(rect.xMin, rect.yMin));
                vh.AddVert(offset +new Vector3(center.x, center.y),
                    icon.color, new Vector2(rect.xMin, rect.yMax));
                vh.AddVert(offset +new Vector3(center.x + size, center.y),
                    icon.color, new Vector2(rect.xMax, rect.yMax));
                vh.AddVert(offset +new Vector3(center.x + size, center.y - size),
                    icon.color, new Vector2(rect.xMax, rect.yMin));
                vh.AddTriangle(vertexOffset, vertexOffset + 1, vertexOffset + 2);
                vh.AddTriangle(vertexOffset + 2, vertexOffset + 3, vertexOffset);
                
                vertexOffset += 4;
            }
        }
    }

    [Serializable]
    private struct PlaceholderIcon
    {
        public int charIndex;
        public bool isModifier;
        public Texture atlas;
        public Rect rect;
        public Color color;
    }
    
    [Serializable]
    private struct DisplayIcon
    {
        public Vector3 position;
        public float size;
        public float scale;
        public Rect rect;
        public Color color;
    }
}