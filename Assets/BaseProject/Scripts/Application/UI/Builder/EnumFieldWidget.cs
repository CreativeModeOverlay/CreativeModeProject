using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UI;

namespace CreativeMode
{
    public class EnumFieldWidget : BaseFieldWidget<Enum>, IEnumFieldWidget
    {
        public Dropdown dropdown;

        public override Enum Value
        {
            get => Enum.GetValues(enumType).GetValue(dropdown.value) as Enum;
            set
            {
                dropdown.value = ((IConvertible) value).ToInt32(CultureInfo.InvariantCulture);
            }
        }

        private Type enumType;
        private Dictionary<Enum, string> displayNames = new Dictionary<Enum, string>();

        public EnumFieldWidget SetEnumType(Type type)
        {
            enumType = type;
            UpdateDropdown();

            return this;
        }

        private void UpdateDropdown()
        {
            var values = Enum.GetValues(enumType);
            var options = new List<Dropdown.OptionData>();

            foreach (Enum value in values)
            {
                options.Add(new Dropdown.OptionData(displayNames.TryGetValue(value, out var name) 
                    ? name : value.ToString()));
            }

            dropdown.options = options;
        }

        public Dictionary<Enum, string> DisplayNames
        {
            get => displayNames;
            set
            {
                displayNames = value;
                UpdateDropdown();
            }
        }
    }
}