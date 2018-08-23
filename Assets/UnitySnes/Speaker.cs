using System;
using UnityEngine;

namespace UnitySnes
{
    [RequireComponent(typeof(AudioSource))]
    public class Speaker : MonoBehaviour
    {
        private AudioSource _speaker;
        private float[] _newData = new float[System.AudioBatchSize];

        private void Start()
        {
            _speaker = GetComponent<AudioSource>();
            _speaker.clip = AudioClip.Create("UnitySnes", System.AudioBatchSize / 2, 2, 32040, true, OnAudioRead); //44100
            _speaker.loop = true;
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