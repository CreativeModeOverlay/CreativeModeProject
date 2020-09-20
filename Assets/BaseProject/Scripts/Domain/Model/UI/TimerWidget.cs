using System;

namespace CreativeMode
{
    [Serializable]
    public class TimerWidget : BaseWidget
    {
        public DateTime startTime;
        public DateTime endTime;
    }
}