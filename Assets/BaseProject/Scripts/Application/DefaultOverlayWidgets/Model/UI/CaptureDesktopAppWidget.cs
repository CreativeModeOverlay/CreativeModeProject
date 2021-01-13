using System;

namespace CreativeMode
{
    [Serializable]
    public struct CaptureDesktopAppWidget
    {
        public int monitorId;
        public FocusParams focus;
    }
}