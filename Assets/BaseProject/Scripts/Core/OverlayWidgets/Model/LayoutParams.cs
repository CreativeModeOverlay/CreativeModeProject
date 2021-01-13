using System;

namespace CreativeMode
{
    [Serializable]
    public struct LayoutParams
    {
        public const int UseDefault = 0;
        public const int UseContainerSize = -1;
        
        public int width;
        public int height;
        public int priority;
    }
}