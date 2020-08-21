using System;
using UniRx;
using UnityEngine;

namespace CreativeMode.Impl
{
    [DefaultExecutionOrder(-100)]
    public class DesktopFocusManager : MonoBehaviour
    {
        public IObservable<bool> FocusState => focusStateBehaviour;

        private IDesktopCaptureManager CaptureManager => Instance<IDesktopCaptureManager>.Get();

        [Header("Desktop capture params")] public FocusParams fullscreenParams;
        public FocusParams focusedParams;
        public Vector2 focusWindowMaxSize;

        public BaseFocusableImageWidget desktopImage;

        public WindowInfo FocusedWindow { get; private set; }
        public bool IsFocused { get; private set; }
        public bool PauseFocusUpdates { get; set; }

        private float focusAmount;
        private Rect visibleRegion;
        private bool disableFocusUpdate;
        private CompositeDisposable captureDisposable;

        private BehaviorSubject<bool> focusStateBehaviour
            = new BehaviorSubject<bool>(true);

        private void OnEnable()
        {
            captureDisposable = new CompositeDisposable();

            CaptureManager.FocusedWindow
                .Subscribe(w =>
                {
                    FocusedWindow = w;
                    IsFocused = CanFocusWindow(w);
                })
                .AddTo(this)
                .AddTo(captureDisposable);
        }

        private void OnDisable()
        {
            captureDisposable.Dispose();
        }

        public void Update()
        {
            if (PauseFocusUpdates)
                return;
            
            if (focusStateBehaviour.Value != IsFocused)
            {
                focusStateBehaviour.OnNext(IsFocused);

                if (!IsFocused)
                    desktopImage.Focus = fullscreenParams;
            }

            if (IsFocused)
            {
                focusedParams.focusCenter = FocusedWindow.programRect.center;
                desktopImage.Focus = focusedParams;
            }
            else
            {
                desktopImage.Focus = fullscreenParams;
            }
        }

        private bool CanFocusWindow(WindowInfo window)
        {
            var screenRect = GetScreenRect();

            return window.valid &&
                   window.programRect.width < focusWindowMaxSize.x &&
                   window.programRect.height < focusWindowMaxSize.y &&
                   screenRect.Contains(window.programRect.center);
        }

        private Rect GetScreenRect()
        {
            return new Rect(0, 0, Screen.width, Screen.height);
        }
    }
}