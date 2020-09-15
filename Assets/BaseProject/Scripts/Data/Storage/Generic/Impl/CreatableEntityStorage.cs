using CreativeMode.Generic;
using SQLite;
using UnityEngine.Profiling;

namespace CreativeMode
{
    public class CreatableEntityStorage<D, V> :
        EntityStorage<D, int, V>, ICreatableEntityStorage<V> 
        where D : CreatableEntityDb<V>, new()
    {
        public CreatableEntityStorage(SQLiteConnection connection, IEntitySerializer<V> serializer) 
            : base(connection, serializer) { }
        
        public int Create(V value)
        {
            try
            {
                Profiler.BeginSample("EntityStorage.Create " + typeof(V).Name);
                var entry = new D {Data = Serialize(value)};
                connection.Insert(entry);
                return entry.Id;
            }
            finally
            {
                Profiler.EndSample();
            }
        }
    }
}