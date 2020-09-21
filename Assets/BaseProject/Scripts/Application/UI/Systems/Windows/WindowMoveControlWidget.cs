using System;
using CreativeMode;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowMoveControlWidget : MonoBehaviour
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

    [Header("Cursors")]
    public CursorTexture horizontalResizeCursor;
    public CursorTexture verticalResizeCursor;
    public CursorTexture leftRightResizeCursor;
    public CursorTexture topBottomResizeCursor;

    [Header("Animation")] 
    public bool animateSquashAndStretch;
    public bool animateShake;
    public AnimationCurve stretchCurve;
    public float maxSquish = 2;
    public float minStretch = 0.5f;
    
    private bool lockCursor;
    private bool isDragActive;
    private Rect dragStartRect;
    private Vector2 dragDelta;
    private Side dragSide;

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

    private void OnDragStarted()
    {
        dragStartRect = windowRect.GetPositonRect();
        isDragActive = true;
    }
    
    private void OnDrag(Vector2 delta, Side side)
    {
        dragDelta = delta;
        dragSide = side;
    }
    
    private void OnDragEnded(Vector2 delta, Side side)
    {
        isDragActive = false;
        
        AnimateWindowToPosition(dragStartRect.Resize(delta, side, windowSize));

        lockCursor = false;
        OnClearCursor();
    }

    private void AnimateWindowToPosition(Rect rect)
    {
        var boundsRect = rect.MoveInBounds(GetContainerRect());

        var ease = boundsRect == rect ? Ease.OutElastic : Ease.OutBack;

        windowRect.DOKill();
        windowRect.DOLocalMove(boundsRect.position, 0.5f).SetEase(ease);
        windowRect.DOSizeDelta(boundsRect.size, 0.5f).SetEase(ease);
        windowRect.DOScale(Vector3.one, 0.5f).SetEase(ease);
    }

    private void Update()
    {
        UpdateDragPosition();
    }

    private void UpdateDragPosition()
    {
        if(!isDragActive)
            return;
        
        var rect = dragStartRect;
        var clampedResize = rect.Resize(dragDelta, dragSide, windowSize);
        Rect resultRect;
        float xScale = 1;
        float yScale = 1;
        
        if (animateSquashAndStretch)
        {
            float DivideSize(float left, float right) => right < 1 ? left : left / right;

            var unclampedResize = rect.Resize(dragDelta, dragSide);
            resultRect = new Rect(unclampedResize.position, clampedResize.size);
            
            xScale = stretchCurve.Evaluate(DivideSize(unclampedResize.width, clampedResize.width));
            yScale = stretchCurve.Evaluate(DivideSize(unclampedResize.height, clampedResize.height));

            if (xScale < 0)
                xScale = 0;

            if (yScale < 0)
                yScale = 0;
            
            if (dragSide == Side.Left || dragSide == Side.Right)
            {
                var xScaleInverse = stretchCurve.Evaluate(DivideSize(clampedResize.width, unclampedResize.width));
                
                yScale = Mathf.Clamp(xScaleInverse, minStretch, maxSquish);
                var yCenter = (resultRect.yMin + resultRect.yMax) / 2f;
                var yNew = yCenter + (resultRect.y - yCenter) * yScale;

                resultRect.y = yNew;
            }
            else if (dragSide == Side.Top || dragSide == Side.Bottom)
            {
                var yScaleInverse = stretchCurve.Evaluate(DivideSize(clampedResize.height, unclampedResize.height));
                xScale = Mathf.Clamp(yScaleInverse, minStretch, maxSquish);
                
                var xCenter = (resultRect.xMin + resultRect.xMax) / 2f;
                var xNew = xCenter + (resultRect.x - xCenter) * xScale;
                
                resultRect.x = xNew;
            }
            
            if ((dragSide & Side.Right) == Side.Right)
                resultRect.x = unclampedResize.xMax - (resultRect.width * xScale);

            if ((dragSide & Side.Top) == Side.Top)
                resultRect.y = unclampedResize.yMax - (resultRect.height * yScale);
        }
        else
        {
            resultRect = clampedResize;
        }

        windowRect.DOKill();
        windowRect.SetPositionRect(resultRect);
        windowRect.localScale = new Vector3(xScale, yScale, 1f);
        
        if (animateShake)
        {
            var shake = Mathf.Max(
                Mathf.Clamp01(Mathf.Abs(xScale - 1)), 
                Mathf.Clamp01(Mathf.Abs(yScale - 1)));
            var shakeVector = Vector3.zero;

            if (!Mathf.Approximately(shake, 0))
            {
                shakeVector.x = shake * UnityEngine.Random.value * 10;
                shakeVector.y = shake * UnityEngine.Random.value * 10;
            }
            
            windowRect.localPosition += shakeVector;
        }
    }

    private Rect GetContainerRect()
    {
        return windowRect.parent.AsRectTransform().rect;
    }

    private void OnSetNewCursor(CursorTexture cursor)
    {
        if (!isDragActive)
            Cursor.SetCursor(cursor.cursor, cursor.hotspot, CursorMode.Auto);
    }

    private void OnClearCursor()
    {
        if (!isDragActive)
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private class MoveWatcher : MonoBehaviour, 
        IDragHandler, 
        IBeginDragHandler, 
        IEndDragHandler, 
        IPointerEnterHandler, 
        IPointerExitHandler
    {
        public Side Side { get; set; }
        public WindowMoveControlWidget Owner { get; set; }
        public CursorTexture CursorTexture { get; set; }

        private Vector2 lastDragDelta;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (CursorTexture != null && CursorTexture.cursor != null)
                Owner.OnSetNewCursor(CursorTexture);
            
            Owner.OnDragStarted();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            lastDragDelta = eventData.position - eventData.pressPosition;
            Owner.OnDrag(lastDragDelta, Side);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Owner.OnDragEnded(lastDragDelta, Side);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (CursorTexture != null && CursorTexture.cursor != null)
                Owner.OnSetNewCursor(CursorTexture);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Owner.OnClearCursor();
        }
    }
}
