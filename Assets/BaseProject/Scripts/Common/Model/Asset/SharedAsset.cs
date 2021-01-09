using System;
using UnityEngine;

public struct SharedAsset<T> : IDisposable
{
    public T Asset => provider.asset;
    public bool Exists => provider != null;
    
    private ReferenceProvider provider;
    private bool canDispose;

    public void Dispose()
    {
        if (!canDispose) 
            return;
        
        provider?.OnDisposed();
        canDispose = false;
    }

    public static IReferenceProvider Create(T asset, Func<T, bool> isDisposed, Action<T> dispose)
    {
        return new ReferenceProvider
        {
            asset = asset,
            isAssetDisposed = isDisposed,
            dispose = dispose
        };
    }
    
    public interface IReferenceProvider
    {
        bool IsDisposed { get; }

        void EnableDisposal();
        SharedAsset<T> Provide();
    }
    
    private class ReferenceProvider : IReferenceProvider
    {
        public bool IsDisposed => isDisposed || isAssetDisposed(asset);
        
        public T asset;
        public Func<T, bool> isAssetDisposed;
        private bool isDisposed;
        public Action<T> dispose;
        private int count;
        private bool isDisposalEnabled;

        public void EnableDisposal()
        {
            isDisposalEnabled = true;

            if (count == 0)
                OnDisposed();
        }

        public SharedAsset<T> Provide()
        {
            if (IsDisposed)
            {
                Debug.Log("Attempting to provide disposed asset");
                return default;
            }

            count++;
            return new SharedAsset<T> { provider = this, canDispose = true };
        }

        public void OnDisposed()
        {
            if(IsDisposed || !isDisposalEnabled)
                return;
            
            count--;

            if (count <= 0)
            {
                dispose(asset);
                isDisposed = true;
            }
        }
    }
}