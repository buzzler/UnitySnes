using System;

namespace UnitySnes
{
    public class Buffers : IDisposable
    {
        public float[] AudioBuffer;
        public int AudioPosition;
        public int AudioBufferSize;
        public byte[] VideoBuffer;
        public int VideoLineBytes;
        public short[] InputBuffer;
        
        public Buffers(SystemAvInfo info)
        {
            var w = Convert.ToInt32(info.geometry.base_width);
            var h = Convert.ToInt32(info.geometry.base_height);

            AudioBufferSize = 4096;
            AudioBuffer = new float[AudioBufferSize];
            AudioPosition = 0;
            VideoLineBytes = 2 * w;
            VideoBuffer = new byte[VideoLineBytes * h];
            InputBuffer = new short[16];
        }

        public void Dispose()
        {
            AudioBuffer = null;
            VideoBuffer = null;
            InputBuffer = null;
        }
    }
}