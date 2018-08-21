using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnitySnes
{
    public class GameManager : MonoBehaviour
    {
        public static string Rompath;
        
        public Renderer Display;
        public Speaker Speaker;
        private LibretroWrapper.Wrapper _wrapper;

        private void Start()
        {
            if (!string.IsNullOrEmpty(Rompath))
            {
                Application.targetFrameRate = 60;
                _wrapper = new LibretroWrapper.Wrapper();
                _wrapper.Init(Display, Speaker);
                _wrapper.LoadGame(Rompath);
            }
            else
            {
                SceneManager.LoadScene("loader");
            }
        }

        private void Update()
        {
            if (_wrapper != null)
                _wrapper.Update();
        }

        private void OnDisable()
        {
            if (_wrapper == null) return;
            _wrapper.UnloadGame();
            _wrapper.DeInit();
            _wrapper = null;
        }
    }
}