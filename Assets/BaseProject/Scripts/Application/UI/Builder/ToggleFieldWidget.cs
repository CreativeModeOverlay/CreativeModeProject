using UnityEngine.UI;

namespace CreativeMode
{
    public class ToggleFieldWidget : BaseFieldWidget<bool>, IToggleFieldWidget
    {
        public Toggle toggle;

        public override bool Value
        {
            get => toggle.isOn;
            set => toggle.isOn = value;
        }
    }
}