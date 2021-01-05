using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using UnityRawInput;

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
        private IOverlayManager OverlayManager => Instance<IOverlayManager>.Get();
        private IChatInteractor ChatInteractor => Instance<IChatInteractor>.Get();
        private IDesktopCaptureManager CaptureManager => Instance<IDesktopCaptureManager>.Get();
        private IAppWidgetManager WidgetManager => Instance<IAppWidgetManager>.Get();
        
        public CameraOverlayScene idleScene;
        public CameraOverlayScene mainScene;
        public CameraOverlayScene musicScene;
        public AudioMixerSnapshot mainMixerSnapshot;
        public AudioMixerSnapshot musicBrbMixerSnapshot;

        public ShaderBlendTransition transitionCrossfade;

        private IOverlayElement currentScene;

        private void Awake()
        {
            SubscribeHotkeys();
            InitMusicPlaylist();
            
            /*var panel = WidgetManager.GetPanel("top");
            panel.widgets.Clear();
            panel.AddWidget(WidgetManager.CreateWidget(typeof(MusicPlayerAppWidget)));
            panel.AddWidget(WidgetManager.CreateWidget(typeof(MusicSpectrumAppWidget)));
            panel.AddWidget(WidgetManager.CreateWidget(typeof(MusicWaveformAppWidget)));
            WidgetManager.UpdatePanel(panel);*/
        }

        private void Start()
        {
            OnShowMainScreen();
        }

        private void SubscribeHotkeys()
        {
            InputManager.OnHotkeyPressed(RawKey.F1)
                .Subscribe(_ => { OnShowMainScreen(); });
            
            InputManager.OnHotkeyPressed(RawKey.F2)
                .Subscribe(_ => { OnShowMusicIntermission(); });
            
            InputManager.OnHotkeyPressed(RawKey.F3)
                .Subscribe(_ => { OnShowIdleScreen(); });

            InputManager.OnHotkeyPressed(RawKey.F4)
                .Subscribe(_ => { OnClearChat(); });
            
            InputManager.OnHotkeyPressed(RawKey.MediaPlayPause)
                .Subscribe(_ => { OnMediaPlayPause(); });
            
            InputManager.OnHotkeyPressed(RawKey.MediaNextTrack)
                .Subscribe(_ => { OnMediaNextTrack(); });
            
            InputManager.OnHotkeyPressed(RawKey.F12)
                .Subscribe(_ => { OnMediaNextTrack(); });
            
            InputManager.OnHotkeyPressed(RawKey.MediaPrevTrack)
                .Subscribe(_ => { OnMediaPreviousTrack(); });

            InputManager.OnHotkey(RawKey.LeftControl, RawKey.LeftShift, RawKey.Z)
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