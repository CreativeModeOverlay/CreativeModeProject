using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace CreativeMode
{
    public class CensorRegionWidgetSpawner : MonoBehaviour
    {
        public RectTransform contentRect;
        public CensorRegionWidget prefab;
        
        private Dictionary<ICensorRegion, CensorRegionWidget> spawnedWidgets 
            = new Dictionary<ICensorRegion, CensorRegionWidget>();
        
        private IDesktopCaptureManager CaptureManager => Instance<IDesktopCaptureManager>.Get();

        private void Awake()
        {
            CaptureManager.CensorRegions
                .SubscribeChanges(SpawnCensorWidget, RemoveCensorWidget)
                .AddTo(this);
        }

        private void OnDestroy()
        {
            foreach (var value in spawnedWidgets.Values)
                value.Remove();
            
            spawnedWidgets.Clear();
        }

        private void SpawnCensorWidget(ICensorRegion region)
        {
            if(spawnedWidgets.ContainsKey(region))
                return;
            
            var instance = Instantiate(prefab, contentRect);
            spawnedWidgets[region] = instance;

            instance.gameObject.SetActive(true);
            instance.Region = region;
        }

        private void RemoveCensorWidget(ICensorRegion region)
        {
            if(spawnedWidgets.TryGetValue(region, out var widget))
            {
                widget.Remove();
                spawnedWidgets.Remove(region);
            }
        }
    }
}