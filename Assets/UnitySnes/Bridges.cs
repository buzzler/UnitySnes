using System.Runtime.InteropServices;

namespace UnitySnes
{
    public unsafe class Bridges
    {
#if UNITY_EDITOR
        private const string CoreName = "snes9x2010_libretro";

        [DllImport(CoreName, CallingConvention = CallingConvention.StdCall, EntryPoint = "retro_api_version")]
        public static extern int retro_api_version();

        [DllImport(CoreName, CallingConvention = CallingConvention.StdCall, EntryPoint = "retro_init")]
        public static extern void retro_init();

        [DllImport(CoreName, CallingConvention = CallingConvention.StdCall, EntryPoint = "retro_get_system_info")]
        public static extern void retro_get_system_info(ref SystemInfo info);

        [DllImport(CoreName, CallingConvention = CallingConvention.StdCall, EntryPoint = "retro_get_system_av_info")]
        public static extern void retro_get_system_av_info(ref SystemAvInfo info);

        [DllImport(CoreName, CallingConvention = CallingConvention.StdCall, EntryPoint = "retro_load_game")]
        public static extern bool retro_load_game(ref GameInfo game);

        [DllImport(CoreName, CallingConvention = CallingConvention.StdCall, EntryPoint = "retro_set_video_refresh")]
        public static extern void retro_set_video_refresh(RetroVideoRefreshDelegate r);

        [DllImport(CoreName, CallingConvention = CallingConvention.StdCall, EntryPoint = "retro_set_audio_sample")]
        public static extern void retro_set_audio_sample(RetroAudioSampleDelegate r);

        [DllImport(CoreName, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "retro_set_audio_sample_batch")]
        public static extern void retro_set_audio_sample_batch(RetroAudioSampleBatchDelegate r);

        [DllImport(CoreName, CallingConvention = CallingConvention.StdCall, EntryPoint = "retro_set_input_poll")]
        public static extern void retro_set_input_poll(RetroInputPollDelegate r);

        [DllImport(CoreName, CallingConvention = CallingConvention.StdCall, EntryPoint = "retro_set_input_state")]
        public static extern void retro_set_input_state(RetroInputStateDelegate r);

        [DllImport(CoreName, CallingConvention = CallingConvention.StdCall, EntryPoint = "retro_set_environment")]
        public static extern bool retro_set_environment(RetroEnvironmentDelegate r);

        [DllImport(CoreName, CallingConvention = CallingConvention.StdCall, EntryPoint = "retro_run")]
        public static extern void retro_run();

        [DllImport(CoreName, CallingConvention = CallingConvention.StdCall, EntryPoint = "retro_deinit")]
        public static extern void retro_deinit();

        [DllImport(CoreName, CallingConvention = CallingConvention.StdCall, EntryPoint = "retro_unload_game")]
        public static extern void retro_unload_game();
#elif UNITY_ANDROID
        private const string CoreName = "snes9x2010_libretro";
        
        [DllImport(CoreName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_api_version")]
        public static extern int retro_api_version();

        [DllImport(CoreName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_init")]
        public static extern void retro_init();

        [DllImport(CoreName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_get_system_info")]
        public static extern void retro_get_system_info(ref SystemInfo info);

        [DllImport(CoreName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_get_system_av_info")]
        public static extern void retro_get_system_av_info(ref SystemAvInfo info);

        [DllImport(CoreName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_load_game")]
        public static extern bool retro_load_game(ref GameInfo game);

        [DllImport(CoreName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_set_video_refresh")]
        public static extern void retro_set_video_refresh(RetroVideoRefreshDelegate r);

        [DllImport(CoreName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_set_audio_sample")]
        public static extern void retro_set_audio_sample(RetroAudioSampleDelegate r);

        [DllImport(CoreName, CallingConvention = CallingConvention.Cdecl, EntryPoint =
"retro_set_audio_sample_batch")]
        public static extern void retro_set_audio_sample_batch(RetroAudioSampleBatchDelegate r);

        [DllImport(CoreName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_set_input_poll")]
        public static extern void retro_set_input_poll(RetroInputPollDelegate r);

        [DllImport(CoreName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_set_input_state")]
        public static extern void retro_set_input_state(RetroInputStateDelegate r);

        [DllImport(CoreName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_set_environment")]
        public static extern bool retro_set_environment(RetroEnvironmentDelegate r);

        [DllImport(CoreName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_run")]
        public static extern void retro_run();

        [DllImport(CoreName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_deinit")]
        public static extern void retro_deinit();

        [DllImport(CoreName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "retro_unload_game")]
        public static extern void retro_unload_game();
#elif UNITY_IOS
        [DllImport ("__Internal")]
        public	static extern int retro_api_version();
        
        [DllImport ("__Internal")]
        public static extern void retro_init();
        
        [DllImport ("__Internal")]
        public static extern void retro_get_system_info(ref SystemInfo info);
        
        [DllImport ("__Internal")]
        public static extern void retro_get_system_av_info(ref SystemAvInfo info);
        
        [DllImport ("__Internal")]
        public static extern bool retro_load_game(ref GameInfo game);
        
        [DllImport ("__Internal")]
        public static extern void retro_set_video_refresh(RetroVideoRefreshDelegate r);
        
        [DllImport ("__Internal")]
        public static extern void retro_set_audio_sample(RetroAudioSampleDelegate r);
        
        [DllImport ("__Internal")]
        public static extern void retro_set_audio_sample_batch(RetroAudioSampleBatchDelegate r);
        
        [DllImport ("__Internal")]
        public static extern void retro_set_input_poll(RetroInputPollDelegate r);
        
        [DllImport ("__Internal")]
        public static extern void retro_set_input_state(RetroInputStateDelegate r);
        
        [DllImport ("__Internal")]
        public static extern bool retro_set_environment(RetroEnvironmentDelegate r);
        
        [DllImport ("__Internal")]
        public static extern void retro_run();
        
        [DllImport ("__Internal")]
        public static extern void retro_deinit();
        
        [DllImport ("__Internal")]
        public static extern void retro_unload_game();
#endif

        //typedef void (*retro_video_refresh_t)(const void *data, unsigned width, unsigned height, size_t pitch);
        public delegate void RetroVideoRefreshDelegate(void* data, uint width, uint height, uint pitch);

        //typedef void (*retro_audio_sample_t)(int16_t left, int16_t right);
        public delegate void RetroAudioSampleDelegate(short left, short right);

        //typedef size_t (*retro_audio_sample_batch_t)(const int16_t *data, size_t frames);
        public delegate void RetroAudioSampleBatchDelegate(short* data, uint frames);

        //typedef void (*retro_input_poll_t)(void);
        public delegate void RetroInputPollDelegate();

        //typedef int16_t (*retro_input_state_t)(unsigned port, unsigned device, unsigned index, unsigned id);
        public delegate short RetroInputStateDelegate(uint port, uint device, uint index, uint id);

        //typedef bool (*retro_environment_t)(unsigned cmd, void *data);
        public delegate bool RetroEnvironmentDelegate(uint cmd, void* data);
    }
}