using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class LinearLayoutGroup : HorizontalOrVerticalLayoutGroup
    {
        public static readonly string layoutOrientationProperty = nameof(layoutOrientation);
        
        [SerializeField]
        private Orientation layoutOrientation;

        public Orientation Orientation
        {
            get => layoutOrientation;
            set
            {
                layoutOrientation = value;
                LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
            }
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, layoutOrientation == Orientation.Vertical);
        }

        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, layoutOrientation == Orientation.Vertical);
        }

        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, layoutOrientation == Orientation.Vertical);
        }

        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, layoutOrientation == Orientation.Vertical);
        }
    }
}