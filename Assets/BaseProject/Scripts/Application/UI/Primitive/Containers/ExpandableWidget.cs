using System;
using DG.Tweening;
using UnityEngine;

namespace CreativeMode
{
    [Serializable]
    public class ExpandableWidget
    {
        public RectTransform rect;
        public float expandDuration = 0.5f;
        public float collapseDuration = 0.5f;
        public Ease easeCollapse = Ease.OutExpo;
        public Ease easeExpand = Ease.OutExpo;

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    
                    if (isExpanded)
                    {
                        OnExpand();
                    }
                    else
                    {
                        OnCollapse();
                    }
                }
            }
        }

        
        private bool isExpanded;
        private bool isOriginalSizeSet;
        private Vector2 originalAnchorMin;
        private Vector2 originalAnchorMax;
        private Vector2 originalAnchorPos;
        private Vector2 originalSizeDelta;

        private void OnCollapse()
        {
            rect.DOAnchorMax(originalAnchorMin, collapseDuration)
                .SetEase(easeCollapse);
            rect.DOAnchorMax(originalAnchorMax, collapseDuration)
                .SetEase(easeCollapse);
            rect.DOAnchorPos(originalAnchorPos, collapseDuration)
                .SetEase(easeCollapse);
            rect.DOSizeDelta(originalSizeDelta, collapseDuration)
                .SetEase(easeCollapse);
        }

        private void OnExpand()
        {
            if (!isOriginalSizeSet)
            {
                originalAnchorMin = rect.anchorMin;
                originalAnchorMax = rect.anchorMax;
                originalSizeDelta = rect.sizeDelta;
                originalAnchorPos = rect.anchoredPosition;
                isOriginalSizeSet = true;
            }
            
            rect.DOAnchorMin(new Vector2(0, 0), expandDuration)
                .SetEase(easeExpand);
            rect.DOAnchorMax(new Vector2(1, 1), expandDuration)
                .SetEase(easeExpand);
            rect.DOAnchorPos(new Vector2(0, 0), expandDuration)
                .SetEase(easeExpand);
            rect.DOSizeDelta(new Vector2(0, 0), expandDuration)
                .SetEase(easeExpand);
        }
    }
}