using System;
using SQLite;

namespace CreativeMode
{
    public class MediaEntryDB
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }
        
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        
        public string Source { get; set; }
        public TimeSpan Duration { get; set; }
    }
}