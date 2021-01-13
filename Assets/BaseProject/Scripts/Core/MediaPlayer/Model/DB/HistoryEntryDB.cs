using System;
using SQLite;

namespace CreativeMode
{
    internal class HistoryEntryDB : MediaEntryDB
    {
        [Indexed]
        public DateTime Date { get; set; }
    }
}