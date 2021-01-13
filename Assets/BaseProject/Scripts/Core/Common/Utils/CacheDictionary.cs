using System;
using System.Collections.Generic;

namespace CreativeMode
{
    public class CacheDictionary<K, V>
    {
        private readonly Dictionary<K, Entry> dictionary = new Dictionary<K, Entry>();
        private readonly TimeSpan lifetime;

        public CacheDictionary(TimeSpan lifetime)
        {
            this.lifetime = lifetime;
        }
        
        public void Put(K key, V value)
        {
            dictionary[key] = new Entry
            {
                time = DateTime.Now,
                value = value
            };
        }

        public bool Get(K key, out V value)
        {
            if (dictionary.TryGetValue(key, out var existing) && 
                DateTime.Now - existing.time < lifetime)
            {
                value = existing.value;
                return true;
            }

            value = default;
            return false;
        }
        
        private struct Entry
        {
            public DateTime time;
            public V value;
        }
    }
}