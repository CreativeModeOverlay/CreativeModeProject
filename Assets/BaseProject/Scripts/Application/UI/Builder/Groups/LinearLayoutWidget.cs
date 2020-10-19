namespace CreativeMode
{
    public class LinearLayoutWidget : BaseGroupWidget, ILinearLayoutWidget
    {
        public LinearLayoutGroup linearLayout;

        public Orientation Orientation
        {
            get => linearLayout.Orientation;
            set => SetLinearLayoutOrientation(linearLayout, value);
        }

        protected override Orientation GroupDefaultOrientation => linearLayout.Orientation;

        private void Awake()
        {
            SetLinearLayoutOrientation(linearLayout, linearLayout.Orientation);
        }
    }
}