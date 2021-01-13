using UnityEngine;

namespace CreativeMode
{
    public static class RectUtils
    {
        public static Rect Lerp(Rect from, Rect to, float position)
        {
            return Rect.MinMaxRect(
                Mathf.Lerp(from.xMin, to.xMin, position),
                Mathf.Lerp(from.yMin, to.yMin, position),
                Mathf.Lerp(from.xMax, to.xMax, position),
                Mathf.Lerp(from.yMax, to.yMax, position));
        }

        public static Rect Crop(this Rect from, Rect bounds)
        {
            return Rect.MinMaxRect(
                Mathf.Clamp(from.xMin, bounds.xMin, bounds.xMax),
                Mathf.Clamp(from.yMin, bounds.yMin, bounds.yMax),
                Mathf.Clamp(from.xMax, bounds.xMin, bounds.xMax),
                Mathf.Clamp(from.yMax, bounds.yMin, bounds.yMax));
        }

        public static Rect ApplyOffset(this Rect rect, float offset)
        {
            var sizeMargin = offset * 2;
            return new Rect(rect.x + offset, rect.y + offset, 
                rect.width - sizeMargin, rect.height - sizeMargin);
        }

        public static Rect ApplyOffset(this Rect rect, RectOffset offset)
        {
            return new Rect(rect.x + offset.left, rect.y + offset.bottom,
                rect.width - offset.horizontal, 
                rect.height - offset.vertical);
        }
        
        public static Rect ApplyOffset(this Rect rect, RectOffset offset, float scale)
        {
            return new Rect(rect.x + offset.left * scale, rect.y + offset.bottom * scale,
                rect.width - offset.horizontal * scale, 
                rect.height - offset.vertical * scale);
        }

        public static Rect Scale(this Rect rect, float scale)
        {
            return new Rect(rect.x * scale, rect.y * scale, 
                rect.width * scale, rect.height * scale);
        }

        public static Rect Envelope(this Rect rect, Rect container, TextAnchor smallSizeAnchor)
        {
            var widthDiff = rect.width - container.width;
            var heightDiff = rect.height - container.height;

            if (widthDiff < 0)
            {
                
                rect.x = container.xMin - widthDiff * smallSizeAnchor.GetHorizontal01Scale();;
            }
            else
            {
                if (rect.xMax < container.xMax)
                {
                    rect.x +=  container.xMax - rect.xMax;
                }
                else if (rect.xMin > container.xMin)
                {
                    rect.x += container.xMin - rect.xMin;
                }
            }

            if (heightDiff < 0)
            {
                rect.y = container.yMin - heightDiff * smallSizeAnchor.GetVertical01Scale();
            }
            else
            {
                if (rect.yMax < container.yMax)
                {
                    rect.y +=  container.yMax - rect.yMax;
                }
                else if (rect.yMin > container.yMin)
                {
                    rect.y += container.yMin - rect.yMin;
                }
            }

            return rect;
        }

        public static Rect MoveInBounds(this Rect rect, Rect container)
        {
            if (rect.x < container.x)
                rect.x = container.x;

            if (rect.y < container.y)
                rect.y = container.y;

            if (rect.xMax > container.xMax)
                rect.x = container.xMax - rect.width;
        
            if (rect.yMax > container.yMax)
                rect.y = container.yMax - rect.height;

            return rect;
        }

        public static Rect Resize(this Rect rect, Vector2 delta, Side side)
        {
            var horizontalMove = delta.x;
            var verticalMove = delta.y;

            if ((side & Side.Horizontal) == Side.Horizontal)
            {
                rect.x += horizontalMove;
            }
            else
            {
                if ((side & Side.Left) == Side.Left)
                {
                    rect.xMin += horizontalMove;
                }
                else if ((side & Side.Right) == Side.Right)
                {
                    rect.xMax += horizontalMove;
                }
            }
        
            if ((side & Side.Vertical) == Side.Vertical)
            {
                rect.y += verticalMove;
            }
            else
            {
                if ((side & Side.Bottom) == Side.Bottom)
                {
                    rect.yMin += verticalMove;
                }
                else if ((side & Side.Top) == Side.Top)
                {
                    rect.yMax += verticalMove;
                }
            }

            return rect;
        }
        
        public static Rect Resize(this Rect rect, Vector2 delta, Side side, ContentSize sizeLimits)
        {
            var horizontalMove = delta.x;
            var verticalMove = delta.y;

            if ((side & Side.Horizontal) == Side.Horizontal)
            {
                rect.x += horizontalMove;
            }
            else
            {
                if ((side & Side.Left) == Side.Left)
                {
                    rect.xMin = Mathf.Clamp(rect.xMin + horizontalMove,
                        rect.xMax - sizeLimits.maxWidth,
                        rect.xMax - sizeLimits.minWidth);
                }
                else if ((side & Side.Right) == Side.Right)
                {
                    rect.xMax = Mathf.Clamp(rect.xMax + horizontalMove,
                        rect.xMin + sizeLimits.minWidth,
                        rect.xMin + sizeLimits.maxWidth);
                }
            }
        
            if ((side & Side.Vertical) == Side.Vertical)
            {
                rect.y += verticalMove;
            }
            else
            {
                if ((side & Side.Bottom) == Side.Bottom)
                {
                    rect.yMin = Mathf.Clamp(rect.yMin + verticalMove,
                            rect.yMax - sizeLimits.maxHeight,
                            rect.yMax - sizeLimits.minHeight);
                }
                else if ((side & Side.Top) == Side.Top)
                {
                    rect.yMax = Mathf.Clamp(rect.yMax + verticalMove,
                            rect.yMin + sizeLimits.minHeight,
                            rect.yMin + sizeLimits.maxHeight);
                }
            }

            return rect;
        }
    }
}