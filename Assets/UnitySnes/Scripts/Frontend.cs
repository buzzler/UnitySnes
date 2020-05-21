using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnitySnes
{
    public class Frontend : MonoBehaviour
    {
        public Renderer Display;
        public AudioSource AudioSource;
        public Canvas Canvas;
        public SimpleFilter Filter;

        private Backend _backend;
        private Texture2D _texture;

        private void Start()
        {
            Application.targetFrameRate = 60;
        }

        private void Update()
        {
            if (Canvas.transform.childCount == 0)
            {
                _backend.Loop();
                OnInputUpdate();
                OnAudioUpdate();
                OnVideoUpdate();
            }
        }

        #region PrivateInterface

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
            Backend.Buffers.LastKey = receivedKeystroke;
            var t = InputMapper?.GetKey(receivedKeystroke);
            if (t == null)
                return;
            
            var inputBuffer = Backend.Buffers.InputBuffer;
            inputBuffer[t.Item1] = t.Item2;
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
            
            if (Input.GetMouseButton(1))
                OnMenuOpen("ui/menus");
#endif
            if (Input.touchCount == 2)
                OnMenuOpen("ui/menus");
        }

        private byte[] GetRom(string filename = "")
        {
            if (string.IsNullOrEmpty(filename))
                filename = "game.bytes";

            var primary = Path.Combine(Backend.Buffers.PersistentDataPath, filename);
            var secondary = Path.Combine(Backend.Buffers.StreamingAssets, filename);
            var filepath = File.Exists(primary) ? primary : secondary;
            Backend.Buffers.LastFilePath = filepath;

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

        public void LoadState(string filepath = "")
        {
#if UNITY_EDITOR
            Debug.Log($"load sate: {filepath}");
#endif
            _backend?.LoadState(filepath);
        }

        public void SaveState()
        {
            if (string.IsNullOrEmpty(Backend.Buffers.LastFilePath))
                return;
            var filename =
                $"{Path.GetFileNameWithoutExtension(Backend.Buffers.LastFilePath)}_{DateTime.Now:yyyy-MM-dd_HHmmss}.sav";
            var filepath = Path.Combine(Backend.Buffers.PersistentDataPath, filename);
#if UNITY_EDITOR
            Debug.Log($"save sate: {filepath}");
#endif
            _backend?.SaveState(filepath);
        }

        public string[] GetStateFilePaths()
        {
            if (string.IsNullOrEmpty(Backend.Buffers.LastFilePath))
                return new string[0];
            var pattern = $"{Path.GetFileNameWithoutExtension(Backend.Buffers.LastFilePath)}*.sav";
#if UNITY_EDITOR
            Debug.Log($"search pattern: {pattern}");
#endif
            return Directory.GetFiles(Backend.Buffers.PersistentDataPath, pattern);
        }
        
        private string GetSramFilePaths()
        {
            return _GetFilePaths(".sram");
        }

        private string GetRtcFilePaths()
        {
            return _GetFilePaths(".rtc");
        }

        private string _GetFilePaths(string ext)
        {
            return string.IsNullOrEmpty(Backend.Buffers.LastFilePath)
                ? string.Empty
                : Path.ChangeExtension(Backend.Buffers.LastFilePath, ext);
        }

        public void TurnOn(string filename = "")
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
            _backend.Init();
            _backend.LoadGame(GetRom(filename));
            _backend.LoadSram(GetSramFilePaths());
            _backend.LoadRtc(GetRtcFilePaths());
            Debug.Log($"load game: {Backend.RomHeader.GameTitle}");

            _texture = new Texture2D(buffers.VideoUnitSize, buffers.VideoUnitSize,
                buffers.VideoSupport16Bit ? TextureFormat.RGB565 : TextureFormat.RGBA32,
                false)
            {
                filterMode = FilterMode.Point
            };
            InputMapper = new InputMapper();
            InputMapper.SetKeyAsICade();
            
            Display.material.mainTexture = _texture;
            AudioSource.clip = AudioClip.Create(name, buffers.AudioBufferSize / 2, 2, 44100, true, OnAudioRead);
            AudioSource.playOnAwake = false;
            AudioSource.spatialBlend = 0;
            AudioSource.loop = true;
            Bridges.SetupExternalInput();
        }

        public void TurnOff()
        {
            if (_backend == null) return;
            _backend.DeInit();

            AudioSource.Stop();
            AudioSource.clip = null;
            Display.material.mainTexture = null;
            _texture = null;
        }

        public void ChangeGame(string filename = "")
        {
            _backend?.LoadGame(GetRom(filename));
            Debug.Log($"load game: {Backend.RomHeader.GameTitle}");
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
            if (pauseStatus)
                OnApplicationQuit();
        }

        private void OnApplicationQuit()
        {
            var sram = GetSramFilePaths();
            var rtc = GetRtcFilePaths();
            _backend?.SaveSram(sram);
            _backend?.SaveRtc(rtc);
        }

        #endregion

        #region PublicInterface

        public InputMapper InputMapper { get; private set; }

        public void OnMenuOpen(string asset, params object[] args)
        {
            var tr = Canvas.transform as RectTransform;
            for (var i = 0; i < tr.childCount; i++)
            {
                var child = tr.GetChild(i).gameObject;
                if (child.name != asset)
                    DestroyImmediate(child);
            }
            
            if (tr.childCount == 0 && !string.IsNullOrEmpty(asset))
            {
                var go = Instantiate(Resources.Load<GameObject>(asset), tr);
                var rect = go.transform as RectTransform;

                var s = new Vector3();
                s.x = tr.rect.height * 0.87873462f / rect.rect.height;
                s.y = s.x;
                rect.localScale = s;
                go.name = asset;
                if (args !=null || args.Length > 0)
                {
                    var view = go.GetComponent<Views>();
                    if (view != null)
                        view.SetArguments(args);
                }
            }

            if (!AudioSource.isPlaying && tr.childCount == 0)
                AudioSource.Play();
            else if (AudioSource.isPlaying)
                AudioSource.Stop();
        }

        public void OnMenuLoadGame()
        {
            OnMenuOpen("ui/loadgame");
        }

        public void OnMenuReset()
        {
            _backend.Reset();
            OnMenuOpen("");
        }

        public void OnMenuSaveState()
        {
            OnMenuOpen("ui/savestate");
        }

        public void OnMenuLoadState()
        {
            OnMenuOpen("ui/loadstate");
        }

        public void OnMenuSetting()
        {
            OnMenuOpen("ui/settings");
        }

        public void OnMenuFilter() 
        {
            OnMenuOpen("ui/filters");
        }

        public void OnMenuController(int player)
        {
            OnMenuOpen("ui/controller", player);
        }
        
        #endregion
    }
}