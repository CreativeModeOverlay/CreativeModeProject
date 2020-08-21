using System;
using System.Collections.Generic;
using System.Text;
using CreativeMode;
using UnityEngine;
using UnityEngine.UI;

public class CreativeText : Text
{
    public float iconSize = 1.2f;
    public float iconPosition = 0f;
    public string iconPlaceholderString;

    private bool isUpdated;
    private List<IconOverlay> overlays = new List<IconOverlay>();

    public void SetText(TextWithIcons textWithIcons)
    {
        if (!Application.isPlaying)
            return;
        
        var builder = new StringBuilder(textWithIcons.text);
        var offset = 0;

        foreach (var o in overlays)
            o.ClearIcons();

        if (textWithIcons.icons != null && textWithIcons.icons.Length > 0)
        {
            var tagStart = "<color=#00000000>";
            var tagEnd = "</color>";
            var formattedPlaceholder = tagStart + iconPlaceholderString + tagEnd;
            var previousIcon = new IconData();

            foreach (var e in textWithIcons.icons)
            {
                var overlay = GetOrCreateOverlay(e.atlas);
                var newIcon = new IconData { rect = e.rect };

                if (e.isModifier)
                {
                    newIcon.startIndex = previousIcon.startIndex;
                    newIcon.endIndex = previousIcon.endIndex;
                    newIcon.sizeScale = 1.2f;
                }
                else
                {
                    var startPosition = e.position + offset;
                    newIcon.startIndex = startPosition + tagStart.Length;
                    newIcon.endIndex = startPosition + formattedPlaceholder.Length - tagEnd.Length;
                    newIcon.sizeScale = 1f;
                    
                    builder.Insert(startPosition, formattedPlaceholder);
                    offset += formattedPlaceholder.Length;
                }
                
                overlay.AddIcon(newIcon);
                previousIcon = newIcon;
            }
        }

        text = builder.ToString();
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);
        isUpdated = true;
    }

    private void LateUpdate()
    {
        if (isUpdated)
        {
            isUpdated = false;

            foreach (var o in overlays)
                o.SetAllDirty();
        }
    }

    private IconOverlay GetOrCreateOverlay(Texture texture)
    {
        foreach (var o in overlays)
        {
            if (o.atlas == texture)
                return o;
        }
        
        var overlayObject = new GameObject($"IconOverlay ({texture.name})");
        overlayObject.transform.parent = transform;

        var iconOverlay = overlayObject.AddComponent<IconOverlay>();
        iconOverlay.material = Canvas.GetDefaultCanvasMaterial();
        iconOverlay.atlas = texture;
        iconOverlay.textComponent = this;

        var t = iconOverlay.rectTransform;
        t.pivot = ((RectTransform) transform).pivot;
        t.anchorMin = Vector2.zero;
        t.anchorMax = Vector2.one;
        t.sizeDelta = Vector2.zero;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
        
        overlays.Add(iconOverlay);
        return iconOverlay;
    }

    private class IconOverlay : Graphic
    {
        public CreativeText textComponent;
        public Texture atlas;
        
        private TextGenerator generator;
        private List<IconData> icons = new List<IconData>();

        public void AddIcon(IconData icon)
        {
            icons.Add(icon);
        }

        public void ClearIcons()
        {
            icons.Clear();
            SetAllDirty();
        }

        protected override void Awake()
        {
            base.Awake();
            material = defaultMaterial;
            generator = new TextGenerator();
        }

        public override Texture mainTexture => atlas;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var generationSettings = textComponent.GetGenerationSettings(textComponent.rectTransform.rect.size);
            
            var fontSize = (float) generationSettings.fontSize;
            var halfWidth = fontSize / 2f * textComponent.iconSize;
            var halfHeight = fontSize / 2f * textComponent.iconSize;
            var drawingOffset = fontSize * textComponent.iconPosition;
            var offset = 0;

            generator.Invalidate();
            generator.PopulateWithErrors(textComponent.text, generationSettings, gameObject);

            for (var i = 0; i < icons.Count; i++)
            {
                var emote = icons[i];
                
                if(emote.startIndex >= generator.characterCount || emote.endIndex >= generator.characterCount)
                    continue;

                var startCharacter = generator.characters[emote.startIndex];
                var endCharacter = generator.characters[emote.endIndex];
                var xCenter = (startCharacter.cursorPos.x + endCharacter.cursorPos.x) / 2f;
                var yCenter = endCharacter.cursorPos.y - drawingOffset;
                var rect = emote.rect;
                var halfWidthScaled = halfWidth * emote.sizeScale;
                var halfHeightScaled = halfWidth * emote.sizeScale;

                vh.AddVert(new Vector3(xCenter - halfWidthScaled, yCenter - halfHeightScaled),
                    color, new Vector2(rect.xMin, rect.yMin));
                vh.AddVert(new Vector3(xCenter - halfWidthScaled, yCenter + halfHeightScaled),
                    color, new Vector2(rect.xMin, rect.yMax));
                vh.AddVert(new Vector3(xCenter + halfWidthScaled, yCenter + halfHeightScaled),
                    color, new Vector2(rect.xMax, rect.yMax));
                vh.AddVert(new Vector3(xCenter + halfWidthScaled, yCenter - halfHeightScaled),
                    color, new Vector2(rect.xMax, rect.yMin));
                vh.AddTriangle(offset, offset + 1, offset + 2);
                vh.AddTriangle(offset + 2, offset + 3, offset);
                offset += 4;
            }
        }
    }

    [Serializable]
    private struct IconData
    {
        public int startIndex;
        public int endIndex;
        public Rect rect;
        public float sizeScale;
    }
}