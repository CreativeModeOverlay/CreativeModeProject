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

        public static Rect ClampInBounds(Rect from, Rect bounds)
        {
            return Rect.MinMaxRect(
                Mathf.Clamp(from.xMin, bounds.xMin, bounds.xMax),
                Mathf.Clamp(from.yMin, bounds.yMin, bounds.yMax),
                Mathf.Clamp(from.xMax, bounds.xMin, bounds.xMax),
                Mathf.Clamp(from.yMax, bounds.yMin, bounds.yMax));
        }

        public static Rect Padding(this Rect rect, float margin)
        {
            var sizeMargin = margin * 2;
            return new Rect(rect.x + margin, rect.y + margin, 
                rect.width - sizeMargin, rect.height - sizeMargin);
        }

        public static Rect Padding(this Rect rect, RectOffset offset)
        {
            return new Rect(rect.x + offset.left, rect.y + offset.bottom,
                rect.width - offset.horizontal, 
                rect.height - offset.vertical);
        }
    }
}