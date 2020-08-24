using System;

namespace CreativeMode
{
    public interface IDesktopCaptureManager
    {
        IObservable<WindowInfo> ActiveWindow { get; }
        IObservable<FocusInfo> FocusPoint { get; }

        bool IsZoomActive { get; set; }
        float ZoomAmount { get; set; }
        
        IObservable<MonitorInfo> CaptureMonitor(int index);
    }
}
