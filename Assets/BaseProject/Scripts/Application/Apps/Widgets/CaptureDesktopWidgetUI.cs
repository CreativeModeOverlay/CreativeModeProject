using CreativeMode;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CaptureDesktopWidgetUI : BaseAppWidgetUI<CaptureDesktopWidget>
{
    private IDesktopCaptureManager DesktopCapture => Instance<IDesktopCaptureManager>.Get();
    
    public FocusParams defaultFocus;

    public RawImage desktopImage;
    public FocusableContainerWidget focusableContainer;

    private int currentCaptureId;
    private CompositeDisposable disposables;

    protected override void SetData(CaptureDesktopWidget data)
    {
        base.SetData(data);

        CaptureMonitor(Data.monitorId);
    }

    private void OnEnable()
    {
        CaptureMonitor(Data.monitorId);
    }

    private void OnDisable()
    {
        StopCapture();
    }

    private void CaptureMonitor(int index)
    {
        if(currentCaptureId == index)
            return;

        currentCaptureId = index;
        
        if(!isActiveAndEnabled)
            return;
        
        StopCapture();
        
        disposables = new CompositeDisposable();
        DesktopCapture
            .CaptureMonitor(index)
            .Subscribe(m =>
            {
                desktopImage.texture = m.texture;
                desktopImage.uvRect = Rect.MinMaxRect(0, 1, 1, 0);
                focusableContainer.SetContentSize(m.width, m.height);
            })
            .AddTo(disposables);

        DesktopCapture.FocusPoint
            .Subscribe(f =>
            {
                focusableContainer.Focus = f.isFocused && f.focusMonitorIndex == index 
                    ? f.focusParams : defaultFocus;
            })
            .AddTo(disposables);
    }

    private void StopCapture()
    {
        disposables?.Dispose();
        disposables = null;
        currentCaptureId = -1;
    }
}