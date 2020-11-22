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
            var musicPlayerDb = OpenDb("MusicPlayer");
            var chatDb = OpenDb("Chat");
            var widgets = OpenDb("Widgets");
            var devices = OpenDb("Devices");

            Instance<IMusicPlayerStorage>.Bind().Instance(new MusicPlayerStorage(musicPlayerDb));
            Instance<IChatStorage>.Bind().Instance(new ChatStorage(chatDb));
            Instance<IWidgetStorage>.Bind().Instance(new WidgetStorage(widgets));
            Instance<IDeviceCaptureStorage>.Bind().Instance(new DeviceCaptureStorage(devices));
            
            Instance<IMusicVisualizationProvider>.Bind().UnityObject<MusicVisualizationProvider>();
            Instance<IMusicPlaylistProvider>.Bind().UnityObject<MusicPlaylistProvider>();
            Instance<IMusicPlayer>.Bind().UnityObject<MusicPlayer>();
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
            Instance<AudioLoader>.Bind().Instance(new AudioLoader { MaxThreadCount = 2 });

            Instance<IChatClient>.Bind().Instance(new TwitchClient(
                twitchClientId, twitchAccessToken, twitchOauth, twitchUsername, twitchChannelToJoin));
            
            Instance<IChatInteractor>.Bind().Instance(new ChatInteractor(EmoteSize.Size2x));
        }

        private SQLiteConnection OpenDb(string name)
        {
            var originalDbName = name + ".sqlite";
            var originalDbPath = Path.Combine(Application.persistentDataPath, originalDbName);
            var usedDbPath = originalDbPath;
            
            // Copy original database if it exists,
            // so any changes in editor will not ruin actual application state
            if (Application.isEditor)
            {
                var editorDbName = "Editor-" + originalDbName;
                var editorDbPath = Path.Combine(Application.persistentDataPath, editorDbName);

                if (File.Exists(originalDbPath))
                {
                    if (File.Exists(editorDbPath))
                        File.Delete(editorDbPath);

                    File.Copy(originalDbPath, editorDbPath);
                }

                usedDbPath = editorDbPath;
            }
            
            var connection = new SQLiteConnection(usedDbPath);
            connection.ExecuteScalar<string> ("PRAGMA journal_mode=MEMORY"); // TODO: disable once async access to db is implemented
            connection.AddTo(this);
            
            return connection;
        }
    }
}