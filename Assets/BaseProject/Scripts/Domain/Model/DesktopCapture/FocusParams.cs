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

        public RectOffset crop;
        public RectOffset padding;

        public static bool operator ==(FocusParams c1, FocusParams c2) => c1.Equals(c2);
        public static bool operator !=(FocusParams c1, FocusParams c2) => !c1.Equals(c2);
    }
}