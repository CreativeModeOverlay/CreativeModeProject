using System;

namespace CreativeMode
{
    public interface IAudioWaveformProvider : IDisposable
    {
        WaveformSource Source { get; }
        bool IsSilent { get; }

        void Update();

        void GetWaveform(float[] buffer, int channel);
        void GetSpectrum(float[] buffer, int channel);
    }
}