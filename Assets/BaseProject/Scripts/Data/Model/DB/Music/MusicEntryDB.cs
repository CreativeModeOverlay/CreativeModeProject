using SQLite;

namespace CreativeMode
{
    public class MusicEntryDB
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Url { get; set; }
    }
}