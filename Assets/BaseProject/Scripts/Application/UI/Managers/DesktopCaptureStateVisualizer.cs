using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace CreativeMode
{
    /// <summary>
    /// 
    /// </summary>
    public class DesktopCaptureStateVisualizer : MonoBehaviour
    {
        private IDesktopCaptureManager CaptureManager => Instance<IDesktopCaptureManager>.Get();

        public int monitorIndex;
        public FocusRegionWidget focusRegionWidget;
        public OverlayCensorRegionWidget overlayCensorRegionWidgetPrefab;
        
        private Dictionary<ICensorRegion, OverlayCensorRegionWidget> spawnedWidgets 
            = new Dictionary<ICensorRegion, OverlayCensorRegionWidget>();

        private void Awake()
        {
            CaptureManager.GetMonitorSize(monitorIndex)
                .Subscribe(r => focusRegionWidget.WindowRect = r)
                .AddTo(this);
            
            CaptureManager.FocusPoint
                .Subscribe(f =>
                {
                    focusRegionWidget.FocusRegion = f.focusRegion;
                    focusRegionWidget.IsVisible = f.isFocused && f.focusMonitorIndex == monitorIndex;
                })
                .AddTo(this);

            CaptureManager.CensorRegions
                .SubscribeChanges(SpawnCensorWidget, RemoveCensorWidget)
                .AddTo(this);
        }
        
        private void SpawnCensorWidget(ICensorRegion region)
        {
            var instance = Instantiate(overlayCensorRegionWidgetPrefab, transform);
            spawnedWidgets[region] = instance;
            
            instance.gameObject.SetActive(true);
            instance.Region = region;
        }

        private void RemoveCensorWidget(ICensorRegion region)
        {
            spawnedWidgets[region].Remove();
            spawnedWidgets.Remove(region);
        }
    }
}