using System;
using UnityEngine;

namespace CreativeMode
{
    public interface IDesktopUIManager
    {
        T OpenWindow<T>(GameObject prefab);
        
        IDisposable ShowContextMenu(Vector2 position, Menu menu);
        void ShowNotification(string message, string title = null, Sprite icon = null);
    }
}