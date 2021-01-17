namespace CreativeMode
{
    public interface IVideoLayer : IVideoElement
    {
        IVideoRenderer Background { get; set; }
    }
}