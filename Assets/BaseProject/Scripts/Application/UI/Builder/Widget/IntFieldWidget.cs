namespace CreativeMode
{
    public class IntFieldWidget : BaseInputFieldWidget<int>, IIntFieldWidget
    {
        public override int Value
        {
            get
            {
                int.TryParse(inputField.text, out var result);
                return result;
            }
            set
            {
                inputField.text = value.ToString();
            }
        }

        protected override bool IsValid(string input) => int.TryParse(input, out var _);
    }
}