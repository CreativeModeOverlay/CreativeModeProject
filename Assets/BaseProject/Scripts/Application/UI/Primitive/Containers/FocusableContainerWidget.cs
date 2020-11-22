using System;
using UnityEngine;

namespace CreativeMode
{
    public class FocusableContainerWidget : MonoBehaviour
    {
        public int Width => contentWidth;
        public int Height => contentHeight;

        public Rect VisibleRect { get; private set; }
        public Rect SafeVisibleRect { get; private set; }
        public bool FullContentVisible { get; private set; }

        public RectOffset Padding
        {
            get => padding;
            set
            {
                if (value == padding)
                    return;

                padding = value;
                RecalculateTargetPosition();
            }
        }

        public FocusParams Focus
        {
            get => focus;
            set
            {
                if(focus.Equals(value))
                    return;

                focus = value;
                RecalculateTargetPosition();
            }
        }
        
        [Header("References")]
        public RectTransform contentRect;
        public RectTransform maskRect;
        public RectTransform boundsRect;

        [Header("Visuals")]
        public bool clipMaskToBounds;
        public float displayUpdateSpeed;
        
        [SerializeField]
        private int contentWidth;
        
        [SerializeField]
        private int contentHeight;
        
        [SerializeField]
        private RectOffset padding;
        
        [SerializeField]
        private FocusParams focus;

        private ContentLocation currentLayout;
        private ContentLocation targetLayout;
        private Rect targetLayoutRect;
        private bool skipAnimationOnNextUpdate;

        public void SetContentSize(int width, int height)
        {
            if(contentWidth == width && contentHeight == height)
                return;

            contentWidth = width;
            contentHeight = height;
            skipAnimationOnNextUpdate = true;
        }

        private void OnEnable()
        {
            skipAnimationOnNextUpdate = true;
        }

        private void Update()
        {
            UpdateLayout();
        }
        
        private void RecalculateTargetPosition()
        {
            targetLayout = CalculateContentLocation();
            targetLayoutRect = boundsRect.rect;
        }
        
        private void UpdateLayout()
        {
            var currentRect = boundsRect.rect;

            if (skipAnimationOnNextUpdate || currentRect != targetLayoutRect)
            {
                skipAnimationOnNextUpdate = false;
                var p = CalculateContentLocation();
                currentLayout = p;
                targetLayout = p;
                targetLayoutRect = currentRect;
                ApplyLayout(p);
            }
            else
            {
                currentLayout = LerpLayoutParams(currentLayout, targetLayout, 
                    Time.deltaTime * displayUpdateSpeed);

                ApplyLayout(currentLayout);
            }
        }

        private ContentLocation CalculateContentLocation()
        {
            var screenRect = boundsRect.rect;
            var paddedScreen = screenRect.ApplyOffset(padding);
            
            var sourceSize = new Vector2(contentWidth, contentHeight);
            var focusPoint = focus.normalizedFocusCenter ? focus.focusCenter * sourceSize : focus.focusCenter;
            var sourceRect = new Rect(-focusPoint, sourceSize);
            var croppedSource = sourceRect.ApplyOffset(focus.crop);
            var containerScale = 1f;

            if (focus.zoomUseContainerSize)
            {
                var horizontalScale = paddedScreen.width / croppedSource.width;
                var verticalScale = paddedScreen.height / croppedSource.height;

                containerScale = focus.zoomType == FocusParams.ZoomType.Fit
                    ? Mathf.Max(horizontalScale, verticalScale)
                    : Mathf.Min(horizontalScale, verticalScale);
            }

            var totalScale = containerScale * focus.zoom;
            var scaledSource = croppedSource.Scale(totalScale);

            scaledSource.position += paddedScreen.center;
            
            var envelopedSource = scaledSource.Envelope(paddedScreen, TextAnchor.MiddleCenter);
            var uncroppedSource = envelopedSource.ApplyOffset(focus.crop, -totalScale);

            return new ContentLocation
            {
                contentRect = uncroppedSource,
                maskRect = envelopedSource,
                rectScale = totalScale
            };
        }
        
        private void ApplyLayout(ContentLocation layout)
        {
            maskRect.ResetAnchors();
            contentRect.ResetAnchors();
            
            contentRect.localScale = Vector3.one;
            
            var bounds = boundsRect.rect;
            var resultMask = layout.maskRect;

            if (clipMaskToBounds)
                resultMask = resultMask.Crop(bounds);

            if (Mathf.Approximately(layout.rectScale, 1))
            {
                maskRect.localPosition = new Vector3(
                    Mathf.Round(resultMask.position.x), 
                    Mathf.Round(resultMask.position.y));
            }
            else
            {
                maskRect.localPosition = resultMask.position;
            }
            
            if (layout.rectScale != 0)
            {
                maskRect.sizeDelta = resultMask.size / layout.rectScale;
                maskRect.localScale = new Vector3(layout.rectScale, layout.rectScale, layout.rectScale);
                contentRect.localPosition = (layout.contentRect.position - resultMask.position) / layout.rectScale;
                contentRect.sizeDelta = layout.contentRect.size / layout.rectScale;
            }
            else
            {
                maskRect.sizeDelta = Vector3.zero;
                maskRect.localScale = Vector3.zero;
                contentRect.localPosition = Vector3.zero;
                contentRect.sizeDelta = Vector3.one;
            }

            var boundsSafe = bounds.ApplyOffset(padding);
            var bottomLeft = contentRect.InverseTransformPoint(boundsRect.TransformPoint(bounds.min));
            var topRight = contentRect.InverseTransformPoint(boundsRect.TransformPoint(bounds.max));
            var safeBottomLeft = contentRect.InverseTransformPoint(boundsRect.TransformPoint(boundsSafe.min));
            var safeTopRight = contentRect.InverseTransformPoint(boundsRect.TransformPoint(boundsSafe.max));

            VisibleRect = Rect.MinMaxRect(
                Mathf.Max(bottomLeft.x, 0),
                Mathf.Max(bottomLeft.y, 0),
                Mathf.Min(topRight.x, contentWidth),
                Mathf.Min(topRight.y, contentHeight));

            SafeVisibleRect = Rect.MinMaxRect(
                Mathf.Max(safeBottomLeft.x, 0),
                Mathf.Max(safeBottomLeft.y, 0),
                Mathf.Min(safeTopRight.x, contentWidth),
                Mathf.Min(safeTopRight.y, contentHeight));

            FullContentVisible = Mathf.RoundToInt(SafeVisibleRect.x) == 0 && 
                                 Mathf.RoundToInt(SafeVisibleRect.y) == 0 && 
                                 Mathf.RoundToInt(SafeVisibleRect.xMax) == contentWidth &&
                                 Mathf.RoundToInt(SafeVisibleRect.yMax) == contentHeight;
        }
        
        private ContentLocation LerpLayoutParams(ContentLocation from, ContentLocation to, float t)
        {
            return new ContentLocation
            {
                contentRect = RectUtils.Lerp(from.contentRect, to.contentRect, t),
                maskRect = RectUtils.Lerp(from.maskRect, to.maskRect, t),
                rectScale = Mathf.Lerp(from.rectScale, to.rectScale, t),
            };
        }
        
        private struct ContentLocation
        {
            public float rectScale;
            public Rect maskRect;
            public Rect contentRect;
        }
    }
}