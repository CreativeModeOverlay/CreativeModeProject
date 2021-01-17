using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode.Impl
{
    internal abstract class ContextMenuEntryWidget<T> : MonoBehaviour
        where T : Menu.Entry
    {
        public Image iconWidget;
        public Text titleWidget;

        public T Entry
        {
            get => currentEntry;
            set
            {
                currentEntry = value;
                ApplyEntry(value);
            }
        }

        public ContextMenuWidget ContextMenu { get; set; }

        protected abstract Behaviour ButtonComponent { get; }
        private T currentEntry;

        protected virtual void ApplyEntry(T entry)
        {
            titleWidget.text = entry.title;
            iconWidget.sprite = entry.icon;
            iconWidget.enabled = entry.icon;
            
            ButtonComponent.enabled = entry.isEnabled;
            gameObject.SetActive(entry.isVisible);
        }
    }
}