namespace CreativeMode
{
    public interface IOverlayWidgetController
    {
        bool IsVisible { get; set; }
        LayoutParams LayoutParams { get; set; }

        void Remove();
    }
    
    public interface IOverlayWidgetController<T> : IOverlayWidgetController
        where T : struct
    {
        T Data { get; set; }
    }
}