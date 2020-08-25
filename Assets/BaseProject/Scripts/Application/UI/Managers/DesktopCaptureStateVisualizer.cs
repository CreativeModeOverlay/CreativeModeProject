using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace CreativeMode
{
    public class DesktopCaptureStateVisualizer : MonoBehaviour
    {
        private IDesktopCaptureManager CaptureManager => Instance<IDesktopCaptureManager>.Get();

        public int monitorIndex;
        public FocusRegionWidget focusRegionWidget;
        public CensorRegionWidget censorRegionWidgetPrefab;
        
        private Dictionary<ICensorRegion, CensorRegionWidget> spawnedWidgets 
            = new Dictionary<ICensorRegion, CensorRegionWidget>();

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

            foreach (var region in CaptureManager.GetActiveCensorRegions())
                SpawnCensorWidget(region);

            CaptureManager.OnCensorRegionAdded
                .Subscribe(SpawnCensorWidget)
                .AddTo(this);
            
            CaptureManager.OnCensorRegionRemoved
                .Subscribe(RemoveCensorWidget)
                .AddTo(this);
        }
        
        private void SpawnCensorWidget(ICensorRegion region)
        {
            var instance = Instantiate(censorRegionWidgetPrefab, transform);
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