using System;
using System.Collections.Generic;
using CreativeMode.Impl;
using UniRx;
using UnityEngine;

namespace CreativeMode
{
    public class DeviceCaptureManager : MonoBehaviour, IDeviceCaptureManager
    {
        public int defaultWidth;
        public int defaultHeight;
        public int defaultRefreshRate;
        public bool pauseInactive;
        
        public bool useMipMaps;
        public float mipMapBias = -1f;
        
        private IDeviceCaptureStorage Storage => Instance<IDeviceCaptureStorage>.Get();
        
        private ItemWatcher<string> captureDeviceWatcher = new ItemWatcher<string>();
        
        private List<CaptureInfo> activeCaptures = new List<CaptureInfo>();
        private Dictionary<string, IObservable<DeviceCapture>> activeCaptureTextures 
            = new Dictionary<string, IObservable<DeviceCapture>>();

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
            
            return new DeviceCaptureParams { resolution = resolution};
        }

        public IObservable<DeviceCapture> CaptureDevice(string deviceName)
        {
            if (activeCaptureTextures.TryGetValue(deviceName, out var existingCapture))
                return existingCapture;
            
            CaptureInfo capture = new CaptureInfo();
            
            DeviceCapture StartOrUpdateCapture(DeviceCaptureParams captureParams)
            {
                var currentRes = capture.captureParams.resolution;
                var newRes = captureParams.resolution;
                
                if (!capture.webCam 
                    || currentRes.width != newRes.width 
                    || currentRes.height != newRes.height 
                    || currentRes.refreshRate != newRes.refreshRate)
                {
                    Destroy(capture.webCam);
                    Destroy(capture.mipMapTexture);
                    
                    capture.webCam = new WebCamTexture(deviceName, 
                        newRes.width, newRes.height, newRes.refreshRate);
                    capture.webCam.Play();

                    if (useMipMaps)
                    {
                        capture.mipMapTexture = new RenderTexture(newRes.width, newRes.height, 0, 
                            RenderTextureFormat.Default, 8)
                        {
                            filterMode = FilterMode.Trilinear, 
                            anisoLevel = 16,
                            useMipMap = true,
                            mipMapBias = mipMapBias
                        };
                        capture.mipMapTexture.Create();
                        capture.outputTexture = capture.mipMapTexture;
                    }
                    else
                    {
                        capture.outputTexture = capture.webCam;
                    }
                }
                
                return new DeviceCapture
                {
                    texture = capture.outputTexture,
                    width = newRes.width,
                    height = newRes.height
                };
            }

            void StopCapture()
            {
                if (pauseInactive)
                {
                    capture.webCam.Pause();
                }
                else
                {
                    capture.webCam.Stop();
                    Destroy(capture.webCam);
                }
                
                Destroy(capture.mipMapTexture);
            }
            
            var textureCapture = captureDeviceWatcher.EveryUpdate()
                .Select(_ => GetCaptureParams(deviceName))
                .Select(StartOrUpdateCapture)
                .DistinctUntilChanged()
                .DoOnSubscribe(() =>
                {
                    activeCaptures.Add(capture);
                })
                .Finally(() =>
                {
                    StopCapture();
                    activeCaptures.Remove(capture);
                })
                .Share();
            
            activeCaptureTextures[deviceName] = textureCapture;

            return textureCapture;
        }

        private void Update()
        {
            if (useMipMaps)
            {
                for (var i = 0; i < activeCaptures.Count; i++)
                {
                    var info = activeCaptures[i];

                    if (info.webCam.didUpdateThisFrame)
                        Graphics.Blit(info.webCam, info.mipMapTexture);
                }
            }
        }

        private class CaptureInfo
        {
            public DeviceCaptureParams captureParams;
            public WebCamTexture webCam;
            public RenderTexture mipMapTexture;
            public Texture outputTexture;
        }
    }
}