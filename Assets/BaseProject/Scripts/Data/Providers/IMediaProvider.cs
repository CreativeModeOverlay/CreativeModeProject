using System;
using System.Collections.Generic;

namespace CreativeMode
{
    public interface IMediaProvider
    {
        IObservable<List<MediaInfo>> GetMediaByUrl(string url, 
            int maxWidth = Int32.MaxValue, 
            int maxHeight = Int32.MaxValue,
            bool preferAudioOnly = false);

        IObservable<List<SongLyrics>> GetMediaLyrics(string url);
        
        void Prefetch(string url);
    }
}