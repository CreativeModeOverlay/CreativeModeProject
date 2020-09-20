using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class CensorRegionWidget : MonoBehaviour
    {
        [SerializeField]
        private Text regionName;

        [SerializeField]
        private Graphic appearFlashGraphic;
        
        [SerializeField]
        private CanvasGroup widgetGroup;

        public ICensorRegion Region
        {
            get => region;
            set
            {
                this.region = value;
                UpdateCensorRegion();
            }
        }

        private ICensorRegion region;
        private bool isRemoved;

        private void Awake()
        {
            if (appearFlashGraphic)
            {
                appearFlashGraphic.gameObject.SetActive(true);
                appearFlashGraphic.DOFade(0f, 0.15f)
                    .OnComplete(() => appearFlashGraphic.gameObject.SetActive(false));
            }
        }

        public virtual void Remove()
        {
            isRemoved = true;
            
            if (widgetGroup)
            {
                widgetGroup.DOFade(0f, 0.25f)
                    .OnComplete(() => Destroy(gameObject));
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        protected virtual void Update()
        {
            UpdateCensorRegion();
        }

        private void UpdateCensorRegion()
        {
            if(isRemoved || Region == null)
                return;
            
            regionName.text = Region.Title;
            transform.AsRectTransform().SetPositionRect(Region.Rect);
        }
    }
}