using SQLite;

namespace CreativeMode.Impl
{
    internal class PlaylistEntryDB : MediaEntryDB
    {
        [Indexed]
        public int SetId { get; set; }
    }
}