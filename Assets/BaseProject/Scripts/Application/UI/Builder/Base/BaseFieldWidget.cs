using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public abstract class BaseFieldWidget<T> : BaseInterfaceWidget, IFieldWidget<T>
    {
        public Text titleText;
        public Text subtitleText;
        public CanvasGroup canvasGroup;

        public string Title
        {
            get => titleText.text;
            set
            {
                titleText.text = value;
                UpdateTextFieldVisibility(titleText);
            }
        }

        public string Subtitle
        {
            get => subtitleText.text;
            set
            {
                subtitleText.text = value;
                UpdateTextFieldVisibility(subtitleText);
            }
        }

        public bool IsVisible
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        public bool IsEnabled
        {
            get => canvasGroup.interactable;
            set => canvasGroup.interactable = value;
        }

        public virtual bool IsInputValid => true;

        public abstract T Value { get; set; }
        
        public T ValueOrDefault(T defaultValue = default)
        {
            return IsInputValid ? Value : defaultValue;
        }

        protected void Awake()
        {
            titleText.text = "Field";
            subtitleText.text = "";
        }

        protected virtual void Start()
        {
            UpdateTextFieldVisibility(titleText);
            UpdateTextFieldVisibility(subtitleText);
        }

        private void UpdateTextFieldVisibility(Text text)
        {
            var isVisible = !string.IsNullOrWhiteSpace(text.text);
            var isObjectActive = text.gameObject.activeSelf;

            if (isVisible != isObjectActive)
                text.gameObject.SetActive(isVisible);
        }
    }
}