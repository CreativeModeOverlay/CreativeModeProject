using System.Collections.Generic;
using DG.Tweening;
using ThreeDISevenZeroR.XmlUI;
using UnityEngine;

namespace CreativeMode
{
    public class DoTweenChildAnimator : IChildElementAnimator
    {
        private struct AppearData
        {
            public XmlLayoutElement element;
            public CanvasGroup group;
            public Rect target;
        }
        
        private struct DisappearData
        {
            public XmlLayoutElement element;
            public CanvasGroup group;
        }
        
        private struct MoveData
        {
            public XmlLayoutElement element;
            public Vector2 vector;
        }
        
        private List<AppearData> childAppearList = new List<AppearData>();
        private List<DisappearData> childDisappearList = new List<DisappearData>();
        private List<MoveData> childMoveList = new List<MoveData>();
        private List<MoveData> containerResize = new List<MoveData>();

        private float disappearAnimationDuration = 0.25f;
        private float moveAnimationDuration = 0.25f;
        private float appearAnimationDuration = 0.25f;
        
        public void AnimateChildAppear(XmlLayoutElement element, CanvasGroup group, Rect target)
        {
            childAppearList.Add(new AppearData
            {
                element = element,
                group = group,
                target = target
            });
        }

        public void AnimateChildDisappear(XmlLayoutElement element, CanvasGroup group)
        {
            childDisappearList.Add(new DisappearData
            {
                element = element,
                group = group
            });
        }
        
        public void AnimateChildMove(XmlLayoutElement element, Vector2 position)
        {
            childMoveList.Add(new MoveData
            {
                element = element,
                vector = position
            });
        }

        public void AnimateContainerResize(XmlLayoutElement element, Vector2 size)
        {
            containerResize.Add(new MoveData
            {
                element = element,
                vector = size
            });
        }

        public void StartAnimation()
        {
            var offset = 0f;

            var useDisappear = childDisappearList.Count > 0;
            var useMoveOrResize = childMoveList.Count > 0 || containerResize.Count > 0;
            var useAppear = childAppearList.Count > 0;

            if (useDisappear)
            {
                for (var i = 0; i < childDisappearList.Count; i++)
                    DoChildDisappear(childDisappearList[i], offset);

                offset += disappearAnimationDuration;
            }

            if (useMoveOrResize)
            {
                for (var i = 0; i < containerResize.Count; i++)
                    DoContainerResize(containerResize[i], offset);
                
                for (var i = 0; i < childMoveList.Count; i++)
                    DoChildMove(childMoveList[i], offset);

                offset += moveAnimationDuration;
            }

            if (useAppear)
            {
                for (var i = 0; i < childAppearList.Count; i++)
                    DoChildAppear(childAppearList[i], offset);
            }
        }

        private void DoChildDisappear(DisappearData data, float offset)
        {
            var rect = data.element.RectTransform;

            DOTween.Sequence()
                .SetId(this)
                .SetDelay(offset)
                .OnComplete(() =>
                {
                    data.element.DeactivateHierarchy();
                });

            //rect.DOScale(Vector2.zero, disappearAnimationDuration).SetId(this).SetDelay(offset);
        }
        
        private void DoChildAppear(AppearData data, float offset)
        {
            var rect = data.element.RectTransform;

            DOTween.Sequence()
                .SetId(this)
                .SetDelay(offset)
                .OnComplete(() =>
                {
                    rect.anchoredPosition = data.target.position;
                    rect.sizeDelta = data.target.size;

                    data.element.ActivateHierarchy();
                });
        }

        private void DoChildMove(MoveData data, float offset)
        {
            var rect = data.element.RectTransform;

            rect.DOAnchorPos(data.vector, moveAnimationDuration)
                .SetId(this)
                .SetDelay(offset);
        }
        
        private void DoContainerResize(MoveData data, float offset)
        {
            var rect = data.element.RectTransform;

            rect.DOSizeDelta(data.vector, moveAnimationDuration)
                .SetId(this)
                .SetDelay(offset);
        }

        public void FinishAnimation()
        {
            DOTween.Kill(this, true);
            
            childAppearList.Clear();
            childDisappearList.Clear();
            childMoveList.Clear();
            containerResize.Clear();
        }
    }
}