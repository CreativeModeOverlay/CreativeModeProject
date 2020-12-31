using System;

namespace CreativeMode
{
    public struct MediaInfo
    {
        // Url which can be used to reconstruct MediaInfo object from IMediaProvider
        public string sourceUrl;

        public string thumbnailUrl;
        public string streamUrl;
        public string audioStreamUrl;

        public string artist;
        public string album;
        public string title;
        public string year;

        public TimeSpan duration;
        public string source;
    }
}