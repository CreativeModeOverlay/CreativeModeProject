using System;
using CreativeMode;
using UniRx;

public class CaptureDeviceWidgetUI : BaseWidgetUI<CaptureDeviceWidget>
{
    public string deviceName;
    public FocusableImageWidget focusableImage;

    private string currentDeviceName;
    private IDisposable currentCapture;
    private IDeviceCaptureManager Manager => Instance<IDeviceCaptureManager>.Get();

    private void Awake()
    {
        if (!string.IsNullOrWhiteSpace(deviceName))
            CaptureDevice(deviceName);
    }

    protected override void SetData(CaptureDeviceWidget data)
    {
        base.SetData(data);

        CaptureDevice(data.deviceName);
        focusableImage.Focus = data.focus;
    }

    private void CaptureDevice(string name)
    {
        if (name != currentDeviceName)
        {
            currentDeviceName = name;
            currentCapture?.Dispose();
            currentCapture = Manager.CaptureDevice(name)
                .Subscribe(c => focusableImage.SetTexture(c.texture, c.width, c.height));
        }
    }
}
