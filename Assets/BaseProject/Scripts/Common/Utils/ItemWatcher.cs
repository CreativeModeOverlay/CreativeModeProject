using System;
using UniRx;

namespace CreativeMode.Impl
{
    public class ItemWatcher<T>
    {
        private readonly Subject<T> itemUpdated = new Subject<T>();
        private readonly Subject<T> itemDeleted = new Subject<T>();

        public void NotifyUpdate(T id) => itemUpdated.OnNext(id);
        public void NotifyStop(T id) => itemDeleted.OnNext(id);
        public void NotifyStopAll() => itemDeleted.OnNext(default);

        public IObservable<Unit> EveryUpdate(T id) => itemUpdated
            .Where(i => i.Equals(id))
            .Select(i => Unit.Default)
            .TakeUntil(itemDeleted.Where(i => i == null || i.Equals(id)))
            .StartWith(Unit.Default);
    }
}