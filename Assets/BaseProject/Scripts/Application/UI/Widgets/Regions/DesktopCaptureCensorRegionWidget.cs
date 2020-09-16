using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class DesktopCaptureCensorRegionWidget : MonoBehaviour
    {
        [SerializeField]
        private Text titleText;
        
        public DesktopCaptureWidgetUI Widget { get; set; }
        public ICensorRegion Region { get; set; }
        
        public void Remove()
        {
            Destroy(gameObject);
        }
        
        protected void Update()
        {
            if(Region == null)
                return;
            
            titleText.text = $"Censored ({Region.Title})";
            ((RectTransform) transform).SetRect(Region.Rect);
        }
    }
}