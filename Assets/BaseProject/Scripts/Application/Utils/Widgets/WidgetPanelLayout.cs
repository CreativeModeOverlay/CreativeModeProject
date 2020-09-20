using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreativeMode
{
    [Serializable]
    public class WidgetPanelLayout
    {
        public RectTransform.Axis layoutAxis;
        public bool reverseArrangement;
        
        public TextAnchor childAlignment;

        public RectOffset padding;
        public float spacing;

        public Rect[] LayoutWidgets<T>(Rect layoutRect, List<T> widgets) 
            where T : IWidget
        {
            var paddedRect = layoutRect.ApplyOffset(padding);
            var containerPositions = new Rect[widgets.Count];
            var position = paddedRect.position;
            var totalSize = new Vector2(-spacing, -spacing);

            var horizontalScale = childAlignment.GetHorizontal01Scale();
            var verticalScale = childAlignment.GetVertical01Scale();

            int GetSize(int size, int defaultValue, int container, int min, int max)
            {
                var value = size;

                if (size == WidgetLayoutParams.UseDefault)
                    value = defaultValue;
                else if (size == WidgetLayoutParams.UseContainerSize)
                    value = container;

                return Mathf.Clamp(value, min, max);
            }
            
            for (var i = 0; i < widgets.Count; i++)
            {
                var index = reverseArrangement ? widgets.Count - 1 - i : i;
                
                var widget = widgets[index];
                var width = GetSize(widget.Layout.width, widget.Size.defaultWidth, 
                    (int) paddedRect.width, widget.Size.minWidth, widget.Size.maxWidth);
                
                var height = GetSize(widget.Layout.height, widget.Size.defaultHeight, 
                    (int) paddedRect.height, widget.Size.minHeight, widget.Size.maxHeight);

                var xLocalOffset = 0f;
                var yLocalOffset = 0f;

                if (layoutAxis == RectTransform.Axis.Horizontal)
                {
                    yLocalOffset = (paddedRect.height - height) * verticalScale;
                }
                else
                {
                    xLocalOffset = (paddedRect.width - width) * horizontalScale;
                }

                containerPositions[index] = new Rect(position.x + xLocalOffset, position.y + yLocalOffset, width, height);

                if (layoutAxis == RectTransform.Axis.Horizontal)
                {
                    var advance = width + spacing;
                    position.x += advance;
                    
                    totalSize.x += advance;
                    totalSize.y = Mathf.Max(height, totalSize.y);
                }
                else
                {
                    var advance = height + spacing;
                    position.y += advance;

                    totalSize.x = Mathf.Max(width, totalSize.x);
                    totalSize.y += advance;
                }
            }

            if (layoutAxis == RectTransform.Axis.Horizontal)
            {
                var xAnchorOffset = (paddedRect.width - totalSize.x) * horizontalScale;

                if (xAnchorOffset != 0)
                {
                    for (var i = 0; i < containerPositions.Length; i++) 
                        containerPositions[i].x += xAnchorOffset;
                }
            }
            else
            {
                var yAnchorOffset = (paddedRect.height - totalSize.y) * verticalScale;

                if (yAnchorOffset != 0)
                {
                    for (var i = 0; i < containerPositions.Length; i++)
                        containerPositions[i].y += yAnchorOffset;
                }
            }

            return containerPositions;
        }
  
        public interface IWidget
        {
             UIContentSize Size { get; }
             WidgetLayoutParams Layout { get; }
        }
    }
}