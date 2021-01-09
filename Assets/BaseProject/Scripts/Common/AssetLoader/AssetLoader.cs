using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UniRx;

public abstract class AssetLoader<T>
{
    private readonly Dictionary<string, ScheduledLoading> scheduledObservables
        = new Dictionary<string, ScheduledLoading>();
    
    private readonly Dictionary<string, IObservable<SharedAsset<T>.IReferenceProvider>> loadingObservables 
        = new Dictionary<string, IObservable<SharedAsset<T>.IReferenceProvider>>();
    
    private readonly Dictionary<string, SharedAsset<T>.IReferenceProvider> loadedAssets 
        = new Dictionary<string, SharedAsset<T>.IReferenceProvider>();

    private int usedThreadCount;
    private readonly Queue<ScheduledLoading> scheduledQueue 
        = new Queue<ScheduledLoading>();

    public int MaxThreadCount { get; set; } = 8;
    
    protected abstract IObservable<SharedAsset<T>.IReferenceProvider> CreateAssetProvider(string url);

    public void PreloadAsset(string url)
    {
        if(scheduledObservables.ContainsKey(url) || loadingObservables.ContainsKey(url))
            return;
        
        if(loadedAssets.TryGetValue(url, out var loadedFile) && !loadedFile.IsDisposed)
            return;

        (usedThreadCount >= MaxThreadCount
            ? ScheduleLoading(url)
            : LoadAsset(url)).Subscribe();
    }

    public IObservable<SharedAsset<T>> GetAsset(string url)
    {
        if (scheduledObservables.TryGetValue(url, out var scheduledObservable))
            return scheduledObservable.observable.Select(r => r.Provide());
        
        if(loadedAssets.TryGetValue(url, out var loadedImage))
        {
            if (!loadedImage.IsDisposed)
                return Observable.Return(loadedImage.Provide());

            loadedAssets.Remove(url);
        }

        if (loadingObservables.TryGetValue(url, out var loadingObservable))
            return loadingObservable.Select(r => r.Provide());

        return (usedThreadCount >= MaxThreadCount 
            ? ScheduleLoading(url) 
            : LoadAsset(url))
            .Select(o => o.Provide());
    }

    private IObservable<SharedAsset<T>.IReferenceProvider> LoadAsset(string url)
    {
        SharedAsset<T>.IReferenceProvider createdProvider = null;
        
        void OnFinished()
        {
            usedThreadCount--;
            loadingObservables.Remove(url);
            ExecuteNextScheduledTask();

            // TODO: Hacky, create garbage collection?
            if (createdProvider != null)
            {
                Observable.Timer(TimeSpan.FromSeconds(5))
                    .Subscribe(e => { createdProvider.EnableDisposal(); });
            }
        }

        var loading = CreateAssetProvider(url)
            .Select(asset =>
            {
                createdProvider = asset;
                loadedAssets[url] = createdProvider;
                return asset;
            })
            .DoOnCancel(OnFinished)
            .DoOnError(e => OnFinished())
            .DoOnCompleted(OnFinished)
            .Share();

        usedThreadCount++;
        loadingObservables[url] = loading;
        return loading;
    }

    protected IObservable<Stream> GetAssetStream(string url, bool requireSeek = true)
    {
        return Observable.Start(() =>
        {
            var stream = WebRequest.Create(url)
                .GetResponse()
                .GetResponseStream();

            if (requireSeek && !stream.CanSeek)
            {
                using (stream)
                {
                    var memoryStream = new MemoryStream(stream.Length > 0 ? (int) stream.Length : 0);
                    stream.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                    return memoryStream;
                }
            }

            return stream;
        }, Scheduler.ThreadPool);
    }

    private IObservable<SharedAsset<T>.IReferenceProvider> ScheduleLoading(string url)
    {
        var subject = new Subject<Unit>();
        var loading = new ScheduledLoading
        {
            url = url,
            subject = subject,
            observable = subject.SelectMany(LoadAsset(url))
                .Share()
        };

        scheduledObservables[url] = loading;
        scheduledQueue.Enqueue(loading);
        return loading.observable;
    }

    private void ExecuteNextScheduledTask()
    {
        if (scheduledQueue.Count > 0)
        {
            var task = scheduledQueue.Dequeue();
            scheduledObservables.Remove(task.url);
            task.Start();
        }
    }

    private class ScheduledLoading
    {
        public string url;
        public Subject<Unit> subject;
        public IObservable<SharedAsset<T>.IReferenceProvider> observable;

        public void Start()
        {
            subject.OnNext(Unit.Default);
            subject.OnCompleted();
        }
    }
}
