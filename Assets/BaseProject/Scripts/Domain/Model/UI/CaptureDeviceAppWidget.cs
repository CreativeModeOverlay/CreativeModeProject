using System;

namespace CreativeMode
{
    [Serializable]
    public class CaptureDeviceAppWidget : AppWidget
    {
        public string deviceName;
        public FocusParams focus = FocusParams.GetDefault();
    }
}