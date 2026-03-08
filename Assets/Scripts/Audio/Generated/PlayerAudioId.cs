// This file is auto-generated. Do not modify manually.
// Regenerate using the button on AudioData assets or Tools > Audio > Generate All Audio ID Enums

using UnityEngine;

namespace Audio
{
    public enum PlayerAudioId
    {
        PlayerHitAttack,
        PlayerMissAttack,
    }

    public static partial class AudioIdExtensions
    {
        public static string ToId(this PlayerAudioId audioId) => audioId.ToString();
    }

    public partial class FusionAudioManager
    {
        public void PlayAudio(PlayerAudioId audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayAudio(AudioTrackType.PlayerFX, audioId.ToId(), fade, delay, speed);

        public void PlayAudio(PlayerAudioId audioId, float fadeDuration, float delay = 0.0f, float speed = 1.0f) =>
            PlayAudio(AudioTrackType.PlayerFX, audioId.ToId(), fadeDuration, delay, speed);

        public void PlayAudio(PlayerAudioId audioId, AudioSource source, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayAudio(AudioTrackType.PlayerFX, audioId.ToId(), source, fade, delay, speed);

        public void PlayRandomAudio(PlayerAudioId audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayRandomAudio(AudioTrackType.PlayerFX, audioId.ToId(), fade, delay, speed);

        public void PlayRandomAudio(PlayerAudioId audioId, AudioSource source, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            PlayRandomAudio(AudioTrackType.PlayerFX, audioId.ToId(), source, fade, delay, speed);

        public void StopAudio(PlayerAudioId audioId, bool fade = false, float delay = 0.0f) =>
            StopAudio(AudioTrackType.PlayerFX, audioId.ToId(), fade, delay);

        public void PauseAudio(PlayerAudioId audioId, float delay = 0.0f) =>
            PauseAudio(AudioTrackType.PlayerFX, audioId.ToId(), delay);

        public void ResumeAudio(PlayerAudioId audioId, float delay = 0.0f) =>
            ResumeAudio(AudioTrackType.PlayerFX, audioId.ToId(), delay);

        public void RestartAudio(PlayerAudioId audioId, bool fade = false, float delay = 0.0f, float speed = 1.0f) =>
            RestartAudio(AudioTrackType.PlayerFX, audioId.ToId(), fade, delay, speed);

        public bool IsAudioPlaying(PlayerAudioId audioId) =>
            IsAudioPlaying(AudioTrackType.PlayerFX, audioId.ToId());
    }
}
