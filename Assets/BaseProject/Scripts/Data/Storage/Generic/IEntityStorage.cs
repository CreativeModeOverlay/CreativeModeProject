using System.Collections.Generic;

namespace CreativeMode.Generic
{
    public interface IEntityStorage<K, V>
    {
        bool Contains(K key);
        
        V Get(K key);
        void InsertOrUpdate(K key, V value);
        void Delete(K key);

        void DeleteAll();
        IEnumerable<KeyValuePair<K, V>> GetAll();
    }

    public interface ICreatableEntityStorage<V> : IEntityStorage<int, V>
    {
        int Create(V value);
    }
}