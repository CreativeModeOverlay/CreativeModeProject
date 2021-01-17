namespace CreativeMode
{
    public interface IVideoElement : IVideoRenderer
    {
        void OnElementEnabled();
        void OnElementDisabled();
    }
}