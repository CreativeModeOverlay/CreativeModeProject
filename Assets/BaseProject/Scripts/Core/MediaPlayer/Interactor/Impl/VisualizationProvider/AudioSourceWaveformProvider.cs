using UnityEngine;

namespace CreativeMode.Impl
{
    public class AudioSourceWaveformProvider : IAudioWaveformProvider
    {
        private float lastHeardSampleTime;
        
        private readonly AudioSource audioSource;
        private readonly FFTWindow window;
        private readonly float[] silenceDetectBuffer;
        
        public WaveformSource Source { get; }
        public bool IsSilent => Time.time - lastHeardSampleTime > 1f;

        public AudioSourceWaveformProvider(AudioSource audioSource, FFTWindow window, WaveformSource sourceType)
        {
            this.audioSource = audioSource;
            this.window = window;
            silenceDetectBuffer = new float[512];
            lastHeardSampleTime = Time.time;

            Source = sourceType;
        }

        public void Update()
        {
            for (var c = 0; c < 2; c++)
            {
                audioSource.GetOutputData(silenceDetectBuffer, c);
            
                for (var i = 0; i < silenceDetectBuffer.Length; i++)
                {
                    if (Mathf.Abs(silenceDetectBuffer[i]) > 0.0001f)
                    {
                        lastHeardSampleTime = Time.time;
                        break;
                    }
                }
            }
        }

        public void GetWaveform(float[] buffer, int channel)
        {
            audioSource.GetOutputData(buffer, channel);
        }

        public void GetSpectrum(float[] buffer, int channel)
        {
            audioSource.GetSpectrumData(buffer, channel, window);
        }
        
        public void Dispose() {}
    }
}