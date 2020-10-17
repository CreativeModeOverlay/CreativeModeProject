using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace CreativeMode
{
    public class DragAndDropManager : MonoBehaviour
    {
        public static DragAndDropManager Instance { get; private set; }

        private readonly Dictionary<int, DraggedObject> draggedObjects = new Dictionary<int, DraggedObject>();
        private readonly Dictionary<int, DragAndDropReceiver> activeReceivers = new Dictionary<int, DragAndDropReceiver>();

        public IObservable<DraggedObject[]> DraggedObjects => draggedObjectsSubject;
        public IObservable<DragStartEvent> DragStarted => dragStartedSubject;
        public IObservable<DragEndEvent> DragEnded => dragEndedSubject;
        
        private readonly BehaviorSubject<DraggedObject[]> draggedObjectsSubject 
            = new BehaviorSubject<DraggedObject[]>(new DraggedObject[0]);
        
        private readonly Subject<DragStartEvent> dragStartedSubject = new Subject<DragStartEvent>();
        private readonly Subject<DragEndEvent> dragEndedSubject = new Subject<DragEndEvent>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            DraggedObjects.Subscribe(o =>
            {
                if (o.Length > 0)
                {
                    CursorManager.Instance.LockCursor(this, CursorType.DragAndDrop);
                }
                else
                {
                    CursorManager.Instance.UnlockCursor(this);
                }
            });
        }

        public void SetActiveReceiver(int pointerId, DragAndDropReceiver receiver)
        {
            activeReceivers[pointerId] = receiver;
        }

        public void SetDraggedObject(int pointerId, string objectName, object dragAndDropObject)
        {
            if (dragAndDropObject == null)
            {
                Debug.Log("Trying to drag null object");
                return;
            }

            var draggedObject = new DraggedObject
            {
                name = objectName,
                value = dragAndDropObject
            };
            
            draggedObjects[pointerId] = draggedObject;
            draggedObjectsSubject.OnNext(draggedObjects.Values.ToArray());
            dragStartedSubject.OnNext(new DragStartEvent
            {
                pointerId = pointerId,
                draggedObject = draggedObject
            });
        }

        public object GetDraggedObject(int pointerId)
        {
            if(draggedObjects.TryGetValue(pointerId, out var draggedObject)) 
                return draggedObject.value;
            
            return null;
        }

        public void ApplyDraggedObject(int pointerId)
        {
            var isReceived = false;

            draggedObjects.TryGetValue(pointerId, out var draggedObject);
            
            if (activeReceivers.TryGetValue(pointerId, out var receiver) && 
                receiver != null && draggedObject.value != null)
            {
                isReceived = receiver.ReceiveObject(draggedObject.value);
            }
            
            dragEndedSubject.OnNext(new DragEndEvent
            {
                pointerId = pointerId,
                isSuccess = isReceived,
                draggedObject = draggedObject
            });
            
            activeReceivers.Remove(pointerId);
            
            if (draggedObjects.Remove(pointerId))
                draggedObjectsSubject.OnNext(draggedObjects.Values.ToArray());
        }
        
        public struct DragStartEvent
        {
            public int pointerId;
            public DraggedObject draggedObject;
        }
        
        public struct DragEndEvent
        {
            public int pointerId;
            public bool isSuccess;
            public DraggedObject draggedObject;
        }
    }
}