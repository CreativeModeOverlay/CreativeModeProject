﻿using System;

namespace CreativeMode
{
    [Serializable]
    public class BaseWidget { }
    
    public class CaptureDeviceWidget : BaseWidget
    {
        public string deviceName;
    }
    
    public class TextNoteWidget : BaseWidget
    {
        public int[] noteId;
    }
    
    public class TimerWidget : BaseWidget
    {
        public DateTime startTime;
        public DateTime endTime;
    }
    
    public class MusicPlayerWidget : BaseWidget { }
    public class MusicSpectrumWidget : BaseWidget { }
    public class MusicWaveformWidget : BaseWidget { }
    public class SongLyricsWidget : BaseWidget { }
}