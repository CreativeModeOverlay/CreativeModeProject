namespace CreativeMode
{
    public interface IDeviceCaptureStorage
    {
        DeviceCaptureParamsDB GetCaptureParams(string id);
        void PutCaptureParams(DeviceCaptureParamsDB captureParams);
    }
}