using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class BaseInterfaceWidget : MonoBehaviour, IInterfaceWidget
    {
        private LayoutParams currentLayoutParams;
        private LayoutElement currentLayoutElement;
        
        public LayoutParams LayoutParams
        {
            get => currentLayoutParams;
            set
            {
                currentLayoutParams = value;
                UpdateLayoutParams();
            }
        }
        
        private void UpdateLayoutParams()
        {
            var hasAny = LayoutParams.minWidth >= 0 || LayoutParams.minHeight >= 0 || 
                         LayoutParams.preferredWidth >= 0 || LayoutParams.preferredHeight >= 0 || 
                         LayoutParams.flexibleWidth >= 0 || LayoutParams.flexibleHeight >= 0;

            if (hasAny && !currentLayoutElement)
            {
                currentLayoutElement = gameObject.AddComponent<LayoutElement>();
                currentLayoutElement.layoutPriority = 10;
            }
            else if (!hasAny && currentLayoutElement)
            {
                Destroy(currentLayoutElement);
            }

            if (currentLayoutElement)
            {
                currentLayoutElement.minWidth = LayoutParams.minWidth;
                currentLayoutElement.minHeight = LayoutParams.minHeight;
                currentLayoutElement.preferredWidth = LayoutParams.preferredWidth;
                currentLayoutElement.preferredHeight = LayoutParams.preferredHeight;
                currentLayoutElement.flexibleWidth = LayoutParams.flexibleWidth;
                currentLayoutElement.flexibleHeight = LayoutParams.flexibleHeight;
            }
        }
    }
}