using System;
using UnityEngine;

namespace UnitySnes
{
    public class Speaker : MonoBehaviour
    {
        public AudioSource AudioSource;
        private float[] _newData = new float[System.AudioBatchSize];

        private void Start()
        {
            AudioSource = GetComponent<AudioSource>();
            AudioSource.clip = AudioClip.Create("UnitySnes", System.AudioBatchSize / 2, 2, 32040, true, OnAudioRead); //44100
        }

        public void UpdateAudio(float[] sampleData)
        {
            _newData = sampleData;
            AudioSource.Play();
        }

        private void OnAudioRead(float[] sampleData)
        {
            Array.Copy(_newData, sampleData, sampleData.Length);
        }
    }
}