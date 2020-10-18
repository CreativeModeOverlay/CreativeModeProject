using System.Collections.Generic;
using UnityEngine.UI;

namespace CreativeMode
{
    public class DropdownFieldWidget : BaseFieldWidget<string>, IDropdownFieldWidget
    {
        public Dropdown dropdown;
        
        private string[] values;

        public override string Value
        {
            get => dropdown.captionText.text;
            set
            {
                for (var i = 0; i < values.Length; i++)
                {
                    if (values[i] == value)
                    {
                        dropdown.value = i;
                        return;
                    }
                }

                dropdown.captionText.text = value;
            }
        }

        public string[] Values
        {
            get => values;
            set
            {
                values = value;
                UpdateOptions();
            }
        }

        private void UpdateOptions()
        {
            var options = new List<Dropdown.OptionData>();
            
            foreach (var value in values)
            {
                options.Add(new Dropdown.OptionData(value));
            }

            dropdown.options = options;
        }
    }
}