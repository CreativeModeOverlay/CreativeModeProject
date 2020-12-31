using System;
using System.Runtime.InteropServices;
using System.Threading;
using CSCore.Utils.Buffer;
using LibVLCSharp;
using UnityEngine;
using Object = System.Object;

namespace CreativeMode
{
    public class VlcMediaPlayerBehaviour : MonoBehaviour
    {
        public AudioSource source;
        public int bufferSizeInSeconds = 10;

        public int BufferedSampleCount { get; private set; }
        public int BufferedLengthMs { get; private set; }
        public int ChannelCount { get; private set; }
        public int SampleRate { get; private set; }

        public bool IsMediaSet => vlcPlayer?.Media != null;
        public bool IsPlaying => vlcPlayer?.IsPlaying ?? false;
        public bool IsBuffering => isBufferingOccured;

        private float currentPosition;

        public float Position
        {
            get
            {
                return currentPosition;
            }
            set
            {
                vlcPlayer?.SetTime(Mathf.FloorToInt(value * 1000f));
            }
        }

        public float AudioSourcePosition
        {
            get => source.time;
        }

        public float Duration => vlcPlayer?.Length / 1000f ?? 0;

        public Texture VideoTexture => outputVideo;

        public int VideoWidth { get; private set; }
        public int VideoHeight { get; private set; }
        
        private AudioClip outputAudio;
        private Texture2D outputVideo;
        private AudioBuffer currentBuffer = new AudioBuffer
        {
            samples = new FixedSizeBuffer<float>(0),
            isReady = false,
            restartPlayback = RestartReason.NoRestart
        };
        
        private AudioBuffer clipBuffer;

        private bool isDestroyed;
        private bool isVlcPlaying;
        private bool isUnitySourcePlaying;

        private short[] audioShortBuffer = new short[0];
        private float[] audioFloatBuffer = new float[0];

        private LibVLC vlcLib;
        private MediaPlayer vlcPlayer;
        private Media vlcMedia;
        private bool isVlcInitialized;
        private bool isBufferingOccured;

        private IntPtr vlcTexturePointer;
        private bool vlcTextureUpdated;
        private Thread vlcTextureUpdateThread;
        private readonly Object vlcTextureUpdateMonitor = new object();

        public PlaybackState State { get; private set; }

        public void SetMedia(string url, string audioUrl = null)
        {
            InitVlc();

            if (vlcMedia != null)
            {
                vlcPlayer.Stop();
                vlcMedia.Dispose();
                vlcMedia = null;
            }
            
            var newMedia = CreateMedia(url, audioUrl);
            vlcPlayer.Media = newMedia;
            vlcMedia = newMedia;
            State = PlaybackState.Idle;
        }

        public void Play()
        {
            InitVlc();
            vlcPlayer.Play();
        }

        public void Pause()
        {
            InitVlc();
            vlcPlayer.Pause();
        }

        public void Restart()
        {
            InitVlc();
            vlcPlayer.Media = vlcPlayer.Media;
            vlcPlayer.Play();
        }

        public void Stop()
        {
            InitVlc();
            vlcPlayer.Stop();
        }

        private void InitVlc()
        {
            if (isVlcInitialized)
                return;

            Core.Initialize(Application.dataPath);

            vlcLib = new LibVLC("--no-osd", "--verbose=2");
            vlcPlayer = new MediaPlayer(vlcLib);
            vlcPlayer.SetLogoInt(VideoLogoOption.Opacity, 0);
            vlcPlayer.NetworkCaching = 5000;

            vlcPlayer.SetAudioFormatCallback(
                OnVlcAudioSetup, OnVlcAudioCleanup);

            vlcPlayer.SetAudioCallbacks(OnVlcPlayAudio,
                OnVlcPauseAudio, OnVlcResumeAudio,
                OnVlcFlushAudio, OnVlcDrainAudio);

            vlcPlayer.NothingSpecial += (o, a) => OnVlcStateChanged(VLCState.NothingSpecial);
            vlcPlayer.Opening += (o, a) => OnVlcStateChanged(VLCState.Opening);
            vlcPlayer.Playing += (o, a) => OnVlcStateChanged(VLCState.Playing);
            vlcPlayer.Paused += (o, a) => OnVlcStateChanged(VLCState.Paused);
            vlcPlayer.Stopped += (o, a) => OnVlcStateChanged(VLCState.Stopped);
            vlcPlayer.EndReached += (o, a) => OnVlcStateChanged(VLCState.Ended);
            vlcPlayer.EncounteredError += (o, a) => OnVlcStateChanged(VLCState.Error);
            vlcPlayer.Buffering += (o, a) =>
            {
                isBufferingOccured = (int) a.Cache != 100;
                OnVlcStateChanged(VLCState.Buffering);
            };

            vlcTextureUpdateThread = new Thread(() =>
            {
                lock (vlcTextureUpdateMonitor)
                {
                    while (!isDestroyed)
                    {
                        Monitor.Wait(vlcTextureUpdateMonitor);
                        
                        vlcTexturePointer = vlcPlayer.GetTexture(out var hasUpdates);
                            
                        if (hasUpdates)
                            vlcTextureUpdated = true;
                    }
                }
            });
            vlcTextureUpdateThread.Start();

            isVlcInitialized = true;
        }

        private void OnDestroy()
        {
            isDestroyed = true;

            Destroy(outputAudio);
            Destroy(outputVideo);

            UpdateVideoThread();
            vlcTextureUpdateThread?.Join();
            
            vlcMedia?.Dispose();
            vlcPlayer?.Dispose();
            vlcLib?.Dispose();

            vlcMedia = null;
            vlcPlayer = null;
            vlcLib = null;
        }

        private void Update()
        {
            UpdateCurrentPosition();
            UpdateAudio();
            UpdateVideo();
            UpdateVideoThread();
        }

        private void UpdateVideoThread()
        {
            lock (vlcTextureUpdateMonitor)
            {
                Monitor.Pulse(vlcTextureUpdateMonitor);
            }
        }

        private void UpdateCurrentPosition()
        {
            // Workaround for case, where if media player paused, .Time keeps increasing as if it is playing
            switch (State)
            {
                case PlaybackState.Playing:
                    currentPosition = vlcPlayer?.Time / 1000f ?? 0f;
                    break;
                    
                case PlaybackState.Finished:
                    currentPosition = Duration;
                    break;
                    
                case PlaybackState.Idle:
                case PlaybackState.Stopped:
                    currentPosition = 0;
                    break;
            }
        }

        private void UpdateAudio()
        {
            var currentRate = outputAudio ? outputAudio.frequency : 0;
            var currentChannelCount = outputAudio ? outputAudio.channels : 0;

            if (currentRate != SampleRate ||
                currentChannelCount != ChannelCount || 
                clipBuffer != currentBuffer)
            {
                source.clip = null;
                Destroy(outputAudio);
                outputAudio = null;

                if (SampleRate != 0 && ChannelCount != 0)
                {
                    Debug.Log("Audio source recreated");

                    clipBuffer = currentBuffer;
                    var sampleBuffer = clipBuffer;
                    outputAudio = AudioClip.Create("Buffered source", Int32.MaxValue,
                        ChannelCount, SampleRate, true, data => { OnClipReadCallback(sampleBuffer, data); });

                    isUnitySourcePlaying = false;
                    source.clip = outputAudio;
                    source.loop = true;
                }
            }

            if (clipBuffer != null && clipBuffer.restartPlayback != RestartReason.NoRestart)
            {
                Debug.Log($"Audio source restarted, cause: {clipBuffer.restartPlayback}");
                clipBuffer.restartPlayback = RestartReason.NoRestart;

                source.Stop();
                
                if(isVlcPlaying) 
                    source.Play();
            }

            if (isVlcPlaying != isUnitySourcePlaying)
            {
                if (isVlcPlaying)
                {
                    source.Play();
                }
                else
                {
                    source.Pause();
                }
            }

            isUnitySourcePlaying = isVlcPlaying;
        }

        private void UpdateVideo()
        {
            if (vlcPlayer == null)
                return;

            var ptr = vlcTexturePointer;
            
            uint textureHeight = 0;
            uint textureWidth = 0;
            vlcPlayer.Size(0, ref textureWidth, ref textureHeight);

            VideoWidth = (int) textureWidth;
            VideoHeight = (int) textureHeight;
            
            if (!outputVideo)
            {
                if (textureWidth != 0 && textureHeight != 0 && vlcTextureUpdated && ptr != IntPtr.Zero)
                {
                    Destroy(outputVideo);

                    // Debug.Log("Video texture created");
                    outputVideo = Texture2D.CreateExternalTexture((int) textureWidth, (int) textureHeight,
                        TextureFormat.ARGB32, false, true, ptr);
                    outputVideo.filterMode = FilterMode.Trilinear;
                }
            }
            else if (outputVideo)
            {
                outputVideo.UpdateExternalTexture(ptr);
            }
        }

        private Media CreateMedia(string url, string audioUrl = null)
        {
            InitVlc();

            var media = new Media(vlcLib, new Uri(url));

            if (audioUrl != null)
                media.AddSlave(MediaSlaveType.Audio, 1, audioUrl);

            return media;
        }

        private int OnVlcAudioSetup(ref IntPtr opaque, ref IntPtr format, ref uint rate, ref uint channels)
        {
            // Debug.Log("AudioSetup");
            isVlcPlaying = true;
            
            SetFormat((int) rate, (int) channels);
            CreateNewBuffer();
            return 0;
        }

        private void OnVlcAudioCleanup(IntPtr opaque)
        {
            // Debug.Log("AudioCleanup");
            PauseOutput(true);
        }

        private void OnVlcPlayAudio(IntPtr data, IntPtr samples, uint count, long pts)
        {
            int sampleCount = (int) count * 2;

            if (audioShortBuffer.Length < sampleCount)
                Array.Resize(ref audioShortBuffer, sampleCount);

            if (audioFloatBuffer.Length < sampleCount)
                Array.Resize(ref audioFloatBuffer, sampleCount);

            Marshal.Copy(samples, audioShortBuffer, 0, sampleCount);

            for (var i = 0; i < audioShortBuffer.Length; i++)
                audioFloatBuffer[i] = (float) audioShortBuffer[i] / Int16.MaxValue;

            PushSamples(audioFloatBuffer, sampleCount);
        }

        private void OnVlcDrainAudio(IntPtr data)
        {
            // Debug.Log("DrainAudio");
        }

        private void OnVlcFlushAudio(IntPtr data, long pts)
        {
            // Debug.Log("FlushAudio");
            CreateNewBuffer();
        }

        private void OnVlcResumeAudio(IntPtr data, long pts)
        {
            // Debug.Log("ResumeAudio");
            PauseOutput(false);
        }

        private void OnVlcPauseAudio(IntPtr data, long pts)
        {
            // Debug.Log("PauseAudio");
            PauseOutput(true);
        }

        private void OnVlcEndReached(object sender, EventArgs args)
        {
            Debug.Log("End Reached");
        }

        private void OnVlcStateChanged(VLCState newState)
        {
            switch (newState)
            {
                case VLCState.Paused: 
                    State = PlaybackState.Paused;
                    break;
                    
                case VLCState.Stopped: 
                    State = PlaybackState.Stopped;
                    break;
                    
                case VLCState.Playing:
                case VLCState.Opening: 
                case VLCState.Buffering:
                    State = PlaybackState.Playing;
                    break;
                    
                case VLCState.Ended:
                    State = PlaybackState.Finished;
                    break;
                    
                case VLCState.Error:
                    State = PlaybackState.Error;
                    break;
                    
                default:
                    State = PlaybackState.Idle;
                    break;
            }
            
            UpdateCurrentPosition();
        }

        private void UpdateBufferInfo()
        {
            BufferedSampleCount = currentBuffer.samples.Buffered;
            BufferedLengthMs = SamplesToMiliseconds(BufferedSampleCount);
        }

        private void SetFormat(int sampleRate, int channelCount)
        {
            SampleRate = sampleRate;
            ChannelCount = channelCount;
        }

        private void CreateNewBuffer()
        {
            var oldBuffer = currentBuffer;

            if (oldBuffer != null)
            {
                oldBuffer.isReady = false;
                oldBuffer.samples.Clear();
            }

            currentBuffer = new AudioBuffer
            {
                samples = new FixedSizeBuffer<float>(bufferSizeInSeconds * SampleRate),
                restartPlayback = RestartReason.BufferRecreated,
                isReady = false
            };
        }

        private void PushSamples(float[] vlcSamples, int count)
        {
            var buff = currentBuffer;

            if (buff.discardSamples > 0)
            {
                var toSkip = Mathf.Min(count, buff.discardSamples);
                var left = count - toSkip;
                
                buff.discardSamples -= toSkip;
                buff.samples.Write(vlcSamples, toSkip, left);
            }
            else
            {
                buff.samples.Write(vlcSamples, 0, count);
            }

            if (!buff.isReady && buff.samples.Length > 4096)
            {
                buff.isReady = true;
                buff.restartPlayback = RestartReason.BufferIsReady;
            }

            UpdateBufferInfo();
        }

        private void PauseOutput(bool isPaused)
        {
            isVlcPlaying = !isPaused;
        }

        private void OnClipReadCallback(AudioBuffer inBuffer, float[] outBuffer)
        {
            if (isDestroyed)
                return;

            var shouldRead = inBuffer.isReady && inBuffer.restartPlayback == RestartReason.NoRestart;
            
            if(shouldRead && inBuffer.samples.Buffered < outBuffer.Length)
                Thread.Sleep(1); // Last ditch effort
            
            var count = shouldRead ? inBuffer.samples.Read(outBuffer, 0, outBuffer.Length) : 0;

            if (shouldRead)
                inBuffer.discardSamples += outBuffer.Length - count;
            
            for (var i = count; i < outBuffer.Length; i++)
                outBuffer[i] = 0;
            
            UpdateBufferInfo();
        }

        private int SamplesToMiliseconds(int sample)
        {
            return (int) ((double) sample / SampleRate / ChannelCount * 1000);
        }

        public enum PlaybackState
        {
            Idle,
            Playing,
            Paused,
            Stopped,
            Finished,
            Error
        }
        
        private class AudioBuffer
        {
            public FixedSizeBuffer<float> samples;
            public bool isReady;
            public int discardSamples;
            public RestartReason restartPlayback;
        }
        
        private enum RestartReason
        {
            NoRestart,
            BufferRecreated,
            BufferIsReady
        }
    }
}