using UnityEngine;

namespace CreativeMode
{
    public interface IUIElement
    {
        GameObject Root { get; }
        UIContentSize Size { get; }
    }
}