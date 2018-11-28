using System;
using System.IO;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace UnitySnes
{
    public class Backend
    {
        public static SystemInfo SystemInfo; // set by Init
        public static GameInfo GameInfo; // set by LoadGame
        public static SystemAvInfo SystemAvInfo; // set by LoadGame
        public static RomHeader RomHeader; // set by LoadGame
        public static Buffers Buffers; // set by LoadGame
        private static Action<IntPtr, uint, uint, uint> _onVideoRefresh;
        private Bridges.RetroEnvironmentDelegate _environment;
        private Bridges.RetroVideoRefreshDelegate _videoRefresh;
        private Bridges.RetroAudioSampleDelegate _audioSample;
        private Bridges.RetroAudioSampleBatchDelegate _audioSampleBatch;
        private Bridges.RetroInputPollDelegate _inputPoll;
        private Bridges.RetroInputStateDelegate _inputState;
        private Bridges.RetroControllerPortDevideDelegate _controllerPortDevide;

        public Backend(Buffers buffers)
        {
            Buffers = buffers;
        }

        public void Loop()
        {
            Bridges.retro_run();
        }

        public void Reset()
        {
            Bridges.retro_reset();
        }

        public void SaveSram(string filepath)
        {
            SaveMemory(filepath, MemoryType.SaveRam);
        }

        public void LoadSram(string filepath)
        {
            LoadMemory(filepath, MemoryType.SaveRam);
        }

        public void SaveRtc(string filepath)
        {
            SaveMemory(filepath, MemoryType.Rtc);
        }

        public void LoadRtc(string filepath)
        {
            LoadMemory(filepath, MemoryType.Rtc);
        }

        public void SaveSystemRam(string filepath)
        {
            SaveMemory(filepath, MemoryType.SystemRam);
        }

        public void LoadSystemRam(string filepath)
        {
            LoadMemory(filepath, MemoryType.SystemRam);
        }

        public void SaveVideoRam(string filepath)
        {
            SaveMemory(filepath, MemoryType.VideoRam);
        }

        public void LoadVideoRam(string filepath)
        {
            LoadMemory(filepath, MemoryType.VideoRam);
        }

        public void SaveState(string filepath)
        {
            var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(Buffers.StateBuffer, 0);
            Bridges.retro_serialize(ptr, Buffers.StateBufferSize);
            using (var file = File.OpenWrite(filepath))
                file.Write(Buffers.StateBuffer, 0, Buffers.StateBuffer.Length);
        }

        public void LoadState(string filepath)
        {
            if (!File.Exists(filepath))
                return;
            using (var file = File.OpenRead(filepath))
                file.Read(Buffers.StateBuffer, 0, Buffers.StateBuffer.Length);
            var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(Buffers.StateBuffer, 0);
            Bridges.retro_unserialize(ptr, Buffers.StateBufferSize);
        }
        
        private void SaveMemory(string filepath, uint memoryType)
        {
            var size = Bridges.retro_get_memory_size(memoryType);
            var ptr = Bridges.retro_get_memory_data(memoryType);
            if (size <= 0)
                return;
            
            var bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, (int) size);
            using (var file = File.OpenWrite(filepath))
                file.Write(bytes, 0, (int) size);
        }

        private void LoadMemory(string filepath, uint memoryType)
        {
            if (!File.Exists(filepath))
                return;
            var size = Bridges.retro_get_memory_size(memoryType);
            var ptr = Bridges.retro_get_memory_data(memoryType);
            if (size <= 0)
                return;

            var bytes = new byte[size];
            using (var file = File.OpenRead(filepath))
                file.Read(bytes, 0, (int) size);
            Marshal.Copy(bytes, 0, ptr, (int) size);
        }
        
        public unsafe void Init()
        {
            _environment = RetroEnvironment;
            _videoRefresh = RetroVideoRefresh;
            _audioSample = RetroAudioSample;
            _audioSampleBatch = RetroAudioSampleBatch;
            _inputPoll = RetroInputPoll;
            _inputState = RetroInputState;
            _controllerPortDevide = RetroControllerPortDevide;
            
            SystemInfo = new SystemInfo();
            Bridges.retro_get_system_info(ref SystemInfo);
            Bridges.retro_set_environment(_environment);
            Bridges.retro_set_video_refresh(_videoRefresh);
            Bridges.retro_set_audio_sample(_audioSample);
            Bridges.retro_set_audio_sample_batch(_audioSampleBatch);
            Bridges.retro_set_input_poll(_inputPoll);
            Bridges.retro_set_input_state(_inputState);
            Bridges.retro_set_controller_port_device(_controllerPortDevide);
            Bridges.retro_init();
        }

        public void DeInit()
        {
            Bridges.retro_unload_game();
            Bridges.retro_deinit();
        }
        
        public unsafe void LoadGame(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return;

            var offset = (0x400 << bytes[0x7FD7] == bytes.Length) ? 0x7000 : 0xF000;
            RomHeader = new RomHeader
            {
                GameTitle = System.Text.Encoding.GetEncoding("iso-2022-jp").GetString(bytes, offset | 0xFC0, 21).Trim(),
                RomType = bytes[offset | 0xFD6],
                RomSize = 0x400 << bytes[offset | 0xFD7],
                SaveRamSize = 0x400 << bytes[offset | 0xFD8],
                CountryCode = bytes[offset | 0xFD9],
                License = bytes[offset | 0xFDA],
                Version = bytes[offset | 0xFDB]
            };
            
            
            var arrayPointer = Marshal.AllocHGlobal(bytes.Length * Marshal.SizeOf(typeof(byte)));
            Marshal.Copy(bytes, 0, arrayPointer, bytes.Length);

            GameInfo = new GameInfo
            {
                path = (char*) Marshal.StringToHGlobalUni(Path.Combine(Buffers.TemporaryDataPath, "temp.bytes")).ToPointer(),
                size = (uint) bytes.Length,
                data = arrayPointer.ToPointer()
            };

            Bridges.retro_unload_game();
            if (!Bridges.retro_load_game(ref GameInfo))
                throw new ArgumentException();

            SystemAvInfo = new SystemAvInfo();
            Bridges.retro_get_system_av_info(ref SystemAvInfo);
            Buffers.SetSystemAvInfo(SystemAvInfo);
            Buffers.SetStateSize(Bridges.retro_serialize_size());
        }

        [MonoPInvokeCallback(typeof(Bridges.RetroVideoRefreshDelegate))]
        private static unsafe void RetroVideoRefresh(void* data, uint width, uint height, uint pitch)
        {
            _onVideoRefresh((IntPtr) data, width, height, pitch);
            Buffers.VideoUpdated = true;
        }

        private static void Argb1555ToRgb565(IntPtr pixels, uint width, uint height, uint pitch)
        {
            var videoBuffer = Buffers.VideoBuffer;
            var index = (int) ((Buffers.VideoUnitSize - height) / 2f) * Buffers.VideoLineBytes;
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
        
        private static void Argb32ToRgb565(IntPtr pixels, uint width, uint height, uint pitch)
        {
            var videoBuffer = Buffers.VideoBuffer;
            var index = (int) ((Buffers.VideoUnitSize - height) / 2f) * Buffers.VideoLineBytes;
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
        
        private static void Rgb565ToRgb565(IntPtr pixels, uint width, uint height, uint pitch)
        {
            var videoBuffer = Buffers.VideoBuffer;
            var videoLineByte = Buffers.VideoLineBytes;
            var videoOffset = (int) ((Buffers.VideoUnitSize - height) / 2f) * videoLineByte;
            for (var y = 0; y < height; y++)
            {
                Marshal.Copy(pixels, videoBuffer, videoOffset + y * videoLineByte, videoLineByte);
                pixels = new IntPtr(pixels.ToInt64() + pitch);
            }
        }

        private static void Argb1555ToRgb32(IntPtr pixels, uint width, uint height, uint pitch)
        {
        }

        private static void Argb32ToRgb32(IntPtr pixels, uint width, uint height, uint pitch)
        {
        }

        private static void Rgb565ToRgb32(IntPtr pixels, uint width, uint height, uint pitch)
        {
            var videoBuffer = Buffers.VideoBuffer;
            var index = (int) ((Buffers.VideoUnitSize - height) / 2f) * Buffers.VideoLineBytes;
            var yPtr = pixels;
            for (var y = 0; y < height; y++)
            {
                var xPtr = yPtr;
                for (var x = 0 ; x < width; x++)
                {
                    var packed = Marshal.ReadInt16(xPtr);
                    videoBuffer[index++] = (byte) (((packed >> 11) & 0x001F) * 8.2258f);
                    videoBuffer[index++] = (byte) (((packed >> 5) & 0x003F) * 4.04762f);
                    videoBuffer[index++] = (byte) ((packed & 0x001F) * 8.2258f);
                    videoBuffer[index++] = 0xFF;
                    xPtr = new IntPtr(xPtr.ToInt64() + 2);
                }
                yPtr = new IntPtr(yPtr.ToInt64() + pitch);
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
                Buffers.AudioBuffer[Buffers.AudioPosition++] = Marshal.ReadInt16((IntPtr) data) / 32768f;
                data += offset;
                
                if (Buffers.AudioPosition >= Buffers.AudioBufferSize - 1)
                {
                    Buffers.AudioUpdated = true;
                    Buffers.AudioPosition = 0;
                    
                    var tmp = Buffers.AudioBuffer;
                    Buffers.AudioBuffer = Buffers.AudioBufferFlush;
                    Buffers.AudioBufferFlush = tmp;
                }
            }
        }
        
        [MonoPInvokeCallback(typeof(Bridges.RetroControllerPortDevideDelegate))]
        private static void RetroControllerPortDevide(uint port, uint device)
        {
            // Unused
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
                    RetroEnvironmentSetSystemMessage((IntPtr) data);
                    break;
                case Environment.RetroEnvironmentSetRotation:
                    break;
                case Environment.RetroEnvironmentShutdown:
                    RetroEnvironmentShutdown();
                    break;
                case Environment.RetroEnvironmentSetPerformanceLevel:
                    break;
                case Environment.RetroEnvironmentGetSystemDirectory:
                    break;
                case Environment.RetroEnvironmentSetPixelFormat:
                    RetroEnvironmentSetPixelFormat(*(PixelFormat*) data);
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

        private static unsafe void RetroEnvironmentSetSystemMessage(IntPtr ptr)
        {
            var message = Marshal.PtrToStructure<SystemMessage>(ptr);
            Buffers.SystemMessage = Marshal.PtrToStringAuto((IntPtr) message.message);
            Buffers.SystemMessageFrames = message.frames;
        }

        private static void RetroEnvironmentShutdown()
        {
            Bridges.retro_unload_game();
            Bridges.retro_deinit();
        }

        private static void RetroEnvironmentSetPixelFormat(PixelFormat pixelFormat)
        {
            if (Buffers.VideoSupport16Bit)
            {
                switch (pixelFormat)
                {
                    case PixelFormat.RetroPixelFormat0Rgb1555:
                        _onVideoRefresh = Argb1555ToRgb565;
                        break;
                    case PixelFormat.RetroPixelFormatXrgb8888:
                        _onVideoRefresh = Argb32ToRgb565;
                        break;
                    case PixelFormat.RetroPixelFormatRgb565:
                        _onVideoRefresh = Rgb565ToRgb565;
                        break;
                    default:
                        _onVideoRefresh = RetroVideoRefreshUnknown;
                        break;
                }
            }
            else
            {
                switch (pixelFormat)
                {
                    case PixelFormat.RetroPixelFormat0Rgb1555:
                        _onVideoRefresh = Argb1555ToRgb32;
                        break;
                    case PixelFormat.RetroPixelFormatXrgb8888:
                        _onVideoRefresh = Argb32ToRgb32;
                        break;
                    case PixelFormat.RetroPixelFormatRgb565:
                        _onVideoRefresh = Rgb565ToRgb32;
                        break;
                    default:
                        _onVideoRefresh = RetroVideoRefreshUnknown;
                        break;
                }
            }
        }
    }
}