﻿using System;

namespace UnitySnes
{
    public class Buffers : IDisposable
    {
        public float[] AudioBuffer;
        public int AudioPosition;
        public int AudioBufferSize;
        public bool AudioUpdated;
        public byte[] VideoBuffer;
        public bool VideoSupport16Bit;
        public int VideoLineBytes;
        public bool VideoUpdated;
        public short[] InputBuffer;
        
        public Buffers(bool videoSupport16Bit)
        {
            AudioBufferSize = 0;
            AudioBuffer = null;
            AudioPosition = 0;
            AudioUpdated = false;
            VideoSupport16Bit = videoSupport16Bit;
            VideoLineBytes = 0;
            VideoBuffer = null;
            VideoUpdated = false;
            InputBuffer = new short[16];
        }

        public void SetSystemAvInfo(SystemAvInfo info)
        {
            var w = Convert.ToInt32(info.geometry.base_width);
            var h = Convert.ToInt32(info.geometry.base_height);
            
            AudioBufferSize = 4096;
            AudioBuffer = new float[AudioBufferSize];
            VideoLineBytes = (VideoSupport16Bit ? 2 : 3) * w;
            VideoBuffer = new byte[VideoLineBytes * h];
        }

        public void Dispose()
        {
            AudioBuffer = null;
            VideoBuffer = null;
            InputBuffer = null;
        }
    }
}