using System;
using UnityEngine;

namespace CreativeMode
{
    [Serializable]
    public struct FocusParams
    {
        public Vector2 focusCenter;
        public float zoom;
        public bool zoomUseContainerSize;
        
        public float cropLeft;
        public float cropRight;
        public float cropTop;
        public float cropBottom;
        
        public float paddingLeft;
        public float paddingRight;
        public float paddingTop;
        public float paddingBottom;

        public static bool operator ==(FocusParams c1, FocusParams c2) => c1.Equals(c2);
        public static bool operator !=(FocusParams c1, FocusParams c2) => !c1.Equals(c2);
    }
}