using System;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class OverlayWidgetRenderer : MonoBehaviour, ILayoutElement
    {
        private IOverlayWidgetUIFactory UIFactory => Instance<IOverlayWidgetUIFactory>.Get();
        
        public RectTransform widgetRoot;

        public CanvasGroup canvasGroup;

        public IOverlayWidgetUI WidgetUI { get; private set; }
        public object Data
        {
            get => widgetData;
            set
            {
                DataType = value?.GetType();
                widgetData = value;

                UpdateWidget(widgetData);
            }
        }

        public Type DataType
        {
            get => typeof(object);
            set
            {
                if (WidgetUI != null && WidgetUI.DataType != value)
                    RemoveWidget();

                if (WidgetUI == null && value != null)
                    CreateWidget(value);
            }
        }

        private object widgetData;

        private void RemoveWidget()
        {
            if (WidgetUI == null) 
                return;
            
            WidgetUI.OnWidgetDestroyed();
            
            Destroy(WidgetUI.Root);
            WidgetUI = null;
        }

        private void UpdateWidget(object data)
        {
            WidgetUI?.SetData(data);
        }

        private void CreateWidget(Type type)
        {
            if (UIFactory.SupportsUI(type))
            {
                var widget = UIFactory.CreateWidgetUI(type, widgetRoot.gameObject);
                WidgetUI = widget;
                
                TransformUtils.FillRectParent(widget.Root, widgetRoot);

                var size = widget.Size;
                
                minWidth = size.minWidth;
                minHeight = size.minHeight;
                preferredWidth = size.defaultWidth;
                preferredHeight = size.defaultHeight;
                
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) transform);
            }
        }

        public void CalculateLayoutInputHorizontal()
        {
            // noop
        }

        public void CalculateLayoutInputVertical()
        {
            // noop
        }

        public float minWidth { get; private set; }
        public float minHeight { get; private set; }
        public float preferredWidth { get; private set; }
        public float preferredHeight { get; private set; }
        
        public float flexibleWidth => -1;
        public float flexibleHeight => -1;
        public int layoutPriority => 1;
    }
}