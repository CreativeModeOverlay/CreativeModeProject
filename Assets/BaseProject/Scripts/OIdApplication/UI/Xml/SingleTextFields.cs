using UnityEngine.UI;

namespace CreativeMode.UI
{
    public class TextField : BaseSingleInputValueField<string>
    {
        protected override bool TryParse(string text, out string result)
        {
            result = text;
            return true;
        }
    }

    public class IntField : BaseSingleInputValueField<int>
    {
        protected override bool TryParse(string text, out int result) => 
            int.TryParse(text, out result);
    }
    
    public class FloatField : BaseSingleInputValueField<float>
    {
        protected override bool TryParse(string text, out float result) => 
            float.TryParse(text, out result);
    }

    public class BooleanField : BaseValueField<bool>
    {
        public Toggle toggle;

        public override bool Value
        {
            get => toggle.isOn;
            set => toggle.isOn = value;
        }
        
        public override bool IsValid => true;
    }
}