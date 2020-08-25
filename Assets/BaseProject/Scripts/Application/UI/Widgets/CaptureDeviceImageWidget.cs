using CreativeMode;
using UnityEngine;

public class CaptureDeviceImageWidget : BaseFocusableImageWidget
{
    public string deviceName;
    public int requestedWidth;
    public int requestedHeight;
    public int requestedRefreshRate;
    public float mipMapBias = -1f;

    private WebCamTexture deviceTexture;
    private RenderTexture outputTexture;

    private void Awake()
    {
        deviceTexture = new WebCamTexture(deviceName, requestedWidth, requestedHeight, requestedRefreshRate);
        outputTexture = new RenderTexture(requestedWidth, requestedHeight, 0, RenderTextureFormat.Default, 8)
        {
            filterMode = FilterMode.Trilinear, 
            anisoLevel = 16,
            useMipMap = true,
            mipMapBias = mipMapBias
        };
        outputTexture.Create();

        SetTexture(outputTexture, requestedWidth, requestedHeight);
    }

    protected override void Update()
    {
        if (deviceTexture.didUpdateThisFrame)
        {
            Graphics.Blit(deviceTexture, outputTexture);
        }

        base.Update();
    }

    private void OnEnable()
    {
        deviceTexture.Play();
    }

    private void OnDisable()
    {
        deviceTexture.Pause();
    }
}
