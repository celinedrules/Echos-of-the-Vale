// This file is auto-generated. Do not modify manually.
// Regenerate using the button on AudioData assets or Tools > Audio > Generate All Audio ID Enums

using UnityEngine;

namespace Audio
{
    public enum EnemyAudioId
    {
        EnemyHitAttack,
        EnemyMissAttack,
    }

    public static partial class AudioIdExtensions
    {
        public static string ToId(this EnemyAudioId audioId) => audioId.ToString();
    }

    public partial class FusionAudioManager
    {
        public void PlayAudio(EnemyAudioId audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayAudio(AudioTrackType.EnemyFX, audioId.ToId(), fade, delay, speed);

        public void PlayAudio(EnemyAudioId audioId, float fadeDuration, float delay = 0.0f, float speed = 1.0f) =>
            PlayAudio(AudioTrackType.EnemyFX, audioId.ToId(), fadeDuration, delay, speed);

        public void PlayAudio(EnemyAudioId audioId, AudioSource source, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayAudio(AudioTrackType.EnemyFX, audioId.ToId(), source, fade, delay, speed);

        public void PlayRandomAudio(EnemyAudioId audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayRandomAudio(AudioTrackType.EnemyFX, audioId.ToId(), fade, delay, speed);

        public void PlayRandomAudio(EnemyAudioId audioId, AudioSource source, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayRandomAudio(AudioTrackType.EnemyFX, audioId.ToId(), source, fade, delay, speed);

        public void StopAudio(EnemyAudioId audioId, bool fade = false, float delay = 0.0f) =>
            StopAudio(AudioTrackType.EnemyFX, audioId.ToId(), fade, delay);

        public void PauseAudio(EnemyAudioId audioId, float delay = 0.0f) =>
            PauseAudio(AudioTrackType.EnemyFX, audioId.ToId(), delay);

        public void ResumeAudio(EnemyAudioId audioId, float delay = 0.0f) =>
            ResumeAudio(AudioTrackType.EnemyFX, audioId.ToId(), delay);

        public void RestartAudio(EnemyAudioId audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            RestartAudio(AudioTrackType.EnemyFX, audioId.ToId(), fade, delay, speed);

        public bool IsAudioPlaying(EnemyAudioId audioId) =>
            IsAudioPlaying(AudioTrackType.EnemyFX, audioId.ToId());
    }
}
