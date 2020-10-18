using System;
using System.Globalization;

namespace CreativeMode
{
    public class FloatFieldWidget : BaseInputFieldWidget<float>, IFloatFieldWidget
    {
        public override float Value
        {
            get
            {
                float.TryParse(inputField.text, out var value);
                return value;
            }
            set
            {
                inputField.text = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        protected override bool IsValid(string input) => 
            float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var _);
    }
}