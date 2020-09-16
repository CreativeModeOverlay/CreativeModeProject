using System.Collections.Generic;

namespace CreativeMode.Generic
{
    public interface IEntityStorage<K, V>
    {
        bool SupportsInsert { get; }
        K Insert(V value);
        
        bool Contains(K key);
        
        V Get(K key);
        void Put(K key, V value);
        void Delete(K key);

        void DeleteAll();
        IEnumerable<KeyValuePair<K, V>> GetAll();
    }
}