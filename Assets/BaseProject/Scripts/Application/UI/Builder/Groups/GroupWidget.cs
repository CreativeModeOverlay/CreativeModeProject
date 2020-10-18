using UnityEngine.UI;

namespace CreativeMode
{
    public class GroupWidget : BaseGroupWidget, IGroupWidget
    {
        public Text titleText;

        public string Title
        {
            get => titleText.text;
            set => titleText.text = value;
        }
    }
}