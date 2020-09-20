using System;
using CreativeMode;
using UniRx;
using UnityEngine.UI;

public class CaptureDeviceWidgetUI : BaseWidgetUI<CaptureDeviceWidget>
{
    public RawImage deviceImage;
    public FocusableContainerWidget focusableContainer;

    private string currentDeviceName;
    private IDisposable currentCapture;
    private IDeviceCaptureManager Manager => Instance<IDeviceCaptureManager>.Get();

    protected override void SetData(CaptureDeviceWidget data)
    {
        base.SetData(data);

        focusableContainer.Focus = data.focus;
        
        if(!isActiveAndEnabled)
            return;
        
        CaptureDevice(data.deviceName);
    }
    
    private void OnEnable()
    {
        CaptureDevice(Data.deviceName);
    }

    private void OnDisable()
    {
        StopCapture();
    }

    private void CaptureDevice(string name)
    {
        if (string.IsNullOrWhiteSpace(name) 
            || name == currentDeviceName 
            || !isActiveAndEnabled) 
            return;

        StopCapture();
        currentCapture = Manager.CaptureDevice(name)
            .Subscribe(c =>
            {
                deviceImage.texture = c.texture;
                focusableContainer.SetContentSize(c.width, c.height);
            });
    }

    private void StopCapture()
    {
        currentCapture?.Dispose();
        currentDeviceName = null;
    }
}
