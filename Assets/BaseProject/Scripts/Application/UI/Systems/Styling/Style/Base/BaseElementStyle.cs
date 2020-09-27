using UnityEngine;

namespace CreativeMode
{
    public abstract class BaseElementStyle<S, T> : ScriptableObject
    {
        protected const string MenuCategory = "UI Style/";
    
        [SerializeField] private S[] innherits;
        [SerializeField] private T overrides;

        public S[] Innherits => innherits;
        public T Overrides => overrides;
    }
}