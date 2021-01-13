using System.Linq;
using CreativeMode.Impl;
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

        private void Awake()
        {
            MainThreadDispatcher.Initialize();

            Application.targetFrameRate = framerate;
            WindowsUtils.DisableWindowGhosting();
            SetupInstances();
        }

        private void OnDestroy()
        {
            RawKeyInput.Stop();
        }

        private void SetupInstances()
        {
            CommonUtilsModule.Init();
            MediaPlayerModule.Init();
            VideoCaptureModule.Init();
            OverlaySceneModule.Init();
            InputModule.Init();
            ChatModule.Init();
            OverlayWidgetsModule.Init();
            
            Instance<IDesktopUIManager>.BindUnityObject<DesktopUIManager>();
            Instance<LayoutInflater>.BindUnityObject<LayoutInflater>();

            Instance<IMediaProvider>.Get().GetMediaByUrl(@"E:\Music\Stream\VGM")
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(m =>
                {
                    Instance<IMediaPlaylistProvider>.Get().ResetQueueToPlaylist(0);
                    Instance<IMediaPlaylistProvider>.Get()
                        .AddToQueue(0, CollectionUtils.Shuffle(m.Select(i => (MediaMetadata) i)));
                });
        }
    }
}