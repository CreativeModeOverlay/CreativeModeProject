using SQLite;

namespace CreativeMode
{
    public abstract class EntityTableDb<K>
    {
        [PrimaryKey]
        public K Id { get; set; }
        public byte[] Data { get; set; }
    }
}