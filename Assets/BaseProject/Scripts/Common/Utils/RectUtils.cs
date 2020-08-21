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

        public static Rect Margin(this Rect rect, float margin)
        {
            var sizeMargin = margin * 2;
            return new Rect(rect.x + margin, rect.y + margin, 
                rect.width - sizeMargin, rect.height - sizeMargin);
        }
    }
}