using UnityEngine;

namespace CreativeMode
{
    public class SongLyrics
    {
        public string voice;
        public Line[] lines;
        
        public class Line
        {
            public string text;
            public float startTime;
            public float endTime;

            // Visualization
            public Color? color;
            public Position? position;
            public string font;
        
            public enum Position
            {
                Left, 
                Center, 
                Right
            }
        }
    }
}