using System;
using System.Linq;

namespace CreativeMode
{
    public class MediaMetadata
    {
        public string url;
        public string thumbnailUrl;
        public string source;
        public TimeSpan duration;

        public string title;
        public string artist;
        public string album;
        public string year;

        public string DottedInfoLine => string.Join(" • ", new[] {album, artist, year}
            .Where(s => !string.IsNullOrWhiteSpace(s)));
        
        public static implicit operator MediaMetadata(MediaInfo i)
        {
            return new MediaMetadata
            {
                url = i.sourceUrl,
                thumbnailUrl = i.thumbnailUrl,
                source = i.source,
                duration = i.duration,
                
                title = i.title,
                artist = i.artist,
                album = i.album,
                year = i.year
            };
        }
    }
}