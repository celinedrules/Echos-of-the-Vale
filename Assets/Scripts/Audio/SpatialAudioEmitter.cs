// Done
using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SpatialAudioEmitter : MonoBehaviour
    {
        private static float _globalVolume = 1f;
        public static float GlobalVolume
        {
            get => _globalVolume;
            set
            {
                _globalVolume = Mathf.Clamp01(value);
                OnGlobalVolumeChanged?.Invoke(_globalVolume);
            }
        }

        public static event System.Action<float> OnGlobalVolumeChanged;

        [SerializeField] private float minDistance = 1f;
        [SerializeField] private float maxDistance = 20f;
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;

        private AudioSource _source;

        public AudioSource Source => _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            ConfigureSource();
        }

        private void OnEnable()
        {
            OnGlobalVolumeChanged += ApplyGlobalVolume;
        }

        private void OnDisable()
        {
            OnGlobalVolumeChanged -= ApplyGlobalVolume;
        }

        private void ConfigureSource()
        {
            _source.spatialBlend = 1f; // Full 3D
            _source.rolloffMode = AudioRolloffMode.Linear;
            _source.minDistance = minDistance;
            _source.maxDistance = maxDistance;
            _source.playOnAwake = false;
            _source.volume = volume * _globalVolume;
        }

        private void ApplyGlobalVolume(float globalVolume)
        {
            _source.volume = volume * globalVolume;
        }

        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            _source.volume = volume * _globalVolume;
        }
    }
}