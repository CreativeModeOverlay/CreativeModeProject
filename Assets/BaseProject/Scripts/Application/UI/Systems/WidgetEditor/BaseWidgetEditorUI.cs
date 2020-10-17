﻿using System;
using UnityEngine;

namespace CreativeMode
{
    public abstract class BaseWidgetEditorUI<T> : MonoBehaviour, IWidgetEditorUI
        where T : AppWidget
    {
        public UIContentSize size = UIContentSize.GetDefault();

        public GameObject Root => gameObject;
        public UIContentSize Size => size;

        public Type DataType => typeof(T);

        public AppWidget Data
        {
            get => GetData();
            set
            {
                if (value is T tValue)
                    SetData(tValue);
            }
        }

        protected abstract void SetData(T widget);
        protected abstract T GetData();
    }
}