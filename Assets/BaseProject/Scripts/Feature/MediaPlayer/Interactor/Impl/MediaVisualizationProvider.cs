using System;
using System.Collections.Generic;
using System.Linq;
using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.SoundIn;
using UniRx;
using UnityEngine;

namespace CreativeMode.Impl
{
    public class MediaVisualizationProvider : MonoBehaviour, IMediaVisualizationProvider
    {
        private IImageLoader ImageLoader => Instance<IImageLoader>.Get();
        
        public int waveformBufferSize = 16384;
        public int spectrumBufferSize = 8192;
        public FFTWindow spectrumWindow = FFTWindow.BlackmanHarris;

        public AudioSource outputAudioSource;
        public MediaPlayer musicPlayer;

        private Func<float[]> lWaveformBuffer;
        private Func<float[]> rWaveformBuffer;
        private Func<float[]> cWaveformBuffer;
        private Func<float[]> lSpectrumBuffer;
        private Func<float[]> rSpectrumBuffer;
        private Func<float[]> cSpectrumBuffer;

        private IAudioWaveformProvider currentProvider;
        private IAudioWaveformProvider[] waveformProviders;

        public IObservable<Palette> MusicPalette { get; private set; }
        public WaveformSource WaveformSource => currentProvider.Source;
        
        public int ChannelCount => outputAudioSource.clip ? outputAudioSource.clip.channels : 0;

        public bool IsMusicChangeAnimationActive => visualizerElements.Any(e => 
            e.IsMusicChangeAnimationActive);

        private List<IMusicVisualizerElement> visualizerElements = new List<IMusicVisualizerElement>();

        private void Awake()
        {
            waveformProviders = new IAudioWaveformProvider[]
            {
                new AudioSourceWaveformProvider(outputAudioSource, spectrumWindow, 
                    WaveformSource.MediaPlayer),
                
                new WasapiWaveformProvider(
                    waveformBufferSize, spectrumBufferSize * 2, WindowFunctions.Hamming,
                    new WasapiLoopbackCapture(), WaveformSource.SystemLoopback),
                
                new WasapiWaveformProvider(
                    waveformBufferSize, spectrumBufferSize * 2, WindowFunctions.Hamming, 
                    new WasapiCapture { 
                        Device = MMDeviceEnumerator.DefaultAudioEndpoint(DataFlow.Capture, Role.Communications)
                    }, WaveformSource.Microphone)
            };
            currentProvider = waveformProviders[0];

            lWaveformBuffer = CreateBufferGetter(waveformBufferSize, buffer => 
                currentProvider.GetWaveform(buffer, 0));
            
            rWaveformBuffer = CreateBufferGetter(waveformBufferSize, buffer => 
                currentProvider.GetWaveform(buffer, 1));

            lSpectrumBuffer = CreateBufferGetter(spectrumBufferSize, buffer => 
                currentProvider.GetSpectrum(buffer, 0));
            
            rSpectrumBuffer = CreateBufferGetter(spectrumBufferSize, buffer => 
                currentProvider.GetSpectrum(buffer, 1));
            
            cWaveformBuffer = CreateCenterBufferGetter(waveformBufferSize, lWaveformBuffer, rWaveformBuffer);
            cSpectrumBuffer = CreateCenterBufferGetter(spectrumBufferSize, lSpectrumBuffer, rSpectrumBuffer);

            MusicPalette = musicPlayer.CurrentMedia
                .SelectMany(c => ImageLoader.GetImage(c.thumbnailUrl))
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

        private void Update()
        {
            var isProviderSet = false;
            
            for (var i = 0; i < waveformProviders.Length; i++)
            {
                var provider = waveformProviders[i];
                provider.Update();

                if (!provider.IsSilent && !isProviderSet)
                {
                    currentProvider = provider;
                    isProviderSet = true;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var provider in waveformProviders)
                provider.Dispose();
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
    }
}