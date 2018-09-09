using System.Text.RegularExpressions;
using UnityEngine;

namespace UnitySnes
{
    public class SimpleFilter : MonoBehaviour
    {
        public Shader Shader;
        public Camera Camera;
        private int _passes;
        private RenderTexture _texture1;
        private RenderTexture _texture2;
        private Material _material;
        private Vector2 _internal;

        private void Start()
        {
            var shadername = Shader.name;
            {
                var match = new Regex(".+/([1-9])x.+").Match(shadername);
                _passes = match.Success ? int.Parse(match.Groups[1].Value) : 1;
            }
            
            _internal = new Vector2(Mathf.RoundToInt(224f / Screen.height * Screen.width), 224f);
            if (!(_internal.x % 2).Equals(0f))
                _internal.x--;
            if (!(_internal.y % 2).Equals(0f))
                _internal.y--;
            
            _material = new Material(Shader);
            _texture1 = new RenderTexture((int)_internal.x, (int)_internal.y, 0);
            _texture1.filterMode = FilterMode.Point;
            _texture1.Create();
            if (_passes != 1)
            {
                _texture2 = new RenderTexture((int) _internal.x * _passes, (int) _internal.y * _passes, 0);
                _texture2.filterMode = FilterMode.Point;
                _texture2.Create();
            }
        }

        private void OnPreRender()
        {
            Camera.targetTexture = _texture1;
        }
        
        private void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            src.filterMode = FilterMode.Point;
            _material.SetVector("texture_size", _internal);
            _material.SetTexture("decal", src);
            _material.SetTexture("_BackgroundTexture", src);
            _material.SetTexture("_MainTex", src);
            
            if (_passes == 1)
            {
                Camera.targetTexture = null;
                Graphics.Blit(src, null, _material);
            }
            else
            {
                Graphics.Blit(src, _texture2, _material);
                Camera.targetTexture = null;
                Graphics.Blit(_texture2, null as RenderTexture);
            }
        }

        private void OnDestroy()
        {
            if (_texture1 != null)
                _texture1.DiscardContents();
            if (_texture2 != null)
                _texture2.DiscardContents();
            if (_material != null)
                _material.mainTexture = null;

            Shader = null;
            Camera = null;
            _texture1 = null;
            _texture2 = null;
            _material = null;
        }
    }
}