using System;
using System.Collections.Generic;
using uDesktopDuplication;
using UniRx;
using UnityEngine;

namespace CreativeMode.Impl
{
    [DefaultExecutionOrder(-100)]
    public class DesktopCaptureManager : MonoBehaviour, IDesktopCaptureManager
    {
        public Vector2 maxFocusedWindowSize;
        public FocusParams windowFocusParams;
        public FocusParams zoomFocusParams;

        public IObservable<WindowInfo> ActiveWindow { get; private set; }
        public IObservable<FocusInfo> FocusPoint { get; private set; }
        
        public bool IsZoomActive { get; set; }
        public float ZoomAmount { get; set; }

        public IObservable<ICensorRegion> OnCensorRegionAdded => onCensorRegionAddedSubject;
        public IObservable<ICensorRegion> OnCensorRegionRemoved => onCensorRegionRemovedSubject;

        private readonly ReplaySubject<Unit> everyInit = new ReplaySubject<Unit>(1);
        private readonly List<int> activeMonitors = new List<int>();
        private readonly Dictionary<int, IObservable<MonitorInfo>> monitorObservables 
            = new Dictionary<int, IObservable<MonitorInfo>>();

        private Subject<ICensorRegion> onCensorRegionAddedSubject = new Subject<ICensorRegion>();
        private Subject<ICensorRegion> onCensorRegionRemovedSubject = new Subject<ICensorRegion>();
        private List<ICensorRegion> activeCensorRegions = new List<ICensorRegion>();

        private CensorRegion windowCensorRegion = new CensorRegion();
        private bool isWindowCensoringActive;

        private void Awake()
        {
            ZoomAmount = zoomFocusParams.zoom;
            
            ActiveWindow = Observable.EveryUpdate()
                .Select(_ => WindowsUtils.GetFocusedWindow())
                .SubscribeOn(Scheduler.ThreadPool)
                .ObserveOn(Scheduler.MainThread)
                .Share();

            FocusPoint = ActiveWindow.Select(w =>
            {
                if (IsZoomActive)
                {
                    var zoomFocus = zoomFocusParams;
                    zoomFocus.zoom = ZoomAmount;
                    zoomFocus.focusCenter = GetCursorPosition();

                    return new FocusInfo
                    {
                        isFocused = true,
                        focusParams = zoomFocus,
                        focusRegion = GetScreenRect(),
                        focusMonitorIndex = GetMonitorIndex(zoomFocus.focusCenter),
                    };
                }
                
                if (CanFocusOnWindow(w))
                {
                    var windowFocus = windowFocusParams;
                    windowFocus.focusCenter = w.programRect.center;

                    return new FocusInfo
                    {
                        isFocused = true,
                        focusRegion = w.programRect,
                        focusParams = windowFocus,
                        focusMonitorIndex = GetMonitorIndex(windowFocus.focusCenter),
                    };
                }

                return default;
            }).Share();
            
            onCensorRegionAddedSubject.Subscribe(r => activeCensorRegions.Add(r));
            onCensorRegionRemovedSubject.Subscribe(r => activeCensorRegions.Remove(r));
        }

        private void OnEnable()
        {
            Manager.onReinitialized += OnInitialize;
        }

        private void OnDisable()
        {
            Manager.onReinitialized -= OnInitialize;
        }

        private void Start()
        {
            OnInitialize();
        }

        private void Update()
        {
            Capture();
        }
        
        public IObservable<MonitorInfo> CaptureMonitor(int monitorIndex)
        {
            if (monitorObservables.TryGetValue(monitorIndex, out var existingSubscription))
                return existingSubscription;

            var subscription = everyInit.Select(_ =>
            {
                var monitor = Manager.GetMonitor(monitorIndex);
                monitor.CreateTextureIfNeeded();
                monitor.texture.wrapMode = TextureWrapMode.Clamp;

                return new MonitorInfo
                {
                    texture = monitor.texture,
                    width = monitor.width,
                    height = monitor.height
                };
            }).Finally(() => {
                activeMonitors.Remove(monitorIndex);
                monitorObservables.Remove(monitorIndex);
            }).Share();

            activeMonitors.Add(monitorIndex);
            monitorObservables[monitorIndex] = subscription;

            return subscription;
        }

        public IObservable<Rect> GetMonitorSize(int index)
        {
            return everyInit.Select(m =>
            {
                var monitor = Manager.GetMonitor(index);
                return new Rect(0, 0, monitor.width, monitor.height);
            });
        }

        public ICensorRegion[] GetActiveCensorRegions()
        {
            return activeCensorRegions.ToArray();
        }

        private void OnInitialize()
        {
            everyInit.OnNext(Unit.Default);
        }

        private void Capture()
        {
            for (var i = 0; i < activeMonitors.Count; i++)
            {
                Manager.GetMonitor(activeMonitors[i]).Render();
            }
        }

        private int GetMonitorIndex(Vector2 point)
        {
            return 0; // TODO: actual monitor index?
        }
        
        private bool CanFocusOnWindow(WindowInfo window)
        {
            var screenRect = GetScreenRect();

            return window.valid &&
                   window.programRect.width < maxFocusedWindowSize.x &&
                   window.programRect.height < maxFocusedWindowSize.y &&
                   screenRect.Contains(window.programRect.center);
        }

        private Vector2 GetCursorPosition()
        {
            return Input.mousePosition;
        }

        private Rect GetScreenRect()
        {
            return new Rect(0, 0, Screen.width, Screen.height);
        }
        
        private class CensorRegion : ICensorRegion
        {
            public string Title { get; set; }
            public Rect Rect { get; set; }
        }
    }
}