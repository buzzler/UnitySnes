using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UnitySnes
{
    public class ViewController : Views
    {
        public Button[] Buttons;
        public Text[] Labels;
        public Text[] Descriptions;
        private bool _waiting;

        protected override void Start()
        {
            base.Start();
            UpdateLabel();
        }

        public IEnumerator OnTouchButton(int snesInput)
        {
            if (_waiting)
                yield break;

            _waiting = true;
            var inputMapper = Frontend.InputMapper;
            var buffers = Backend.Buffers;

            inputMapper.SetKey(snesInput, string.Empty, string.Empty);
            buffers.LastKey = string.Empty;
            UpdateLabel(snesInput, "waiting");
            yield return null;

            // detect new press key
            while (string.IsNullOrEmpty(buffers.LastKey))
                yield return null;
            var press = buffers.LastKey;
            buffers.LastKey = string.Empty;

            // detect new release key
            while (string.IsNullOrEmpty(buffers.LastKey))
                yield return null;
            var release = buffers.LastKey;
            buffers.LastKey = string.Empty;

            inputMapper.SetKey(snesInput, press, release);
            UpdateLabel();
            _waiting = false;
        }

        public void OnTouchDefault()
        {
            Frontend.InputMapper?.SetKeyAsICade();
            UpdateLabel();
        }
        
        public void OnTouchBack()
        {
            Frontend.OnMenuOpen("ui/settings");
        }

        private void UpdateLabel(int snesInput, string label)
        {
            var inputMapper = Frontend.InputMapper;
            var t = inputMapper.GetKey(snesInput);
            Buttons[snesInput].onClick.AddListener(() => { StartCoroutine(OnTouchButton(snesInput)); });
            Labels[snesInput].text = label;

            var empty1 = string.IsNullOrEmpty(t.Item1);
            var empty2 = string.IsNullOrEmpty(t.Item2);
            
            if (!empty1 && !empty2)
                Descriptions[snesInput].text = $"press: {t.Item1} release: {t.Item2}";
            else if (empty1 && !empty2)
                Descriptions[snesInput].text = $"release: {t.Item2}";
            else if (empty2 && !empty1)
                Descriptions[snesInput].text = $"press: {t.Item1}";
            else
                Descriptions[snesInput].text = "";
        }
        
        public void UpdateLabel()
        {
            foreach (var button in Buttons)
                button.onClick.RemoveAllListeners();

            UpdateLabel(SnesInput.Up, "UP");
            UpdateLabel(SnesInput.Down, "DOWN");
            UpdateLabel(SnesInput.Left, "LEFT");
            UpdateLabel(SnesInput.Right, "RIGHT");
            UpdateLabel(SnesInput.Select, "SELECT");
            UpdateLabel(SnesInput.Start, "START");
            UpdateLabel(SnesInput.A, "A");
            UpdateLabel(SnesInput.B, "B");
            UpdateLabel(SnesInput.X, "X");
            UpdateLabel(SnesInput.Y, "Y");
            UpdateLabel(SnesInput.L, "L");
            UpdateLabel(SnesInput.R, "R");
        }
    }
}
