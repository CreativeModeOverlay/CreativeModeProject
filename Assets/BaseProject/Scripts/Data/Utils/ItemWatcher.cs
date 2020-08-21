using System;
using UniRx;

namespace CreativeMode.Impl
{
    public class ItemWatcher<T>
    {
        private readonly Subject<T> itemUpdated = new Subject<T>();
        private readonly Subject<T> itemDeleted = new Subject<T>();

        public void NotifyUpdate(T id) => itemUpdated.OnNext(id);
        public void NotifyDelete(T id) => itemDeleted.OnNext(id);

        public IObservable<Unit> EveryUpdate() => Observable.ReturnUnit()
            .Merge(
                itemUpdated.Select(_ => Unit.Default), 
                itemDeleted.Select(_ => Unit.Default));
        
        public IObservable<Unit> EveryUpdate(T id) => Observable.ReturnUnit()
            .Merge(
                itemUpdated
                    .Where(i => i.Equals(id))
                    .Select(i => Unit.Default),
                
                itemDeleted
                    .Where(i => i.Equals(id))
                    .Select<T, Unit>(t => throw new Exception("Item is deleted")) // TODO: Specific exception
            );
    }
}