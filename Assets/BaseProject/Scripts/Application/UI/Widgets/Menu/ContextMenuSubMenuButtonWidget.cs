using System;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class ContextMenuSubMenuButtonWidget : ContextMenuEntryWidget<Menu.SubMenu>
    {
        public Button buttonWidget;
        
        public Vector2 SubMenuSpawnPosition
        {
            get
            {
                var rect = ((RectTransform) transform).rect;
                return new Vector2(rect.xMax, rect.yMax); // top right
            }
        }

        public event Action OnSubMenuOpen;
        
        protected override Behaviour ButtonComponent => buttonWidget;
        
        private void Awake()
        {
            buttonWidget.onClick.AddListener(() => OnSubMenuOpen?.Invoke());
        }
    }
}