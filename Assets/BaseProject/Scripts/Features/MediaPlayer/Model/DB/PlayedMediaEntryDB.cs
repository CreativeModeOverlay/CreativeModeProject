using System;
using SQLite;

namespace CreativeMode
{
    internal class PlayedMediaEntryDB
    {
        [PrimaryKey]
        public string Url { get; set; }
        public DateTime Date { get; set; }
    }
}