﻿using UnityEngine.UI;

namespace CreativeMode
{
    public class GroupWidget : BaseGroupWidget, IGroupWidget
    {
        public Text titleText;
        public LinearLayoutGroup linearLayout;

        public string Title
        {
            get => titleText.text;
            set => titleText.text = value;
        }

        public Orientation Orientation
        {
            get => linearLayout.Orientation;
            set => SetLinearLayoutOrientation(linearLayout, value);
        }

        protected override Orientation GroupDefaultOrientation => Orientation;

        private void Awake()
        {
            SetLinearLayoutOrientation(linearLayout, linearLayout.Orientation);
        }
    }
}