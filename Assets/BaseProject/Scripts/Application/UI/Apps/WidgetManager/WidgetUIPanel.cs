using System;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace CreativeMode
{
    public class WidgetUIPanel : MonoBehaviour
    {
        public string panelId;
        
        public RectTransform widgetRoot;
        public WidgetUIContainer uiContainerPrefab;

        [Header("Display")] 
        public WidgetPanelLayout layout;
        public AnimationTimings animationTimings;
        public TextAnchor appearDirection;

        private List<ContainerInfo> currentContainers = new List<ContainerInfo>();
        private IWidgetUIFactory Factory => Instance<IWidgetUIFactory>.Get();
        private IWidgetManager Manager => Instance<IWidgetManager>.Get();

        public void Start()
        {
            Manager.WidgetsForPanel(panelId)
                .Subscribe(OnListUpdated)
                .AddTo(this);

            Manager.WidgetUpdated
                .Subscribe(OnWidgetUpdated)
                .AddTo(this);
        }

        [ContextMenu("UpdateLayout")]
        public void UpdateLayout()
        {
            var rects = CalculateContainerPositions(currentContainers);
            float animOffset = 0f;

            for (var i = 0; i < currentContainers.Count; i++)
            {
                var c = currentContainers[i];
                var r = rects[i];
                
                if (r != c.targetRect)
                {
                    c.targetRect = r;
                    AnimateMove(c.container, r, animOffset);
                    animOffset += animationTimings.moveAnimationStagger;
                }
            }
        }

        private void OnWidgetUpdated(WidgetData widget)
        {
            for (var i = 0; i < currentContainers.Count; i++)
            {
                var container = currentContainers[i];
                if (container.widgetId == widget.id)
                    container.Widget.SetData(widget.data);
            }
        }

        private void OnListUpdated(IReadOnlyList<WidgetPanel.Widget> newList)
        {
            var unclaimedContainers = new List<ContainerInfo>(currentContainers);
            currentContainers.Clear();
            
            ContainerInfo ReuseContainer(int id)
            {
                for (var i = 0; i < unclaimedContainers.Count; i++)
                {
                    var container = unclaimedContainers[i];

                    if (container.widgetId == id)
                    {
                        unclaimedContainers.RemoveAt(i);
                        return container;
                    }
                }

                return null;
            }
            
            var addedContainers = new List<ContainerInfo>(newList.Count);

            for (var i = 0; i < newList.Count; i++)
            {
                var panel = newList[i];
                var container = ReuseContainer(panel.widgetId);

                if (container == null)
                {
                    container = CreateContainer(panel);
                    addedContainers.Add(container);
                }

                container.Layout = panel.layout;
                currentContainers.Add(container);
            }

            AnimateLayoutChanges(currentContainers, addedContainers, unclaimedContainers);
        }

        private void AnimateLayoutChanges(
            List<ContainerInfo> allContainers, 
            List<ContainerInfo> addedContainers, 
            List<ContainerInfo> removedContainers)
        {
            var rects = CalculateContainerPositions(allContainers);
            var hasMoves = IsLayoutDifferent(allContainers, addedContainers, rects);
            var movedContainerCount = allContainers.Count - addedContainers.Count;
            var removeStaggerOffset = 0f;
            var moveStaggerOffset = 0f;
            var addStaggerOffset = 0f;

            var removeDuration = removedContainers.Count > 0
                ? animationTimings.removeAnimationDuration +
                  (removedContainers.Count - 1) * animationTimings.removeAnimationStagger
                : 0;

            var moveDuration = hasMoves && movedContainerCount > 0
                ? animationTimings.moveAnimationDuration +
                  (movedContainerCount - 1) * animationTimings.moveAnimationStagger
                : 0;

            var moveOffset = removeDuration;
            var addOffset = moveOffset + moveDuration;

            for (var i = 0; i < removedContainers.Count; i++)
            {
                AnimateRemove(removedContainers[i].container, removeStaggerOffset);
                removeStaggerOffset += animationTimings.removeAnimationStagger;
            }
            
            for (var i = 0; i < rects.Length; i++)
            {
                var containerRect = rects[i];
                var info = allContainers[i];
                info.targetRect = containerRect;

                if (addedContainers.Contains(info))
                {
                    AnimateAppear(info.container, containerRect, addOffset + addStaggerOffset);
                    addStaggerOffset += animationTimings.addAnimationStagger;
                }
                else
                {
                    AnimateMove(info.container, containerRect, moveOffset + moveStaggerOffset);
                    moveStaggerOffset += animationTimings.moveAnimationStagger;
                }
            }
        }

        private Rect[] CalculateContainerPositions(List<ContainerInfo> allContainers)
        {
            return layout.LayoutWidgets(transform.AsRectTransform().rect, allContainers);
        }

        private bool IsLayoutDifferent(List<ContainerInfo> currentContainers, 
            List<ContainerInfo> ignoreContainers, Rect[] newLayout)
        {
            if (currentContainers.Count != newLayout.Length)
                return true;

            for (var i = 0; i < currentContainers.Count; i++)
            {
                var c = currentContainers[i];

                if (!ignoreContainers.Contains(c) && c.targetRect != newLayout[i])
                    return true;
            }

            return false;
        }

        private ContainerInfo CreateContainer(WidgetPanel.Widget p)
        {
            var container = Instantiate(uiContainerPrefab, widgetRoot);
            var rectTransform = (RectTransform) container.transform;
            container.gameObject.SetActive(false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = Vector2.zero;

            var widgetData = Manager.GetWidget(p.widgetId);

            if (Factory.CanCreateUI(widgetData.type))
            {
                var ui = Factory.CreateWidgetUI(widgetData.type);
                ui.SetData(widgetData.data);
                container.PutWidget(ui);
            }

            return new ContainerInfo
            {
                widgetId = widgetData.id,
                container = container,
                Layout = p.layout
            };
        }
        
        private void AnimateAppear(WidgetUIContainer container, Rect targetPosition, float offset)
        {
            var rectTransform = (RectTransform) container.transform;
            rectTransform.localPosition = targetPosition.position + GetAppearOffset();
            rectTransform.sizeDelta = targetPosition.size;
            container.widgetCanvasGroup.alpha = 0f;

            container.gameObject.SetActive(true);

            rectTransform.DOLocalMove(targetPosition.position, animationTimings.addAnimationDuration)
                .SetAutoKill()
                .SetDelay(offset)
                .SetEase(animationTimings.addEase);
            
            container.widgetCanvasGroup.DOFade(1f, animationTimings.addAnimationDuration)
                .SetDelay(offset)
                .SetEase(animationTimings.addEase);
        }

        private void AnimateMove(WidgetUIContainer container, Rect targetPosition, float offset)
        {
            var rectTransform = (RectTransform) container.transform;

            rectTransform.DOLocalMove(targetPosition.position, animationTimings.moveAnimationDuration)
                .SetDelay(offset)
                .SetEase(animationTimings.moveEase);
            
            rectTransform.DOSizeDelta(targetPosition.size, 
                animationTimings.moveAnimationDuration)
                .SetDelay(offset)
                .SetEase(animationTimings.moveEase);
        }

        private Vector2 GetAppearOffset()
        {
            var appearScale = appearDirection.GetSignedScale();
            var containerSize = transform.AsRectTransform().rect.size;
            return Vector2.Scale(appearScale, containerSize);
        }

        private void AnimateRemove(WidgetUIContainer container, float offset)
        {
            var rectTransform = (RectTransform) container.transform;
            var currentPosition = rectTransform.localPosition;
            var removeOffset = (Vector3) GetAppearOffset();
            var canvasGroup = container.widgetCanvasGroup;
            
            rectTransform.DOLocalMove(currentPosition + removeOffset, animationTimings.removeAnimationDuration)
                .SetAutoKill()
                .SetDelay(offset)
                .SetEase(animationTimings.removeEase)
                .OnComplete(() =>
                {
                    DOTween.Kill(rectTransform);
                    DOTween.Kill(canvasGroup);
                    Destroy(container.gameObject);
                });
            
            canvasGroup.DOFade(0, animationTimings.removeAnimationDuration)
                .SetAutoKill()
                .SetDelay(offset)
                .SetId(container)
                .SetEase(animationTimings.removeEase);
        }
        
        [Serializable]
        public class AnimationTimings
        {
            public float removeAnimationDuration = 0.25f;
            public float removeAnimationStagger = 0.1f;

            public float addAnimationDuration = 0.25f;
            public float addAnimationStagger = 0.1f;

            public float moveAnimationDuration = 0.5f;
            public float moveAnimationStagger = 0;

            public Ease removeEase;
            public Ease addEase;
            public Ease moveEase;
        }
        
        private class ContainerInfo : WidgetPanelLayout.IWidget
        {
            public Rect targetRect;
            public int widgetId;
            
            public WidgetUIContainer container;

            public WidgetUISize Size => container.Widget.Size;
            public IWidgetUI Widget => container.Widget;
            public WidgetLayoutParams Layout { get; set; }
        }
    }
}
