namespace CreativeMode
{
    public interface IVideoTransition : IVideoElement
    {
        IVideoRenderer From { get; set; }
        IVideoRenderer To { get; set; }

        bool IsTransitionFinished { get; }
        
        void StartTransition();
        void FinishTransition();
    }
}