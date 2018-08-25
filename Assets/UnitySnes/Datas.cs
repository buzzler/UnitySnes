using System.Runtime.InteropServices;

namespace UnitySnes
{
    public enum PixelFormat
    {
        RetroPixelFormat0Rgb1555 = 0,
        RetroPixelFormatXrgb8888 = 1,
        RetroPixelFormatRgb565 = 2,
        RetroPixelFormatUnknown = int.MaxValue
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SystemAvInfo
    {
        public Geometry geometry;
        public Timing timing;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GameInfo
    {
        public char* path;
        public void* data;
        public uint size;
        public char* meta;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Geometry
    {
        public readonly uint base_width;
        public readonly uint base_height;
        public readonly uint max_width;
        public readonly uint max_height;
        public readonly float aspect_ratio;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Timing
    {
        public double fps;
        public double sample_rate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SystemInfo
    {
        public char* library_name;
        public char* library_version;
        public char* valid_extensions;

        [MarshalAs(UnmanagedType.U1)] public bool need_fullpath;

        [MarshalAs(UnmanagedType.U1)] public bool block_extract;
    }

    public class Environment
    {
        public const uint RetroEnvironmentSetRotation = 1;
        public const uint RetroEnvironmentGetOverscan = 2;
        public const uint RetroEnvironmentGetCanDupe = 3;
        public const uint RetroEnvironmentGetVariable = 4;
        public const uint RetroEnvironmentSetVariables = 5;
        public const uint RetroEnvironmentSetMessage = 6;
        public const uint RetroEnvironmentShutdown = 7;
        public const uint RetroEnvironmentSetPerformanceLevel = 8;
        public const uint RetroEnvironmentGetSystemDirectory = 9;
        public const uint RetroEnvironmentSetPixelFormat = 10;
        public const uint RetroEnvironmentSetInputDescriptors = 11;
        public const uint RetroEnvironmentSetKeyboardCallback = 12;
    }
}