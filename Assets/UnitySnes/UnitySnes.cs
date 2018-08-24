using System;
using UnityEngine;

namespace UnitySnes
{
    public class UnitySnes : MonoBehaviour
    {
        public TextAsset Rom;
        public Renderer Display;
        public AudioSource AudioSource;
        public Texture2D Texture;
        
        private System _system;
        private float[] _audioBuffer;

        private const int AudioBufferSize = 4096;
        
        private void Start()
        {
            Application.targetFrameRate = 60;
            _system = new System();
            _audioBuffer = new float[AudioBufferSize];
            
            AudioSource.clip = AudioClip.Create("UnitySnes", AudioBufferSize/2, 2, 44100, true, OnAudioRead);
            AudioSource.loop = true;
            AudioSource.Stop();
            
            _system.Init(OnVideoUpdate, OnAudioUpdate);
            _system.LoadGame(Rom.bytes);
            
            var w = Convert.ToInt32(_system.SystemAvInfo.geometry.base_width);
            var h = Convert.ToInt32(_system.SystemAvInfo.geometry.base_height);
            Texture = new Texture2D(w, h, TextureFormat.RGB565, false) {filterMode = FilterMode.Point};
            Display.material.mainTexture = Texture;
        }

        private void Update()
        {
            if (_system != null)
                _system.Update();
        }

        private void OnDisable()
        {
            if (_system == null) return;
            _system.UnloadGame();
            _system.DeInit();
            _system = null;
        }

        private void OnVideoUpdate(Buffers buffers)
        {
            Texture.LoadRawTextureData(buffers.VideoBuffer);
            Texture.Apply();
        }
        
        private void OnAudioUpdate(Buffers buffers)
        {
            _audioBuffer = buffers.AudioBuffer;
            AudioSource.Play();
        }
        
        private void OnAudioRead(float[] sampleData)
        {
            Array.Copy(_audioBuffer, sampleData, sampleData.Length);
        }
    }
}