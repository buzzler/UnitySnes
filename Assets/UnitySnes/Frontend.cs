using System;
using System.IO;
using UnityEngine;

namespace UnitySnes
{
    public class Frontend : MonoBehaviour
    {
        public Renderer Display;
        public AudioSource AudioSource;
        
        private Backend _backend;
        private Texture2D _texture;

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
            _texture.LoadRawTextureData(buffer.VideoBuffer);
            _texture.Apply();
            buffer.VideoUpdated = false;
        }
        
        public void OnInputDetected(string receivedKeystroke)
        {
#if !UNITY_EDITOR
            var inputBuffer = Backend.Buffers.InputBuffer;
            switch (receivedKeystroke)
            {
                case "H":
                    inputBuffer[SnesInput.B] = 1; break;
                case "R":
                    inputBuffer[SnesInput.B] = 0; break;
                case "Y":
                    inputBuffer[SnesInput.Y] = 1; break;
                case "T":
                    inputBuffer[SnesInput.Y] = 0; break;
                case "L":
                    inputBuffer[SnesInput.Select] = 1; break;
                case "V":
                    inputBuffer[SnesInput.Select] = 0; break;
                case "O":
                    inputBuffer[SnesInput.Start] = 1; break;
                case "G":
                    inputBuffer[SnesInput.Start] = 0; break;
                case "W":
                    inputBuffer[SnesInput.Up] = 1; break;
                case "E":
                    inputBuffer[SnesInput.Up] = 0; break;
                case "X":
                    inputBuffer[SnesInput.Down] = 1; break;
                case "Z":
                    inputBuffer[SnesInput.Down] = 0; break;
                case "A":
                    inputBuffer[SnesInput.Left] = 1; break;
                case "Q":
                    inputBuffer[SnesInput.Left] = 0; break;
                case "D":
                    inputBuffer[SnesInput.Right] = 1; break;
                case "C":
                    inputBuffer[SnesInput.Right] = 0; break;
                case "U":
                    inputBuffer[SnesInput.A] = 1; break;
                case "F":
                    inputBuffer[SnesInput.A] = 0; break;
                case "J":
                    inputBuffer[SnesInput.X] = 1; break;
                case "N":
                    inputBuffer[SnesInput.X] = 0; break;
                case "K":
                    inputBuffer[SnesInput.L] = 1; break;
                case "P":
                    inputBuffer[SnesInput.L] = 0; break;
                case "I":
                    inputBuffer[SnesInput.R] = 1; break;
                case "M":
                    inputBuffer[SnesInput.R] = 0; break;
            }
#endif
        }

        private void OnInputUpdate()
        {
#if UNITY_EDITOR
            var inputBuffer = Backend.Buffers.InputBuffer;
            inputBuffer[SnesInput.B] = (short) (Input.GetKey(KeyCode.Z) ? 1 : 0);
            inputBuffer[SnesInput.Y] = (short) (Input.GetKey(KeyCode.A)? 1 : 0);
            inputBuffer[SnesInput.Select] = (short) (Input.GetKey(KeyCode.Space) ? 1 : 0);
            inputBuffer[SnesInput.Start] = (short) (Input.GetKey(KeyCode.Return) ? 1 : 0);
            inputBuffer[SnesInput.Up] = (short) (Input.GetKey(KeyCode.UpArrow) ? 1 : 0);
            inputBuffer[SnesInput.Down] = (short) (Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
            inputBuffer[SnesInput.Left] = (short) (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            inputBuffer[SnesInput.Right] = (short) (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
            inputBuffer[SnesInput.A] = (short) (Input.GetKey(KeyCode.X) ? 1 : 0);
            inputBuffer[SnesInput.X] = (short) (Input.GetKey(KeyCode.S) ? 1 : 0);
            inputBuffer[SnesInput.L] = (short) (Input.GetKey(KeyCode.Q) ? 1 : 0);
            inputBuffer[SnesInput.R] = (short) (Input.GetKey(KeyCode.W) ? 1 : 0);
            inputBuffer[SnesInput.L2] = (short) (Input.GetKey(KeyCode.E) ? 1 : 0);
            inputBuffer[SnesInput.R2] = (short) (Input.GetKey(KeyCode.R) ? 1 : 0);
            inputBuffer[SnesInput.L3] = (short) (Input.GetKey(KeyCode.T) ? 1 : 0);
            inputBuffer[SnesInput.R3] = (short) (Input.GetKey(KeyCode.Y) ? 1 : 0);
#endif
        }

        public byte[] GetRom(string filename = "")
        {
            if (string.IsNullOrEmpty(filename))
                filename = "game.bytes";
            
            var primary = Path.Combine(Backend.Buffers.PersistentDataPath, filename);
            var secondary = Path.Combine(Backend.Buffers.StreamingAssets, filename);
            var filepath = File.Exists(primary) ? primary : secondary;
            
#if UNITY_ANDROID && !UNITY_EDITOR
            var www = new WWW(filepath);
            while (!www.isDone)
            {
            }

            return www.bytes;
#else
            return File.ReadAllBytes(filepath);
#endif
        }

        private string GetSramFilepath(string filename = "")
        {
            if (string.IsNullOrEmpty(filename))
                filename = "game.sram";
            return Path.Combine(Backend.Buffers.PersistentDataPath, filename);
        }

        private string GetRtcFilepath(string filename = "")
        {
            if (string.IsNullOrEmpty(filename))
                filename = "game.rtc";
            return Path.Combine(Backend.Buffers.PersistentDataPath, filename);
        }

        private void TurnOn()
        {
            if (_backend != null) return;
            var buffers = new Buffers()
            {
                VideoSupport16Bit = UnityEngine.SystemInfo.SupportsTextureFormat(TextureFormat.RGB565),
                SystemDirectory = Application.persistentDataPath,
                PersistentDataPath = Application.persistentDataPath,
                TemporaryDataPath = Application.temporaryCachePath,
                StreamingAssets = Application.streamingAssetsPath
            };
            _backend = new Backend(buffers);
            _backend.On(GetRom());
            _backend.LoadSram(GetSramFilepath(buffers.GameName));
            _backend.LoadRtc(GetRtcFilepath(buffers.GameName));

            _texture = new Texture2D(buffers.VideoUnitSize, buffers.VideoUnitSize,
                buffers.VideoSupport16Bit ? TextureFormat.RGB565 : TextureFormat.RGBA32,
                false)
            {
                filterMode = FilterMode.Point
            };
            
            Display.material.mainTexture = _texture;
            AudioSource.clip = AudioClip.Create(name, buffers.AudioBufferSize / 2, 2, 44100, true, OnAudioRead);
            AudioSource.playOnAwake = false;
            AudioSource.spatialBlend = 0;
            AudioSource.loop = true;
            Bridges.SetupExternalInput();
        }

        private void TurnOff()
        {
            if (_backend == null) return;
            OnApplicationQuit();
            _backend.Off();

            AudioSource.Stop();
            AudioSource.clip = null;
            Display.material.mainTexture = null;
            _texture = null;
        }

        private void OnEnable()
        {
            TurnOn();
        }

        private void OnDisable()
        {
            TurnOff();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) return;
            OnApplicationQuit();
        }

        private void OnApplicationQuit()
        {
            if (_backend == null) return;
            _backend.SaveSram(GetSramFilepath(Backend.Buffers.GameName));
            _backend.SaveRtc(GetRtcFilepath(Backend.Buffers.GameName));
        }
    }
}