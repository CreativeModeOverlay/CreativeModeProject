using UnityEngine;

namespace CreativeMode
{
    public abstract class ElementStyle : ScriptableObject
    {
        protected const string MenuCategory = "UI Style/";
        
        [SerializeField] private ElementStyle[] inherits;
        
        public ElementStyle[] Inherits => inherits;
        public abstract object Overrides { get; }
    }
    
    public abstract class ElementStyle<T> : ElementStyle
    {
        [SerializeField] private T overrides;
        
        public override object Overrides => overrides;
    }
}