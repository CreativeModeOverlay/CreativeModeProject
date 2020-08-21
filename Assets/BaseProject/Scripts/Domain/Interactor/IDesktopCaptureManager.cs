using System;

namespace CreativeMode
{
    public interface IDesktopCaptureManager
    {
        IObservable<WindowInfo> FocusedWindow { get; }
        
        IObservable<MonitorInfo> CaptureMonitor(int index);
    }
}
