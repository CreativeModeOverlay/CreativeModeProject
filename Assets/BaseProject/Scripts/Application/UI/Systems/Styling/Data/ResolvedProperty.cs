using UnityEngine;
using UnityEngine.EventSystems;

namespace CreativeMode
{
    public struct ResolvedProperty
    {
        public string propertyName;
        public object value;
        
        public ScriptableObject valueOrigin;
        public UIBehaviour valueTarget;
    }
}