using SQLite;

namespace CreativeMode
{
    public abstract class EntityDb<K, V>
    {
        [PrimaryKey]
        public virtual K Id { get; set; }
        public byte[] Data { get; set; }
    }
}