// Done
using System.Linq;
using UnityEngine;

namespace Audio
{
    public static class AudioUtilities
    {
        public static AudioObject GetAudioObjectFromTrack(string audioId, AudioTrack track) =>
            track.audio.FirstOrDefault(audio => audio.audioId == audioId);

        public static AudioClip GetAudioClipFromAudioTrack(string audioId, AudioTrack track) =>
            GetAudioObjectFromTrack(audioId, track)?.clip;

        public static AudioClip GetRandomAudioClipFromAudioTrack(string audioId, AudioTrack track)
        {
            AudioClip[] matchingClips = track.audio
                .Where(audio => audio.audioId == audioId && audio.clip != null)
                .Select(audio => audio.clip)
                .ToArray();

            if (matchingClips.Length == 0)
                return null;

            return matchingClips[Random.Range(0, matchingClips.Length)];
        }

        public static bool IsAudioPlaying(string audioId, AudioTrack track)
        {
            return track.Source.isPlaying && track.Source.clip == GetAudioClipFromAudioTrack(audioId, track);
        }
    }
}