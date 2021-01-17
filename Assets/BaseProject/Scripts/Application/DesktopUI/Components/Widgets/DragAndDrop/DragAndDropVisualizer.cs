using System;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace CreativeMode
{
    public class DragAndDropVisualizer : MonoBehaviour
    {
        public GenericText itemPrefab;
        public RectTransform root;
        
        private readonly List<DragAndDropInstance> instances 
            = new List<DragAndDropInstance>();

        private void Start()
        {
            DragAndDropManager.Instance.DragStarted
                .Subscribe(s => OnDragStart(s.pointerId, s.draggedObject.name))
                .AddTo(this);
            
            DragAndDropManager.Instance.DragEnded
                .Subscribe(s => OnDragEnd(s.pointerId, s.isSuccess))
                .AddTo(this);
        }

        private void OnDragStart(int pointerId, string name)
        {
            var panel = Instantiate(itemPrefab, root, false);
            panel.gameObject.SetActive(true);
            panel.Text = name;

            AnimateAppear(panel);

            instances.Add(new DragAndDropInstance
            {
                pointerId = pointerId,
                panel = panel
            });
        }

        private void OnDragEnd(int pointerId, bool isSuccess)
        {
            for (var i = 0; i < instances.Count; i++)
            {
                var instance = instances[i];

                if (instance.pointerId != pointerId) 
                    continue;
                
                AnimateDisappear(instance.panel, isSuccess);
                instances.RemoveAt(i);
                return;
            }
        }
        
        private void Update()
        {
            for (var i = 0; i < instances.Count; i++)
            {
                var instance = instances[i];

                DesktopUIInputModule.Instance.GetPointerData(instance.pointerId, out var pointer);
                MoveToPointerPosition(instance.panel, pointer.position);
            }
        }

        private void AnimateAppear(GenericText panel)
        {
            panel.RectTransform.localScale = Vector2.zero;
            panel.Group.alpha = 0;

            DOTween.Sequence()
                .Insert(0, panel.RectTransform.DOScale(Vector3.one, 0.25f))
                .Insert(0, panel.Group.DOFade(1f, 0.25f));
        }

        private void MoveToPointerPosition(GenericText panel, Vector2 position)
        {
            panel.RectTransform.localPosition = position;
        }

        private void AnimateDisappear(GenericText panel, bool isSuccess)
        {
            DOTween.Sequence()
                .Insert(0, panel.RectTransform.DOScale(isSuccess 
                    ? new Vector3(0.5f, 0.5f, 0.5f) 
                    : new Vector3(1.5f, 1.5f, 1.5f), 0.25f))
                .Insert(0, panel.Group.DOFade(0f, 0.25f))
                .OnComplete(() => Destroy(panel.gameObject));
        }
        
        private struct DragAndDropInstance
        {
            public int pointerId;
            public GenericText panel;
        }
    }
}