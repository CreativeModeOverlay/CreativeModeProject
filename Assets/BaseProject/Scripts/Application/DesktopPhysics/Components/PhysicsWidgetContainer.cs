using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CreativeMode
{
    public class PhysicsWidgetContainer : MonoBehaviour
    {
        [FormerlySerializedAs("widgetCollider")]
        public BoxCollider2D widgetCollider2d;

        public BoxCollider widgetCollider3d;
        public RectTransform[] skipTransforms;

        public int maxWidth;
        public int maxHeight;
        public int maxPassCount = 8;
        private HashSet<RectTransform> skipTransformsSet;

        private void Awake()
        {
            skipTransformsSet = new HashSet<RectTransform>(skipTransforms);
        }

        public void SetEnableCollisions(bool enable)
        {
            if (widgetCollider2d)
                widgetCollider2d.enabled = enable;

            if (widgetCollider3d)
                widgetCollider3d.enabled = enable;
        }

        [ContextMenu("Update Container")]
        public void UpdateContainerSize()
        {
            var maxChildWidth = 0f;
            var maxChildHeight = 0f;

            for (var p = 0; p < maxPassCount; p++)
            {
                var currentMaxChildWidth = 0f;
                var currentMaxChildHeight = 0f;

                for (var i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i) as RectTransform;

                    if (child == null || skipTransformsSet.Contains(child))
                        continue;

                    child.sizeDelta = new Vector2(maxWidth, maxHeight);

                    LayoutRebuilder.ForceRebuildLayoutImmediate(child);
                    var childWidth = Mathf.Min(LayoutUtility.GetPreferredWidth(child), maxWidth);
                    var childHeight = Mathf.Min(LayoutUtility.GetPreferredHeight(child), maxHeight);
                    child.sizeDelta = new Vector2(childWidth, childHeight);

                    currentMaxChildWidth = Mathf.Max(childWidth, currentMaxChildWidth);
                    currentMaxChildHeight = Mathf.Max(childHeight, currentMaxChildHeight);
                }

                var isMeasureComplete = currentMaxChildWidth == maxChildWidth &&
                                        currentMaxChildHeight == maxChildHeight;

                maxChildWidth = currentMaxChildWidth;
                maxChildHeight = currentMaxChildHeight;

                if (isMeasureComplete)
                    break;
            }

            var rectTransform = (RectTransform) transform;
            rectTransform.sizeDelta = new Vector2(maxChildWidth, maxChildHeight);

            if (widgetCollider2d)
            {
                widgetCollider2d.size = new Vector2(maxChildWidth, maxChildHeight);
            }

            if (widgetCollider3d)
            {
                widgetCollider3d.size = new Vector3(maxChildWidth, maxChildHeight, 0.1f);
            }
        }
    }
}