using System;

namespace CreativeMode
{
    [Serializable]
    public class CaptureDeviceWidget : Widget
    {
        public string deviceName;
        public FocusParams focus;
    }
}