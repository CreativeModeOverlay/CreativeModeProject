﻿using UnityEngine;

namespace CreativeMode
{
    public interface ICensorRegionController
    {
        string Title { get; set; }
        Rect Rect { get; set; }
        
        void Remove();
    }
}