using CreativeMode.Impl;

namespace CreativeMode
{
    public class VideoCaptureModule : ModuleBase
    {
        protected override void ProvideImplementations()
        {
            base.ProvideImplementations();

            Instance<IDeviceCaptureStorage>.Bind(() =>
            {
                var devicesDb = DatabaseUtils.OpenDb("Devices");
                return new DeviceCaptureStorage(devicesDb);
            });
            
            Instance<IDesktopCaptureManager>.BindUnityObject<DesktopCaptureManager>();
            Instance<IDeviceCaptureManager>.BindUnityObject<DeviceCaptureManager>();
        }
    }
}