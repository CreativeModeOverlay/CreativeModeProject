using System;
using SQLite;

namespace CreativeMode.Impl
{
    public class PlayedMusicEntryDB
    {
        [PrimaryKey]
        public string Url { get; set; }
        public DateTime Date { get; set; }
    }
}