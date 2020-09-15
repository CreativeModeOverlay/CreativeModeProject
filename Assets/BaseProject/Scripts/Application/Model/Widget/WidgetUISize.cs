using System;

namespace CreativeMode
{
    [Serializable]
    public class WidgetUISize
    {
        public int defaultWidth;
        public int defaultHeight;
        
        public int minWidth;
        public int minHeight;
        public int maxWidth = Int32.MaxValue;
        public int maxHeight = Int32.MaxValue;

        public bool CanResizeWidth => minWidth >= maxWidth;
        public bool CanResizeHeight => minHeight >= maxHeight;
    }
}