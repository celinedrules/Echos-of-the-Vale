// This file is auto-generated. Do not modify manually.
// Regenerate using the button on AudioData assets or Tools > Audio > Generate All Audio ID Enums

using UnityEngine;

namespace Audio
{
    public enum BackgroundAudioId
    {
        Grasslands,
        MainMenu,
    }

    public static partial class AudioIdExtensions
    {
        public static string ToId(this BackgroundAudioId audioId) => audioId.ToString();
    }

    public partial class FusionAudioManager
    {
        public void PlayAudio(BackgroundAudioId audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayAudio(AudioTrackType.Background, audioId.ToId(), fade, delay, speed);

        public void PlayAudio(BackgroundAudioId audioId, float fadeDuration, float delay = 0.0f, float speed = 1.0f) =>
            PlayAudio(AudioTrackType.Background, audioId.ToId(), fadeDuration, delay, speed);

        public void PlayAudio(BackgroundAudioId audioId, AudioSource source, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayAudio(AudioTrackType.Background, audioId.ToId(), source, fade, delay, speed);

        public void PlayRandomAudio(BackgroundAudioId audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayRandomAudio(AudioTrackType.Background, audioId.ToId(), fade, delay, speed);

        public void PlayRandomAudio(BackgroundAudioId audioId, AudioSource source, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayRandomAudio(AudioTrackType.Background, audioId.ToId(), source, fade, delay, speed);

        public void StopAudio(BackgroundAudioId audioId, bool fade = false, float delay = 0.0f) =>
            StopAudio(AudioTrackType.Background, audioId.ToId(), fade, delay);

        public void PauseAudio(BackgroundAudioId audioId, float delay = 0.0f) =>
            PauseAudio(AudioTrackType.Background, audioId.ToId(), delay);

        public void ResumeAudio(BackgroundAudioId audioId, float delay = 0.0f) =>
            ResumeAudio(AudioTrackType.Background, audioId.ToId(), delay);

        public void RestartAudio(BackgroundAudioId audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            RestartAudio(AudioTrackType.Background, audioId.ToId(), fade, delay, speed);

        public bool IsAudioPlaying(BackgroundAudioId audioId) =>
            IsAudioPlaying(AudioTrackType.Background, audioId.ToId());
    }
}
