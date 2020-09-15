using System.Collections.Generic;
using System.Linq;
using CreativeMode.Generic;
using SQLite;
using UnityEngine.Profiling;

namespace CreativeMode
{
    public class EntityStorage<D, K, V> : IEntityStorage<K, V> 
        where D : EntityDb<K, V>, new()
    {
        protected readonly SQLiteConnection connection;
        private readonly IEntitySerializer<V> serializer;

        public EntityStorage(SQLiteConnection connection, IEntitySerializer<V> serializer)
        {
            this.serializer = serializer;
            this.connection = connection;

            connection.CreateTable<D>();
        }
        
        public bool Contains(K key)
        {
            return connection.Table<D>()
                .FirstOrDefault() != null;
        }

        public V Get(K key)
        {
            var entity = connection.Table<D>()
                .FirstOrDefault(o => o.Id.Equals(key));

            return entity == null ? default : Deserialize(entity.Data);
        }

        public void InsertOrUpdate(K key, V value)
        {

            connection.InsertOrReplace(new D {Id = key, Data = Serialize(value)});
        }

        public void Delete(K key)
        {
            connection.Delete<D>(key);
        }

        public void DeleteAll()
        {
            connection.DeleteAll<D>();
        }

        public IEnumerable<KeyValuePair<K, V>> GetAll()
        {
            return connection.Table<D>()
                .Select(e => new KeyValuePair<K, V>(e.Id, Deserialize(e.Data)));
        }

        protected byte[] Serialize(V value)
        {
            return serializer.Serialize(value);
        }

        protected V Deserialize(byte[] data)
        {
            return serializer.Deserialize(data);
        }
    }
}