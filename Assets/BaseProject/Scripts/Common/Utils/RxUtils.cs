using System;
using System.Collections.Generic;
using System.Linq;
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

    public static IDisposable SubscribeChanges<T>(this IObservable<ICollection<T>> o, Action<T> onAdded, Action<T> onRemoved)
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
}
