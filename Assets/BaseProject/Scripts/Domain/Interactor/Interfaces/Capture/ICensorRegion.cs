﻿using UnityEngine;

namespace CreativeMode
{
    public interface ICensorRegion
    {
        int Monitor { get; }
        string Title { get; }
        Rect Rect { get; }
    }
}