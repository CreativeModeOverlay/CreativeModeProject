using System;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class GenericButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Text text;

        private Action onClickListener;
     
        public Sprite Icon
        {
            get => icon ? icon.sprite : null;
            set => icon.sprite = value;
        }

        public string Text
        {
            get => text.text;
            set => text.text = value;
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