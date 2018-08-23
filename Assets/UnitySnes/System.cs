using System;
using System.IO;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace UnitySnes
{
    public class System
    {
        public const int AudioBatchSize = 4096;

        private static readonly float[] AudioBatch = new float[AudioBatchSize]; // static for IL2CPP
        private static int _batchPosition; // static for IL2CPP
        private static PixelFormat _pixelFormat; // static for IL2CPP

        private Renderer _gameRenderer;
        private static Speaker _gameSpeaker; // static for IL2CPP
        private static Texture2D _gameTexture; // static for IL2CPP

        private static byte[] _src; // static for IL2CPP
        private static byte[] _dst; // static for IL2CPP
        private static int _linebytes; // static for IL2CPP

        private Bridges.RetroEnvironmentDelegate _environment;
        private Bridges.RetroVideoRefreshDelegate _videoRefresh;
        private Bridges.RetroAudioSampleDelegate _audioSample;
        private Bridges.RetroAudioSampleBatchDelegate _audioSampleBatch;
        private Bridges.RetroInputPollDelegate _inputPoll;
        private Bridges.RetroInputStateDelegate _inputState;

        public unsafe void Init(Renderer renderer, Speaker speaker)
        {
            _gameRenderer = renderer;
            _gameSpeaker = speaker;
            _gameTexture = null;

            Bridges.retro_api_version();
            var info = new SystemInfo();
            Bridges.retro_get_system_info(ref info);
            Console.WriteLine(info);

            _environment = RetroEnvironment;
            _videoRefresh = RetroVideoRefresh;
            _audioSample = RetroAudioSample;
            _audioSampleBatch = RetroAudioSampleBatch;
            _inputPoll = RetroInputPoll;
            _inputState = RetroInputState;
            Bridges.retro_set_environment(_environment);
            Bridges.retro_set_video_refresh(_videoRefresh);
            Bridges.retro_set_audio_sample(_audioSample);
            Bridges.retro_set_audio_sample_batch(_audioSampleBatch);
            Bridges.retro_set_input_poll(_inputPoll);
            Bridges.retro_set_input_state(_inputState);
            Bridges.retro_init();
        }

        public void DeInit()
        {
            Bridges.retro_deinit();
        }

        public void Update()
        {
            Bridges.retro_run();
        }

        [MonoPInvokeCallback(typeof(Bridges.RetroVideoRefreshDelegate))]
        private static unsafe void RetroVideoRefresh(void* data, uint width, uint height, uint pitch)
        {
            var pixels = (IntPtr) data;
            switch (_pixelFormat)
            {
                case PixelFormat.RetroPixelFormat0Rgb1555:
                    for (var i = 0; i < height; i++)
                    {
                        for (var j = 0; j < width; j++)
                        {
                            var packed = Marshal.ReadInt16(pixels);
                            var color = new Color(((packed >> 10) & 0x001F) / 31.0f,
                                ((packed >> 5) & 0x001F) / 31.0f, (packed & 0x001F) / 31.0f, 1.0f);
                            _gameTexture.SetPixel(i, j, color);
                        }

                        _gameTexture.Apply();
                    }

                    break;
                case PixelFormat.RetroPixelFormatXrgb8888:
                    for (var i = 0; i < height; i++)
                    {
                        for (var j = 0; j < width; j++)
                        {
                            var packed = Marshal.ReadInt32(pixels);
                            var color = new Color(((packed >> 16) & 0x00FF) / 255.0f,
                                ((packed >> 8) & 0x00FF) / 255.0f, (packed & 0x00FF) / 255.0f, 1.0f);
                            _gameTexture.SetPixel(i, j, color);
                        }
                    }

                    _gameTexture.Apply();
                    break;
                case PixelFormat.RetroPixelFormatRgb565:
                    for (var k = 0; k < height; k++)
                    {
                        Marshal.Copy(pixels, _dst, k * _linebytes, _linebytes);
                        pixels = new IntPtr(pixels.ToInt64() + pitch);
                    }

                    _gameTexture.LoadRawTextureData(_dst);
                    _gameTexture.Apply();
                    break;
                case PixelFormat.RetroPixelFormatUnknown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [MonoPInvokeCallback(typeof(Bridges.RetroAudioSampleDelegate))]
        private static void RetroAudioSample(short left, short right)
        {
            // Unused.
        }

        [MonoPInvokeCallback(typeof(Bridges.RetroAudioSampleBatchDelegate))]
        private static unsafe void RetroAudioSampleBatch(short* data, uint frames)
        {
            for (var i = 0; i < (int) frames; i++)
            {
                var chunk = Marshal.ReadInt16((IntPtr) data);
                data += sizeof(short); // Set pointer to next chunk.
                var value = chunk / 32768f; // Divide by Int16 max to get correct float value.
                value = Mathf.Clamp(value, -1.0f, 1.0f); // Unity's audio only takes values between -1 and 1.

                AudioBatch[_batchPosition] = value;
                _batchPosition++;

                // When the batch is filled send it to the speakers.
                if (_batchPosition >= AudioBatchSize - 1)
                {
                    _gameSpeaker.UpdateAudio(AudioBatch);
                    _batchPosition = 0;
                }
            }
        }

        [MonoPInvokeCallback(typeof(Bridges.RetroInputPollDelegate))]
        private static void RetroInputPoll()
        {
            // Unused
        }
#if NO_INPUT
            [MonoPInvokeCallback(typeof(Bridges.RetroInputStateDelegate))]
            private static short RetroInputState(uint port, uint device, uint index, uint id)
            {
                return 0;
            }
#elif UNITY_EDITOR
        [MonoPInvokeCallback(typeof(Bridges.RetroInputStateDelegate))]
        private static short RetroInputState(uint port, uint device, uint index, uint id)
        {
            switch (id)
            {
                case 0:
                    return Input.GetKey(KeyCode.Z) || Input.GetButton("B") ? (short) 1 : (short) 0; // B
                case 1:
                    return Input.GetKey(KeyCode.A) || Input.GetButton("Y") ? (short) 1 : (short) 0; // Y
                case 2:
                    return Input.GetKey(KeyCode.Space) || Input.GetButton("SELECT")
                        ? (short) 1
                        : (short) 0; // SELECT
                case 3:
                    return Input.GetKey(KeyCode.Return) || Input.GetButton("START")
                        ? (short) 1
                        : (short) 0; // START
                case 4:
                    return Input.GetKey(KeyCode.UpArrow) || Input.GetAxisRaw("DpadX") >= 1.0f
                        ? (short) 1
                        : (short) 0; // UP
                case 5:
                    return Input.GetKey(KeyCode.DownArrow) || Input.GetAxisRaw("DpadX") <= -1.0f
                        ? (short) 1
                        : (short) 0; // DOWN
                case 6:
                    return Input.GetKey(KeyCode.LeftArrow) || Input.GetAxisRaw("DpadY") <= -1.0f
                        ? (short) 1
                        : (short) 0; // LEFT
                case 7:
                    return Input.GetKey(KeyCode.RightArrow) || Input.GetAxisRaw("DpadY") >= 1.0f
                        ? (short) 1
                        : (short) 0; // RIGHT
                case 8:
                    return Input.GetKey(KeyCode.X) || Input.GetButton("A") ? (short) 1 : (short) 0; // A
                case 9:
                    return Input.GetKey(KeyCode.S) || Input.GetButton("X") ? (short) 1 : (short) 0; // X
                case 10:
                    return Input.GetKey(KeyCode.Q) || Input.GetButton("L") ? (short) 1 : (short) 0; // L
                case 11:
                    return Input.GetKey(KeyCode.W) || Input.GetButton("R") ? (short) 1 : (short) 0; // R
                case 12:
                    return Input.GetKey(KeyCode.E) ? (short) 1 : (short) 0;
                case 13:
                    return Input.GetKey(KeyCode.R) ? (short) 1 : (short) 0;
                case 14:
                    return Input.GetKey(KeyCode.T) ? (short) 1 : (short) 0;
                case 15:
                    return Input.GetKey(KeyCode.Y) ? (short) 1 : (short) 0;
                default:
                    return 0;
            }
        }
#else
            public static bool PressingB;
            public static bool PressingY;
            public static bool PressingSelect;
            public static bool PressingStart;
            public static bool PressingUp;
            public static bool PressingDown;
            public static bool PressingLeft;
            public static bool PressingRight;
            public static bool PressingA;
            public static bool PressingX;
            public static bool PressingL1;
            public static bool PressingR1;

            [MonoPInvokeCallback(typeof(Bridges.RetroInputStateDelegate))]
            private static short RetroInputState(uint port, uint device, uint index, uint id)
            {
                switch (id)
                {
                    case 0:
                        return PressingB ? (short) 1 : (short) 0; // B
                    case 1:
                        return PressingY ? (short) 1 : (short) 0; // Y
                    case 2:
                        return PressingSelect ? (short) 1 : (short) 0; // SELECT
                    case 3:
                        return PressingStart ? (short) 1 : (short) 0; // START
                    case 4:
                        return PressingUp ? (short) 1 : (short) 0; // UP
                    case 5:
                        return PressingDown ? (short) 1 : (short) 0; // DOWN
                    case 6:
                        return PressingLeft ? (short) 1 : (short) 0; // LEFT
                    case 7:
                        return PressingRight ? (short) 1 : (short) 0; // RIGHT
                    case 8:
                        return PressingA ? (short) 1 : (short) 0; // A
                    case 9:
                        return PressingX ? (short) 1 : (short) 0; // X
                    case 10:
                        return PressingL1 ? (short) 1 : (short) 0; // L
                    case 11:
                        return PressingR1 ? (short) 1 : (short) 0; // R
                    default:
                        return 0;
                }
            }
#endif
        [MonoPInvokeCallback(typeof(Bridges.RetroEnvironmentDelegate))]
        private static unsafe bool RetroEnvironment(uint cmd, void* data)
        {
            switch (cmd)
            {
                case Environment.RetroEnvironmentGetOverscan:
                    break;
                case Environment.RetroEnvironmentGetVariable:
                    break;
                case Environment.RetroEnvironmentSetVariables:
                    break;
                case Environment.RetroEnvironmentSetMessage:
                    break;
                case Environment.RetroEnvironmentSetRotation:
                    break;
                case Environment.RetroEnvironmentShutdown:
                    break;
                case Environment.RetroEnvironmentSetPerformanceLevel:
                    break;
                case Environment.RetroEnvironmentGetSystemDirectory:
                    break;
                case Environment.RetroEnvironmentSetPixelFormat:
                    _pixelFormat = *(PixelFormat*) data;
                    break;
                case Environment.RetroEnvironmentSetInputDescriptors:
                    break;
                case Environment.RetroEnvironmentSetKeyboardCallback:
                    break;
                default:
                    return false;
            }

            return true;
        }

        private unsafe char* StringToChar(string s)
        {
            var p = Marshal.StringToHGlobalUni(s);
            return (char*) p.ToPointer();
        }

        private unsafe GameInfo LoadGameInfo(byte[] bytes, string path = null)
        {
            var gameInfo = new GameInfo();

            var arrayPointer = Marshal.AllocHGlobal(bytes.Length * Marshal.SizeOf(typeof(byte)));
            Marshal.Copy(bytes, 0, arrayPointer, bytes.Length);

            gameInfo.path = StringToChar(string.IsNullOrEmpty(path) ? Path.GetTempFileName() : path);
            gameInfo.size = (uint) bytes.Length;
            gameInfo.data = arrayPointer.ToPointer();

            return gameInfo;
        }

        public bool LoadGame(byte[] bytes)
        {
            return LoadGame(LoadGameInfo(bytes));
        }

        public bool LoadGame(GameInfo gameInfo)
        {
            var ret = Bridges.retro_load_game(ref gameInfo);
            Console.WriteLine("Game information: {0}", gameInfo);

            var av = new SystemAvInfo();
            Bridges.retro_get_system_av_info(ref av);
            Console.WriteLine("SYstem AV information: {0}", av);

            var w = Convert.ToInt32(av.geometry.base_width);
            var h = Convert.ToInt32(av.geometry.base_height);
            _gameTexture = new Texture2D(w, h, TextureFormat.RGB565, false) {filterMode = FilterMode.Point};
            _gameRenderer.material.mainTexture = _gameTexture;
            _linebytes = 2 * w;
            _dst = new byte[_linebytes * h];

            return ret;
        }

        public void UnloadGame()
        {
            Bridges.retro_unload_game();
        }
    }
}