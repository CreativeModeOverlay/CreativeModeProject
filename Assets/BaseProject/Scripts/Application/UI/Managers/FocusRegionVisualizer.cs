using UniRx;
using UnityEngine;

namespace CreativeMode
{
    public class FocusRegionVisualizer : MonoBehaviour
    {
        private IDesktopCaptureManager CaptureManager => Instance<IDesktopCaptureManager>.Get();
        
        public int monitorIndex;
        public FocusRegionWidget focusRegionWidget;
        
        private void Awake()
        {
            CaptureManager.GetMonitorSize(monitorIndex)
                .Subscribe(r => focusRegionWidget.WindowRect = r)
                .AddTo(this);
            
            CaptureManager.FocusPoint
                .Subscribe(f =>
                {
                    focusRegionWidget.FocusRegion = f.focusRegion;
                    focusRegionWidget.IsVisible = f.isFocused && f.focusMonitorIndex == monitorIndex;
                })
                .AddTo(this);
        }
    }
}