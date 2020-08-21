using System.IO;
using CreativeMode.Impl;
using SQLite;
using UniRx;
using UnityEngine;
using UnityRawInput;

namespace CreativeMode
{
    [DefaultExecutionOrder(-100000)]
    public class CreativeModeApp : MonoBehaviour
    {
        public int framerate = 60;

        [SerializeField] public string twitchOauth;
        [SerializeField] public string twitchUsername;
        [SerializeField] public string twitchChannelToJoin;

        private void Awake()
        {
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
            var notesDb = OpenDb("Notes");
            var chatDb = OpenDb("Chat");

            Instance<IMusicPlayerStorage>.Bind().Instance(new MusicPlayerStorage(musicPlayerDb));
            Instance<INoteStorage>.Bind().Instance(new NoteStorage(notesDb));
            Instance<IChatStorage>.Bind().Instance(new ChatStorage(chatDb));
            
            Instance<IMusicVisualizer>.Bind().UnityObject<MusicVisualizer>();
            Instance<IMusicPlaylistProvider>.Bind().UnityObject<MusicPlaylistProvider>();
            Instance<IMusicPlayer>.Bind().UnityObject<MusicPlayer>();
            
            Instance<IDesktopCaptureManager>.Bind().UnityObject<DesktopCaptureManager>();
            
            Instance<IOverlayManager>.Bind().UnityObject<OverlayManager>();
            Instance<IInputManager>.Bind().Instance(new InputManager());

            Instance<ImageLoader>.Bind().Instance(new ImageLoader { MaxThreadCount = 4 });
            Instance<AudioLoader>.Bind().Instance(new AudioLoader { MaxThreadCount = 2 });

            Instance<IChatClient>.Bind().Instance(new TwitchClient(twitchOauth, twitchUsername, twitchChannelToJoin));
            Instance<IChatInteractor>.Bind()
                .Instance(new ChatInteractor(
                    new IconAtlas(Instance<ImageLoader>.Get(), 2048, 2048, 64, 64)));
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

            return new SQLiteConnection(usedDbPath).AddTo(this);
        }
    }
}