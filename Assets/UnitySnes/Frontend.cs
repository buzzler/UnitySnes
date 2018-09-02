using System;
using System.IO;
using UnityEngine;

namespace UnitySnes
{
    public class Frontend : MonoBehaviour
    {
        public Renderer Display;
        public AudioSource AudioSource;
        public Texture2D Texture;
        
        private Backend _backend;
        private const string RomFilename = "game.bytes";
        private const string SramFilename = "game.srm";
        private const string RtcFilename = "game.rtc";
        
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

        private void TurnOn()
        {
            if (_backend != null) return;
            var buffers = new Buffers()
            {
                VideoSupport16Bit = UnityEngine.SystemInfo.SupportsTextureFormat(TextureFormat.RGB565),
                SystemDirectory = Application.persistentDataPath
            };
            _backend = new Backend(buffers);

            var romfilepath = Path.Combine(Application.persistentDataPath, RomFilename);
            if (File.Exists(romfilepath))
            {
                _backend.On(File.ReadAllBytes(romfilepath));
            }
            else
            {
                romfilepath = Path.Combine(Application.streamingAssetsPath, RomFilename);
                if (File.Exists(romfilepath))
                    _backend.On(File.ReadAllBytes(romfilepath));
                else
                    throw new ArgumentException();
            }
            var sramfilepath = Path.Combine(Application.persistentDataPath, SramFilename);
            if (File.Exists(sramfilepath))
                _backend.LoadSram(sramfilepath);
            var rtcfilepath = Path.Combine(Application.persistentDataPath, RtcFilename);
            if (File.Exists(rtcfilepath))
                _backend.LoadRtc(rtcfilepath);
            
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
            OnApplicationQuit();
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

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) return;
            OnApplicationQuit();
        }

        private void OnApplicationQuit()
        {
            if (_backend == null) return;
            _backend.SaveSram(Path.Combine(Application.persistentDataPath, SramFilename));
            _backend.SaveRtc(Path.Combine(Application.persistentDataPath, RtcFilename));
        }
    }
}