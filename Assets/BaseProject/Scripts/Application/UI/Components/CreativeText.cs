using System;
using System.Collections.Generic;
using System.Text;
using CreativeMode;
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

    private TextWithIcons currentText;

    private List<IconDisplayLayer> displayLayers 
        = new List<IconDisplayLayer>();
    
    private List<PlaceholderIcon> placeholderIcons 
        = new List<PlaceholderIcon>();

    public void SetText(TextWithIcons textWithIcons)
    {
        if (!Application.isPlaying)
            return;

        currentText = textWithIcons;
        UpdateText();
    }

    private void UpdateText()
    {
        placeholderIcons.Clear();

        if (currentText.icons != null && currentText.icons.Length > 0)
        {
            var builder = new StringBuilder(currentText.text);
            var previousIcon = new PlaceholderIcon();
            var offset = 0;

            foreach (var e in currentText.icons)
            {
                GetOrCreateLayer(e.atlas);
                var newIcon = new PlaceholderIcon
                {
                    atlas = e.atlas,
                    rect = e.rect,
                    isModifier = e.isModifier
                };

                if (e.isModifier)
                {
                    newIcon.charIndex = previousIcon.charIndex;
                }
                else
                {
                    var startPosition = e.position + offset;
                    newIcon.charIndex = startPosition;

                    builder.Insert(startPosition, iconPlaceholderText);
                    offset += iconPlaceholderText.Length;
                }

                placeholderIcons.Add(newIcon);
                previousIcon = newIcon;
            }

            text = builder.ToString();
        }
        else
        {
            text = currentText.text;
        }
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
                scale = icon.isModifier ? modifierIconScale * iconScale : iconScale
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
                    color, new Vector2(rect.xMin, rect.yMin));
                vh.AddVert(offset +new Vector3(center.x, center.y),
                    color, new Vector2(rect.xMin, rect.yMax));
                vh.AddVert(offset +new Vector3(center.x + size, center.y),
                    color, new Vector2(rect.xMax, rect.yMax));
                vh.AddVert(offset +new Vector3(center.x + size, center.y - size),
                    color, new Vector2(rect.xMax, rect.yMin));
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
    }
    
    [Serializable]
    private struct DisplayIcon
    {
        public Vector3 position;
        public float size;
        public float scale;
        public Rect rect;
    }
}