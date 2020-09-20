using UnityEngine;

namespace CreativeMode
{
    public struct LyricLine
    {
        public AudioMetadata metadata;
        
        public string voiceId;
        public string voiceDisplayName;
        public SongLyrics lyrics;

        public int lineIndex;
        public string lineText;
        public CustomFont lineFont;
        public TextAlignment? lineAlignment;
        public Color32? lineColor;
        
        public class CustomFont
        { 
            public Font font;
            public float scale = 1f;
            public bool alignByGeometry;
        }
    }
}