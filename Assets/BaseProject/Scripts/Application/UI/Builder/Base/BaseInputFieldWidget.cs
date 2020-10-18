using UnityEngine.UI;

namespace CreativeMode
{
    public abstract class BaseInputFieldWidget<T> : BaseFieldWidget<T>, IInputFieldWidget<T>
    {
        public InputField inputField;
        public Text placeholderText;

        public string Placeholder
        {
            get => placeholderText.text;
            set => placeholderText.text = value;
        }
        
        public override bool IsInputValid => IsValid(inputField.text);

        protected abstract bool IsValid(string input);
    }
}