using System.IO;
using CreativeMode.Impl;
using SQLite;
using ThreeDISevenZeroR.XmlUI;
using UniRx;
using UnityEngine;
using UnityRawInput;

namespace CreativeMode
{
    /// <summary>
    /// Dependency injection configuration and other settings that should be applied
    /// before running original app logic
    /// </summary>
    [DefaultExecutionOrder(-100000)]
    public class CreativeModeEnvironment : MonoBehaviour
    {
        [Header("Application settings")]
        public int framerate = 60;

        [Header("Twitch settings")] 
        [SerializeField] public string twitchClientId;
        [SerializeField] public string twitchAccessToken;
        [SerializeField] public string twitchOauth;
        [SerializeField] public string twitchUsername;
        [SerializeField] public string twitchChannelToJoin;

        public string youtubeDlPath = "youtube-dl";
        
        private void Awake()
        {
            MainThreadDispatcher.Initialize();

            Application.targetFrameRate = framerate;
            WindowsUtils.DisableWindowGhosting();
            RawKeyInput.Start(true);
            SetupInstances();
        }

        private void OnDestroy()
        {
            RawKeyInput.Stop();
        }

        private void SetupInstances()
        {
            var chatDb = DatabaseUtils.OpenDb("Chat");
            var devices = DatabaseUtils.OpenDb("Devices");
            var youtubeDl = new YoutubeDL(youtubeDlPath);
            
            MediaPlayerModule.Init();
            
            Instance<IChatStorage>.Bind().Instance(new ChatStorage(chatDb));
            Instance<IDeviceCaptureStorage>.Bind().Instance(new DeviceCaptureStorage(devices));
            
            Instance<IMediaVisualizationProvider>.Bind().UnityObject<MediaVisualizationProvider>();
            Instance<IMediaPlaylistProvider>.Bind().UnityObject<MediaPlaylistProvider>();
            Instance<IMediaPlayer>.Bind().UnityObject<MediaPlayer>();
            Instance<IMediaProvider>.Bind().Instance(new MediaProvider(youtubeDl));
            Instance<IDesktopCaptureManager>.Bind().UnityObject<DesktopCaptureManager>();
            Instance<IDeviceCaptureManager>.Bind().UnityObject<DeviceCaptureManager>();
            
            Instance<IDesktopUIManager>.Bind().UnityObject<DesktopUIManager>();
            Instance<IOverlayManager>.Bind().UnityObject<OverlaySceneManager>();
            Instance<IInputManager>.Bind().Instance(new InputManager());
            Instance<IAppWidgetManager>.Bind().Instance(new AppWidgetManager());
            Instance<IAppWidgetRegistry>.Bind().UnityObject<AppWidgetRegistry>();
            Instance<IAppWidgetUIFactory>.Bind().UnityObject<AppWidgetUIFactory>();
            Instance<LayoutInflater>.Bind().UnityObject<LayoutInflater>();

            Instance<ImageLoader>.Bind().Instance(new ImageLoader { MaxThreadCount = 4 });

            Instance<IChatProvider>.Bind().Instance(new TwitchProvider(
                twitchClientId, twitchAccessToken, twitchOauth, twitchUsername, twitchChannelToJoin));
            
            Instance<IChatInteractor>.Bind().Instance(new ChatInteractor(EmoteSize.Size2x));
        }
    }
}