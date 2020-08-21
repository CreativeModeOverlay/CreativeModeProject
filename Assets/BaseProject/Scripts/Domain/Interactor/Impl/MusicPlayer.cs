using System;
using DG.Tweening;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;

namespace CreativeMode.Impl
{
    public class MusicPlayer : MonoBehaviour, IMusicPlayer
    {
        private IMusicPlaylistProvider Playlist => Instance<IMusicPlaylistProvider>.Get();
        
        public MusicVisualizer visualizer;
        public IObservable<AudioMetadata> CurrentMusic => onMusicChangedSubject;

        public float FadeInDuration { get; set; } = 0.5f;
        public float FadeOutDuration { get; set; } = 0.5f;

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
            get => sourceState == AudioSourceState.Finished ? Duration : outputAudioSource.time;
            set => outputAudioSource.time = value;
        }

        public float Duration => outputAudioSource.clip 
            ? outputAudioSource.clip.length 
            : 0;
        
        public AudioSource outputAudioSource;
        
        private readonly ReplaySubject<AudioMetadata> onMusicChangedSubject = 
            new ReplaySubject<AudioMetadata>(1);

        private AudioMetadata currentMusicInfo;
        private SharedAsset<AudioAsset> currentMusic;
        
        private AudioSourceState sourceState;
        private MediaPlayerState playerState;
        private IDisposable musicLoadDisposable;
        private AudioLoader AudioLoader => Instance<AudioLoader>.Get();
        private bool musicMarkedAsPlayed;

        private void Update()
        {
            if (sourceState == AudioSourceState.Playing && !outputAudioSource.isPlaying)
            {
                sourceState = AudioSourceState.Finished;
                OnPlaybackFinished();
            }

            if (playerState == MediaPlayerState.WaitForVisualizersAndPlay && !visualizer.IsMusicChangeAnimationActive)
                Play();
        }

        public void Play()
        {
            DOTween.Kill(outputAudioSource);

            if (visualizer.IsMusicChangeAnimationActive)
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

            if (outputAudioSource.clip)
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
            OnNextPlaylistEntry(Playlist.AdvanceToNext());
        }
        
        public void Previous()
        {
            OnNextPlaylistEntry(Playlist.ReturnToPrevious());
        }

        private void OnNextPlaylistEntry([CanBeNull] string url)
        {
            if (url != null)
            {
                LoadMedia(url);
            }
            else
            {
                Stop();
            }
        }

        private void OnPlaybackFinished()
        {
            Next();
        }

        private void LoadMedia(string url)
        {
            Debug.Log($"Loading music: {url}");
            musicLoadDisposable?.Dispose();
            musicLoadDisposable = AudioLoader.GetAsset(url)
                .Subscribe(OnMediaLoaded);
        }

        private void OnMediaLoaded(SharedAsset<AudioAsset> audio)
        {
            outputAudioSource.clip = null;
            sourceState = AudioSourceState.Stopped;
            currentMusic.Dispose();
            
            var asset = audio.Asset;
            
            currentMusic = audio;
            currentMusicInfo = asset.Meta;
            musicMarkedAsPlayed = false;
            
            onMusicChangedSubject.OnNext(asset.Meta);
            outputAudioSource.clip = audio.Asset.Clip;
            sourceState = AudioSourceState.Stopped;
            DOTween.Kill(outputAudioSource);

            switch (playerState)
            {
                case MediaPlayerState.Playing:
                case MediaPlayerState.FadeIn:
                    if (!visualizer.IsMusicChangeAnimationActive)
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
            if(!outputAudioSource.clip)
                return;
            
            switch (sourceState)
            {
                case AudioSourceState.Finished: 
                case AudioSourceState.Stopped: 
                    outputAudioSource.Play();
                    break;
                
                case AudioSourceState.Paused: 
                    outputAudioSource.UnPause(); 
                    break;
            }
            
            sourceState = AudioSourceState.Playing;
        }

        private void PauseSource()
        {
            if(!outputAudioSource.clip)
                return;
            
            switch (sourceState)
            {
                case AudioSourceState.Playing:
                    outputAudioSource.Pause();
                    sourceState = AudioSourceState.Paused;
                    break;
            }
        }

        private void StopSource()
        {
            if(!outputAudioSource.clip)
                return;
            
            switch (sourceState)
            {
                case AudioSourceState.Paused:
                case AudioSourceState.Playing:
                    outputAudioSource.Stop();
                    sourceState = AudioSourceState.Stopped;
                    break;
            }
        }

        private enum AudioSourceState
        {
            Stopped,
            Playing,
            Paused,
            Finished
        }
        
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