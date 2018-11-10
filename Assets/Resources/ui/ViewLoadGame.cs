using System;
using System.Collections;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Ping = System.Net.NetworkInformation.Ping;

namespace UnitySnes
{
    public class ViewLoadGame : Views
    {
        public Text Text;
        private bool canceled;
        private bool reachable;

        protected override void Start()
        {
            base.Start();
            StartCoroutine(_Load());
        }

        public void OnTouchBack()
        {
            canceled = true;
            Frontend.OnMenuOpen("ui/menus");
        }

        private void SetText(string message)
        {
            Text.text = message;
        }

        public IEnumerator _Ping(string ipaddress)
        {
            yield return StartCoroutine(_ShowText("connecting\n[SFC]", 0.5f));
            var p = new UnityEngine.Ping(ipaddress);
            while (!p.isDone)
                yield return null;
            reachable = p.time >= 0;
        }

        private IEnumerator _ShowText(string message, float time)
        {
            SetText(message);
            yield return new WaitForSeconds(time);
        }
        
        private IEnumerator _Load()
        {
            yield return StartCoroutine(_Ping("192.168.10.1"));
            if (reachable)
            {
                yield return StartCoroutine(_LoadLocal("http://192.168.10.1:8081/"));
            }
            else
            {
                yield return StartCoroutine(_ShowText("network error\nmake sure WiFi 'SFC'", 2f));
                OnTouchBack();
            }
        }

        private IEnumerator _LoadLocal(string url)
        {
            using (var www = new WWW(url))
            {
                while (!www.isDone)
                {
                    SetText($"downloading\n[{www.progress:P}]");
                    yield return null;
                }
                
                var headers = www.responseHeaders;
                var filename = headers["Filename"];
                var contenttype = headers["Content-Type"];
                var status = headers["STATUS"];

                if (string.IsNullOrEmpty(www.error) && 
                    !string.IsNullOrEmpty(filename) &&
                    contenttype == "application/octet-stream" && 
                    status.Contains("200"))
                {
                    // clear
                    var caches = Directory.GetFiles(Application.persistentDataPath, "*.sfc");
                    foreach (var cach in caches)
                        File.Delete(cach);
                    
                    // new cach
                    var bytes = www.bytes;
                    var filepath = Path.Combine(Application.persistentDataPath, filename);
                    File.WriteAllBytes(filepath, bytes);
                    
                    // load
                    SetText($"loading");
                    yield return new WaitForSeconds(1f);
                    Frontend.ChangeGame(filename);
                    Frontend.OnMenuOpen("");
                }
                else
                {
                    yield return StartCoroutine(_ShowText(www.text, 2f));
                    yield return new WaitForSeconds(2f);
                    OnTouchBack();
                }
            }
        }
    }
}