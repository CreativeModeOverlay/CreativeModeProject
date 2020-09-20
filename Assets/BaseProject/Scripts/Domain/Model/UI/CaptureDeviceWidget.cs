using System;

namespace CreativeMode
{
    [Serializable]
    public class CaptureDeviceWidget : BaseWidget
    {
        public string deviceName;
        public FocusParams focus;
    }
}