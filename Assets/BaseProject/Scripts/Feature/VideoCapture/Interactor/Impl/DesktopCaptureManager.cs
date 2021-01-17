using System;
using System.Collections.Generic;
using uDesktopDuplication;
using UniRx;
using UnityEngine;

namespace CreativeMode.Impl
{
    [DefaultExecutionOrder(-100)]
    internal class DesktopCaptureManager : MonoBehaviour, IDesktopCaptureManager
    {
        public Vector2 maxFocusedWindowSize;
        public FocusParams windowFocusParams;
        public FocusParams zoomFocusParams;

        public IObservable<WindowInfo> ActiveWindow { get; private set; }
        public IObservable<FocusInfo> FocusPoint { get; private set; }
        public IObservable<ICensorRegion[]> CensorRegions => censorRegionsSubject;

        public bool IsZoomActive { get; set; }
        public float ZoomAmount { get; set; }

        private readonly ReplaySubject<Unit> everyInit = new ReplaySubject<Unit>(1);
        private readonly List<int> activeMonitors = new List<int>();
        private readonly Dictionary<int, IObservable<CapturedVideo>> monitorObservables 
            = new Dictionary<int, IObservable<CapturedVideo>>();

        private BehaviorSubject<ICensorRegion[]> censorRegionsSubject = 
            new BehaviorSubject<ICensorRegion[]>(new ICensorRegion[0]);
        
        private List<ICensorRegion> currentCensorRegions = new List<ICensorRegion>();

        private void Start()
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
        }

        private void OnEnable()
        {
            Manager.onReinitialized += OnInitialize;
        }

        private void OnDisable()
        {
            Manager.onReinitialized -= OnInitialize;
        }
        
        private void Update()
        {
            Capture();
        }
        
        public IObservable<CapturedVideo> CaptureMonitor(int monitorIndex)
        {
            if (monitorObservables.TryGetValue(monitorIndex, out var existingSubscription))
                return existingSubscription;

            var subscription = everyInit.Select(_ =>
            {
                var monitor = Manager.GetMonitor(monitorIndex);
                monitor.CreateTextureIfNeeded();
                monitor.texture.wrapMode = TextureWrapMode.Clamp;

                return new CapturedVideo
                {
                    texture = monitor.texture,
                    width = monitor.width,
                    height = monitor.height
                };
            }).Finally(() => {
                activeMonitors.Remove(monitorIndex);
                monitorObservables.Remove(monitorIndex);
            }).Replay(1).RefCount();

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

        public ICensorRegionController CreateCensorRegion()
        {
            var region = new CensorRegion(this);
            currentCensorRegions.Add(region);
            censorRegionsSubject.OnNext(currentCensorRegions.ToArray());
            return region;
        }

        private void RemoveCensorRegion(ICensorRegion controller)
        {
            currentCensorRegions.Remove(controller);
            censorRegionsSubject.OnNext(currentCensorRegions.ToArray());
        }

        private void OnInitialize()
        {
            everyInit.OnNext(Unit.Default);
        }

        private void Capture()
        {
            for (var i = 0; i < activeMonitors.Count; i++)
                Manager.GetMonitor(activeMonitors[i]).Render();
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

        private class CensorRegion : ICensorRegion, ICensorRegionController
        {
            public int Monitor { get; set; }
            public string Title { get; set; }
            public Rect Rect { get; set; }

            private DesktopCaptureManager manager;
            
            public CensorRegion(DesktopCaptureManager manager)
            {
                this.manager = manager;
            }
            
            public void Remove()
            {
                manager.RemoveCensorRegion(this);
            }
        }
    }
}