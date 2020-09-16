using System.Collections.Generic;
using CreativeMode;
using UnityEngine;
using UniRx;

public class DesktopCaptureWidgetUI : BaseWidgetUI<DesktopCaptureWidget>
{
    [SerializeField] 
    public int monitorIndex;
    public FocusParams defaultFocus;
    public FocusableImageWidget focusableImage;

    public DesktopCaptureCensorRegionWidget overlayCensorRegionWidgetPrefab;

    private CompositeDisposable disposables;
    private IDesktopCaptureManager DesktopCapture => Instance<IDesktopCaptureManager>.Get();
    
    private Dictionary<ICensorRegion, DesktopCaptureCensorRegionWidget> spawnedWidgets 
        = new Dictionary<ICensorRegion, DesktopCaptureCensorRegionWidget>();

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
                focusableImage.Focus = f.isFocused && f.focusMonitorIndex == monitorIndex 
                    ? f.focusParams : defaultFocus;
            })
            .AddTo(disposables);

        DesktopCapture.CensorRegions
            .SubscribeChanges(SpawnCensorWidget, RemoveCensorWidget)
            .AddTo(disposables);
    }

    private void OnDisable()
    {
        disposables?.Dispose();
    }

    private void SetMonitor(MonitorInfo m)
    {
        focusableImage.SetTexture(m.texture, m.width, m.height);
    }

    private void SpawnCensorWidget(ICensorRegion region)
    {
        var instance = Instantiate(overlayCensorRegionWidgetPrefab, 
            focusableImage.targetImage.transform);
        spawnedWidgets[region] = instance;
            
        instance.gameObject.SetActive(true);
        instance.Region = region;
        instance.Widget = this;
    }

    private void RemoveCensorWidget(ICensorRegion region)
    {
        spawnedWidgets[region].Remove();
        spawnedWidgets.Remove(region);
    }
}