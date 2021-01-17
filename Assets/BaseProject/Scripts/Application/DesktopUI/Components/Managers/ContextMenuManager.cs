using System;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode.Impl
{
    internal class ContextMenuManager : MonoBehaviour, ContextMenuWidget.ISubMenuHandler
    {
        public RectTransform uiRoot;
        public Button contextMenuClosePanel;

        public ContextMenuWidget contextMenuWidgetPrefab;
        private IDisposable topContextMenuDisposable;
        
        private readonly Stack<ContextMenuWidget> contextMenuWidgets = new Stack<ContextMenuWidget>();
        
        private void Awake()
        {
            contextMenuClosePanel.gameObject.SetActive(false);
            contextMenuClosePanel.onClick.AddListener(() =>
            {
                topContextMenuDisposable?.Dispose();
            });
        }

        public IDisposable ShowContextMenu(Vector2 position, Menu menu)
        {
            topContextMenuDisposable?.Dispose();

            var menuInstance = Instantiate(contextMenuWidgetPrefab, uiRoot);
            menuInstance.SetMenu(menu, this);
            menuInstance.transform.localPosition = position;
            
            menuInstance.OnCloseListener += isSelected =>
            {
                topContextMenuDisposable = null;
                OnContextMenuClose();
            };
            topContextMenuDisposable = Disposable.Create(() => menuInstance.Close());

            OnContextMenuOpen(menuInstance);

            return topContextMenuDisposable;
        }


        public ContextMenuWidget ShowContextSubMenu(ContextMenuWidget widget, Vector2 relativePosition, Menu menu)
        {
            var menuInstance = Instantiate(contextMenuWidgetPrefab, uiRoot);
            menuInstance.SetMenu(menu, this);

            var subMenuPosition = (Vector2) widget.transform.localPosition + relativePosition;
            menuInstance.transform.localPosition = subMenuPosition;
            menuInstance.OnCloseListener += isSelected => OnContextMenuClose();

            OnContextMenuOpen(menuInstance);

            return menuInstance;
        }

        private void OnContextMenuOpen(ContextMenuWidget widget)
        {
            contextMenuWidgets.Push(widget);
            FocusOnLastContextMenu();
        }

        private void OnContextMenuClose()
        {
            contextMenuWidgets.Pop();
            FocusOnLastContextMenu();
        }

        private void FocusOnLastContextMenu()
        {
            if (contextMenuWidgets.Count == 0)
            {
                contextMenuClosePanel.gameObject.SetActive(false);
                uiRoot.DOAnchorPos(Vector2.zero, 0.25f);
                return;
            }

            contextMenuClosePanel.gameObject.SetActive(true);
            
            var screenRect = uiRoot.rect;
            var widgetRect = new Rect();
            var isFirstRect = true;

            foreach (var widget in contextMenuWidgets)
            {
                var rect = widget.Rect;
                rect.position += (Vector2) widget.transform.localPosition;

                if (isFirstRect)
                {
                    isFirstRect = false;
                    widgetRect = rect;
                }
                else
                {
                    widgetRect.xMin = Mathf.Min(widgetRect.xMin, rect.xMin);
                    widgetRect.xMax = Mathf.Max(widgetRect.xMax, rect.xMax);
                    widgetRect.yMin = Mathf.Min(widgetRect.yMin, rect.yMin);
                    widgetRect.yMax = Mathf.Max(widgetRect.yMax, rect.yMax);
                }
            }

            var offset = Vector2.zero;
            
            if (widgetRect.xMin < screenRect.xMin)
                offset.x += screenRect.xMin - widgetRect.xMin;

            if (widgetRect.xMax > screenRect.xMax)
                offset.x -= widgetRect.xMax - screenRect.xMax;

            if (widgetRect.yMin < screenRect.yMin)
                offset.y += screenRect.yMin - widgetRect.yMin;
            
            if (widgetRect.yMax > screenRect.yMax)
                offset.y -= widgetRect.yMax - screenRect.yMax;

            DOTween.Kill(uiRoot);
            uiRoot.DOAnchorPos(offset, 0.25f);
        }
    }
}