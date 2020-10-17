using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class GenericPanel : MonoBehaviour
    {
        public RectTransform RectTransform => transform as RectTransform;

        [SerializeField] 
        private Text text;

        [SerializeField]
        private CanvasGroup group;
        
        public string Text
        {
            get => text.text;
            set => text.text = value;
        }

        public CanvasGroup Group => group;
    }
}