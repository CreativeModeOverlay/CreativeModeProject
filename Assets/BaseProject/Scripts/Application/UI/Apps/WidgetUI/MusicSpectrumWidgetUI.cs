﻿using CreativeMode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class MusicSpectrumWidgetUI : BaseWidgetUI<MusicSpectrumWidget>
{
    [Header("Spectrum")]
    public AudioChannel channel;
    public float frequencyRange;
    public float minValue;
    public bool reverse;
    public bool maxPeak;
    public float scaleMin;
    public float scaleMax;
    public float scaleFadeSpeed;
    public AnimationCurve frequencyScale = AnimationCurve.Linear(0, 0.1f, 0.25f, 1f);
    
    [Header("Display")]
    public RectOffset padding;
    public float bandSpacing;
    public float bandWidth;
    public Material spectrumRenderMaterial;

    private int bandCount;
    private float[] bandValue;
    private Texture2D bandTexture;
    private float adaptiveScalePosition = 1f;
    private SpectrumGraphic graphic;

    private IMusicVisualizationProvider MusicVisualizer => Instance<IMusicVisualizationProvider>.Get();

    private void Awake()
    {
        graphic = UIUtils.CreateInnerGraphic<SpectrumGraphic>(this);
        graphic.widget = this;
    }

    private void OnDestroy()
    {
        Destroy(bandTexture);
    }

    private void UpdateBandCount()
    {
        var t = (RectTransform) transform;
        
        var rect = t.rect;
        var layoutWidth = rect.width - padding.left - padding.right;
        var newBandCount = (int) ((layoutWidth + bandSpacing) / (bandWidth + bandSpacing));

        if (newBandCount != bandCount)
        {
            bandCount = newBandCount;
            graphic.SetVerticesDirty();
        }
    }

    private void Update()
    {
        if(!Application.isPlaying)
            return;
        
        UpdateBandCount();
        ClearBandValues();

        UpdateBandValues(MusicVisualizer.GetSpectrum(channel));
        UpdateBandMaterial();
    }

    private void ClearBandValues()
    {
        if (bandValue == null || bandValue.Length != bandCount)
        {
            bandValue = new float[bandCount];
        }
        else
        {
            for (var i = 0; i < bandValue.Length; i++)
            {
                bandValue[i] = 0;
            }
        }
    }

    private void UpdateBandValues(float[] fftSamples)
    {
        var sampleLength = fftSamples.Length * Mathf.Clamp01(frequencyRange);
        var maxValue = 0f;
        
        for (var i = 0; i < bandValue.Length; i++)
        {
            var tStart = i / (float) bandValue.Length;
            var tEnd = (i + 1) / (float) bandValue.Length;
            var start = Mathf.FloorToInt(sampleLength * tStart);
            var end = Mathf.FloorToInt(sampleLength * tEnd);
            var value = 0f;

            if (maxPeak)
            {
                for (var s = start; s < end; s++)
                {
                    value = Mathf.Max(value, fftSamples[s]);
                }
            }
            else
            {
                for (var s = start; s < end; s++)
                {
                    value += fftSamples[s];
                }

                value /= end - start;
            }

            var scaledValue = value * frequencyScale.Evaluate(value);

            maxValue = Mathf.Max(maxValue, scaledValue);
            bandValue[i] = value;
        }

        adaptiveScalePosition = Mathf.Lerp(adaptiveScalePosition, maxValue, scaleFadeSpeed * Time.deltaTime);
    }
    
    private void UpdateBandMaterial()
    {
        if(bandCount == 0)
            return;
        
        if (!bandTexture || bandTexture.width != bandCount)
        {
            Destroy(bandTexture);
            bandTexture = new Texture2D(bandCount, 1, TextureFormat.RFloat, false);
            graphic.SetAllDirty();
        }
        
        var adaptiveScaleValue = Mathf.Clamp(adaptiveScalePosition, scaleMin, scaleMax);

        for (var i = 0; i < bandCount; i++)
        {
            var valueIndex = reverse ? bandCount - 1 - i : i;
            bandValue[i] = adaptiveScalePosition > 0 
                ? minValue + bandValue[valueIndex] / adaptiveScaleValue
                : 1;
        }

        bandTexture.SetPixelData(bandValue, 0);
        bandTexture.Apply();
    }
    
    private class SpectrumGraphic : Graphic
    {
        public MusicSpectrumWidgetUI widget;
        
        public override Texture mainTexture => widget.bandTexture;
        public override Material materialForRendering => widget.spectrumRenderMaterial;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        
            var rect = GetPixelAdjustedRect();
            var vertexOffset = 0;

            var xMin = rect.x + widget.padding.left;
            var xMax = xMin + widget.bandWidth;
            var stride = widget.bandWidth + widget.bandSpacing;
        
            var yMin = rect.y + widget.padding.bottom;
            var yMax = rect.y + rect.height - widget.padding.top - widget.padding.bottom;
        
            for (var b = 0; b < widget.bandCount; b++)
            {
                var uvPosition = (float) b / widget.bandCount;
            
                Color32 color32 = color;
                vh.AddVert(new Vector3(xMin, yMin), color32, 
                    new Vector2(uvPosition, 0f), new Vector2(0, 0), 
                    Vector3.zero, Vector4.zero);
            
                vh.AddVert(new Vector3(xMin, yMax), color32, 
                    new Vector2(uvPosition, 1f), new Vector2(0, 1), 
                    Vector3.zero, Vector4.zero);
            
                vh.AddVert(new Vector3(xMax, yMax), color32, 
                    new Vector2(uvPosition, 1f), new Vector2(1, 1), 
                    Vector3.zero, Vector4.zero);
            
                vh.AddVert(new Vector3(xMax, yMin), color32, 
                    new Vector2(uvPosition, 0f), new Vector2(1, 0), 
                    Vector3.zero, Vector4.zero);

                vh.AddTriangle(vertexOffset, vertexOffset + 1, vertexOffset + 2);
                vh.AddTriangle(vertexOffset + 2, vertexOffset + 3, vertexOffset);

                vertexOffset += 4;

                xMin += stride;
                xMax += stride;
            }
        }
    }
}
