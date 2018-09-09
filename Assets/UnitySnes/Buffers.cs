using System;
using UnityEngine;

namespace UnitySnes
{
    public class Buffers : IDisposable
    {
        // required
        public float[] AudioBufferFlush;
        public float[] AudioBuffer;
        public int AudioPosition;
        public int AudioBufferSize;
        public bool AudioUpdated;
        public byte[] VideoBuffer;
        public int VideoWidth;
        public int VideoHeight;
        public int VideoUnitSize;
        public bool VideoSupport16Bit;
        public int VideoLineBytes;
        public bool VideoUpdated;
        public short[] InputBuffer;
        public byte[] StateBuffer;
        public uint StateBufferSize;
        
        // optional
        public string SystemDirectory;
        public string SystemMessage;
        public uint SystemMessageFrames;

        public string GameName;
        public string PersistentDataPath;
        public string TemporaryDataPath;
        public string StreamingAssets;
        
        public Buffers()
        {
            AudioBufferFlush = null;
            AudioBufferSize = 0;
            AudioBuffer = null;
            AudioPosition = 0;
            AudioUpdated = false;
            VideoSupport16Bit = true;
            VideoLineBytes = 0;
            VideoBuffer = null;
            VideoWidth = 256;
            VideoHeight = 224;
            VideoUnitSize = 0;
            VideoUpdated = false;
            InputBuffer = new short[16];
            StateBuffer = null;
            SystemDirectory = "";
            SystemMessage = "";
            SystemMessageFrames = 0;
            GameName = "";

        }

        public void SetSystemAvInfo(SystemAvInfo info)
        {
            VideoWidth = Convert.ToInt32(info.geometry.base_width);
            VideoHeight = Convert.ToInt32(info.geometry.base_height);
            
            AudioBufferSize = 4096;
            AudioBufferFlush = new float[AudioBufferSize];
            AudioBuffer = new float[AudioBufferSize];
            VideoLineBytes = (VideoSupport16Bit ? 2 : 4) * VideoWidth;
            VideoUnitSize = VideoWidth;
            VideoBuffer = new byte[VideoLineBytes * VideoUnitSize];
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

            SystemDirectory = "";
            SystemMessage = "";
        }
    }
}