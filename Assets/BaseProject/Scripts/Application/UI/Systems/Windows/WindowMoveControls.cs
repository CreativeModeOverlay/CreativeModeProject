using System;
using CreativeMode;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowMoveControls : MonoBehaviour
{
    public RectTransform windowRect;
    public UIContentSize windowSize;

    public GameObject resizeTopLeftControl;
    public GameObject resizeTopControl;
    public GameObject resizeTopRightControl;
    public GameObject resizeLeftControl;
    public GameObject resizeRightControl;
    public GameObject resizeBottomLeftControl;
    public GameObject resizeBottomControl;
    public GameObject resizebottomRightControl;
    public GameObject dragControl;

    public CursorTexture horizontalResizeCursor;
    public CursorTexture verticalResizeCursor;
    public CursorTexture leftRightResizeCursor;
    public CursorTexture topBottomResizeCursor;
    
    private bool lockCursor;

    private void Awake()
    {
        AddMoveHandler(resizeTopLeftControl, Side.Top | Side.Left);
        AddMoveHandler(resizeTopControl, Side.Top);
        AddMoveHandler(resizeTopRightControl, Side.Top | Side.Right);
        AddMoveHandler(resizeLeftControl, Side.Left);
        AddMoveHandler(resizeRightControl, Side.Right);
        AddMoveHandler(resizeBottomLeftControl, Side.Bottom | Side.Left);
        AddMoveHandler(resizeBottomControl, Side.Bottom);
        AddMoveHandler(resizebottomRightControl, Side.Bottom | Side.Right);
        AddMoveHandler(dragControl, Side.Top | Side.Bottom | Side.Left | Side.Right);
    }

    private void AddMoveHandler(GameObject obj, Side side)
    {
        if (!obj)
            return;

        var watcher = obj.AddComponent<MoveWatcher>();
        watcher.Owner = this;
        watcher.Side = side;

        switch (side)
        {
            case Side.Top:
            case Side.Bottom: 
                watcher.CursorTexture = verticalResizeCursor;
                break;
            
            case Side.Left:
            case Side.Right:
                watcher.CursorTexture = horizontalResizeCursor;
                break;
            
            case Side.Top | Side.Left:
            case Side.Bottom | Side.Right:
                watcher.CursorTexture = leftRightResizeCursor;
                break;
            
            case Side.Top | Side.Right:
            case Side.Bottom | Side.Left:
                watcher.CursorTexture = topBottomResizeCursor;
                break;
        }
    }
    
    private void OnContentMove(Vector2 delta, Side side)
    {
        var rect = windowRect.GetPositonRect();
        var horizontalMove = delta.x;
        var verticalMove = delta.y;

        if ((side & Side.Horizontal) == Side.Horizontal)
        {
            rect.x += horizontalMove;
        }
        else
        {
            if ((side & Side.Left) == Side.Left)
            {
                rect.xMin = Mathf.Clamp(rect.xMin + horizontalMove, 
                    rect.xMax - windowSize.maxWidth,
                    rect.xMax - windowSize.minWidth);
            }
            else if ((side & Side.Right) == Side.Right)
            {
                rect.xMax = Mathf.Clamp(rect.xMax + horizontalMove, 
                    rect.xMin + windowSize.minWidth,
                    rect.xMin + windowSize.maxWidth);
            }
        }
        
        if ((side & Side.Vertical) == Side.Vertical)
        {
            rect.y += verticalMove;
        }
        else
        {
            if ((side & Side.Bottom) == Side.Bottom)
            {
                rect.yMin = Mathf.Clamp(rect.yMin + verticalMove, 
                    rect.yMax - windowSize.maxHeight,
                    rect.yMax - windowSize.minHeight);
            }
            else if ((side & Side.Top) == Side.Top)
            {
                rect.yMax = Mathf.Clamp(rect.yMax + verticalMove, 
                    rect.yMin + windowSize.minHeight,
                    rect.yMin + windowSize.maxHeight);
            }
        }

        windowRect.DOKill();
        windowRect.SetPositionRect(rect);
    }

    private void OnDragEnded()
    {
        var containerRect = windowRect.parent.AsRectTransform().rect;
        var thisRect = windowRect.GetPositonRect();

        if (thisRect.x < containerRect.x)
            thisRect.x = containerRect.x;

        if (thisRect.y < containerRect.y)
            thisRect.y = containerRect.y;

        if (thisRect.xMax > containerRect.xMax)
            thisRect.x = containerRect.xMax - thisRect.width;
        
        if (thisRect.yMax > containerRect.yMax)
            thisRect.y = containerRect.yMax - thisRect.height;

        windowRect.DOLocalMove(thisRect.position, 0.25f)
            .SetEase(Ease.OutBack);

        lockCursor = false;
        OnClearCursor();
    }

    private void OnSetNewCursor(CursorTexture cursor, bool l)
    {
        if (!lockCursor)
        {
            lockCursor = l;
            Cursor.SetCursor(cursor.cursor, cursor.hotspot, CursorMode.Auto);
        }
    }

    private void OnClearCursor()
    {
        if (!lockCursor)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    private class MoveWatcher : MonoBehaviour, 
        IDragHandler, 
        IBeginDragHandler, 
        IEndDragHandler, 
        IPointerEnterHandler, 
        IPointerExitHandler
    {
        public Side Side { get; set; }
        public WindowMoveControls Owner { get; set; }
        public CursorTexture CursorTexture { get; set; }

        public void OnDrag(PointerEventData eventData)
        {
            Owner.OnContentMove(eventData.delta, Side);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var delta = eventData.position - eventData.pressPosition;
            Owner.OnContentMove(delta, Side);
            
            if (CursorTexture != null && CursorTexture.cursor != null)
                Owner.OnSetNewCursor(CursorTexture, true);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Owner.OnDragEnded();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (CursorTexture != null && CursorTexture.cursor != null)
                Owner.OnSetNewCursor(CursorTexture, false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Owner.OnClearCursor();
        }
    }

    [Flags]
    private enum Side
    {
        Top = 1, 
        Left = 2, 
        Right = 4, 
        Bottom = 8,
        
        Horizontal = Left | Right,
        Vertical = Bottom | Top,
        
        All = Horizontal | Vertical
    }
}
