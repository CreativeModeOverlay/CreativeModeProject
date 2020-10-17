using System;
using CreativeMode;
using UniRx;
using UnityEngine.UI;

public class CaptureDeviceWidgetUI : BaseAppWidgetUI<CaptureDeviceAppWidget>
{
    private IDeviceCaptureManager DeviceCapture => Instance<IDeviceCaptureManager>.Get();
    
    public RawImage deviceImage;
    public FocusableContainerWidget focusableContainer;

    private string currentDeviceName;
    private IDisposable currentCapture;
    
    protected override void SetData(CaptureDeviceAppWidget data)
    {
        base.SetData(data);

        //focusableContainer.Focus = data.focus;
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
        if (string.IsNullOrWhiteSpace(name) || name == currentDeviceName) 
            return;

        currentDeviceName = name;
        
        if(!isActiveAndEnabled)
            return;

        StopCapture();
        currentCapture = DeviceCapture.CaptureDevice(name)
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
