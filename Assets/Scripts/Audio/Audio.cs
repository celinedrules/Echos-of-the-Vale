// Done

using Data.Audio;
using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class Audio : MonoBehaviour
    {
        [SerializeField] private AudioData audioData;

        public AudioSource AudioSource { get; private set; }
        public AudioData AudioData => audioData;

        private void OnValidate()
        {
            if (!AudioSource)
                AudioSource = GetComponent<AudioSource>();
        }

        private void Awake()
        {
            if (!AudioSource)
                AudioSource = GetComponent<AudioSource>();
        }
    }
}