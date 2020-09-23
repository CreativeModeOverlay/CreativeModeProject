using System;
using UnityEngine;

namespace CreativeMode.Impl
{
    public class DesktopUIManager : MonoBehaviour, IDesktopUIManager
    {
        public ContextMenuManager contextMenuManager;

        public IDisposable ShowContextMenu(Vector2 position, Menu menu) => 
            contextMenuManager.ShowContextMenu(position, menu);

        public void ShowNotification(string message, string title = null, Sprite icon = null)
        {
            // TODO
        }
    }
}