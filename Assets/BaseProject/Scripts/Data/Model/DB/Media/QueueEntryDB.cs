using SQLite;

namespace CreativeMode
{
    public class QueueEntryDB : MediaEntryDB
    {
        [Indexed]
        public int SetId { get; set; }
    }
}