using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace CreativeMode
{
    internal class OverlayWidgetUIPanel : MonoBehaviour, IOverlayWidgetUIPanel
    {
        public string Id => panelId;
        public RectOffset ContentPadding => contentPadding;

        public RectTransform widgetRoot;
        public OverlayWidgetRenderer uiRendererPrefab;

        [Header("Display")] 
        public WidgetPanelLayout layout;
        public AnimationTimings animationTimings;
        public TextAnchor appearDirection;

        [Header("Meta")]
        [SerializeField]
        private string panelId;
        [SerializeField]
        private RectOffset contentPadding;

        private bool isLayoutChanged;
        private readonly List<WidgetController> currentControllers = new List<WidgetController>();
        private readonly List<WidgetController> controllersToAdd = new List<WidgetController>();
        private readonly List<WidgetController> controllersToRemove = new List<WidgetController>();
        
        public IOverlayWidgetController<T> AddWidget<T>() where T : struct
        {
            var widgetRenderer = Instantiate(uiRendererPrefab, widgetRoot);
            var rectTransform = (RectTransform) widgetRenderer.transform;
            widgetRenderer.gameObject.SetActive(false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = Vector2.zero;
            
            widgetRenderer.DataType = typeof(T);
            var controller = new WidgetController<T>()
            {
                panel = this,
                renderer = widgetRenderer,
                IsVisible = true
            };

            controllersToAdd.Add(controller);

            return controller;
        }

        private void Update()
        {
            if (controllersToAdd.Count > 0 || 
                controllersToRemove.Count > 0 || 
                isLayoutChanged)
            {
                currentControllers.AddRange(controllersToAdd);
                AnimateLayoutChanges(currentControllers, controllersToAdd, controllersToRemove);
                controllersToAdd.Clear();
                controllersToRemove.Clear();
                
                isLayoutChanged = false;
            }
        }

        private void RemoveWidgetController(WidgetController controller)
        {
            if (controllersToAdd.Contains(controller))
            {
                controllersToAdd.Remove(controller);
                return;
            }
            
            if (currentControllers.Remove(controller))
                controllersToRemove.Add(controller);
        }

        private void MarkLayoutAsDirty()
        {
            isLayoutChanged = true;
        }
        
        private void AnimateLayoutChanges(
            List<WidgetController> currentContainers, 
            List<WidgetController> addedContainers, 
            List<WidgetController> removedContainers)
        {
            var rects = CalculateWidgetPositions(currentContainers);
            var hasMoves = IsLayoutDifferent(currentContainers, addedContainers, rects);
            var movedContainerCount = currentContainers.Count - addedContainers.Count;
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
                AnimateRemove(removedContainers[i].renderer, removeStaggerOffset);
                removeStaggerOffset += animationTimings.removeAnimationStagger;
            }
            
            for (var i = 0; i < rects.Length; i++)
            {
                var containerRect = rects[i];
                var info = currentContainers[i];
                info.targetRect = containerRect;

                if (addedContainers.Contains(info))
                {
                    AnimateAppear(info.renderer, containerRect, addOffset + addStaggerOffset);
                    addStaggerOffset += animationTimings.addAnimationStagger;
                }
                else
                {
                    AnimateMove(info.renderer, containerRect, moveOffset + moveStaggerOffset);
                    moveStaggerOffset += animationTimings.moveAnimationStagger;
                }
            }
        }

        private Rect[] CalculateWidgetPositions(List<WidgetController> allControllers)
        {
            return layout.LayoutWidgets(transform.AsRectTransform().rect, allControllers);
        }

        private bool IsLayoutDifferent(List<WidgetController> currentControllers, 
            List<WidgetController> ignoreControllers, Rect[] newLayout)
        {
            if (currentControllers.Count != newLayout.Length)
                return true;

            for (var i = 0; i < currentControllers.Count; i++)
            {
                var c = currentControllers[i];

                if (!ignoreControllers.Contains(c) && c.targetRect != newLayout[i])
                    return true;
            }

            return false;
        }

        private void AnimateAppear(OverlayWidgetRenderer container, Rect targetPosition, float offset)
        {
            var rectTransform = (RectTransform) container.transform;
            rectTransform.localPosition = targetPosition.position + GetAppearOffset();
            rectTransform.sizeDelta = targetPosition.size;
            container.canvasGroup.alpha = 0f;

            container.gameObject.SetActive(true);

            rectTransform.DOLocalMove(targetPosition.position, animationTimings.addAnimationDuration)
                .SetAutoKill()
                .SetDelay(offset)
                .SetEase(animationTimings.addEase);
            
            container.canvasGroup.DOFade(1f, animationTimings.addAnimationDuration)
                .SetDelay(offset)
                .SetEase(animationTimings.addEase);
        }

        private void AnimateMove(OverlayWidgetRenderer container, Rect targetPosition, float offset)
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

        private void AnimateRemove(OverlayWidgetRenderer container, float offset)
        {
            var rectTransform = (RectTransform) container.transform;
            var currentPosition = rectTransform.localPosition;
            var removeOffset = (Vector3) GetAppearOffset();
            var canvasGroup = container.canvasGroup;
            
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

        public bool IsVisible { get; set; }
        
        private class WidgetController : WidgetPanelLayout.IWidget
        {
            private bool isVisible = true;
            private LayoutParams layoutParams;
            
            public OverlayWidgetUIPanel panel;
            public Rect targetRect;
            public OverlayWidgetRenderer renderer;

            public bool IsVisible
            {
                get => isVisible;
                set
                {
                    isVisible = value;
                    NotifyLayoutDirty();
                }
            }

            public LayoutParams LayoutParams
            {
                get => layoutParams;
                set
                {
                    layoutParams = value;
                    NotifyLayoutDirty();
                }
            }

            public ContentSize Size => renderer.WidgetUI?.Size ?? ContentSize.GetDefault();

            public void Remove()
            {
                panel.RemoveWidgetController(this);
                panel = null;
            }

            private void NotifyLayoutDirty()
            {
                if(panel) 
                    panel.MarkLayoutAsDirty();
            }
        }
        
        private class WidgetController<T> : WidgetController, IOverlayWidgetController<T> 
            where T : struct
        {
            public T Data
            {
                get => (T) renderer.Data;
                set => renderer.Data = value;
            }
        }
    }
}
