using System;
using CreativeMode;
using CreativeMode.Impl;
using UnityEngine;
using UniRx;

public class DesktopCaptureWidget : BaseFocusableImageWidget
{
    [SerializeField] 
    private int monitorIndex;

    public override bool FlipY => true;

    private IDisposable monitorSubscription;
    private IDesktopCaptureManager DesktopCapture => Instance<IDesktopCaptureManager>.Get();

    private void OnEnable()
    {
        monitorSubscription = DesktopCapture
            .CaptureMonitor(monitorIndex)
            .Subscribe(SetMonitor);
    }

    private void OnDisable()
    {
        monitorSubscription?.Dispose();
    }

    private void SetMonitor(MonitorInfo m)
    {
        SetTexture(m.texture, m.width, m.height);
    }
}