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
        private const int AudioBufferSize = 4096;
        private float[] _audioBuffer;
        
        private void Start()
        {
            Application.targetFrameRate = 60;
            _audioBuffer = new float[AudioBufferSize];
            AudioSource.clip = AudioClip.Create("UnitySnes", AudioBufferSize/2, 2, 44100, true, OnAudioRead);
            AudioSource.loop = true;
            AudioSource.Stop();

            TurnOn();
        }

        private void Update()
        {
            OnInputUpdate();
            if (_system != null)
                _system.Update();
        }

        private void OnAudioUpdate()
        {
            _audioBuffer = System.Buffers.AudioBuffer;
            AudioSource.Play();
        }
        
        private void OnAudioRead(float[] sampleData)
        {
            Array.Copy(_audioBuffer, sampleData, sampleData.Length);
        }
        
        private void OnVideoUpdate()
        {
            Texture.LoadRawTextureData(System.Buffers.VideoBuffer);
            Texture.Apply();
        }

        private void OnInputUpdate()
        {
#if UNITY_EDITOR
            var inputBuffer = System.Buffers.InputBuffer;
            inputBuffer[0] = (short) (Input.GetKey(KeyCode.Z) || Input.GetButton("B") ? 1 : 0);
            inputBuffer[1] = (short) (Input.GetKey(KeyCode.A) || Input.GetButton("Y") ? 1 : 0);
            inputBuffer[2] = (short) (Input.GetKey(KeyCode.Space) || Input.GetButton("SELECT") ? 1 : 0);
            inputBuffer[3] = (short) (Input.GetKey(KeyCode.Return) || Input.GetButton("START") ? 1 : 0);
            inputBuffer[4] = (short) (Input.GetKey(KeyCode.UpArrow) || Input.GetAxisRaw("DpadX")>=1f ? 1 : 0);
            inputBuffer[5] = (short) (Input.GetKey(KeyCode.DownArrow) || Input.GetAxisRaw("DpadX")<=-1f ? 1 : 0);
            inputBuffer[6] = (short) (Input.GetKey(KeyCode.LeftArrow) || Input.GetAxisRaw("DpadY")<=-1f ? 1 : 0);
            inputBuffer[7] = (short) (Input.GetKey(KeyCode.RightArrow) || Input.GetAxisRaw("DpadY")>=1f ? 1 : 0);
            inputBuffer[8] = (short) (Input.GetKey(KeyCode.X) || Input.GetButton("A") ? 1 : 0);
            inputBuffer[9] = (short) (Input.GetKey(KeyCode.S) || Input.GetButton("X") ? 1 : 0);
            inputBuffer[10] = (short) (Input.GetKey(KeyCode.Q) || Input.GetButton("L") ? 1 : 0);
            inputBuffer[11] = (short) (Input.GetKey(KeyCode.W) || Input.GetButton("R") ? 1 : 0);
            inputBuffer[12] = (short) (Input.GetKey(KeyCode.E) ? 1 : 0);
            inputBuffer[13] = (short) (Input.GetKey(KeyCode.R) ? 1 : 0);
            inputBuffer[14] = (short) (Input.GetKey(KeyCode.T) ? 1 : 0);
            inputBuffer[15] = (short) (Input.GetKey(KeyCode.Y) ? 1 : 0);
#endif
        }

        private void TurnOn()
        {
            if (_system != null)
                return;
            
            _system = new System();
            _system.Init(OnVideoUpdate, OnAudioUpdate);
            _system.LoadGame(Rom.bytes);
            
            var w = Convert.ToInt32(System.SystemAvInfo.geometry.base_width);
            var h = Convert.ToInt32(System.SystemAvInfo.geometry.base_height);
            Texture = new Texture2D(w, h, TextureFormat.RGB565, false) {filterMode = FilterMode.Point};
            Display.material.mainTexture = Texture;
        }

        private void TurnOff()
        {
            if (_system == null)
                return;

            Display.material.mainTexture = null;
            Texture = null;
            _system.UnloadGame();
            _system.DeInit();
            _system = null;
        }
        
        private void OnDisable()
        {
            TurnOff();
        }
    }
}