using SQLite;

namespace CreativeMode
{
    public class CreatableEntityDb<V> : EntityDb<int, V>
    {
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }
    }
}