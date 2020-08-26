using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class OverlayCensorRegionWidget : MonoBehaviour
    {
        [SerializeField]
        private Text titleText;
        
        public ICensorRegion Region { get; set; }

        public void Remove()
        {
            Destroy(gameObject);
        }
        
        protected void Update()
        {
            if(Region == null)
                return;
            
            titleText.text = Region.Title;
            ((RectTransform) transform).SetRect(Region.Rect);
        }
    }
}