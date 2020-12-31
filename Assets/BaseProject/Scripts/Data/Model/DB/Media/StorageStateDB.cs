using SQLite;

namespace CreativeMode.Impl
{
    public class StorageStateDB
    {
        [PrimaryKey]
        public int Id { get; set; } = 0;
        public int HistoryPosition { get; set; }
        public bool Shuffle { get; set; }
        public bool SkipRepeats { get; set; }
        public int CurrentSetId { get; set; }
    }
}