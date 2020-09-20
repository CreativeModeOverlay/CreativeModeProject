using System;

namespace CreativeMode
{
    [Serializable]
    public class TimerWidget : Widget
    {
        public DateTime startTime;
        public DateTime endTime;
    }
}