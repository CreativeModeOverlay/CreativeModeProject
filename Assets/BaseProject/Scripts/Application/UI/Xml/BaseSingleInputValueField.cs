using UnityEngine.UI;

namespace CreativeMode.UI
{
    public abstract class BaseSingleInputValueField<T> : BaseValueField<T>
    {
        public InputField inputField;

        public override T Value
        {
            get
            {
                if (TryParse(inputField.text, out var result)) 
                    return result;

                return default;
            }
            set => inputField.text = value?.ToString();
        }

        public override bool IsValid => TryParse(inputField.text, out var _);

        protected abstract bool TryParse(string text, out T result);
    }
}