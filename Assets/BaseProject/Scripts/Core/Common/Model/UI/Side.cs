using System;

namespace CreativeMode
{
    [Flags]
    public enum Side
    {
        Top = 1, 
        Left = 2, 
        Right = 4, 
        Bottom = 8,
        
        Horizontal = Left | Right,
        Vertical = Bottom | Top,
        
        All = Horizontal | Vertical
    }
}