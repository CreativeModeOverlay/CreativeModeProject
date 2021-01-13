using System;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CreativeMode
{
    public class DragAndDropReceiver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private DragAndDropManager Manager => DragAndDropManager.Instance;

        public Graphic receiveStateGraphic;
        public Color canReceiveTint;
        public Color canApplyObjectTint;
        public Color cannotApplyObjectTint;

        private Func<object, bool> objectValidator;
        private Action<object> objectReceiver;
        private IDisposable draggedObjectsDisposable;
        
        private bool isSupportedObjectDragged;
        private object draggedObject;
        
        private void Awake()
        {
            receiveStateGraphic.color = Color.clear;
            
            SetReceiver<string>(s => {});
        }

        private void OnEnable()
        {
            draggedObjectsDisposable = Manager.DraggedObjects.Subscribe(o =>
            {
                isSupportedObjectDragged = o.Any(v => IsObjectSupported(v.value));
                UpdateReceiveState();
            });
        }

        private void OnDisable()
        {
            draggedObjectsDisposable?.Dispose();
        }

        public void SetReceiver<T>(Action<T> onReceive)
        {
            objectValidator = o => o is T;
            objectReceiver = o => onReceive((T) o);
        }
        
        public void SetReceiver<T>(Func<T, bool> onValidate, Action<T> onReceive)
        {
            SetReceiver(o => o is T t && onValidate(t), o => onReceive((T) o));
        }

        public void SetReceiver(Func<object, bool> onValidate, Action<object> onReceive)
        {
            objectValidator = onValidate;
            objectReceiver = onReceive;
        }

        public void ClearReceiver()
        {
            objectValidator = null;
            objectReceiver = null;
            isSupportedObjectDragged = false;
        }

        public bool ReceiveObject(object receivedObject)
        {
            draggedObject = null;

            if (!IsObjectSupported(receivedObject)) 
                return false;
            
            OnObjectReceived(receivedObject);
            return true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            draggedObject = Manager.GetDraggedObject(eventData.pointerId);
            
            if(draggedObject == null)
                return;

            Manager.SetActiveReceiver(eventData.pointerId, this);
            UpdateReceiveState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            draggedObject = null;
            Manager.SetActiveReceiver(eventData.pointerId, null);
            UpdateReceiveState();
        }
        
        private bool IsObjectSupported(object obj)
        {
            return objectValidator?.Invoke(obj) ?? false;
        }

        private void OnObjectReceived(object obj)
        {
            objectReceiver?.Invoke(obj);
        }

        private void UpdateReceiveState()
        {
            receiveStateGraphic.DOKill(receiveStateGraphic);

            var color = draggedObject != null
                ? IsObjectSupported(draggedObject) ? canApplyObjectTint : cannotApplyObjectTint 
                : isSupportedObjectDragged ? canReceiveTint : Color.clear;
            
            receiveStateGraphic.DOColor(color, 0.25f);
        }
    }
}