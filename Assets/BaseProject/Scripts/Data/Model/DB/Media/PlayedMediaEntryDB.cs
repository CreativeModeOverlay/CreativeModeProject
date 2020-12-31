using System;
using SQLite;

namespace CreativeMode
{
    public class PlayedMediaEntryDB
    {
        [PrimaryKey]
        public string Url { get; set; }
        public DateTime Date { get; set; }
    }
}