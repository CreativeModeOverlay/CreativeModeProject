using System;
using SQLite;

namespace CreativeMode
{
    public class HistoryEntryDB : MediaEntryDB
    {
        [Indexed]
        public DateTime Date { get; set; }
    }
}