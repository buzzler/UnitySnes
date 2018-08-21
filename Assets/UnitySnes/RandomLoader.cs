using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnitySnes
{
    public class RandomLoader : MonoBehaviour
    {
        public UnityEngine.UI.Text Text;
        private List<string> _builder;
        private int _lines;
        private const int MaxLine = 19;

        private void WriteLine(string format, params object[] args)
        {
            if (_lines >= MaxLine)
                _builder.RemoveAt(0);
            else
                _lines++;
            _builder.Add(string.Format(format, args));
            Text.text = string.Join("\n", _builder.ToArray());
            if (Application.isEditor)
                Debug.LogFormat(format, args);
        }

        private void Awake()
        {
            _builder = new List<string>();
            _builder.Add(Text.text);
            _lines = 0;
        }
    
        private IEnumerator Start()
        {
            var host = new Uri("http://pi.unityscene.com/roms/sfc/");
            var regex = new Regex(" \\]\"></td><td><a href=\"([0-9a-zA-Z/%\\(\\) \\.,'!\\+-\\[\\]_]+)\">(.*)</a>");
            var files = new Dictionary<Uri, string>();
            {
                var request = WebRequest.Create(host);
                using (var response = request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null) yield break;
                    using (var reader = new StreamReader(responseStream))
                    {
                        var pagesource = reader.ReadToEnd();
                        if (!regex.IsMatch(pagesource))
                            yield break;

                        var time1 = Time.realtimeSinceStartup;
                        var matchs = regex.Matches(pagesource);
                        foreach (Match match in matchs)
                        {
                            if (match.Groups.Count != 3)
                                continue;
                            var path = match.Groups[1].Value;
                            var filename = match.Groups[2].Value;

                            if (name == "Parent Directory")
                                continue;

                            files.Add(new Uri(host, path), filename);
                            WriteLine("{0} .. found", filename);
                        
                            if (Time.realtimeSinceStartup - time1 > Time.deltaTime)
                            {
                                time1 = Time.realtimeSinceStartup;
                                yield return null;
                            }
                        }
                    }
                }
            }

            var names = (from exist in Directory.GetFiles(Application.persistentDataPath, "*",
                    SearchOption.TopDirectoryOnly)
                select Path.GetFileName(exist)).ToList();

            var time2 = Time.realtimeSinceStartup;
            var totalTargets = files.Count;
            var currentTarget = 1;
            foreach (var file in files)
            {
                var uri = file.Key;
                var filepath = Path.Combine(Application.persistentDataPath, file.Value);
                if (names.Contains(file.Value) || file.Value == "Parent Directory")
                {
                    WriteLine("({0}/{1}) cached.. {2}", currentTarget++, totalTargets, file.Value);
                    names.Remove(file.Value);
                
                    if (Time.realtimeSinceStartup - time2 > Time.deltaTime)
                    {
                        time2 = Time.realtimeSinceStartup;
                        yield return null;
                    }
                
                    continue;
                }

                WriteLine("({0}/{1}) download.. {2}", currentTarget++, totalTargets, uri.AbsoluteUri);
                var request = WebRequest.Create(uri);
                request.Method = "POST";
                using (var response = request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var fileStream = File.OpenWrite(filepath))
                {
                    CopyStream(responseStream, fileStream);
#if UNITY_IOS
                    UnityEngine.iOS.Device.SetNoBackupFlag(filepath);
#endif
                }

                yield return null;
            }

            // clear caches
            foreach (var cacheName in names)
            {
                var cachepath = Path.Combine(Application.persistentDataPath, cacheName);
                if (File.Exists(cachepath))
                    File.Delete(cachepath);
                WriteLine("clear.. {0}", cachepath);
            }
        
            var downloaded = (from exist in Directory.GetFiles(Application.persistentDataPath, "*",
                    SearchOption.TopDirectoryOnly)
                select Path.GetFileName(exist)).ToList();

            // random select
            var total = (double) downloaded.Count;
            if (total > 0)
            {
                var selected = (int) (new System.Random().NextDouble() * total);
                var selectedfilepath = Path.Combine(Application.persistentDataPath, downloaded[selected]);
                GameManager.Rompath = selectedfilepath;

                // next scene
                yield return new WaitForSeconds(2.0f);
                SceneManager.LoadScene("main");
            }
            else
            {
                WriteLine("no caches!");
            }
        }

        private static void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[32768];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                output.Write(buffer, 0, read);
        }
    }
}