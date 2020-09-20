using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace CreativeMode.Impl
{
    public class MusicVisualizationProvider : MonoBehaviour, IMusicVisualizationProvider
    {
        public int waveformBufferSize = 16384;
        public int spectrumBufferSize = 8192;
        public FFTWindow spectrumWindow = FFTWindow.BlackmanHarris;

        public LyricFont[] lyricsFonts;
        public LyricVoice[] lyricsVoices;
        
        public AudioSource outputAudioSource;
        public MusicPlayer musicPlayer;

        private Func<float[]> lWaveformBuffer;
        private Func<float[]> rWaveformBuffer;
        private Func<float[]> cWaveformBuffer;
        private Func<float[]> lSpectrumBuffer;
        private Func<float[]> rSpectrumBuffer;
        private Func<float[]> cSpectrumBuffer;

        public IObservable<Palette> MusicPalette { get; private set; }
        public int ChannelCount => outputAudioSource.clip ? outputAudioSource.clip.channels : 0;

        public bool IsMusicChangeAnimationActive => visualizerElements.Any(e => 
            e.IsMusicChangeAnimationActive);

        private ReplaySubject<LyricLine> allLyricsSubject = new ReplaySubject<LyricLine>(1);
        private Dictionary<string, ReplaySubject<LyricLine>> lyricsSubjects 
            = new Dictionary<string, ReplaySubject<LyricLine>>();

        private List<LyricsManager> lyricsManagers = new List<LyricsManager>();
        private List<IMusicVisualizerElement> visualizerElements = new List<IMusicVisualizerElement>();
        private ImageLoader ImageLoader => Instance<ImageLoader>.Get();

        private void Awake()
        {
            lWaveformBuffer = CreateBufferGetter(waveformBufferSize, buffer => 
                outputAudioSource.GetOutputData(buffer, 0));
            
            rWaveformBuffer = CreateBufferGetter(waveformBufferSize, buffer => 
                outputAudioSource.GetOutputData(buffer, 1));

            lSpectrumBuffer = CreateBufferGetter(spectrumBufferSize, buffer => 
                outputAudioSource.GetSpectrumData(buffer, 0, spectrumWindow));
            
            rSpectrumBuffer = CreateBufferGetter(spectrumBufferSize, buffer => 
                outputAudioSource.GetSpectrumData(buffer, 1, spectrumWindow));
            
            cWaveformBuffer = CreateCenterBufferGetter(waveformBufferSize, lWaveformBuffer, rWaveformBuffer);
            cSpectrumBuffer = CreateCenterBufferGetter(spectrumBufferSize, lSpectrumBuffer, rSpectrumBuffer);

            MusicPalette = musicPlayer.CurrentMusic
                .SelectMany(c => ImageLoader.GetAsset(c.coverUrl))
                .SelectMany(a =>
                {
                    return Observable.CreateSafe<Palette>(subscriber =>
                    {
                        PaletteGenerator.FromTexture(a.Asset.StaticImage.texture, p =>
                        {
                            subscriber.OnNext(p);
                            subscriber.OnCompleted();
                        });
                        return null;
                    });
                })
                .Replay(1).RefCount();
        }

        public float[] GetWaveform(AudioChannel channel)
        {
            switch (channel)
            {
                case AudioChannel.Left: return lWaveformBuffer();
                case AudioChannel.Center: return cWaveformBuffer();
                case AudioChannel.Right: return rWaveformBuffer();
            }

            return null;
        }

        public float[] GetSpectrum(AudioChannel channel)
        {
            switch (channel)
            {
                case AudioChannel.Left: return lSpectrumBuffer();
                case AudioChannel.Center: return cSpectrumBuffer();
                case AudioChannel.Right: return rSpectrumBuffer();
            }

            return null;
        }

        public void AddVisualizer(IMusicVisualizerElement visualizer)
        {
            visualizerElements.Add(visualizer);
        }

        public void RemoveVisualizer(IMusicVisualizerElement visualizer)
        {
            visualizerElements.Remove(visualizer);
        }

        public IObservable<LyricLine> GetLyrics(string voiceId)
        {
            return GetLyricsSubject(voiceId);
        }

        public IObservable<LyricLine> GetAllLyrics()
        {
            return allLyricsSubject;
        }

        private ReplaySubject<LyricLine> GetLyricsSubject(string voiceId)
        {
            if (lyricsSubjects.TryGetValue(voiceId, out var subject))
                return subject;
            
            subject = new ReplaySubject<LyricLine>(1);
            lyricsSubjects[voiceId] = subject;
            return subject;
        }
  
        private Func<float[]> CreateBufferGetter(int size, Action<float[]> value)
        {
            var buffer = new float[size];
            var lastTime = 0f;

            return () =>
            {
                var newTime = Time.time;

                if (newTime != lastTime)
                {
                    value(buffer);
                    lastTime = newTime;
                }
                
                return buffer;
            };
        }

        private Func<float[]> CreateCenterBufferGetter(int size, Func<float[]> lBuffer, Func<float[]> rBuffer)
        {
            return CreateBufferGetter(size, buffer =>
            {
                var l = lBuffer();
                var r = rBuffer();

                for (var i = 0; i < size; i++)
                {
                    buffer[i] = (l[i] + r[i]) / 2f;
                }
            });
        }
        
        [Serializable]
        public class LyricFont
        {
            public string name;
            public Font font;
            public float scale = 1f;
            public bool alignByGeometry;
        }
    
        [Serializable]
        public class LyricVoice
        {
            public string voiceName;
            public string displayName;
        }
        
        public struct LyricsManager
        {
            public ReplaySubject<LyricLine> lyricLine;
            public LyricsSampler lyricsSampler;
        }
    }
}