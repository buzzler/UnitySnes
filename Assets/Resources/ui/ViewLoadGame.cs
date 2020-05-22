using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace UnitySnes
{
    public class ViewLoadGame : Views
    {
        public GameObject ProgressBar;
        public RectTransform In;
        public Text Text;
        private bool _reachable;

        protected override void Start()
        {
            base.Start();
            StartCoroutine(_Load());
        }

        public void OnTouchBack()
        {
            Frontend.OnMenuOpen("ui/menus");
        }

        private void SetText(string message)
        {
            Text.text = message;
        }

        public void SetProgress(float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            if (!ProgressBar.activeSelf && value > 0f)
                ProgressBar.SetActive(true);
            else if (ProgressBar.activeSelf && value > 0.999f)
                ProgressBar.SetActive(false);
            In.localScale = new Vector3(value, 1f, 1f);
        }

        public IEnumerator _Ping(string ipaddress)
        {
            yield return StartCoroutine(_ShowText("connecting\n[SFC]", 0.5f));
            var p = new Ping(ipaddress);
            while (!p.isDone)
                yield return null;
            _reachable = p.time >= 0;
        }

        private IEnumerator _ShowText(string message, float time)
        {
            SetText(message);
            yield return new WaitForSeconds(time);
        }
        
        private IEnumerator _Load()
        {
            const string host = "192.168.0.1";
            const int port = 80;
            yield return StartCoroutine(_Ping(host));
            if (_reachable)
            {
                yield return StartCoroutine(_LoadList($"http://{host}:{port}/"));
            }
            else
            {
                yield return StartCoroutine(_ShowText("network error\nmake sure WiFi 'SFC'", 2f));
                OnTouchBack();
            }
        }

        private IEnumerator _LoadList(string url)
        {
            var files = new List<Uri>();
            
            SetText("thinking");
            using (var www = new WWW(url))
            {
                while (!www.isDone)
                    yield return null;
                
                var retrode = JsonUtility.FromJson<Retrode>(www.text);
                foreach (var file in retrode.files)
                    files.Add(new Uri($"{retrode.url}{file}"));
            }

            if (files.Count == 0)
            {
                yield return StartCoroutine(_ShowText("empty rom\ncheck your cartridge slot", 2f));
                OnTouchBack();
            }
            else
            {
                var rom = string.Empty;
                for (var index = 0; index < files.Count; index++)
                {
                    var file = files[index];
                    var www = new WWW(file.AbsoluteUri);
                    while (!www.isDone)
                    {
                        SetText($"download ({index+1}/{files.Count})\n[{www.progress:P}]");
                        SetProgress(www.progress);
                        yield return null;
                    }

                    var fileext = Path.GetExtension(file.AbsolutePath);
                    var filename = Path.GetFileName(file.AbsolutePath);
                    var filepath = Path.Combine(Application.persistentDataPath, filename);
                    
                    Debug.Log(filepath);
                    File.WriteAllBytes(filepath, www.bytes);
                    
                    if (string.IsNullOrEmpty(rom) && (fileext == ".sfc" || fileext == ".smc"))
                        rom = filename;
                }

                if (string.IsNullOrEmpty(rom))
                {
                    yield return StartCoroutine(_ShowText("empty rom\ncheck your cartridge slot", 2f));
                    OnTouchBack();
                }
                else
                {
                    SetText($"loading");
                    yield return new WaitForSeconds(1f);
                    Frontend.ChangeGame(rom);
                    Frontend.OnMenuOpen("");
                }
            }
        }
    }
}