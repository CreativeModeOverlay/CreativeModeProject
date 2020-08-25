using System;
using UnityEngine;

namespace CreativeMode
{
    public interface IDesktopCaptureManager
    {
        IObservable<WindowInfo> ActiveWindow { get; }
        IObservable<FocusInfo> FocusPoint { get; }
        IObservable<ICensorRegion> OnCensorRegionAdded { get; }
        IObservable<ICensorRegion> OnCensorRegionRemoved { get; }

        bool IsZoomActive { get; set; }
        float ZoomAmount { get; set; }

        IObservable<MonitorInfo> CaptureMonitor(int index);
        IObservable<Rect> GetMonitorSize(int index);

        ICensorRegion[] GetActiveCensorRegions();
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
