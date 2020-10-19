namespace CreativeMode
{
    public class TextFieldWidget : BaseInputFieldWidget<string>, ITextFieldWidget
    {
        public override string Value
        {
            get => inputField.text;
            set => inputField.text = value;
        }

        protected override bool IsValid(string input) => true;
    }
}