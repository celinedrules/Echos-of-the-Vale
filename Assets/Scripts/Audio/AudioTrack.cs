// Done
using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    [System.Serializable]
    public class AudioObject
    {
        public string audioId;
        public AudioClip clip;
        public bool loop;
    }

    [System.Serializable]
    public class AudioTrack
    {
        public Audio audioComponent;
        [Range(0f, 1f)] public float volume = 1f;
        public AudioObject[] audio;

        public AudioSource Source => audioComponent != null ? audioComponent.AudioSource : null;

        public void ApplyVolume()
        {
            if (Source != null)
                Source.volume = volume;
        }

        public IEnumerable<string> GetAudioIds()
        {
            if (audioComponent != null && audioComponent.AudioData != null)
                return audioComponent.AudioData.AudioIds ?? System.Array.Empty<string>();

            return System.Array.Empty<string>();
        }
    }
}