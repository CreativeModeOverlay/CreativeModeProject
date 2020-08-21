﻿namespace CreativeMode
{
    public interface IOverlayLayer : IOverlayElement
    {
        IOverlayRenderer Background { get; set; }
    }
}