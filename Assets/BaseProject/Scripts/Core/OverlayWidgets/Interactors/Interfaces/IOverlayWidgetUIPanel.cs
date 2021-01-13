using UnityEngine;

namespace CreativeMode
{
    public interface IOverlayWidgetUIPanel
    {
        string Id { get; }
        
        bool IsVisible { get; set; }
        RectOffset ContentPadding { get; }

        IOverlayWidgetController<T> AddWidget<T>() 
            where T : struct;
    }
}