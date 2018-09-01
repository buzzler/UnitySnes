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
        
        public void OnInputDetected(string receivedKeystroke)
        {
#if UNITY_IOS
            var inputBuffer = Backend.Buffers.InputBuffer;
            switch (receivedKeystroke)
            {
                case "K":
                    inputBuffer[0] = 1; break;
                case "P":
                    inputBuffer[0] = 0; break;
                case "I":
                    inputBuffer[1] = 1; break;
                case "M":
                    inputBuffer[1] = 0; break;
                case "Y":
                    inputBuffer[2] = 1; break;
                case "T":
                    inputBuffer[2] = 0; break;
                case "U":
                    inputBuffer[3] = 1; break;
                case "F":
                    inputBuffer[3] = 0; break;
                case "W":
                    inputBuffer[4] = 1; break;
                case "E":
                    inputBuffer[4] = 0; break;
                case "X":
                    inputBuffer[5] = 1; break;
                case "Z":
                    inputBuffer[5] = 0; break;
                case "A":
                    inputBuffer[6] = 1; break;
                case "Q":
                    inputBuffer[6] = 0; break;
                case "D":
                    inputBuffer[7] = 1; break;
                case "C":
                    inputBuffer[7] = 0; break;
                case "L":
                    inputBuffer[8] = 1; break;
                case "V":
                    inputBuffer[8] = 0; break;
                case "O":
                    inputBuffer[9] = 1; break;
                case "G":
                    inputBuffer[9] = 0; break;
                case "H":
                    inputBuffer[10] = 1; break;
                case "R":
                    inputBuffer[10] = 0; break;
                case "J":
                    inputBuffer[11] = 1; break;
                case "N":
                    inputBuffer[11] = 0; break;
            }
#endif
        }

        private void OnInputUpdate()
        {
#if UNITY_EDITOR
            var inputBuffer = Backend.Buffers.InputBuffer;
            inputBuffer[0] = (short) (Input.GetKey(KeyCode.Z) ? 1 : 0);
            inputBuffer[1] = (short) (Input.GetKey(KeyCode.A)? 1 : 0);
            inputBuffer[2] = (short) (Input.GetKey(KeyCode.Space) ? 1 : 0);
            inputBuffer[3] = (short) (Input.GetKey(KeyCode.Return) ? 1 : 0);
            inputBuffer[4] = (short) (Input.GetKey(KeyCode.UpArrow) ? 1 : 0);
            inputBuffer[5] = (short) (Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
            inputBuffer[6] = (short) (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            inputBuffer[7] = (short) (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
            inputBuffer[8] = (short) (Input.GetKey(KeyCode.X) ? 1 : 0);
            inputBuffer[9] = (short) (Input.GetKey(KeyCode.S) ? 1 : 0);
            inputBuffer[10] = (short) (Input.GetKey(KeyCode.Q) ? 1 : 0);
            inputBuffer[11] = (short) (Input.GetKey(KeyCode.W) ? 1 : 0);
            inputBuffer[12] = (short) (Input.GetKey(KeyCode.E) ? 1 : 0);
            inputBuffer[13] = (short) (Input.GetKey(KeyCode.R) ? 1 : 0);
            inputBuffer[14] = (short) (Input.GetKey(KeyCode.T) ? 1 : 0);
            inputBuffer[15] = (short) (Input.GetKey(KeyCode.Y) ? 1 : 0);
#endif
        }

        private void TurnOn()
        {
            if (_backend != null) return;
            var buffers = new Buffers()
            {
                VideoSupport16Bit = UnityEngine.SystemInfo.SupportsTextureFormat(TextureFormat.RGB565),
                SystemDirectory = Application.persistentDataPath
            };
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
            Bridges.SetupExternalInput();
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