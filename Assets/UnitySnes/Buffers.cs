using System;
using UnityEngine;

namespace UnitySnes
{
    public class Buffers : IDisposable
    {
        public float[] AudioBufferFlush;
        public float[] AudioBuffer;
        public int AudioPosition;
        public int AudioBufferSize;
        public bool AudioUpdated;
        public byte[] VideoBuffer;
        public bool VideoSupport16Bit;
        public int VideoLineBytes;
        public bool VideoUpdated;
        public short[] InputBuffer;
        public byte[] StateBuffer;
        public uint StateBufferSize;
            
        public Buffers(bool videoSupport16Bit)
        {
            AudioBufferFlush = null;
            AudioBufferSize = 0;
            AudioBuffer = null;
            AudioPosition = 0;
            AudioUpdated = false;
            VideoSupport16Bit = videoSupport16Bit;
            VideoLineBytes = 0;
            VideoBuffer = null;
            VideoUpdated = false;
            InputBuffer = new short[16];
            StateBuffer = null;
        }

        public void SetSystemAvInfo(SystemAvInfo info)
        {
            var w = Convert.ToInt32(info.geometry.base_width);
            var h = Convert.ToInt32(info.geometry.base_height);
            
            AudioBufferSize = 4096;
            AudioBufferFlush = new float[AudioBufferSize];
            AudioBuffer = new float[AudioBufferSize];
            VideoLineBytes = (VideoSupport16Bit ? 2 : 3) * w;
            VideoBuffer = new byte[VideoLineBytes * h];
        }

        public void SetStateSize(uint size)
        {
            StateBufferSize = size;
            StateBuffer = new byte[StateBufferSize];
        }

        public void Dispose()
        {
            AudioBufferFlush = null;
            AudioBuffer = null;
            VideoBuffer = null;
            InputBuffer = null;
        }
    }
}