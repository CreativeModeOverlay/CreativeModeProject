using System;

namespace CreativeMode
{
    [Serializable]
    public struct StyleReference<S, T> 
        where S : BaseElementStyle<S, T>
    {
        public S[] innherits;
        public T overrides;
    }
}