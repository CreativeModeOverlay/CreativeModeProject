using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CreativeMode
{
    public class ContextMenuWidget : MonoBehaviour
    {
        public ContextMenuButtonWidget buttonPrefab;
        public ContextMenuToggleButtonWidget toggleButtonPrefab;
        public ContextMenuSubMenuButtonWidget subMenuButtonPrefab;
        public GameObject dividerPrefab;
        public float maxHeight = 400;
        public float subMenuAutoExpandTimeout = 0.25f;
        public Vector2 subMenuSpawnOffset;
        public CanvasGroup contextMenuGroup;
        public Animator contextMenuAnimator;

        public RectTransform ownSizeContainer;
        public RectTransform entryContainer;
        public ScrollRect entryScrollRect;

        public event Action<bool> OnCloseListener;
        public Rect Rect => transform.AsRectTransform().rect;

        private Menu currentMenu;
        private ISubMenuHandler subMenuHandler;
        private List<GameObject> spawnedObjects = new List<GameObject>();

        private ContextMenuSubMenuButtonWidget currentOpenedSubMenu;
        private ContextMenuWidget currentSubMenu;
        private Action subMenuAutoExpandAction;
        private float subMenuAutoExpandTime;

        public void SetMenu(Menu menu, ISubMenuHandler handler)
        {
            currentMenu = menu;
            subMenuHandler = handler;
            BuildMenu(currentMenu);
        }

        public void Close(bool selected = false)
        {
            CloseSubMenu();

            contextMenuAnimator.SetBool("IsVisible", false);
            contextMenuGroup.interactable = false;
            contextMenuGroup.blocksRaycasts = false;
            Destroy(gameObject, 0.5f);
            OnCloseListener?.Invoke(selected);
        }

        private void BuildMenu(Menu menu)
        {
            ClearMenu();
            
            if(menu == null || menu.Size == 0)
                return;

            var currentGroup = menu[0].group;
            
            for (var i = 0; i < menu.Size; i++)
            {
                var entry = menu[i];

                if(!entry.isVisible)
                    continue;

                if (entry.group != currentGroup)
                {
                    currentGroup = entry.group;
                    AddDivider();
                }

                switch (entry)
                {
                    case Menu.Button b: AddButton(b); break;
                    case Menu.Toggle t: AddToggle(t); break;
                    case Menu.SubMenu t: AddSubMenu(t); break;
                }
            }

            UpdateSize();
        }

        private void UpdateSize()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(entryContainer);

            var entriesHeight = entryContainer.rect.height;
            var hasOverflow = entriesHeight > maxHeight;

            ownSizeContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 
                hasOverflow ? maxHeight : entriesHeight);
            
            entryScrollRect.verticalNormalizedPosition = 1f;
            entryScrollRect.vertical = hasOverflow;
        }

        private void AddDivider()
        {
            var obj = Instantiate(dividerPrefab, entryContainer);
            spawnedObjects.Add(obj);
        }

        private void AddButton(Menu.Button button)
        {
            var obj = Instantiate(buttonPrefab, entryContainer);
            obj.ContextMenu = this;
            obj.Entry = button;

            if (button.closeOnClick)
                obj.OnClicked += () => Close(true);

            spawnedObjects.Add(obj.gameObject);
        }

        private void AddToggle(Menu.Toggle toggle)
        {
            var obj = Instantiate(toggleButtonPrefab, entryContainer);
            obj.ContextMenu = this;
            obj.Entry = toggle;
            
            if (toggle.closeOnChange)
                obj.OnChanged += () => Close(true);

            spawnedObjects.Add(obj.gameObject);
        }

        private void AddSubMenu(Menu.SubMenu subMenu)
        {
            var obj = Instantiate(subMenuButtonPrefab, entryContainer);
            obj.ContextMenu = this;
            obj.Entry = subMenu;
            obj.OnSubMenuOpen += () => OpenSubMenu(obj, subMenu);
            
            WatchAutoExpand(obj.gameObject, () =>
            {
                OpenSubMenu(obj, subMenu);
            });
            
            spawnedObjects.Add(obj.gameObject);
        }

        private void ClearMenu()
        {
            foreach (var spawnedObject in spawnedObjects)
                Destroy(spawnedObject);
            spawnedObjects.Clear();
        }

        private void OpenSubMenu(ContextMenuSubMenuButtonWidget widget, Menu.SubMenu subMenu)
        {
            if(currentOpenedSubMenu == widget)
                return;
            
            CloseSubMenu();

            var offset = widget.SubMenuSpawnPosition + subMenuSpawnOffset;
            var worldPoint = widget.transform.TransformPoint(offset);
            var localOffset = transform.InverseTransformPoint(worldPoint);
            var subMenuWidget = subMenuHandler.ShowContextSubMenu(this, localOffset, subMenu.subMenu);

            currentOpenedSubMenu = widget;
            currentSubMenu = subMenuWidget;
            subMenuWidget.OnCloseListener += isSelected =>
            {
                currentSubMenu = null;
                currentOpenedSubMenu = null;

                if (subMenu.closeParentOnSelect && isSelected)
                    Close(true);
            };
        }

        private void CloseSubMenu()
        {
            if (currentSubMenu)
            {
                currentSubMenu.Close();
                currentSubMenu = null;
            }
        }
        
        private void WatchAutoExpand(GameObject root, Action onAutoExpand)
        {
            var trigger = root.AddComponent<EventTrigger>();

            var enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener(l =>
            {
                subMenuAutoExpandAction = onAutoExpand;
                subMenuAutoExpandTime = 0;
            });
            
            var exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener(l =>
            {
                subMenuAutoExpandAction = null;
            });
            
            trigger.triggers.Add(enterEntry);
            trigger.triggers.Add(exitEntry);
        }

        private void UpdateAutoExpand()
        {
            if (subMenuAutoExpandAction == null)
                return;

            var newTime = subMenuAutoExpandTime + Time.deltaTime;
            var menuAutoFocusTime = subMenuAutoExpandTimeout;

            if (subMenuAutoExpandTime <= menuAutoFocusTime && newTime > menuAutoFocusTime)
            {
                subMenuAutoExpandAction();
                subMenuAutoExpandAction = null;
            }

            subMenuAutoExpandTime = newTime;
        }

        private void Update()
        {
            UpdateAutoExpand();
        }

        public interface ISubMenuHandler
        {
            ContextMenuWidget ShowContextSubMenu(ContextMenuWidget widget, Vector2 relativePosition, Menu menu);
        }
    }
}