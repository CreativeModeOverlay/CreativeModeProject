﻿namespace CreativeMode
{
    public interface IOverlayTransition : IOverlayElement
    {
        IOverlayRenderer From { get; set; }
        IOverlayRenderer To { get; set; }

        bool IsTransitionFinished { get; }
        
        void StartTransition();
        void FinishTransition();
    }
}