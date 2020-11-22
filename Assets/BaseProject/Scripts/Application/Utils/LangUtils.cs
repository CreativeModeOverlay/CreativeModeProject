using System;

namespace CreativeMode
{
    public static class LangUtils
    {
        
        
        public static T Config<T>(this T obj, Action<T> configurator)
        {
            if (obj != null && configurator != null)
                configurator(obj);
            
            return obj;
        }
    }
}