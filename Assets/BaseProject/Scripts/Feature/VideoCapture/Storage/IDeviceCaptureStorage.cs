namespace CreativeMode
{
    internal interface IDeviceCaptureStorage
    {
        DeviceCaptureParamsDB GetCaptureParams(string id);
        void PutCaptureParams(DeviceCaptureParamsDB captureParams);
    }
}