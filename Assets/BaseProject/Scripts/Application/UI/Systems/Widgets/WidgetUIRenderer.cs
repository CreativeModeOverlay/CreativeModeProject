using System;
using CreativeMode.Impl;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class WidgetUIRenderer : MonoBehaviour, ILayoutElement
    {
        private IWidgetUIFactory UIFactory => Instance<IWidgetUIFactory>.Get();
        
        public RectTransform widgetRoot;

        public IWidgetUI WidgetUI { get; private set; }
        public Widget Data
        {
            get => widgetData;
            set
            {
                widgetData = value;
                
                if (widgetData == null)
                {
                    RemoveWidget();
                    return;
                }

                Type dataType = widgetData.GetType();

                if (WidgetUI != null && WidgetUI.DataType != dataType)
                    RemoveWidget();

                if (WidgetUI == null)
                    CreateWidget(dataType);

                UpdateWidget(widgetData);
            }
        }

        private Widget widgetData;

        private void RemoveWidget()
        {
            WidgetUI = null;
        }

        private void UpdateWidget(Widget data)
        {
            WidgetUI.SetData(data);
        }

        private void CreateWidget(Type type)
        {
            if (UIFactory.SupportsUI(type))
            {
                var widget = UIFactory.CreateWidgetUI(type);
                WidgetUI = widget;
                
                TransformUtils.FillRectParent(widget.Root, widgetRoot);

                var size = widget.Size;
                
                minWidth = size.minWidth;
                minHeight = size.minHeight;
                preferredWidth = size.defaultWidth;
                preferredHeight = size.defaultHeight;
                
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform) transform);
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