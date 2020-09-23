using System;
using System.Collections.Generic;
using CreativeMode.Impl;
using UniRx;
using UnityEngine;

namespace CreativeMode
{
    public class DeviceCaptureManager : MonoBehaviour, IDeviceCaptureManager
    {
        private IDeviceCaptureStorage Storage => Instance<IDeviceCaptureStorage>.Get();
        
        public int defaultWidth;
        public int defaultHeight;
        public int defaultRefreshRate;
        public bool pauseInactive;
        
        public bool useMipMaps;
        public float mipMapBias = -1f;

        private ItemWatcher<string> captureDeviceWatcher = new ItemWatcher<string>();
        private List<CaptureInfo> activeCaptures = new List<CaptureInfo>();
        private Dictionary<string, IObservable<DeviceCapture>> activeCaptureTextures 
            = new Dictionary<string, IObservable<DeviceCapture>>();

        private void Update()
        {
            for (var i = 0; i < activeCaptures.Count; i++)
            {
                activeCaptures[i].UpdateMips();
            }
        }

        private void OnDestroy()
        {
            captureDeviceWatcher.NotifyStopAll();
            foreach (var capture in activeCaptures)
            {
                capture.Stop(false);
            }
        }

        public void SetCaptureParams(string deviceName, DeviceCaptureParams captureParams)
        {
            Storage.PutCaptureParams(new DeviceCaptureParamsDB
            {
                Id = deviceName,
                ResolutionWidth = captureParams.resolution.width,
                ResolutionHeight = captureParams.resolution.height,
                RefreshRate = captureParams.resolution.refreshRate
            });
            captureDeviceWatcher.NotifyUpdate(deviceName);
        }

        public DeviceCaptureParams GetCaptureParams(string deviceName)
        {
            var captureParams = Storage.GetCaptureParams(deviceName);
            var resolution = captureParams != null 
                ? new Resolution
                {
                    width = captureParams.ResolutionWidth,
                    height = captureParams.ResolutionHeight,
                    refreshRate = captureParams.RefreshRate
                } 
                : new Resolution
                {
                    width = defaultWidth,
                    height = defaultHeight,
                    refreshRate = defaultRefreshRate
                };
            
            return new DeviceCaptureParams { resolution = resolution };
        }

        public IObservable<DeviceCapture> CaptureDevice(string deviceName)
        {
            if (activeCaptureTextures.TryGetValue(deviceName, out var existingCapture))
                return existingCapture;

            var capture = new CaptureInfo { deviceName = deviceName };
            var observable = captureDeviceWatcher.EveryUpdate(deviceName)
                .Select(_ => capture.Update(this, GetCaptureParams(deviceName)))
                .DoOnSubscribe(() =>
                {
                    activeCaptures.Add(capture);
                })
                .Finally(() =>
                {
                    activeCaptures.Remove(capture);
                    capture.Stop(pauseInactive);
                })
                .Replay(1).RefCount();

            activeCaptureTextures[deviceName] = observable;
            return observable;
        }

        private class CaptureInfo
        {
            public string deviceName;

            public WebCamTexture webCam;
            public RenderTexture mipMapTexture;
            public Texture outputTexture;
            
            public DeviceCapture Update(DeviceCaptureManager manager, DeviceCaptureParams captureParams)
            {
                var newRes = captureParams.resolution;

                if (!webCam
                    || webCam.width != newRes.width
                    || webCam.height != newRes.height
                    || (int) webCam.requestedFPS != newRes.refreshRate)
                {
                    if (webCam)
                    {
                        webCam.Stop();
                        Destroy(webCam);
                        webCam = null;
                    }

                    webCam = new WebCamTexture(deviceName, 
                        newRes.width, newRes.height, newRes.refreshRate);
                }
   
                if (manager.useMipMaps)
                {
                    if (!mipMapTexture
                        || mipMapTexture.width != newRes.width
                        || mipMapTexture.height != newRes.height)
                    {
                        Destroy(mipMapTexture);
                        
                        mipMapTexture = new RenderTexture(newRes.width, newRes.height, 0,
                            RenderTextureFormat.Default, 8)
                        {
                            filterMode = FilterMode.Trilinear,
                            anisoLevel = 16,
                            useMipMap = true,
                            mipMapBias = manager.mipMapBias
                        };
                        mipMapTexture.Create();
                    }

                    outputTexture = mipMapTexture;
                }
                else
                {
                    outputTexture = webCam;
                }

                if (!webCam.isPlaying)
                {
                    Debug.Log($"Capture Play: {deviceName}");
                    webCam.Play();
                }

                return new DeviceCapture
                {
                    texture = outputTexture,
                    width = newRes.width,
                    height = newRes.height
                };
            }
            
            public void Stop(bool pause)
            {
                if (webCam)
                {
                    if (pause)
                    {
                        Debug.Log($"Capture Pause: {deviceName}");
                        webCam.Pause();
                    }
                    else
                    {
                        Debug.Log($"Capture Stop: {deviceName}");
                        webCam.Stop();
                    }
                }
                
                Destroy(mipMapTexture);
                mipMapTexture = null;
            }

            public void UpdateMips()
            {
                if(webCam && mipMapTexture && webCam.didUpdateThisFrame)
                    Graphics.Blit(webCam, mipMapTexture);
            }
        }
    }
}