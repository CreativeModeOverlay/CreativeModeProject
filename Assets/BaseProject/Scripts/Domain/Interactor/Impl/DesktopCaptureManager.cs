using System;
using System.Collections.Generic;
using uDesktopDuplication;
using UniRx;
using UnityEngine;

namespace CreativeMode.Impl
{
    public class DesktopCaptureManager : MonoBehaviour, IDesktopCaptureManager
    {
        private ReplaySubject<Unit> everyInit = new ReplaySubject<Unit>(1);
        
        private List<int> activeMonitors = new List<int>();
        private Dictionary<int, IObservable<MonitorInfo>> monitorObservables 
            = new Dictionary<int, IObservable<MonitorInfo>>();

        private IObservable<WindowInfo> focusedWindowShare = Observable.EveryUpdate()
            .Select(_ => WindowsUtils.GetFocusedWindow())
            .SubscribeOn(Scheduler.ThreadPool)
            .ObserveOn(Scheduler.MainThread)
            .Share();

        public IObservable<WindowInfo> FocusedWindow => focusedWindowShare;

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
    }
}