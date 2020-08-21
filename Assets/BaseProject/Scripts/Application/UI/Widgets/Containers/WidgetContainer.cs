﻿using System;
using UnityEngine;

namespace CreativeMode
{
    public class WidgetContainer : MonoBehaviour, WidgetPanel.IWidgetContainer
    {
        public RectTransform widgetRoot;
        public RectTransform sizeTransform;
        public CanvasGroup widgetCanvasGroup;
        public RectTransform.Axis containerAxis;

        private GameObject currentWidget;

        public int Size
        {
            get
            {
                return (int) (containerAxis == RectTransform.Axis.Horizontal
                    ? sizeTransform.rect.width
                    : sizeTransform.rect.height);
            }
            set
            {
                sizeTransform.SetSizeWithCurrentAnchors(containerAxis, value);
            }
        }

        public bool IsEmpty => !currentWidget;

        public void PutWidget(GameObject widget)
        {
            if(currentWidget)
                throw new ArgumentException("Container already contains widget");
            
            widget.transform.parent = widgetRoot;
            currentWidget = widget;
        }

        public GameObject PopWidget()
        {
            if (currentWidget)
            {
                var widget = currentWidget;
                currentWidget.transform.parent = null;
                currentWidget = null;
                return widget;
            }

            return null;
        }
    }
}