using System;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class GenericButton : GenericText
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;

        private Action onClickListener;
     
        public Sprite Icon
        {
            get => icon ? icon.sprite : null;
            set => icon.sprite = value;
        }

        public Action OnClick
        {
            get => onClickListener;
            set => onClickListener = value;
        }

        private void Awake()
        {
            button.onClick.AddListener(() => { onClickListener?.Invoke(); });
        }
    }
}