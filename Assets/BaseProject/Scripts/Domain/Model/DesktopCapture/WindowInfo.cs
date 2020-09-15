﻿using UnityEngine;

namespace CreativeMode
{
    public struct WindowInfo
    {
        public bool valid;
        public int processId;
        public int desktop;
        public Rect rect;
        public Rect programRect;
        public string title;
        public string processName;
        public string exePath;
    }
}