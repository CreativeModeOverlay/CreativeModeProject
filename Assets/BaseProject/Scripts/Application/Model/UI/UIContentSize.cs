using System;
using UnityEngine;

namespace CreativeMode
{
    [Serializable]
    public struct UIContentSize
    {
        public int defaultWidth;
        public int defaultHeight;
        
        public int minWidth;
        public int minHeight;
        public int maxWidth;
        public int maxHeight;

        public bool CanResizeWidth => minWidth < maxWidth;
        public bool CanResizeHeight => minHeight < maxHeight;

        public static UIContentSize GetDefault()
        {
            return new UIContentSize
            {
                maxWidth = Int32.MaxValue,
                maxHeight = Int32.MaxValue
            };
        }

        public UIContentSize Apply(RectOffset offset)
        {
            var h = offset.horizontal;
            var v = offset.vertical;
            
            return new UIContentSize
            {
                defaultWidth = defaultWidth + h,
                defaultHeight = defaultHeight + v,
                minWidth = minWidth + h,
                minHeight = minHeight + v,
                maxWidth = maxWidth + h,
                maxHeight = maxHeight + v
            };
        }
    }
}