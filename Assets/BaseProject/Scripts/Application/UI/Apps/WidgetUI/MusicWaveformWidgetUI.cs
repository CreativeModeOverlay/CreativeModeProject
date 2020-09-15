using CreativeMode;
using UnityEngine;
using UnityEngine.UI;

public class MusicWaveformWidgetUI : BaseGraphicWidgetUI<MusicWaveformWidget>
{
    [Header("Waveform")]
    public AudioChannel channel = AudioChannel.Center;
    public int sampleCount = 512;
    public float scaleMin;
    public float scaleMax;
    public float scaleFadeSpeed;
    
    [Header("Display")]
    public RectOffset padding;

    public override Texture mainTexture => waveformTexture;
    
    private float[] accumulatedSamples;
    private IMusicVisualizationProvider MusicVisualizer => Instance<IMusicVisualizationProvider>.Get();
    private Texture2D waveformTexture;
    private float adaptiveScalePosition = 1f;
    private float adaptiveScaleOffset = 1f;

    protected override void OnDestroy()
    {
        if(!Application.isPlaying)
            return;

        Destroy(waveformTexture);
    }
    
    private void Update()
    {
        if(!Application.isPlaying)
            return;
        
        UpdateTexture();
        UpdateSamples();
        ApplyTexture();
    }
    
    private void UpdateTexture()
    {
        if (accumulatedSamples == null || !waveformTexture || waveformTexture.width != sampleCount)
        {
            Destroy(waveformTexture);
            accumulatedSamples = new float[sampleCount];
            waveformTexture = new Texture2D(accumulatedSamples.Length, 1, TextureFormat.RFloat, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            SetAllDirty();
        }
    }

    private void UpdateSamples()
    {
        MusicVisualizer.GetWaveform(channel)
            .ResampleTo(accumulatedSamples);
        
        var maxValue = accumulatedSamples[0];
        var minValue = accumulatedSamples[0];

        for (var i = 1; i < accumulatedSamples.Length; i++)
        {
            var v = accumulatedSamples[i];
            if (v > maxValue) maxValue = v;
            if (v < minValue) minValue = v;
        }

        var diff = (maxValue - minValue) / 2f;
        var offset = (maxValue + minValue) / 2f;

        adaptiveScalePosition = Mathf.Lerp(adaptiveScalePosition, diff, scaleFadeSpeed * Time.deltaTime);
        adaptiveScaleOffset = Mathf.Lerp(adaptiveScaleOffset, offset, scaleFadeSpeed * Time.deltaTime);
        var adaptiveScaleValue = Mathf.Clamp(adaptiveScalePosition, scaleMin, scaleMax);

        for (var i = 0; i < accumulatedSamples.Length; i++)
        { 
            accumulatedSamples[i] = Mathf.Clamp((accumulatedSamples[i] - adaptiveScaleOffset) / adaptiveScaleValue, -1f, 1f) ;
        }
    }

    private void ApplyTexture()
    {
        waveformTexture.SetPixelData(accumulatedSamples, 0);
        waveformTexture.Apply();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        var r = GetPixelAdjustedRect();
        var v = new Vector4(r.x + padding.left, r.y + padding.bottom, 
            r.x + r.width - padding.right, r.y + r.height - padding.top);

        Color32 color32 = color;
        vh.Clear();
        vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(0f, -1f));
        vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(0f, 1f));
        vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(1f, 1f));
        vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(1f, -1f));

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}
