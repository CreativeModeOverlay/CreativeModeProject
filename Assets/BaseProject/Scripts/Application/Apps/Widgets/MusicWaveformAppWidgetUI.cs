using CreativeMode;
using UnityEngine;
using UnityEngine.UI;

public class MusicWaveformAppWidgetUI : BaseAppWidgetUI<MusicWaveformAppWidget>
{
    private IMediaVisualizationProvider MusicVisualizer => Instance<IMediaVisualizationProvider>.Get();
    
    [Header("Waveform")]
    public AudioChannel channel = AudioChannel.Center;
    public int sampleCount = 512;
    public float scaleMin;
    public float scaleMax;
    public float scaleFadeSpeed;
    
    [Header("Display")]
    public RectOffset padding;
    public Material waveformRenderMaterial;

    private float[] accumulatedSamples;
    private Texture2D waveformTexture;
    private float adaptiveScalePosition = 1f;
    private float adaptiveScaleOffset = 1f;
    private WaveformGraphic graphic;

    private void Awake()
    {
        graphic = UIUtils.CreateInnerGraphic<WaveformGraphic>(this);
        graphic.widget = this;
    }

    private void OnDestroy()
    {
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
            graphic.SetAllDirty();
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

    private class WaveformGraphic : Graphic
    {
        public MusicWaveformAppWidgetUI widget;
        
        public override Texture mainTexture => widget.waveformTexture;
        public override Material materialForRendering => widget.waveformRenderMaterial;
        
        private static readonly Vector4 s_DefaultTangent = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
        private static readonly Vector3 s_DefaultNormal = Vector3.back;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x + widget.padding.left, r.y + widget.padding.bottom, 
                r.x + r.width - widget.padding.right, r.y + r.height - widget.padding.top);
            var sizeData = new Vector2(r.width, r.height);

            Color32 color32 = color;
            vh.Clear();
            vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(0f, -1f), 
                sizeData, s_DefaultNormal, s_DefaultTangent);
            vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(0f, 1f), 
                sizeData, s_DefaultNormal, s_DefaultTangent);
            vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(1f, 1f), 
                sizeData, s_DefaultNormal, s_DefaultTangent);
            vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(1f, -1f), 
                sizeData, s_DefaultNormal, s_DefaultTangent);

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }
}
