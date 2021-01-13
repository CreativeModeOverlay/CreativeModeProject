using System;
using System.Threading;
using CSCore;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.Utils;
using UnityEngine;

namespace CreativeMode.Impl
{
    public class WasapiWaveformProvider : IAudioWaveformProvider
    {
        private readonly WasapiCapture capture;
        private readonly SoundInSource inSource;
        private readonly ISampleSource inSampleSource;

        private readonly float[] readSampleBuffer;
        private readonly float[] readSampleSplitBuffer;
        private readonly FftProvider fftLeftProvider;
        private readonly FftProvider fftRightProvider;
        private readonly LastSamplesBuffer leftSampleBuffer;
        private readonly LastSamplesBuffer rightSampleBuffer;
        private readonly Complex[] fftLeftComplexBuffer;
        private readonly Complex[] fftRightComplexBuffer;

        private readonly float[] leftWaveformBuffer;
        private readonly float[] rightWaveformBuffer;
        private readonly float[] leftFftBuffer;
        private readonly float[] rightFftBuffer;
        private float currentTime;
        private float lastHeardSampleTime;

        public WaveformSource Source { get; }
        public bool IsSilent { get; private set; }

        private Thread audioCaptureThread;
        private bool stopThread;

        public WasapiWaveformProvider(int waveformBufferSize, int fftBufferSize, WindowFunction function, 
            WasapiCapture wasapiCapture, WaveformSource sourceType)
        {
            Source = sourceType;
            
            readSampleBuffer = new float[8192];
            readSampleSplitBuffer = new float[4096];
            
            fftLeftComplexBuffer = new Complex[fftBufferSize];
            fftRightComplexBuffer = new Complex[fftBufferSize];
            leftSampleBuffer = new LastSamplesBuffer(waveformBufferSize);
            rightSampleBuffer = new LastSamplesBuffer(waveformBufferSize);

            leftWaveformBuffer = new float[waveformBufferSize];
            rightWaveformBuffer = new float[waveformBufferSize];
            leftFftBuffer = new float[fftBufferSize];
            rightFftBuffer = new float[fftBufferSize];

            fftLeftProvider = new FftProvider(1, (FftSize) fftBufferSize) { WindowFunction = function };
            fftRightProvider = new FftProvider(1, (FftSize) fftBufferSize) { WindowFunction = function };

            capture = wasapiCapture;
            capture.Initialize();
            
            inSource = new SoundInSource(capture);
            inSampleSource = inSource.ToSampleSource();

            StartCapture();
        }

        private void CaptureLoop()
        {
            while (!stopThread)
            {
                var size = inSampleSource.Read(readSampleBuffer, 0, readSampleBuffer.Length);
                var splitSize = BufferUtils.ExtractChannel(readSampleBuffer, readSampleSplitBuffer, 
                    size, 0, 2);

                for (var i = 0; i < size; i++)
                {
                    if (readSampleBuffer[i] != 0)
                    {
                        lastHeardSampleTime = currentTime;
                        break;
                    }
                }

                IsSilent = currentTime - lastHeardSampleTime > 1f;

                if(IsSilent)
                    continue;
                
                leftSampleBuffer.Write(readSampleSplitBuffer, 0, splitSize);
                fftLeftProvider.Add(readSampleSplitBuffer, splitSize);
                fftLeftProvider.GetFftData(fftLeftComplexBuffer);

                splitSize = BufferUtils.ExtractChannel(readSampleBuffer, readSampleSplitBuffer, 
                    size, 1, 2);

                rightSampleBuffer.Write(readSampleSplitBuffer, 0, splitSize);
                fftRightProvider.Add(readSampleSplitBuffer, splitSize);
                fftRightProvider.GetFftData(fftRightComplexBuffer);

                lock (this)
                {
                    leftSampleBuffer.Read(leftWaveformBuffer, 0, leftWaveformBuffer.Length);
                    rightSampleBuffer.Read(rightWaveformBuffer, 0, rightWaveformBuffer.Length);

                    for (var i = 0; i < leftFftBuffer.Length; i++)
                        leftFftBuffer[i] = (float) fftLeftComplexBuffer[i].Value;

                    for (var i = 0; i < rightFftBuffer.Length; i++)
                        rightFftBuffer[i] = (float) fftRightComplexBuffer[i].Value;
                }
            }
            
            StartCapture();
        }

        public void StartCapture()
        {
            capture.Start();
            stopThread = false;
            audioCaptureThread = new Thread(CaptureLoop);
            audioCaptureThread.Start();
        }

        public void StopCapture()
        {
            stopThread = true;
            capture.Stop();
        }
        
        public void Update()
        {
            currentTime = Time.time;
        }

        public void GetWaveform(float[] buffer, int channel)
        {
            lock (this)
            {
                Array.Copy(channel == 0 ? leftWaveformBuffer : rightWaveformBuffer, buffer, buffer.Length);
            }
        }

        public void GetSpectrum(float[] buffer, int channel)
        {
            lock (this)
            {
                Array.Copy(channel == 0 ? leftFftBuffer : rightFftBuffer, buffer, buffer.Length);
            }
        }

        public void Dispose()
        {
            StopCapture(); 
            
            capture?.Dispose();
            inSampleSource?.Dispose();
            inSource?.Dispose();
        }
    }
}