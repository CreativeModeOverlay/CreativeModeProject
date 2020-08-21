﻿using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using UnityRawInput;

namespace CreativeMode
{
    public class ExampleApplicationController : MonoBehaviour
    {
        public CameraOverlayScene idleScene;
        public CameraOverlayScene mainScene;
        public CameraOverlayScene musicScene;
        public AudioMixerSnapshot mainMixerSnapshot;
        public AudioMixerSnapshot musicBrbMixerSnapshot;

        public ShaderBlendTransition transitionCrossfade;
        
        private IMusicPlayer MusicPlayer => Instance<IMusicPlayer>.Get();
        private IMusicPlaylistProvider MusicPlaylist => Instance<IMusicPlaylistProvider>.Get();
        private IInputManager InputManager => Instance<IInputManager>.Get();
        private IOverlayManager OverlayManager => Instance<IOverlayManager>.Get();
        private IChatInteractor ChatInteractor => Instance<IChatInteractor>.Get();
        
        private IOverlayElement currentScene;

        private void Awake()
        {
            SubscribeHotkeys();
        }

        private void Start()
        {
            OnShowIdleScreen();
        }

        private void SubscribeHotkeys()
        {
            InputManager.OnHotkey(RawKey.F1)
                .Subscribe(_ => { OnShowMainScreen(); });
            
            InputManager.OnHotkey(RawKey.F2)
                .Subscribe(_ => { OnShowMusicIntermission(); });
            
            InputManager.OnHotkey(RawKey.F3)
                .Subscribe(_ => { OnShowIdleScreen(); });

            InputManager.OnHotkey(RawKey.F4)
                .Subscribe(_ => { OnClearChat(); });
            
            InputManager.OnHotkey(RawKey.MediaPlayPause)
                .Subscribe(_ => { OnMediaPlayPause(); });
            
            InputManager.OnHotkey(RawKey.MediaNextTrack)
                .Subscribe(_ => { OnMediaNextTrack(); });
            
            InputManager.OnHotkey(RawKey.MediaPrevTrack)
                .Subscribe(_ => { OnMediaPreviousTrack(); });
        }

        private void OnClearChat()
        {
            ChatInteractor.ClearChatMessages();
        }

        private void OnShowIdleScreen()
        {
            if (currentScene == idleScene)
                return;

            transitionCrossfade.duration = 2f;
            OverlayManager.Show(idleScene, transitionCrossfade);
            currentScene = idleScene;
        }

        private void OnShowMainScreen()
        {
            if (currentScene == mainScene)
                return;

            OverlayManager.Show(mainScene, transitionCrossfade);
            SetMusicPlaylist(@"D:\Path\To\Your\Normal\Music");
            mainMixerSnapshot.TransitionTo(2f);
            
            currentScene = mainScene;
        }

        private void OnShowMusicIntermission()
        {
            if (currentScene == musicScene)
                return;

            OverlayManager.Show(musicScene, transitionCrossfade);
            
            SetMusicPlaylist(@"D:\Path\To\Your\BRB\Music");
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

        private void SetMusicPlaylist(string path)
        {
            MusicPlaylist.ClearPlaylist();
            MusicPlaylist.AddToPlaylist(Directory.GetFiles(path)
                .Where(f => f.EndsWith("mp3"))
                .ToArray());
            MusicPlaylist.Shuffle = true;
            MusicPlaylist.SkipRepeats = true;
            MusicPlaylist.ResetQueueToPlaylist();
        }
    }
}