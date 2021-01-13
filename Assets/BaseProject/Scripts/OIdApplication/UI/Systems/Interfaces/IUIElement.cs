using UnityEngine;

namespace CreativeMode
{
    public interface IUIElement
    {
        GameObject Root { get; }
        ContentSize Size { get; }
    }
}