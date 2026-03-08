// This file is auto-generated. Do not modify manually.
// Regenerate using the button on AudioData assets or Tools > Audio > Generate All Audio ID Enums

using UnityEngine;

namespace Audio
{
    public enum UiAudioId
    {
        ButtonClicked,
    }

    public static partial class AudioIdExtensions
    {
        public static string ToId(this UiAudioId audioId) => audioId.ToString();
    }

    public partial class FusionAudioManager
    {
        public void PlayAudio(UiAudioId audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayAudio(AudioTrackType.UI, audioId.ToId(), fade, delay, speed);

        public void PlayAudio(UiAudioId audioId, float fadeDuration, float delay = 0.0f, float speed = 1.0f) =>
            PlayAudio(AudioTrackType.UI, audioId.ToId(), fadeDuration, delay, speed);

        public void PlayAudio(UiAudioId audioId, AudioSource source, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayAudio(AudioTrackType.UI, audioId.ToId(), source, fade, delay, speed);

        public void PlayRandomAudio(UiAudioId audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayRandomAudio(AudioTrackType.UI, audioId.ToId(), fade, delay, speed);

        public void PlayRandomAudio(UiAudioId audioId, AudioSource source, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayRandomAudio(AudioTrackType.UI, audioId.ToId(), source, fade, delay, speed);

        public void StopAudio(UiAudioId audioId, bool fade = false, float delay = 0.0f) =>
            StopAudio(AudioTrackType.UI, audioId.ToId(), fade, delay);

        public void PauseAudio(UiAudioId audioId, float delay = 0.0f) =>
            PauseAudio(AudioTrackType.UI, audioId.ToId(), delay);

        public void ResumeAudio(UiAudioId audioId, float delay = 0.0f) =>
            ResumeAudio(AudioTrackType.UI, audioId.ToId(), delay);

        public void RestartAudio(UiAudioId audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            RestartAudio(AudioTrackType.UI, audioId.ToId(), fade, delay, speed);

        public bool IsAudioPlaying(UiAudioId audioId) =>
            IsAudioPlaying(AudioTrackType.UI, audioId.ToId());
    }
}
