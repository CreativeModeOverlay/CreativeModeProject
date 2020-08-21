using System;
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
}
