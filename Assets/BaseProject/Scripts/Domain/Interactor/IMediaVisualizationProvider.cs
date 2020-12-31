using System;

namespace CreativeMode
{
    public interface IMediaVisualizationProvider
    {
        IObservable<Palette> MusicPalette { get; }
        
        int ChannelCount { get; }

        float[] GetWaveform(AudioChannel channel);
        float[] GetSpectrum(AudioChannel channel);
        
        void AddVisualizer(IMusicVisualizerElement visualizer);
        void RemoveVisualizer(IMusicVisualizerElement visualizer);

        IObservable<LyricLine> GetLyrics(string voiceId = "");
        IObservable<LyricLine> GetAllLyrics();
    }
}