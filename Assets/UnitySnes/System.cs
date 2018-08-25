using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;

namespace UnitySnes
{
    public class System
    {
        public static SystemInfo SystemInfo; // set by Init
        public static GameInfo GameInfo; // set by LoadGame
        public static SystemAvInfo SystemAvInfo; // set by LoadGame
        public static Buffers Buffers; // set by LoadGame

        private static Action<IntPtr, uint, uint, uint> _onVideoRefresh;
        private Bridges.RetroEnvironmentDelegate _environment;
        private Bridges.RetroVideoRefreshDelegate _videoRefresh;
        private Bridges.RetroAudioSampleDelegate _audioSample;
        private Bridges.RetroAudioSampleBatchDelegate _audioSampleBatch;
        private Bridges.RetroInputPollDelegate _inputPoll;
        private Bridges.RetroInputStateDelegate _inputState;
        private Thread _thread;
        private bool _active;
        
        public void On(byte[] rom)
        {
            Init();
            LoadGame(rom);
            _active = true;
            _thread = new Thread(Loop);
            _thread.Start();
        }

        public void Off()
        {
            _active = false;
        }
        
        private unsafe void Init()
        {
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

        private void Loop()
        {
            const int frame = (int) (1000f / 60f);
            while (_active)
            {
                Thread.Sleep(frame);
                Bridges.retro_run();
            }
            
            _thread.Abort();
            _thread = null;
            Bridges.retro_unload_game();
            Bridges.retro_deinit();
        }

        [MonoPInvokeCallback(typeof(Bridges.RetroVideoRefreshDelegate))]
        private static unsafe void RetroVideoRefresh(void* data, uint width, uint height, uint pitch)
        {
            _onVideoRefresh((IntPtr) data, width, height, pitch);
            Buffers.VideoUpdated = true;
        }

        private static void RetroVideoRefresh0Rgb1555(IntPtr pixels, uint width, uint height, uint pitch)
        {
            var videoBuffer = Buffers.VideoBuffer;
            var index = 0;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var packed = Marshal.ReadInt16(pixels);
                    var c = ((short) (packed << 1) & 0xFFE0) | packed & 0x001F;
                    videoBuffer[index++] = (byte) (c >> 8);
                    videoBuffer[index++] = (byte) (c & 0xFF);
                    pixels = new IntPtr(pixels.ToInt64() + 2);
                }
            }
        }
        
        private static void RetroVideoRefreshXrgb8888(IntPtr pixels, uint width, uint height, uint pitch)
        {
            var videoBuffer = Buffers.VideoBuffer;
            var index = 0;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var packed = Marshal.ReadInt32(pixels);
                    var r = (short) (((packed >> 16) & 0x00FF) / 7905.0f); // 7905 = 255 * 31
                    var g = (short) (((packed >> 8) & 0x00FF) / 16065.0f); // 16065 = 255 * 63
                    var b = (short) ((packed & 0x00FF) / 7905.0f);
                    var c = (short) (r << 11) | (short) (g << 5) | b;
                    videoBuffer[index++] = (byte) (c >> 8);
                    videoBuffer[index++] = (byte) (c & 0xFF);
                    pixels = new IntPtr(pixels.ToInt64() + 4);
                }
            }
        }
        
        private static void RetroVideoRefreshRgb565(IntPtr pixels, uint width, uint height, uint pitch)
        {
            var videoBuffer = Buffers.VideoBuffer;
            var videoLineByte = Buffers.VideoLineBytes;
            for (var y = 0; y < height; y++)
            {
                Marshal.Copy(pixels, videoBuffer, y * videoLineByte, videoLineByte);
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
                Buffers.AudioBuffer[Buffers.AudioPosition++] = chunk / 32768f;

                if (Buffers.AudioPosition >= Buffers.AudioBufferSize - 1)
                {
                    Buffers.AudioUpdated = true;
                    Buffers.AudioPosition = 0;
                }
            }
        }

        [MonoPInvokeCallback(typeof(Bridges.RetroInputPollDelegate))]
        private static void RetroInputPoll()
        {
            // Unused
        }

        [MonoPInvokeCallback(typeof(Bridges.RetroInputStateDelegate))]
        private static short RetroInputState(uint port, uint device, uint index, uint id)
        {
            return port == 0 ? Buffers.InputBuffer[(int)id] : (short) 0;
        }
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

        private unsafe void LoadGame(byte[] bytes)
        {
            var arrayPointer = Marshal.AllocHGlobal(bytes.Length * Marshal.SizeOf(typeof(byte)));
            Marshal.Copy(bytes, 0, arrayPointer, bytes.Length);

            GameInfo = new GameInfo
            {
                path = (char*) Marshal.StringToHGlobalUni(Path.GetTempFileName()).ToPointer(),
                size = (uint) bytes.Length,
                data = arrayPointer.ToPointer()
            };

            if (!Bridges.retro_load_game(ref GameInfo))
                throw new ArgumentException();
            SystemAvInfo = new SystemAvInfo();
            Bridges.retro_get_system_av_info(ref SystemAvInfo);
            Buffers = new Buffers(SystemAvInfo);
        }
    }
}