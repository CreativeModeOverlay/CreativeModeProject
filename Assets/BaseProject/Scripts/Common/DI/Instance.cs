using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public static class Instance<T>
{
    private static Func<T> factory;
    private static readonly Dictionary<object, Func<T>> taggedFactory = new Dictionary<object, Func<T>>();

    public static T Get(object tag = null)
    {
        if (tag != null)
        {
            if (taggedFactory.TryGetValue(tag, out var tFactory))
                return tFactory();
            
            throw new ArgumentException($"Dependency for type \"{typeof(T).Name}\" with tag {tag} is not bound");
        }

        if (factory != null)
            return factory();

        throw new ArgumentException($"Dependency for type \"{typeof(T).Name}\" is not bound");
    }
    
    public static Binder Bind(bool isLazy = true, object tag = null) => new Binder(isLazy, tag);
    
    public class Binder
    {
        private Func<T> factoryFunc;
        private Func<T> lazyFactoryFunc;
        private readonly bool isLazy;
        private readonly object tag;

        public Binder(bool isLazy, object tag)
        {
            this.isLazy = isLazy;
            this.tag = tag;
        }

        public Binder Factory(Func<T> factory)
        {
            factoryFunc = factory;
            UpdateFactory();
            return this;
        }

        public Binder Instance(T instance) => Factory(() => instance);
        public Binder UnityObject<O>() where O : Object, T => Factory(Object.FindObjectOfType<O>);

        private void UpdateFactory()
        {
            Func<T> resultFactory;
            
            if (isLazy)
            {
                var isCreated = false;
                T instance = default;
                
                resultFactory = () =>
                {
                    if (!isCreated)
                    {
                        instance = factoryFunc();
                        isCreated = true;
                    }

                    return instance;
                };
            }
            else
            {
                resultFactory = factoryFunc;
            }

            if (tag != null)
            {
                taggedFactory[tag] = resultFactory;
            }
            else
            {
                factory = resultFactory;
            }
        }
    }
}