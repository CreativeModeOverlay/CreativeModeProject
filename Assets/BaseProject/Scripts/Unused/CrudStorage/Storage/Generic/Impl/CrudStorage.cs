using System;
using System.Collections.Generic;
using System.Linq;
using CreativeMode.Generic;
using SQLite;

namespace CreativeMode
{
    public class CrudStorage<T, K, V> : ICrudStorage<K, V> 
        where T : EntityTableDb<K>, new()
    {
        protected readonly SQLiteConnection connection;
        private readonly IEntitySerializer<V> serializer;
        private bool supportsInsert;

        public CrudStorage(SQLiteConnection connection, IEntitySerializer<V> serializer)
        {
            this.serializer = serializer;
            this.connection = connection;

            supportsInsert = typeof(K) == typeof(int) || typeof(K) == typeof(long);
            connection.CreateTable<T>(supportsInsert ? CreateFlags.AutoIncPK : CreateFlags.None);
        }

        public bool SupportsInsert => supportsInsert;
        
        public K Insert(V value)
        {
            if(!supportsInsert)
                throw new ArgumentException("Storage does not support insertion");
            
            var entry = new T { Data = Serialize(value) };
            connection.Insert(entry);
            return entry.Id;
        }
        
        public bool Contains(K key)
        {
            return connection.Table<T>()
                .FirstOrDefault() != null;
        }

        public V Get(K key)
        {
            var entity = connection.Table<T>()
                .FirstOrDefault(o => o.Id.Equals(key));

            return entity == null ? default : Deserialize(entity.Data);
        }

        public void Put(K key, V value)
        {

            connection.InsertOrReplace(new T { Id = key, Data = Serialize(value) });
        }

        public void Delete(K key)
        {
            connection.Delete<T>(key);
        }

        public void DeleteAll()
        {
            connection.DeleteAll<T>();
        }

        public IEnumerable<KeyValuePair<K, V>> GetAll()
        {
            return connection.Table<T>()
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