using SQLite;

namespace CreativeMode
{
    internal class QueueEntryDB : MediaEntryDB
    {
        [Indexed]
        public int SetId { get; set; }
        
        [Indexed]
        public int Priority { get; set; }
    }
}