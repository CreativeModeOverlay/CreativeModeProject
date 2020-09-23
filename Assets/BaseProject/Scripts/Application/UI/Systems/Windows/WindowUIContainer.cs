using System;
using CreativeMode;
using UnityEngine;
using UnityEngine.UI;

public class WindowUIContainer : MonoBehaviour, IWindowUIContainer
{
    [Header("References")]
    [SerializeField] private RectPositionControlWidget rectControlWidget;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private Graphic backgroundGraphic;
    [SerializeField] private Text windowTitle;
    
    [Header("Settings")]
    [SerializeField] private RectOffset decorPadding;
    
    public IWindowUI WindowUI { get; private set; }
    public WindowManager Manager { get; set; }

    public RectOffset DecorPadding
    {
        get => decorPadding;
        set
        {
            decorPadding = value;
            UpdateContainerSize();
        }
    }

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
            if(backgroundGraphic)
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
            if(windowTitle) 
                windowTitle.text = value;
        }
    }

    public void PutWindow(IWindowUI window)
    {
        if(WindowUI != null)
            throw new ArgumentException("Container already contains window");
        
        TransformUtils.FillRectParent(window.Root, contentRoot);
        UpdateContainerSize();
        
        WindowUI = window;
    }

    public IWindowUI PopWindow()
    {
        var window = WindowUI;
        WindowUI = null;
        
        if(window != null)
            TransformUtils.ClearRectParent(window.Root);
        
        return window;
    }

    private void UpdateContainerSize()
    {
        if(WindowUI == null)
            return;
        
        rectControlWidget.ContentSize = WindowUI.Size.Apply(decorPadding);
    }

    public void Close()
    {
        
    }
}
