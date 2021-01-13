using System;
using UnityEngine;

namespace CreativeMode
{
    public interface IDeviceCaptureManager
    {
        void SetCaptureParams(string deviceName, VideoCaptureParams captureParams);
        VideoCaptureParams GetCaptureParams(string deviceName);
        
        IObservable<CapturedVideo> CaptureDevice(string deviceName);
    }
}