using System;
using UnityEngine;

namespace CreativeMode
{
    [Serializable]
    public struct FocusParams
    {
        public Vector2 focusCenter;
        public bool normalizedFocusCenter;

        public float zoom;
        public bool zoomUseContainerSize;
        public ZoomType zoomType;

        public RectOffset crop;

        public enum ZoomType { Fit, Crop, }

        public static FocusParams GetDefault()
        {
            return new FocusParams
            {
                focusCenter = new Vector2(0.5f, 0.5f),
                normalizedFocusCenter = true,
                
                zoom = 1f,
                zoomUseContainerSize = true,
                zoomType = ZoomType.Crop,
                
                crop = new RectOffset()
            };
        }
    }
}