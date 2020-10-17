using System;

namespace CreativeMode
{
    [Serializable]
    public class TimerWidget : AppWidget
    {
        public DateTime startTime;
        public DateTime endTime;
    }
}