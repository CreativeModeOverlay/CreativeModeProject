using System;
using UnityEngine;

namespace CreativeMode
{
    public interface IDesktopCaptureManager
    {
        bool IsZoomActive { get; set; }
        float ZoomAmount { get; set; }
        
        IObservable<WindowInfo> ActiveWindow { get; }
        IObservable<FocusInfo> FocusPoint { get; }
        IObservable<ICensorRegion[]> CensorRegions { get; }

        IObservable<MonitorInfo> CaptureMonitor(int index);
        IObservable<Rect> GetMonitorSize(int index);
        ICensorRegionController CreateCensorRegion();
    }
}
