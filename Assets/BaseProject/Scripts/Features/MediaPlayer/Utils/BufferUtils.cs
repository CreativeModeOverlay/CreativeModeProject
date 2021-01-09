namespace CreativeMode
{
    public static class BufferUtils
    {
        public static int ExtractChannel(float[] inBuffer, float[] outBuffer, int size, int channel, int channelCount)
        {
            var position = 0;
            
            for (var i = channel; i < size; i += channelCount)
            {
                outBuffer[position++] = inBuffer[i];
            }

            return position;
        }
    }
}