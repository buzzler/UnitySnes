using System;
using UnityEngine;

namespace UnitySnes
{
    [RequireComponent(typeof(AudioSource))]
    public class Speaker : MonoBehaviour
    {
        private AudioSource _speaker;
        private float[] _newData = new float[LibretroWrapper.Wrapper.AudioBatchSize];

        private void Start()
        {
            _speaker = GetComponent<AudioSource>();
            var clip = AudioClip.Create("Libretro", LibretroWrapper.Wrapper.AudioBatchSize / 2, 2, 44100, true,
                OnAudioRead);
            _speaker.clip = clip;
            _speaker.Play();
            Debug.Log("Unity sample rate: " + AudioSettings.outputSampleRate);
        }

        public void UpdateAudio(float[] sampleData)
        {
            _newData = sampleData;
            _speaker.Play();
        }

        private void OnAudioRead(float[] sampleData)
        {
            Array.Copy(_newData, sampleData, sampleData.Length);
        }
    }
}