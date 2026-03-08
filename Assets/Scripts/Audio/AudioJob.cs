// Done
using UnityEngine;

namespace Audio
{
    public class AudioJob
    {
        public readonly AudioAction Action;
        public readonly AudioTrackType TrackType;
        public readonly string AudioId;
        public readonly bool Fade;
        public readonly float FadeDuration;
        public readonly float Delay;
        public readonly float Speed;
        public readonly bool RandomClip;
        public readonly AudioSource SourceOverride;

        public AudioJob(AudioAction action, AudioTrackType trackType, string audioId, bool fade, float delay, 
            float speed = 1.0f, bool randomClip = false, AudioSource sourceOverride = null, float fadeDuration = 1.0f)
        {
            Action = action;
            TrackType = trackType;
            AudioId = audioId;
            Fade = fade;
            FadeDuration = fadeDuration;
            Delay = delay;
            Speed = speed;
            RandomClip = randomClip;
            SourceOverride = sourceOverride;
        }

        public string Key => SourceOverride != null 
            ? $"{TrackType}_{AudioId}_{SourceOverride.GetInstanceID()}" 
            : $"{TrackType}_{AudioId}";
    }

    public enum AudioAction
    {
        Start,
        Stop,
        Restart,
        Pause,
        Resume
    }
}