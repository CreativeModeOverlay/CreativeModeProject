using System;

namespace CreativeMode
{
    [Serializable]
    public struct StyleProperty<P>
    {
        public bool shouldOverride;
        public P value;

        public static implicit operator StyleProperty<P>(P value)
        {
            return new StyleProperty<P> { value = value};
        }
    }
}