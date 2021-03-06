﻿using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;

namespace CreativeMode
{
    public class DemoApplicationController : MonoBehaviour
    {
        private const int MusicSetMain = 0;
        private const int MusicSetBrb = 1;
        
        private IMediaPlayer MusicPlayer => Instance<IMediaPlayer>.Get();
        private IMediaPlaylistProvider MediaPlaylist => Instance<IMediaPlaylistProvider>.Get();
        private IMediaProvider MediaProvider => Instance<IMediaProvider>.Get();
        private IInputManager InputManager => Instance<IInputManager>.Get();
        private IVideoSceneManager OverlayManager => Instance<IVideoSceneManager>.Get();
        private IChatInteractor ChatInteractor => Instance<IChatInteractor>.Get();
        private IDesktopCaptureManager CaptureManager => Instance<IDesktopCaptureManager>.Get();

        public CameraRenderScene idleScene;
        public CameraRenderScene mainScene;
        public CameraRenderScene musicScene;
        public AudioMixerSnapshot mainMixerSnapshot;
        public AudioMixerSnapshot musicBrbMixerSnapshot;

        public ShaderBlendTransition transitionCrossfade;

        private IVideoElement currentScene;

        private void Start()
        {
            Instance<IMediaProvider>.Get().GetMediaByUrl(@"E:\Music\Stream\VGM")
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(m =>
                {
                    Instance<IMediaPlaylistProvider>.Get().ResetQueueToPlaylist(0);
                    Instance<IMediaPlaylistProvider>.Get()
                        .AddToQueue(0, CollectionUtils.Shuffle(m.Select(i => (MediaMetadata) i)));
                });
            
            SubscribeHotkeys();
            InitMusicPlaylist();
            OnShowMainScreen();
        }

        private void SubscribeHotkeys()
        {
            InputManager.OnHotkeyPressed(WindowsKeyCode.F1)
                .Subscribe(_ => OnShowMainScreen());
            
            InputManager.OnHotkeyPressed(WindowsKeyCode.F2)
                .Subscribe(_ => OnShowMusicIntermission());
            
            InputManager.OnHotkeyPressed(WindowsKeyCode.F3)
                .Subscribe(_ => OnShowIdleScreen());

            InputManager.OnHotkeyPressed(WindowsKeyCode.F4)
                .Subscribe(_ => OnClearChat());
            
            InputManager.OnHotkeyPressed(WindowsKeyCode.MediaPlayPause)
                .Subscribe(_ => OnMediaPlayPause());
            
            InputManager.OnHotkeyPressed(WindowsKeyCode.MediaNextTrack)
                .Subscribe(_ => OnMediaNextTrack());
            
            InputManager.OnHotkeyPressed(WindowsKeyCode.F12)
                .Subscribe(_ => OnMediaNextTrack());
            
            InputManager.OnHotkeyPressed(WindowsKeyCode.MediaPrevTrack)
                .Subscribe(_ => OnMediaPreviousTrack());

            InputManager.OnHotkey(WindowsKeyCode.LeftControl, WindowsKeyCode.LeftShift, WindowsKeyCode.Z)
                .Subscribe(z => CaptureManager.IsZoomActive = z);

            ChatInteractor.ChatEvents.Subscribe(e =>
            {
                switch (e)
                {
                    case AddMediaToQueueChatEvent mediaToAdd:
                        MediaProvider.GetMediaByUrl(mediaToAdd.mediaUrl)
                            .Subscribe(mediaList => {
                                Debug.Log($"Music added to queue: {mediaToAdd.mediaUrl}");
                                MediaPlaylist.AddToQueue(MusicSetMain, mediaList.Select(m => (MediaMetadata) m), -1);
                            });
                        break; 
                }
            });
        }

        private void OnClearChat()
        {
            ChatInteractor.ClearChatMessages();
        }

        private void OnShowIdleScreen()
        {
            if (currentScene == idleScene)
                return;

            OverlayManager.Show(idleScene);
            currentScene = idleScene;
        }

        private void OnShowMainScreen()
        {
            if (currentScene == mainScene)
                return;

            OverlayManager.Show(mainScene, transitionCrossfade);
            //SetMusicPlaylist(@"D:\Music\Stream\BRB");
            mainMixerSnapshot.TransitionTo(2f);
            
            currentScene = mainScene;
        }

        private void OnShowMusicIntermission()
        {
            if (currentScene == musicScene)
                return;

            OverlayManager.Show(musicScene, transitionCrossfade);
            
            //SetMusicPlaylist(@"D:\Path\To\Your\BRB\Music");
            musicBrbMixerSnapshot.TransitionTo(3f);
            
            currentScene = musicScene;
        }

        private void OnMediaPlayPause()
        {
            MusicPlayer.Pause(true);
        }

        private void OnMediaPreviousTrack()
        {
            if (MusicPlayer.Position > 3)
            {
                MusicPlayer.Rewind();
            }
            else
            {
                MusicPlayer.Previous();
            }
        }

        private void OnMediaNextTrack()
        {
            MusicPlayer.Next();
        }

        private void InitMusicPlaylist()
        {
            MediaPlaylist.Shuffle = true;
            MediaPlaylist.SkipRepeats = false;
            MediaPlaylist.CurrentSet = MusicSetMain;
        }
    }
}