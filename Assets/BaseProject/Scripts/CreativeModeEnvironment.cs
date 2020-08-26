﻿using System.IO;
using CreativeMode.Impl;
using SQLite;
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
            var notesDb = OpenDb("Notes");
            var chatDb = OpenDb("Chat");

            Instance<IMusicPlayerStorage>.Bind().Instance(new MusicPlayerStorage(musicPlayerDb));
            Instance<INoteStorage>.Bind().Instance(new NoteStorage(notesDb));
            Instance<IChatStorage>.Bind().Instance(new ChatStorage(chatDb));
            
            Instance<IMusicVisualizationProvider>.Bind().UnityObject<MusicVisualizationProvider>();
            Instance<IMusicPlaylistProvider>.Bind().UnityObject<MusicPlaylistProvider>();
            Instance<IMusicPlayer>.Bind().UnityObject<MusicPlayer>();
            
            Instance<IDesktopCaptureManager>.Bind().UnityObject<DesktopCaptureManager>();
            
            Instance<IOverlayManager>.Bind().UnityObject<OverlaySceneManager>();
            Instance<IInputManager>.Bind().Instance(new InputManager());

            Instance<ImageLoader>.Bind().Instance(new ImageLoader { MaxThreadCount = 4 });
            Instance<AudioLoader>.Bind().Instance(new AudioLoader { MaxThreadCount = 2 });

            Instance<IChatClient>.Bind().Instance(new TwitchClient(twitchOauth, twitchUsername, twitchChannelToJoin));

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

            return new SQLiteConnection(usedDbPath).AddTo(this);
        }
    }
}