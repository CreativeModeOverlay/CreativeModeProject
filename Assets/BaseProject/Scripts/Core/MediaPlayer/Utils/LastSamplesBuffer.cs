using UnityEngine;

namespace CreativeMode
{
    public class LastSamplesBuffer
    {
        private float[] buffer;
        private int bufferPosition;

        public LastSamplesBuffer(int size)
        {
            buffer = new float[size];
        }

        public void Write(float[] inBuffer, int offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                buffer[bufferPosition++] = inBuffer[i + offset];

                if (bufferPosition >= buffer.Length)
                    bufferPosition = 0;
            }
        }

        public int Read(float[] outBuffer, int offset, int count)
        {
            var toRead = Mathf.Min(buffer.Length, count);
            var position = bufferPosition;

            for (var i = 0; i < toRead; i++)
            {
                outBuffer[i + offset] = buffer[position++];

                if (position >= buffer.Length)
                    position = 0;
            }
            
            return toRead;
        }
    }
}