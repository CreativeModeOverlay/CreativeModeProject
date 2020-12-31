using System;

namespace CreativeMode
{
    public interface IMusicPlayer
    {
        IObservable<MediaMetadata> CurrentMedia { get; }
        
        float FadeInDuration { get; set; }
        float FadeOutDuration { get; set; }
        bool IsPlaying { get; }
        bool IsBuffering { get; }
        float Pitch { get; set; }
      
        float NormalizedPosition { get; set; }
        float Position { get; set; }
        float Duration { get; }
        
        void Play();
        void Pause(bool toggle = false);
        void Stop();
        void Rewind();

        void Next();
        void Previous();
    }
}