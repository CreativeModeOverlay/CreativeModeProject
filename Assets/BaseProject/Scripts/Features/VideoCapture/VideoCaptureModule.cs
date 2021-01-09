using CreativeMode.Impl;

namespace CreativeMode
{
    public static class VideoCaptureModule
    {
        public static void Init()
        {
            var devices = DatabaseUtils.OpenDb("Devices");

            Instance<IDeviceCaptureStorage>.Bind(() => new DeviceCaptureStorage(devices));
            
            Instance<IDesktopCaptureManager>.BindUnityObject<DesktopCaptureManager>();
            Instance<IDeviceCaptureManager>.BindUnityObject<DeviceCaptureManager>();
        }
    }
}