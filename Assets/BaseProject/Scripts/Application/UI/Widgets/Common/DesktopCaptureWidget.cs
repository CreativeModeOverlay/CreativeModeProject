using CreativeMode;
using UnityEngine;
using UniRx;

public class DesktopCaptureWidget : BaseFocusableImageWidget
{
    [SerializeField] 
    public int monitorIndex;
    public FocusParams defaultFocus;

    public override bool FlipY => true;

    private CompositeDisposable disposables;
    private IDesktopCaptureManager DesktopCapture => Instance<IDesktopCaptureManager>.Get();

    private void OnEnable()
    {
        disposables = new CompositeDisposable();

        DesktopCapture
            .CaptureMonitor(monitorIndex)
            .Subscribe(SetMonitor)
            .AddTo(disposables);

        DesktopCapture.FocusPoint
            .Subscribe(f =>
            {
                Focus = f.isFocused && f.focusMonitorIndex == monitorIndex 
                    ? f.focusParams : defaultFocus;
            })
            .AddTo(disposables);
    }

    private void OnDisable()
    {
        disposables?.Dispose();
    }

    private void SetMonitor(MonitorInfo m)
    {
        SetTexture(m.texture, m.width, m.height);
    }
}