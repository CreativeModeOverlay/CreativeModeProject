using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class BaseFocusableImageWidget : MonoBehaviour
    {
        public RawImage targetImage;
        public RectTransform screenContainer;
        public ZoomType minZoomType;
        
        public float focusLerpSpeed;

        public float Width => imageWidth;
        public float Height => imageHeight;
        
        public Rect SafeVisibleRegion { get; private set; }
        public Rect FullVisibleRegion { get; private set; }
        public bool FullImageVisible { get; private set; }

        public RectOffset Padding
        {
            get => padding;
            set
            {
                if (padding == value)
                    return;
                
                padding = value;
                RecalculateTargetPosition();
            }
        }
        
        protected virtual FocusParams Focus
        {
            get => focusParams;
            set
            {
                if(focusParams == value)
                    return;
                
                focusParams = value; 
                RecalculateTargetPosition();
            }
        }

        public virtual bool FlipY => false;

        [SerializeField]
        private FocusParams focusParams;
        
        [SerializeField]
        private RectOffset padding;
        
        private DisplayParams currentDisplayParams;
        private DisplayParams targetDisplayParams;
        private Rect screenContainerRect;
        
        private int imageWidth;
        private int imageHeight;

        private void OnEnable()
        {
            ApplyPosition();
        }

        protected virtual void Update()
        {
            var rect = screenContainer.rect;
            
            if (screenContainerRect != rect)
            {
                screenContainerRect = rect;
                ApplyPosition();
            }
            else
            {
                UpdatePosition();
            }
        }

        protected void SetTexture(Texture texture, int width, int height)
        {
            targetImage.texture = texture;
            imageWidth = width;
            imageHeight = height;
            ApplyPosition();
        }

        private void RecalculateTargetPosition()
        {
            targetDisplayParams = CalculateDisplayParams(imageWidth, imageHeight, focusParams);
        }
        
        private void UpdatePosition()
        {
            currentDisplayParams = LerpDisplayParams(currentDisplayParams, targetDisplayParams, 
                Time.deltaTime * focusLerpSpeed);

            ApplyDisplayParams(currentDisplayParams);
        }

        private void ApplyPosition()
        {
            var p = CalculateDisplayParams(imageWidth, imageHeight, focusParams);
            currentDisplayParams = p;
            targetDisplayParams = p;
            ApplyDisplayParams(p);
        }

        private DisplayParams CalculateDisplayParams(int width, int height, FocusParams s)
        {
            var containerRect = screenContainer.rect;

            var screenRect = Rect.MinMaxRect(padding.left, padding.bottom,
                containerRect.width - padding.right, containerRect.height - padding.top);

            var monitorRect = Rect.MinMaxRect(s.crop.left, s.crop.bottom,
                width - s.crop.right, height - s.crop.top);

            var minScale = minZoomType == ZoomType.CenterFit
                ? Mathf.Min(
                    screenRect.width / monitorRect.width,
                    screenRect.height / monitorRect.height)
                : Mathf.Max(
                    screenRect.width / monitorRect.width,
                    screenRect.height / monitorRect.height);
            
            var targetScale = s.zoomUseContainerSize
                ? s.zoom * minScale
                : Mathf.Max(s.zoom, minScale);

            var scaledMonitorRect = new Rect(
                monitorRect.x * targetScale,
                monitorRect.y * targetScale,
                monitorRect.width * targetScale,
                monitorRect.height * targetScale);

            var paddingOffset = new Vector2(padding.left, padding.bottom);

            var positionOffset = new Vector2(
                (screenRect.width - scaledMonitorRect.width) / 2f,
                (screenRect.height - scaledMonitorRect.height) / 2f);

            var halfWidth = screenRect.width / 2f;
            var halfHeight = screenRect.height / 2f;

            var xCapture = Mathf.Clamp(s.focusCenter.x - s.crop.left, monitorRect.xMin, monitorRect.xMax) * targetScale;
            var yCapture = Mathf.Clamp(s.focusCenter.y - s.crop.bottom, monitorRect.yMin, monitorRect.yMax) *
                           targetScale;

            var focusOffset = new Vector2(
                -Mathf.Clamp(xCapture - halfWidth, 0,
                    scaledMonitorRect.width - screenRect.width),
                -Mathf.Clamp(yCapture - halfHeight, 0,
                    scaledMonitorRect.height - screenRect.height));

            var fullyVisible = true;

            if (positionOffset.x < 0)
            {
                fullyVisible = false;
                positionOffset.x = focusOffset.x;
            }

            if (positionOffset.y < 0)
            {
                fullyVisible = false;
                positionOffset.y = focusOffset.y;
            }

            Rect uvRect;

            if (FlipY)
            {
                uvRect = Rect.MinMaxRect(s.crop.left, s.crop.top,
                    width - s.crop.right, height - s.crop.bottom);

                uvRect = Rect.MinMaxRect(
                    uvRect.xMin / width,
                    uvRect.yMax / height,
                    uvRect.xMax / width,
                    uvRect.yMin / height);
            }
            else
            {
                uvRect = Rect.MinMaxRect(s.crop.left, s.crop.bottom,
                    width - s.crop.right, height - s.crop.top);

                uvRect = Rect.MinMaxRect(
                    uvRect.xMin / width,
                    uvRect.yMin / height,
                    uvRect.xMax / width,
                    uvRect.yMax / height);
            }

            return new DisplayParams
            {
                scale = Vector3.one * targetScale,
                imageRect = new Rect(paddingOffset + positionOffset, monitorRect.size),

                uvRect = uvRect,

                safeRegion = new Rect(
                    -focusOffset.x / targetScale + s.crop.left,
                    -focusOffset.y / targetScale + s.crop.bottom,
                    Mathf.Min(screenRect.width, scaledMonitorRect.width) / targetScale,
                    Mathf.Min(screenRect.height, scaledMonitorRect.height) / targetScale),

                fullRegionRect = Rect.MinMaxRect(
                    Mathf.Clamp(SafeVisibleRegion.xMin - padding.left, monitorRect.xMin, monitorRect.xMax),
                    Mathf.Clamp(SafeVisibleRegion.yMin - padding.bottom, monitorRect.yMin, monitorRect.yMax),
                    Mathf.Clamp(SafeVisibleRegion.xMax + padding.right, monitorRect.xMin, monitorRect.xMax),
                    Mathf.Clamp(SafeVisibleRegion.yMax + padding.top, monitorRect.yMin, monitorRect.yMax)),

                fullyVisible = fullyVisible
            };
        }
        
        private void ApplyDisplayParams(DisplayParams p)
        {
            var imageRect = targetImage.rectTransform;
            imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, p.imageRect.width);
            imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, p.imageRect.height);
            imageRect.anchoredPosition = new Vector2(
                Mathf.Round(p.imageRect.position.x), 
                Mathf.Round(p.imageRect.position.y));
            imageRect.localScale = p.scale;
        
            targetImage.uvRect = p.uvRect;
            SafeVisibleRegion = p.safeRegion;
            FullVisibleRegion = p.fullRegionRect;
            FullImageVisible = p.fullyVisible;
        }

        private DisplayParams LerpDisplayParams(DisplayParams from, DisplayParams to, float t)
        {
            return new DisplayParams
            {
                imageRect = RectUtils.Lerp(from.imageRect, to.imageRect, t),
                scale = Vector3.Lerp(from.scale, to.scale, t),
                uvRect = RectUtils.Lerp(from.uvRect, to.uvRect, t),
                safeRegion = RectUtils.Lerp(from.safeRegion, to.safeRegion, t),
                fullRegionRect = RectUtils.Lerp(from.fullRegionRect, to.fullRegionRect, t),
                fullyVisible = to.fullyVisible
            };
        }
        
        [ContextMenu("Update position")]
        private void UpdatePositionContextMenu()
        {
            targetDisplayParams = CalculateDisplayParams(imageWidth, imageHeight, Focus);
        }

        private struct DisplayParams
        {
            public Rect imageRect;
            public Vector3 scale;

            public Rect uvRect;
            public Rect safeRegion;
            public Rect fullRegionRect;
            public bool fullyVisible;
        }
        
        public enum ZoomType
        {
            CenterFit,
            CenterCrop,
        }
    }
}