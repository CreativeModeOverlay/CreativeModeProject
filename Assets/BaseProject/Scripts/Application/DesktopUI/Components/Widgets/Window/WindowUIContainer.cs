using System;
using ThreeDISevenZeroR.XmlUI;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode.Impl
{
    internal class WindowUIContainer : MonoBehaviour, IWindowUIContainer
    {
        [Header("References")] [SerializeField]
        private RectTransform windowTransform;

        [SerializeField] 
        private XmlLayoutElement contentRoot;

        private Text windowTitle;
        private Graphic backgroundGraphic;

        public IWindowUI WindowUI { get; private set; }
        public WindowManager Manager { get; set; }

        public Color BackgroundColor
        {
            get
            {
                if (!backgroundGraphic)
                    return Color.clear;

                return backgroundGraphic.color;
            }
            set
            {
                if (backgroundGraphic)
                    backgroundGraphic.color = value;
            }
        }

        public string Title
        {
            get
            {
                if (!windowTitle)
                    return null;

                return windowTitle.text;
            }
            set
            {
                if (windowTitle)
                    windowTitle.text = value;
            }
        }

        private void Start()
        {
            windowTitle = contentRoot.FindComponentById<Text>("Title");
            backgroundGraphic = contentRoot.FindComponentById<Graphic>("Content");
        }

        private void Update()
        {
            var delta = windowTransform.sizeDelta;

            if (!Mathf.Approximately(contentRoot.Width.Value, delta.x))
                contentRoot.Width = delta.x;

            if (!Mathf.Approximately(contentRoot.Height.Value, delta.y))
                contentRoot.Height = delta.y;
        }

        public void PutWindow(IWindowUI window)
        {
            if (WindowUI != null)
                throw new ArgumentException("Container already contains window");

            /*TransformUtils.FillRectParent(window.Root, contentRoot);
            UpdateContainerSize();*/

            WindowUI = window;
        }

        public IWindowUI PopWindow()
        {
            var window = WindowUI;
            WindowUI = null;

            if (window != null)
                TransformUtils.ClearRectParent(window.Root);

            return window;
        }

        private void UpdateContainerSize()
        {
            if (WindowUI == null)
                return;
        }

        public void Close()
        {
        }
    }
}