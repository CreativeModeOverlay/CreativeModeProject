using System.Collections.Generic;
using CreativeMode;
using UnityEngine;
using UnityEngine.UI;

public class AudioEqualizerWidget : MonoBehaviour
{
    public AudioChannel channel;
    public float scale;
    public float frequencyRange;
    public RawImage bandPrefab;
    public HorizontalLayoutGroup layoutGroup;
    public float minValue;
    public bool reverse;
    public bool maxPeak;
    public AnimationCurve scaleCurve;

    private int bandCount;
    private List<RawImage> bandImages = new List<RawImage>();
    private float[] bandValue;
    private Texture2D bandTexture;

    private IMusicVisualizationProvider MusicVisualizer => Instance<IMusicVisualizationProvider>.Get();

    private void OnDestroy()
    {
        Destroy(bandTexture);
    }

    private void CalculateBandCount()
    {
        var rect = transform as RectTransform;

        if (rect)
        {
            var bandWidth = bandPrefab.rectTransform.rect.width;
            var padding = layoutGroup.padding;
            var spacing = layoutGroup.spacing;
            var layoutWidth = ((RectTransform) layoutGroup.transform).rect.width - padding.left - padding.right;

            bandCount = (int) ((layoutWidth + spacing) / (bandWidth + spacing));
        }
    }

    private void Update()
    {
        CalculateBandCount();
        ClearBandValues();

        AccumulateBandValues(MusicVisualizer.GetSpectrum(channel));

        UpdateBandObjects();
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

    private void AccumulateBandValues(float[] fftSamples)
    {
        var sampleLength = fftSamples.Length * Mathf.Clamp01(frequencyRange);
        
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
            
            var existingValue = bandValue[i];

            if (existingValue < value)
            {
                bandValue[i] = value;
            }
        }
    }

    private void UpdateBandObjects()
    {
        var updateUv = bandImages.Count != bandCount;
        
        if(!updateUv)
            return;
        
        while (bandImages.Count > bandCount)
        {
            var index = bandImages.Count - 1;
            var band = bandImages[index];
            bandImages.RemoveAt(index);
            Destroy(band.gameObject);
        }
        
        while (bandImages.Count < bandCount)
        {
            var newBand = Instantiate(bandPrefab, transform);
            newBand.texture = bandTexture;
            newBand.gameObject.SetActive(true);
            bandImages.Add(newBand);
        }

        for (var i = 0; i < bandImages.Count; i++)
        {
            var t = (float) i / bandImages.Count;
            bandImages[i].uvRect = new Rect(t, 0f, 0f, 1f);
        }
    }

    private void UpdateBandMaterial()
    {
        if (!bandTexture || bandTexture.width != bandImages.Count)
        {
            Destroy(bandTexture);
            bandTexture = new Texture2D(bandCount, 1, TextureFormat.RFloat, false);

            foreach (var bandImage in bandImages)
            {
                bandImage.texture = bandTexture;
            }
        }

        for (var i = 0; i < bandImages.Count; i++)
        {
            var valueIndex = reverse ? bandImages.Count - 1 - i : i;
            var freq = (valueIndex / (float) (bandImages.Count - 1)) * frequencyRange;
            var freqScale = scaleCurve.Evaluate(freq);
            bandValue[i] = minValue + bandValue[valueIndex] * scale * freqScale;
        }

        bandTexture.SetPixelData(bandValue, 0);
        bandTexture.Apply();
    }
}
