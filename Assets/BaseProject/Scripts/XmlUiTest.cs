using System;
using DefaultNamespace;
using ThreeDISevenZeroR.XmlUI;
using UnityEngine;

public class XmlUiTest : MonoBehaviour
{
    public LayoutInflater inflater;
    public TextAsset layoutAsset;
    public TextAsset contentAsset;
    public XmlLayoutElement parentElement;

    private VariableProvider provider;
    public string testText;

    public void Awake()
    {
        provider = new VariableProvider();
        provider.SetValue("Test", "Test title");
        
        var windowLayout = inflater.InflateChild(parentElement, layoutAsset.text, provider);
        var contentLayout = inflater.InflateChild(windowLayout, contentAsset.text, provider);
        
        AddAnimatorRecursive(contentLayout);
    }

    public void Update()
    {
        provider.SetValue("Test", testText);
    }
    
    private void AddAnimatorRecursive(XmlLayoutElement element)
    {
        element.SetChildAnimator(new DoTweenChildAnimator());

        foreach (var child in element.Children)
        {
            AddAnimatorRecursive(child);
        }
    }
}