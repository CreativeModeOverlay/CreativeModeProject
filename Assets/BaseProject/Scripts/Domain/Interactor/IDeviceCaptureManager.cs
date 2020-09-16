using System;
using UnityEngine;

namespace CreativeMode
{
    public interface IDeviceCaptureManager
    {
        void SetCaptureParams(string deviceName, DeviceCaptureParams captureParams);
        DeviceCaptureParams GetCaptureParams(string deviceName);
        
        IObservable<DeviceCapture> CaptureDevice(string deviceName);
    }
}