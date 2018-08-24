using System;
using System.IO;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace UnitySnes
{
    public class System
    {
        public SystemInfo SystemInfo;
        public GameInfo GameInfo;
        public SystemAvInfo SystemAvInfo;

        private static Buffers _buffers;
        private static Action<IntPtr, uint, uint, uint> _onVideoRefresh;
        private static Action<Buffers> _onVideoUpdate;
        private static Action<Buffers> _onAudioUpdate; // static for IL2CPP

        private Bridges.RetroEnvironmentDelegate _environment;
        private Bridges.RetroVideoRefreshDelegate _videoRefresh;
        private Bridges.RetroAudioSampleDelegate _audioSample;
        private Bridges.RetroAudioSampleBatchDelegate _audioSampleBatch;
        private Bridges.RetroInputPollDelegate _inputPoll;
        private Bridges.RetroInputStateDelegate _inputState;

        public unsafe void Init(Action<Buffers> onVideoUpdate, Action<Buffers> onAudioUpdate)
        {
            _onVideoUpdate = onVideoUpdate;
            _onAudioUpdate = onAudioUpdate;

            _environment = RetroEnvironment;
            _videoRefresh = RetroVideoRefresh;
            _audioSample = RetroAudioSample;
            _audioSampleBatch = RetroAudioSampleBatch;
            _inputPoll = RetroInputPoll;
            _inputState = RetroInputState;
            
            SystemInfo = new SystemInfo();
            Bridges.retro_get_system_info(ref SystemInfo);
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
            _onVideoRefresh((IntPtr) data, width, height, pitch);
            _onVideoUpdate(_buffers);
        }

        private static void RetroVideoRefresh0Rgb1555(IntPtr pixels, uint width, uint height, uint pitch)
        {
            var index = 0;
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var packed = Marshal.ReadInt16(pixels);
                    var c = ((short) (packed << 1) & 0xFFE0) | packed & 0x001F;
                    _buffers.VideoBuffer[index++] = (byte) (c >> 8);
                    _buffers.VideoBuffer[index++] = (byte) (c & 0xFF);
                    pixels = new IntPtr(pixels.ToInt64() + 2);
                }
            }
        }
        
        private static void RetroVideoRefreshXrgb8888(IntPtr pixels, uint width, uint height, uint pitch)
        {
            var index = 0;
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var packed = Marshal.ReadInt32(pixels);
                    var r = (short) (((packed >> 16) & 0x00FF) / 7905.0f); // 7905 = 255 * 31
                    var g = (short) (((packed >> 8) & 0x00FF) / 16065.0f); // 16065 = 255 * 63
                    var b = (short) ((packed & 0x00FF) / 7905.0f);
                    var c = (short) (r << 11) | (short) (g << 5) | b;
                    _buffers.VideoBuffer[index++] = (byte) (c >> 8);
                    _buffers.VideoBuffer[index++] = (byte) (c & 0xFF);
                    pixels = new IntPtr(pixels.ToInt64() + 4);
                }
            }
        }
        
        private static void RetroVideoRefreshRgb565(IntPtr pixels, uint width, uint height, uint pitch)
        {
            for (var k = 0; k < height; k++)
            {
                Marshal.Copy(pixels, _buffers.VideoBuffer, k * _buffers.VideoLineBytes, _buffers.VideoLineBytes);
                pixels = new IntPtr(pixels.ToInt64() + pitch);
            }
        }
        
        private static void RetroVideoRefreshUnknown(IntPtr pixels, uint width, uint height, uint pitch)
        {
        }

        [MonoPInvokeCallback(typeof(Bridges.RetroAudioSampleDelegate))]
        private static void RetroAudioSample(short left, short right)
        {
            // Unused.
        }

        [MonoPInvokeCallback(typeof(Bridges.RetroAudioSampleBatchDelegate))]
        private static unsafe void RetroAudioSampleBatch(short* data, uint frames)
        {
            const int offset = sizeof(short);
            for (var i = 0; i < frames; i++)
            {
                var chunk = Marshal.ReadInt16((IntPtr) data);
                data += offset;
                _buffers.AudioBuffer[_buffers.AudioPosition++] = chunk / 32768f;

                if (_buffers.AudioPosition >= _buffers.AudioBufferSize - 1)
                {
                    _onAudioUpdate(_buffers);
                    _buffers.AudioPosition = 0;
                }
            }
        }

        [MonoPInvokeCallback(typeof(Bridges.RetroInputPollDelegate))]
        private static void RetroInputPoll()
        {
            // Unused
        }
#if !UNITY_EDITOR
        [MonoPInvokeCallback(typeof(Bridges.RetroInputStateDelegate))]
        private static short RetroInputState(uint port, uint device, uint index, uint id)
        {
            return 0;
        }
#else
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
                    var pixelFormat = *(PixelFormat*) data;
                    switch (pixelFormat)
                    {
                        case PixelFormat.RetroPixelFormat0Rgb1555:
                            _onVideoRefresh = RetroVideoRefresh0Rgb1555;
                            break;
                        case PixelFormat.RetroPixelFormatXrgb8888:
                            _onVideoRefresh = RetroVideoRefreshXrgb8888;
                            break;
                        case PixelFormat.RetroPixelFormatRgb565:
                            _onVideoRefresh = RetroVideoRefreshRgb565;
                            break;
                        default:
                            _onVideoRefresh = RetroVideoRefreshUnknown;
                            break;
                    }
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

        public unsafe void LoadGame(byte[] bytes, string path = null)
        {
            if (path == null)
                path = Path.GetTempFileName();
            var arrayPointer = Marshal.AllocHGlobal(bytes.Length * Marshal.SizeOf(typeof(byte)));
            Marshal.Copy(bytes, 0, arrayPointer, bytes.Length);

            GameInfo = new GameInfo
            {
                path = (char*) Marshal.StringToHGlobalUni(path).ToPointer(),
                size = (uint) bytes.Length,
                data = arrayPointer.ToPointer()
            };

            LoadGame(GameInfo);
        }

        public void LoadGame(GameInfo gameInfo)
        {
            GameInfo = gameInfo;
            if (!Bridges.retro_load_game(ref gameInfo))
                throw new ArgumentException();
            SystemAvInfo = new SystemAvInfo();
            Bridges.retro_get_system_av_info(ref SystemAvInfo);
            _buffers = new Buffers(SystemAvInfo);
        }

        public void UnloadGame()
        {
            Bridges.retro_unload_game();
        }
    }
}