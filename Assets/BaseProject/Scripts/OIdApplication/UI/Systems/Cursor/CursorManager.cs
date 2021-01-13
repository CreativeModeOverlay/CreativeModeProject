using UnityEngine;

namespace CreativeMode
{
    public class CursorManager : MonoBehaviour
    {
        public static CursorManager Instance { get; private set; }

        [Header("Cursors")]
        public CursorTexture horizontalResizeCursor;
        public CursorTexture verticalResizeCursor;
        public CursorTexture diagonalResizeCursor;
        public CursorTexture inverseDiagonalResizeCursor;
        public CursorTexture dragAndDropCursor;

        private CursorType currentCursorType;
        private bool isCursorLocked;
        private MonoBehaviour lockedCursorOwner;
        private CursorType lockedCursorType;

        private void Awake()
        {
            Instance = this;
        }

        public void SetCursor(CursorType type)
        {
            currentCursorType = type;
            UpdateCursor();
        }

        public void LockCursor(MonoBehaviour owner, CursorType type)
        {
            if (!isCursorLocked || owner == lockedCursorOwner)
            {
                isCursorLocked = true;
                lockedCursorType = type;
                lockedCursorOwner = owner;
                UpdateCursor();
            }
        }

        public void UnlockCursor(MonoBehaviour owner)
        {
            if(owner != lockedCursorOwner)
                return;
            
            lockedCursorType = CursorType.Default;
            isCursorLocked = false;
            lockedCursorOwner = null;
            UpdateCursor();
        }

        private void UpdateCursor()
        {
            CursorTexture texture = GetTexture(isCursorLocked ? lockedCursorType : currentCursorType);

            if (texture != null)
            {
                Cursor.SetCursor(texture.cursor, texture.hotspot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }

        private CursorTexture GetTexture(CursorType type)
        {
            switch (type)
            {
                case CursorType.HorizontalResize: return horizontalResizeCursor;
                case CursorType.VerticalResize: return verticalResizeCursor;
                case CursorType.DiagonalResize: return diagonalResizeCursor;
                case CursorType.InverseDiagonalResize: return inverseDiagonalResizeCursor;
                case CursorType.DragAndDrop: return dragAndDropCursor;
                default: return null;
            }
        }
    }
}