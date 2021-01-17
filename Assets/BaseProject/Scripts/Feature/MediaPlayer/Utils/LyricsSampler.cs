namespace CreativeMode
{
    public struct LyricsSampler
    {
        private SongLyrics currentLyrics;
        private LineInfo lastLine;

        public LyricsSampler(SongLyrics lyrics)
        {
            currentLyrics = lyrics;
            lastLine = default;
        }

        public LineInfo GetLineForTime(float time)
        {
            var startIndex = 0;
            
            if (lastLine.line != null)
            {
                if (IsLineActive(lastLine.line, time))
                    return lastLine;

                if (lastLine.line.startTime < time) 
                    startIndex = lastLine.index;
            }

            for (var i = startIndex; i < currentLyrics.lines.Length; i++)
            {
                var line = currentLyrics.lines[i];

                if (IsLineActive(line, time))
                {
                    lastLine.index = i;
                    lastLine.line = line;
                    return lastLine;
                }
            }

            return new LineInfo
            {
                line = null,
                index = -1
            };
        }

        private bool IsLineActive(SongLyrics.Line line, float time)
        {
            return line.startTime <= time 
                   && line.endTime > time;
        }
        
        public struct LineInfo
        {
            public int index;
            public SongLyrics.Line line;
        }
    }
}