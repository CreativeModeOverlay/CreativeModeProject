using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

/// <summary>
/// Service locator-like instance provider
/// </summary>
public static class Instance<T>
{
    private static Func<T> factory;
    private static readonly Dictionary<string, Func<T>> taggedFactory = new Dictionary<string, Func<T>>();

    public static T Get()
    {
        if (factory != null)
            return factory();

        throw new ArgumentException($"Dependency for type \"{typeof(T).Name}\" is not bound");
    }
    
    public static T Get(string tag)
    {
        if (tag == null)
            return Get();
        
        if (taggedFactory.TryGetValue(tag, out var tFactory))
            return tFactory();
            
        throw new ArgumentException($"Dependency for type \"{typeof(T).Name}\" with tag {tag} is not bound");
    }

    public static void Bind<O>(Func<O> creator, bool singleton = true, string tag = null) 
        where O : T
    {
        Func<T> factoryFunc;
        
        if (singleton)
        {
            var isCreated = false;
            O instance = default;
            
            Func<T> singletonWrapper = () =>
            {
                if (!isCreated)
                {
                    instance = creator();
                    isCreated = true;
                }

                return instance;
            };

            factoryFunc = singletonWrapper;
        }
        else
        {
            factoryFunc = () => creator();
        }

        if (tag == null)
        {
            factory = factoryFunc;
        }
        else
        {
            taggedFactory[tag] = factoryFunc;
        }
    }

    public static void BindUnityObject<O>(bool singleton = true, string tag = null) 
        where O : Object, T => Bind(() => Object.FindObjectOfType<O>(true), singleton, tag);
}