using System;

namespace CreativeMode
{
    [Serializable]
    public class TimerAppWidget : AppWidget
    {
        public DateTime startTime;
        public DateTime endTime;
    }
}