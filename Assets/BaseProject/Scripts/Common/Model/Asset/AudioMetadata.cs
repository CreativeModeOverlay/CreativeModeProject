using System.Linq;

namespace CreativeMode
{
    public struct AudioMetadata
    {
        public string url;
        public string coverUrl;
        
        public string artist;
        public string album;
        public string title;
        public string year;
        
        public SongLyrics[] lyrics;

        public string DottedInfoLine => string
            .Join(" • ", new[] {album, artist, year}
            .Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}