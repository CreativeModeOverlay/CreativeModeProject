using System;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace CreativeMode.Impl
{
    public class MediaPlayer : MonoBehaviour, IMediaPlayer
    {
        private IMediaPlaylistProvider Playlist => Instance<IMediaPlaylistProvider>.Get();
        private IMediaProvider MediaProvider => Instance<IMediaProvider>.Get();
        
        [FormerlySerializedAs("visualizer")]
        public MediaVisualizationProvider visualizationProvider;
        
        public IObservable<MediaMetadata> CurrentMedia => onMusicChangedSubject;

        public float FadeInDuration { get; set; } = 0.5f;
        public float FadeOutDuration { get; set; } = 0.5f;

        public bool IsBuffering => vlcMediaPlayer.IsBuffering;

        public float Pitch
        {
            get => outputAudioSource.pitch;
            set => outputAudioSource.pitch = value;
        }

        public float NormalizedPosition
        {
            get => Mathf.InverseLerp(0, Duration, Position);
            set => Position = Mathf.Clamp01(value / Duration);
        }

        public bool IsPlaying => outputAudioSource.isPlaying;

        public float Position
        {
            get => vlcMediaPlayer.Position;
            set => vlcMediaPlayer.Position = value;
        }

        public float Duration => vlcMediaPlayer.Duration;
        
        public AudioSource outputAudioSource;
        public VlcMediaPlayerBehaviour vlcMediaPlayer;
        
        private readonly ReplaySubject<MediaMetadata> onMusicChangedSubject = 
            new ReplaySubject<MediaMetadata>(1);
 
        private AudioSourceState sourceState;
        private MediaPlayerState playerState;
        private IDisposable musicLoadDisposable;

        private void Update()
        {
            if (sourceState == AudioSourceState.Playing && 
                vlcMediaPlayer.State == VlcMediaPlayerBehaviour.PlaybackState.Finished)
            {
                sourceState = AudioSourceState.Finished;
                OnPlaybackFinished();
            }

            if (playerState == MediaPlayerState.WaitForVisualizersAndPlay && 
                !visualizationProvider.IsMusicChangeAnimationActive)
                Play();
        }

        public void Play()
        {
            DOTween.Kill(outputAudioSource);

            if (visualizationProvider.IsMusicChangeAnimationActive)
            {
                playerState = MediaPlayerState.WaitForVisualizersAndPlay;
                return;
            }

            if (FadeInDuration > 0)
            {
                playerState = MediaPlayerState.FadeIn;
                outputAudioSource.DOFade(1f, FadeInDuration)
                    .OnComplete(() => playerState = MediaPlayerState.Playing);
            }
            else
            {
                playerState = MediaPlayerState.Playing;
                outputAudioSource.volume = 1f;
            }

            if (vlcMediaPlayer.IsMediaSet)
            {
                PlaySource();
            }
            else
            {
                Next();   
            }
        }

        public void Pause(bool toggle)
        {
            if (playerState == MediaPlayerState.FadeOutToPause 
                || playerState == MediaPlayerState.Paused 
                || playerState == MediaPlayerState.Stopped)
            {
                if(toggle) 
                    Play();

                return;
            }

            DOTween.Kill(outputAudioSource);
            
            if (FadeOutDuration > 0)
            {
                playerState = MediaPlayerState.FadeOutToPause;
                outputAudioSource.DOFade(0, FadeOutDuration)
                    .OnComplete(() =>
                    {
                        playerState = MediaPlayerState.Paused;
                        PauseSource();
                    });
            }
            else
            {
                playerState = MediaPlayerState.Paused;
                outputAudioSource.volume = 0;
                PauseSource();
            }
        }

        public void Stop()
        {
            if(playerState == MediaPlayerState.FadeOutToStop || playerState == MediaPlayerState.Stopped)
                return;
            
            DOTween.Kill(outputAudioSource);
            
            if (FadeOutDuration > 0)
            {
                playerState = MediaPlayerState.FadeOutToStop;
                outputAudioSource.DOFade(0, FadeOutDuration)
                    .OnComplete(() =>
                    {
                        playerState = MediaPlayerState.Stopped;
                        StopSource();
                    });
            }
            else
            {
                playerState = MediaPlayerState.Paused;
                outputAudioSource.volume = 0;
                StopSource();
            }
        }

        public void Rewind()
        {
            DOTween.Kill(outputAudioSource);
            
            StopSource();
            
            if (playerState == MediaPlayerState.Playing || 
                playerState == MediaPlayerState.FadeIn)
            {
                Play();
            }
        }

        public void Next()
        {
            OnNextPlaylistEntry(Playlist.AdvanceToNext(Playlist.CurrentSet));
        }
        
        public void Previous()
        {
            OnNextPlaylistEntry(Playlist.ReturnToPrevious());
        }

        private void OnNextPlaylistEntry(MediaMetadata meta)
        {
            if (meta != null)
            {
                LoadMedia(meta);
            }
            else
            {
                Stop();
            }
        }

        private void OnPlaybackFinished()
        {
            Debug.Log("Playback finished!");
            Next();
        }

        private void LoadMedia(MediaMetadata entry)
        {
            Debug.Log($"Loading music: {entry.url}");
            
            vlcMediaPlayer.Stop();
            onMusicChangedSubject.OnNext(entry);
            
            musicLoadDisposable?.Dispose();
            musicLoadDisposable = MediaProvider.GetMediaByUrl(entry.url, 1280, 720, true)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(r => OnMediaDataGet(r.First()));
        }

        private void OnMediaDataGet(MediaInfo media)
        {
            sourceState = AudioSourceState.Stopped;
            
            vlcMediaPlayer.SetMedia(media.streamUrl, media.audioStreamUrl);
            
            DOTween.Kill(outputAudioSource);

            switch (playerState)
            {
                case MediaPlayerState.Playing:
                case MediaPlayerState.FadeIn:
                    if (!visualizationProvider.IsMusicChangeAnimationActive)
                    {
                        PlaySource();
                    }
                    else
                    {
                        playerState = MediaPlayerState.WaitForVisualizersAndPlay;
                    }
                    break;
                
                case MediaPlayerState.Paused:
                case MediaPlayerState.FadeOutToPause:
                case MediaPlayerState.FadeOutToStop:
                case MediaPlayerState.Stopped:
                    StopSource();
                    break;
            }
        }

        private void PlaySource()
        {
            switch (sourceState)
            {
                case AudioSourceState.Finished: 
                case AudioSourceState.Stopped:
                case AudioSourceState.Paused: 
                    vlcMediaPlayer.Play();
                    break;
            }
            
            sourceState = AudioSourceState.Playing;
        }

        private void PauseSource()
        {
            switch (sourceState)
            {
                case AudioSourceState.Playing:
                    vlcMediaPlayer.Pause();
                    sourceState = AudioSourceState.Paused;
                    break;
            }
        }

        private void StopSource()
        {
            switch (sourceState)
            {
                case AudioSourceState.Paused:
                case AudioSourceState.Playing:
                    vlcMediaPlayer.Stop();
                    sourceState = AudioSourceState.Stopped;
                    break;
            }
        }

        // Expected audio source state
        private enum AudioSourceState
        {
            Stopped,
            Playing,
            Paused,
            Finished
        }
        
        // Music player logic state
        private enum MediaPlayerState
        {
            Stopped,
            FadeOutToPause,
            FadeOutToStop,
            FadeIn,
            Playing,
            Paused,
            WaitForVisualizersAndPlay
        }
    }
}