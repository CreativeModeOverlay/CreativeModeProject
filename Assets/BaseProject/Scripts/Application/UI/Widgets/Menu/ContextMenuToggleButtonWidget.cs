using System;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class ContextMenuToggleButtonWidget : ContextMenuEntryWidget<Menu.Toggle>
    {
        public Toggle toggleWidget;

        public event Action OnChanged;

        protected override Behaviour ButtonComponent => toggleWidget;

        protected override void ApplyEntry(Menu.Toggle entry)
        {
            base.ApplyEntry(entry);
            toggleWidget.isOn = entry.isChecked;
        }

        private void Awake()
        {
            toggleWidget.onValueChanged.AddListener(v =>
            {
                OnChanged?.Invoke();
                Entry?.onCheckedChanged?.Invoke(v);
            });
        }
    }
}