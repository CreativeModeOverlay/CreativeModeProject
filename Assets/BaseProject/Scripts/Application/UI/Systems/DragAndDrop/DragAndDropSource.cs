using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CreativeMode
{
    public class DragAndDropSource : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private Func<DraggedObject> objectProvider = () => default;
        private bool isDragStarted;

        public void Awake()
        {
            SetObjectProvider(() => "Some other shit");
        }

        public void SetObjectProvider(Func<object> provider)
        {
            if (provider == null)
            {
                SetObjectProvider(null);
                return;
            }
            
            SetObjectProvider(() =>
            {
                var providedObject = provider();
                return new DraggedObject
                {
                    name = providedObject?.ToString(),
                    value = providedObject
                };
            });
        }
        
        public void SetObjectProvider(Func<DraggedObject> provider)
        {
            objectProvider = provider ?? (() => default);
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            var providedObject = objectProvider.Invoke();

            if (providedObject.value == null)
            {
                isDragStarted = false;
                return;
            }

            isDragStarted = true;
            DragAndDropManager.Instance.SetDraggedObject(
                eventData.pointerId, 
                providedObject.name, 
                providedObject.value);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isDragStarted)
            {
                isDragStarted = false;
                DragAndDropManager.Instance.ApplyDraggedObject(eventData.pointerId);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            // noop
        }
    }
}