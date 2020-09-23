using CreativeMode;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Cursor = UnityEngine.Cursor;

public class RectPositionControlWidget : MonoBehaviour
{
    [Header("References")]
    public RectTransform targetRect;
    public GameObject resizeTopLeftControl;
    public GameObject resizeTopControl;
    public GameObject resizeTopRightControl;
    public GameObject resizeLeftControl;
    public GameObject resizeRightControl;
    public GameObject resizeBottomLeftControl;
    public GameObject resizeBottomControl;
    public GameObject resizebottomRightControl;
    public GameObject dragControl;
    
    [Header("Settings")]
    [SerializeField] private UIContentSize contentSize = UIContentSize.GetDefault();
    [SerializeField] private float resizeHandleSize = 8;

    [Header("Cursors")]
    public CursorTexture horizontalResizeCursor;
    public CursorTexture verticalResizeCursor;
    public CursorTexture leftRightResizeCursor;
    public CursorTexture topBottomResizeCursor;

    [Header("Animation")] 
    public bool animateSquashAndStretch;
    public bool animateShake;
    public AnimationCurve dragSquishStretch;
    public AnimationCurve volumeSquishStretch;

    public UIContentSize ContentSize
    {
        get => contentSize;
        set
        {
            contentSize = value;
            UpdateHandleActive();
        }
    }

    public float ResizeHandleSize
    {
        get => resizeHandleSize;
        set
        {
            resizeHandleSize = value;
        }
    }
    
    private bool isDragActive;
    private Rect dragStartRect;
    private Vector2 dragDelta;
    private Side dragSide;

    private void Awake()
    {
        AddMoveHandle(resizeTopLeftControl, Side.Top | Side.Left);
        AddMoveHandle(resizeTopControl, Side.Top);
        AddMoveHandle(resizeTopRightControl, Side.Top | Side.Right);
        AddMoveHandle(resizeLeftControl, Side.Left);
        AddMoveHandle(resizeRightControl, Side.Right);
        AddMoveHandle(resizeBottomLeftControl, Side.Bottom | Side.Left);
        AddMoveHandle(resizeBottomControl, Side.Bottom);
        AddMoveHandle(resizebottomRightControl, Side.Bottom | Side.Right);
        AddMoveHandle(dragControl, Side.Top | Side.Bottom | Side.Left | Side.Right);

        UpdateHandleActive();
        UpdateHandleSize();
    }

    private void UpdateHandleActive()
    {
        var canResizeInAllDirections = contentSize.CanResizeWidth && contentSize.CanResizeHeight;
        
        resizeBottomLeftControl.SetActive(canResizeInAllDirections);
        resizebottomRightControl.SetActive(canResizeInAllDirections);
        resizeTopLeftControl.SetActive(canResizeInAllDirections);
        resizeTopRightControl.SetActive(canResizeInAllDirections);
        
        resizeLeftControl.SetActive(contentSize.CanResizeWidth);
        resizeRightControl.SetActive(contentSize.CanResizeWidth);
        
        resizeBottomControl.SetActive(contentSize.CanResizeHeight);
        resizeTopControl.SetActive(contentSize.CanResizeHeight);
    }

    private void UpdateHandleSize()
    {
        var diagonalScale = Vector3.one * resizeHandleSize;
        var horizontalScale = new Vector3(resizeHandleSize, 1, 1);
        var verticalScale = new Vector3(1, resizeHandleSize, 1);

        resizeBottomLeftControl.transform.localScale = diagonalScale;
        resizebottomRightControl.transform.localScale = diagonalScale;
        resizeTopLeftControl.transform.localScale = diagonalScale;
        resizeTopRightControl.transform.localScale = diagonalScale;

        resizeLeftControl.transform.localScale = horizontalScale;
        resizeRightControl.transform.localScale = horizontalScale;
        
        resizeBottomControl.transform.localScale = verticalScale;
        resizeTopControl.transform.localScale = verticalScale;
    }

    private void AddMoveHandle(GameObject obj, Side side)
    {
        if (!obj)
            return;

        var watcher = obj.AddComponent<MoveWatcher>();
        watcher.Owner = this;
        watcher.Side = side;
        watcher.UseDragThreshold = side == Side.All;

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
        dragStartRect = targetRect.GetPositonRect();
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
        
        AnimateWindowToPosition(dragStartRect.Resize(delta, side, contentSize));
        OnClearCursor();
    }

    private void AnimateWindowToPosition(Rect rect)
    {
        var boundsRect = rect.MoveInBounds(GetContainerRect());

        var ease = boundsRect == rect ? Ease.OutElastic : Ease.OutBack;

        targetRect.DOKill();
        targetRect.DOLocalMove(boundsRect.position.Round(), 0.5f).SetEase(ease);
        targetRect.DOSizeDelta(boundsRect.size.Round(), 0.5f).SetEase(ease);
        targetRect.DOScale(Vector3.one, 0.5f).SetEase(ease);
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
        var clampedResize = rect.Resize(dragDelta, dragSide, contentSize);
        Rect resultRect;
        float xScale = 1;
        float yScale = 1;
        
        if (animateSquashAndStretch)
        {
            float DivideSize(float left, float right) => right < 1 ? left : left / right;

            var unclampedResize = rect.Resize(dragDelta, dragSide);
            resultRect = new Rect(unclampedResize.position, clampedResize.size);
            
            xScale = dragSquishStretch.Evaluate(DivideSize(unclampedResize.width, clampedResize.width));
            yScale = dragSquishStretch.Evaluate(DivideSize(unclampedResize.height, clampedResize.height));

            if (xScale < 0)
                xScale = 0;

            if (yScale < 0)
                yScale = 0;
            
            if (dragSide == Side.Left || dragSide == Side.Right)
            {
                yScale = volumeSquishStretch.Evaluate(DivideSize(clampedResize.width, unclampedResize.width));
                var yCenter = (resultRect.yMin + resultRect.yMax) / 2f;
                var yNew = yCenter + (resultRect.y - yCenter) * yScale;

                resultRect.y = yNew;
            }
            else if (dragSide == Side.Top || dragSide == Side.Bottom)
            {
                xScale = volumeSquishStretch.Evaluate(DivideSize(clampedResize.height, unclampedResize.height));
                
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

        resultRect.position = resultRect.position.Round();
        resultRect.size = resultRect.size.Round();

        targetRect.DOKill();
        targetRect.SetPositionRect(resultRect);
        targetRect.localScale = new Vector3(xScale, yScale, 1f);
        
        if (animateShake)
        {
            var shake = Mathf.Max(
                Mathf.Clamp01(Mathf.Abs(xScale - 1)), 
                Mathf.Clamp01(Mathf.Abs(yScale - 1)));
            var shakeVector = Vector3.zero;

            if (!Mathf.Approximately(shake, 0))
            {
                shakeVector.x = shake * Random.value * 10;
                shakeVector.y = shake * Random.value * 10;
            }
            
            targetRect.localPosition += shakeVector;
        }
    }

    private Rect GetContainerRect()
    {
        return targetRect.parent.AsRectTransform().rect;
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
        IPointerExitHandler,
        IInitializePotentialDragHandler
    {
        public Side Side { get; set; }
        public RectPositionControlWidget Owner { get; set; }
        public CursorTexture CursorTexture { get; set; }
        public bool UseDragThreshold { get; set; }

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

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = UseDragThreshold;
        }
    }
}
