using System;
using UnityEngine;

namespace CreativeMode
{
    public interface IDesktopUIManager
    {
        IDisposable ShowContextMenu(Vector2 position, Menu menu);
        void ShowNotification(string message, string title = null, Sprite icon = null);
    }
}