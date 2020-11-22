using System;

namespace CreativeMode
{
    [Serializable]
    public class CaptureDesktopAppWidget : AppWidget
    {
        public int monitorId;
        public FocusParams focus = FocusParams.GetDefault();
    }
}