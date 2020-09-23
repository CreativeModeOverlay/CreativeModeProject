using UnityEngine;

namespace CreativeMode
{
    public interface IWindowUI : IUIElement
    {
        IWindowUIContainer Container { get; }
        
        void OnAttached(IWindowUIContainer container);
        void OnDetached();
    }
}