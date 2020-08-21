﻿using System.Collections.Generic;

namespace CreativeMode
{
    public interface IOverlayElement : IOverlayRenderer
    {
        void OnElementEnabled();
        void OnElementDisabled();
    }
}