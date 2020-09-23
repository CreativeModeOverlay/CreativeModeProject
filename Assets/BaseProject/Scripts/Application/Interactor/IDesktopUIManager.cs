using System;
using UnityEngine;

namespace CreativeMode
{
    public interface IDesktopUIManager
    {
        void ShowNotification(string message, string title = null, Sprite icon = null);
        IDisposable ShowContextMenu(Vector2 position, Menu menu);
    }
}