using System;
using UnityEngine;

namespace UnitySnes
{
    public class Frontend : MonoBehaviour
    {
        public TextAsset Rom;
        public Renderer Display;
        public AudioSource AudioSource;
        public Texture2D Texture;
        private Backend _backend;
        
        private void Start()
        {
            Application.targetFrameRate = 60;
        }

        private void Update()
        {
            _backend.Loop();
            OnInputUpdate();
            OnAudioUpdate();
            OnVideoUpdate();
        }

        private void OnAudioUpdate()
        {
            var buffer = Backend.Buffers;
            if (!buffer.AudioUpdated) return;
            AudioSource.Play();
            buffer.AudioUpdated = false;
        }

        private void OnAudioRead(float[] sampleData)
        {
            Array.Copy(Backend.Buffers.AudioBufferFlush, sampleData, sampleData.Length);
        }

        private void OnVideoUpdate()
        {
            var buffer = Backend.Buffers;
            if (!buffer.VideoUpdated) return;
            Texture.LoadRawTextureData(buffer.VideoBuffer);
            Texture.Apply();
            buffer.VideoUpdated = false;
        }

        private void OnInputUpdate()
        {
#if UNITY_EDITOR
            var inputBuffer = Backend.Buffers.InputBuffer;
            inputBuffer[0] = (short) (Input.GetKey(KeyCode.Z) || Input.GetButton("B") ? 1 : 0);
            inputBuffer[1] = (short) (Input.GetKey(KeyCode.A) || Input.GetButton("Y") ? 1 : 0);
            inputBuffer[2] = (short) (Input.GetKey(KeyCode.Space) || Input.GetButton("SELECT") ? 1 : 0);
            inputBuffer[3] = (short) (Input.GetKey(KeyCode.Return) || Input.GetButton("START") ? 1 : 0);
            inputBuffer[4] = (short) (Input.GetKey(KeyCode.UpArrow) || Input.GetAxisRaw("DpadX") >= 1f ? 1 : 0);
            inputBuffer[5] = (short) (Input.GetKey(KeyCode.DownArrow) || Input.GetAxisRaw("DpadX") <= -1f ? 1 : 0);
            inputBuffer[6] = (short) (Input.GetKey(KeyCode.LeftArrow) || Input.GetAxisRaw("DpadY") <= -1f ? 1 : 0);
            inputBuffer[7] = (short) (Input.GetKey(KeyCode.RightArrow) || Input.GetAxisRaw("DpadY") >= 1f ? 1 : 0);
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
            if (_backend != null) return;
            var buffers = new Buffers(UnityEngine.SystemInfo.SupportsTextureFormat(TextureFormat.RGB565));
            _backend = new Backend(buffers);
            _backend.On(Rom.bytes);
            
            var w = Convert.ToInt32(Backend.SystemAvInfo.geometry.base_width);
            var h = Convert.ToInt32(Backend.SystemAvInfo.geometry.base_height);
            Texture =
                new Texture2D(w, h, buffers.VideoSupport16Bit ? TextureFormat.RGB565 : TextureFormat.RGB24, false)
                {
                    filterMode = FilterMode.Point
                };
            Display.material.mainTexture = Texture;
            AudioSource.clip = AudioClip.Create(name, buffers.AudioBufferSize / 2, 2, 44100, true, OnAudioRead);
            AudioSource.playOnAwake = false;
            AudioSource.spatialBlend = 0;
            AudioSource.loop = true;
        }

        private void TurnOff()
        {
            if (_backend == null) return;
            _backend.Off();

            AudioSource.Stop();
            AudioSource.clip = null;
            Display.material.mainTexture = null;
            Texture = null;
        }

        private void OnEnable()
        {
            TurnOn();
        }

        private void OnDisable()
        {
            TurnOff();
        }
    }
}