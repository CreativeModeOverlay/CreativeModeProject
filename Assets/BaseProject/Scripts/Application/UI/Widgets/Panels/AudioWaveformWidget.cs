using CreativeMode;
using UnityEngine;

public class AudioWaveformWidget : MonoBehaviour
{
    public AudioChannel channel = AudioChannel.Center;
    public RectTransform bounds;
    public LineRenderer lineRenderer;

    private float[] accumulatedSamples = new float[512];
    private Vector3[] pointsData = new Vector3[512];

    private IMusicVisualizer MusicVisualizer => Instance<IMusicVisualizer>.Get();

    private void Update()
    {
        ClearAccumulatedSamples();
        AccumulateSamples();
        UpdateLineRenderer();
    }

    private void ClearAccumulatedSamples()
    {
        for (var i = 0; i < accumulatedSamples.Length; i++)
        {
            accumulatedSamples[i] = -float.MaxValue;
        }
    }

    private void AccumulateSamples()
    {
        var samplesData = MusicVisualizer.GetWaveform(channel);
        var pointsPerSample = samplesData.Length / pointsData.Length;

        for (var i = 0; i < pointsData.Length; i++)
        {
            var sample = 0f;

            for (var s = 0; s < pointsPerSample; s++)
            {
                sample += samplesData[i * pointsPerSample + s];
            }

            sample /= pointsPerSample;
            accumulatedSamples[i] = Mathf.Max(accumulatedSamples[i], sample);
        }
    }

    private void UpdateLineRenderer()
    {
        var rect = bounds.rect;
        
        for (var i = 0; i < accumulatedSamples.Length; i++)
        {
            var xT = i / (pointsData.Length - 1f);
            var yT = (accumulatedSamples[i] + 1f) / 2f;
            
            pointsData[i] = new Vector3(
                Mathf.Lerp(rect.xMin, rect.xMax, xT), 
                Mathf.Lerp(rect.yMin, rect.yMax, yT));
        }
        
        lineRenderer.positionCount = pointsData.Length;
        lineRenderer.SetPositions(pointsData);
    }
}
