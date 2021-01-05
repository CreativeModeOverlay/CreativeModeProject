using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public static class RxUtils
{
    public static IObservable<T> MaxItemsPerFrame<T>(this IObservable<T> o, int count)
    {
        var currentDelay = -1;
        var lastSentFrame = 0;
        
        return o.Buffer(count)
            .ObserveOnMainThread()
            .SelectMany(v =>
            {
                var currentFrame = Time.frameCount;
                currentDelay += lastSentFrame - currentFrame + 1;
                lastSentFrame = currentFrame;

                if (currentDelay < 0)
                {
                    currentDelay = 0;
                    return Observable.Return(v);
                }

                return Observable.Return(v)
                    .DelayFrame(currentDelay);
            })
            .SelectMany(l => l);
    }

    public static IDisposable SubscribeChanges<T>(this IObservable<IEnumerable<T>> o, 
        OnAddedDelegate<T> onAdded, 
        OnRemovedDelegate<T> onRemoved)
    {
        var addedItems = new HashSet<T>();
        
        return o.Subscribe(d =>
        {
            var itemsToRemove = new List<T>(addedItems);
            
            foreach (var item in d)
            {
                if (addedItems.Contains(item))
                {
                    itemsToRemove.Remove(item);
                }
                else
                {
                    addedItems.Add(item);
                    onAdded(item);
                }
            }

            foreach (var item in itemsToRemove)
                onRemoved(item);
        });
    }

    public delegate void OnAddedDelegate<T>(T item);
    public delegate void OnRemovedDelegate<T>(T item);
}
