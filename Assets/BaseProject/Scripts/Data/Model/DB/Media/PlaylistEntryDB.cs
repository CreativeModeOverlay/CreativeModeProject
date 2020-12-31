using SQLite;

namespace CreativeMode.Impl
{
    public class PlaylistEntryDB : MediaEntryDB
    {
        [Indexed]
        public int SetId { get; set; }
    }
}