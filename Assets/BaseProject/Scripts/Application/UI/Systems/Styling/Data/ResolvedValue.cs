using UnityEngine;
using UnityEngine.EventSystems;

namespace CreativeMode
{
    public struct ResolvedValue<T>
    {
        public string propertyName;
        public T value;
        public ScriptableObject origin;
    }
}