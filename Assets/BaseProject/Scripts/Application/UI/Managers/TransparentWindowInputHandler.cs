using UnityEngine;
using UnityEngine.EventSystems;

namespace CreativeMode
{
    public class TransparentWindowInputHandler : MonoBehaviour
    {
        public Camera targetCamera;
        public LayerMask layerMask;
        public TransparentWindow transparentWindow;
        
        private TargetJoint2D colliderJoint;
        private PhysicsScene physics3d;
        private PhysicsScene2D physics2d;

        private void Start()
        {
            transparentWindow.AddFocusHandler(IsFocused);
            physics2d = gameObject.scene.GetPhysicsScene2D();
            physics3d = gameObject.scene.GetPhysicsScene();
        }

        private bool IsFocused(Vector2 cursorPosition)
        {
            var eventSystem = EventSystem.current;
		
            if (eventSystem && eventSystem.IsPointerOverGameObject())
                return true;

            if (physics2d.OverlapPoint(targetCamera.ScreenToWorldPoint(cursorPosition), layerMask))
                return true;

            var cursorRay = targetCamera.ScreenPointToRay(cursorPosition);
            if (physics3d.Raycast(cursorRay.origin, cursorRay.direction, layerMask))
                return true;

            return false;
        }

        private void Update()
        {
            if (!Input.GetMouseButton(0))
            {
                if (colliderJoint)
                {
                    Destroy(colliderJoint);
                    colliderJoint = null;
                }

                return;
            }

            Vector2 pos = targetCamera.ScreenToWorldPoint(Input.mousePosition);

            if (colliderJoint)
            {
                colliderJoint.target = pos;
                return;
            }

            if (!Input.GetMouseButtonDown(0))
                return;

            var overlapCollider = physics2d.OverlapPoint(pos, layerMask);
            
            if (!overlapCollider)
                return;

            var attachedRigidbody = overlapCollider.attachedRigidbody;
            if (!attachedRigidbody)
                return;

            colliderJoint = attachedRigidbody.gameObject.AddComponent<TargetJoint2D>();
            colliderJoint.autoConfigureTarget = false;
            colliderJoint.anchor = attachedRigidbody.transform.InverseTransformPoint(pos);
        }
    }
}