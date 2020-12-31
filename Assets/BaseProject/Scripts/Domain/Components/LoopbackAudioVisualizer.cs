using CSCore;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using UnityEngine;

namespace CreativeMode
{
    public class LoopbackAudioVisualizer : MonoBehaviour
    {
        private WasapiLoopbackCapture capture;
        private SoundInSource inSource;
        private ISampleSource inSampleSource;
        private FftProvider fftProvider;
        private NotificationSource notificationStream;

        private void Awake()
        {
            capture = new WasapiLoopbackCapture();
            capture.Initialize();
            
            inSource = new SoundInSource(capture);
            inSampleSource = inSource.ToSampleSource();

            notificationStream = new NotificationSource(inSampleSource)
            {
                Interval = 10
            };

            fftProvider = new FftProvider(capture.WaveFormat.Channels, FftSize.Fft8192)
            {
                WindowFunction = WindowFunctions.HammingPeriodic
            };

            capture.Start();
            
            notificationStream.BlockRead += (sender, args) => { fftProvider.Add(args.Data, args.Length); };
            capture.DataAvailable += (sender, args) => { notificationStream.Read(new float[8192], 0, 8192); };
        }

        private void Update()
        {
            float[] fftBuffer = new float[8192];
            
            fftProvider.GetFftData(fftBuffer);

            for (var i = 0; i < fftBuffer.Length; i += 2)
            {
                var z = i / 50000f;
                Debug.DrawLine(new Vector3(0, 0, z), new Vector3(0, fftBuffer[i], z));
            }
        }

        private void OnDestroy()
        {
            capture?.Dispose();
            inSource?.Dispose();
            inSampleSource?.Dispose();
            notificationStream?.Dispose();
        }
    }
}