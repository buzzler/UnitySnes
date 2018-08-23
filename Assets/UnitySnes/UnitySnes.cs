using UnityEngine;

namespace UnitySnes
{
    public class UnitySnes : MonoBehaviour
    {
        public TextAsset Rom;
        public Renderer Display;
        public Speaker Speaker;
        private System _system;

        private void Start()
        {
            Application.targetFrameRate = 60;
            _system = new System();
            _system.Init(Display, Speaker);                
            _system.LoadGame(Rom.bytes);
        }

        private void Update()
        {
            if (_system != null)
                _system.Update();
        }

        private void OnDisable()
        {
            if (_system == null) return;
            _system.UnloadGame();
            _system.DeInit();
            _system = null;
        }
    }
}