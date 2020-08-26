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
    }

    public interface ICensorRegion
    {
        string Title { get; }
        Rect Rect { get; }
    }

    public interface IDesktopCensorRegion
    {
        string Title { get; set; }
        Rect Rect { get; set; }
        
        void Remove();
    }
}
