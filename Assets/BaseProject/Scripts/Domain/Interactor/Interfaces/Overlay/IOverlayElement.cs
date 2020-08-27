namespace CreativeMode
{
    public interface IOverlayElement : IOverlayRenderer
    {
        void OnElementEnabled();
        void OnElementDisabled();
    }
}