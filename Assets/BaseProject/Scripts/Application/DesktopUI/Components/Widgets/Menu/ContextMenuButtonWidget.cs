using System;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode.Impl
{
    internal class ContextMenuButtonWidget : ContextMenuEntryWidget<Menu.Button>
    {
        public Button buttonWidget;

        public event Action OnClicked;

        protected override Behaviour ButtonComponent => buttonWidget;

        private void Awake()
        {
            buttonWidget.onClick.AddListener(() =>
            {
                OnClicked?.Invoke();
                Entry?.onClick?.Invoke();
            });
        }
    }
}