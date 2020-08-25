using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class CensorRegionWidget : MonoBehaviour
    {
        public Text titleText;
        
        public ICensorRegion Region { get; set; }

        public void Remove()
        {
            Destroy(gameObject);
        }
        
        private void Update()
        {
            if(Region == null)
                return;
            
            titleText.text = Region.Title;
            ((RectTransform) transform).SetRect(Region.Rect);
        }
    }
}