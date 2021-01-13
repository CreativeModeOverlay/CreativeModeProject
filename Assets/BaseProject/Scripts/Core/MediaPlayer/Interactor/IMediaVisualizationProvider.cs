using System;

namespace CreativeMode
{
    public interface IMediaVisualizationProvider
    {
        IObservable<Palette> MusicPalette { get; }

        WaveformSource WaveformSource { get; }
        int ChannelCount { get; }

        float[] GetWaveform(AudioChannel channel);
        float[] GetSpectrum(AudioChannel channel);
        
        void AddVisualizer(IMusicVisualizerElement visualizer);
        void RemoveVisualizer(IMusicVisualizerElement visualizer);
    }
}