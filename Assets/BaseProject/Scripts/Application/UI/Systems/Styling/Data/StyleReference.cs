using System;

namespace CreativeMode
{
    [Serializable]
    public struct StyleReference<O>
    {
        public ElementStyle[] inherits;
        public O overrides;
    }
}