// Done
using System.Collections;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Audio
{
    public partial class FusionAudioManager : Singleton<FusionAudioManager>
    {
        [SerializeField] private bool debug;
        [SerializeField] private AudioTrack[] tracks;

        private Hashtable _audioTable;
        private Hashtable _jobTable;
        private float[] _trackTargetVolumes;

        public AudioTrack[] Tracks
        {
            get => tracks;
            set => tracks = value;
        }

        protected override void Awake()
        {
            base.Awake();
            Configure();
        }

        private void OnDisable() => Dispose();

        private void Configure()
        {
            _audioTable = new Hashtable();
            _jobTable = new Hashtable();
            _trackTargetVolumes = new float[tracks.Length];
            
            // Initialize target volumes from track defaults
            for (int i = 0; i < tracks.Length; i++)
                _trackTargetVolumes[i] = tracks[i].volume;
            
            GenerateAudioTable();
            ApplyAllVolumes();
        }

        private void ApplyAllVolumes()
        {
            foreach (AudioTrack track in tracks)
                track.ApplyVolume();
        }

        private void Dispose()
        {
            if (_jobTable == null)
                return;

            foreach (IEnumerator job in _jobTable.Values)
                StopCoroutine(job);
        }

        private void GenerateAudioTable()
        {
            for (int i = 0; i < tracks.Length; i++)
            {
                AudioTrack track = tracks[i];
                AudioTrackType trackType = (AudioTrackType)i;

                foreach (AudioObject audioObject in track.audio)
                {
                    if (string.IsNullOrEmpty(audioObject.audioId))
                        continue;

                    string key = $"{trackType}_{audioObject.audioId}";

                    if (_audioTable.ContainsKey(key))
                    {
                        LogWarning($"Audio [{trackType}:{audioObject.audioId}] already registered.");
                        continue;
                    }

                    _audioTable.Add(key, track);
                    Log($"Registered audio [{trackType}:{audioObject.audioId}].");
                }
            }
        }

        private void AddJob(AudioJob job)
        {
            RemoveConflictingJobs(job);

            IEnumerator runningJob = RunAudioJob(job);
            _jobTable[job.Key] = runningJob;
            StartCoroutine(runningJob);
        }

        private IEnumerator RunAudioJob(AudioJob job)
        {
            yield return new WaitForSeconds(job.Delay);

            string lookupKey = $"{job.TrackType}_{job.AudioId}";
            AudioTrack track = (AudioTrack)_audioTable[lookupKey];

            // Use override source if provided, otherwise use track's source
            AudioSource source = job.SourceOverride != null ? job.SourceOverride : track.Source;

            if (source == null)
            {
                LogWarning($"No AudioSource available for job [{job.Key}]");
                yield break;
            }

            // Get the audio object to access loop setting
            AudioObject audioObject = AudioUtilities.GetAudioObjectFromTrack(job.AudioId, track);

            // Select clip based on whether random selection is requested
            source.clip = job.RandomClip
                ? AudioUtilities.GetRandomAudioClipFromAudioTrack(job.AudioId, track)
                : audioObject?.clip;

            source.pitch = job.Speed;
            source.loop = audioObject?.loop ?? false;

            switch (job.Action)
            {
                case AudioAction.Start:
                    source.Play();
                    break;
                case AudioAction.Stop:
                    if (!job.Fade)
                        source.Stop();
                    break;
                case AudioAction.Pause:
                    source.Pause();
                    break;
                case AudioAction.Resume:
                    source.UnPause();
                    break;
                case AudioAction.Restart:
                    source.Stop();
                    source.Play();
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }

            if (job.Fade)
            {
                float targetVolume = GetTrackTargetVolume(job.TrackType);
                float initial = job.Action is AudioAction.Start or AudioAction.Restart ? 0 : targetVolume;
                float target = job.Action is AudioAction.Stop ? 0 : targetVolume;
                float duration = job.FadeDuration;
                float timer = 0.0f;

                while (timer <= duration)
                {
                    source.volume = Mathf.Lerp(initial, target, timer / duration);
                    timer += Time.deltaTime;
                    yield return null;
                }

                source.volume = target;

                if (job.Action == AudioAction.Stop)
                    source.Stop();
            }

            _jobTable.Remove(job.Key);
            Log($"Job count: {_jobTable.Count}");

            yield return null;
        }

        private void RemoveJob(string key)
        {
            if (!_jobTable.ContainsKey(key))
            {
                LogWarning($"Trying to stop a job [{key}] that is not running.");
                return;
            }

            IEnumerator runningJob = (IEnumerator)_jobTable[key];
            StopCoroutine(runningJob);
            _jobTable.Remove(key);
        }

        private void RemoveConflictingJobs(AudioJob job)
        {
            if (_jobTable.ContainsKey(job.Key))
                RemoveJob(job.Key);

            string conflictingKey = null;
            AudioTrack audioTrackNeeded = (AudioTrack)_audioTable[job.Key];

            foreach (string key in from DictionaryEntry entry in _jobTable
                     select (string)entry.Key
                     into key
                     let audioTrackInUse = (AudioTrack)_audioTable[key]
                     where audioTrackNeeded.Source == audioTrackInUse.Source
                     select key)
            {
                conflictingKey = key;
            }

            if (!string.IsNullOrEmpty(conflictingKey))
                RemoveJob(conflictingKey);
        }

        // Base methods that typed overloads call into
        public void PlayAudio(AudioTrackType track, string audioId, bool fade = false, float delay = 0.0f,
            float speed = 1.0f) =>
            AddJob(new AudioJob(AudioAction.Start, track, audioId, fade, delay, speed));

        public void PlayAudio(AudioTrackType track, string audioId, float fadeDuration, float delay = 0.0f,
            float speed = 1.0f) =>
            AddJob(new AudioJob(AudioAction.Start, track, audioId, fade: true, delay, speed,
                fadeDuration: fadeDuration));

        public void PlayAudio(AudioTrackType track, string audioId, AudioSource source, bool fade = false,
            float delay = 0.0f, float speed = 1.0f) =>
            AddJob(new AudioJob(AudioAction.Start, track, audioId, fade, delay, speed, sourceOverride: source));

        public void PlayRandomAudio(AudioTrackType track, string audioId, bool fade = false, float delay = 0.0f,
            float speed = 1.0f) =>
            AddJob(new AudioJob(AudioAction.Start, track, audioId, fade, delay, speed, randomClip: true));

        public void PlayRandomAudio(AudioTrackType track, string audioId, AudioSource source, bool fade = false,
            float delay = 0.0f, float speed = 1.0f) =>
            AddJob(new AudioJob(AudioAction.Start, track, audioId, fade, delay, speed, sourceOverride: source,
                randomClip: true));

        public void StopAudio(AudioTrackType track, string audioId, bool fade = false, float delay = 0.0f) =>
            AddJob(new AudioJob(AudioAction.Stop, track, audioId, fade, delay));

        public void StopAudio(AudioTrackType track, string audioId, float fadeDuration, float delay = 0.0f) =>
            AddJob(new AudioJob(AudioAction.Stop, track, audioId, fade: true, delay, fadeDuration: fadeDuration));

        public void StopAudio(AudioSource source, bool fade = false, float delay = 0.0f) => source?.Stop();

        public void PauseAudio(AudioTrackType track, string audioId, float delay = 0.0f) =>
            AddJob(new AudioJob(AudioAction.Pause, track, audioId, false, delay));

        public void ResumeAudio(AudioTrackType track, string audioId, float delay = 0.0f) =>
            AddJob(new AudioJob(AudioAction.Resume, track, audioId, false, delay));

        public void RestartAudio(AudioTrackType track, string audioId, bool fade = false, float delay = 0.0f,
            float speed = 1.0f) =>
            AddJob(new AudioJob(AudioAction.Restart, track, audioId, fade, delay, speed));

        public void StopTrack(AudioTrackType trackType, float fadeDuration = 0f)
        {
            int index = (int)trackType;
            if (index < 0 || index >= tracks.Length) return;

            AudioSource source = tracks[index].Source;
            if (source == null || !source.isPlaying) return;

            if (fadeDuration > 0)
                StartCoroutine(FadeOutTrackCoroutine(source, fadeDuration));
            else
                source.Stop();
        }

        private IEnumerator FadeOutTrackCoroutine(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float timer = 0f;

            while (timer < duration)
            {
                source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }

            source.Stop();
            source.volume = startVolume; // Reset volume for next play
        }

        public bool IsTrackPlaying(AudioTrackType trackType)
        {
            int index = (int)trackType;
            
            if (index < 0 || index >= tracks.Length)
                return false;

            AudioSource source = tracks[index].Source;
            
            return source != null && source.isPlaying;
        }

        public bool IsAudioPlaying(AudioTrackType track, string audioId)
        {
            string key = $"{track}_{audioId}";
            if (_audioTable.Contains(key))
                return AudioUtilities.IsAudioPlaying(audioId, (AudioTrack)_audioTable[key]);

            LogWarning($"Trying to check a clip [{track}:{audioId}] that is not registered.");
            return false;
        }

        public void SetTrackVolume(AudioTrackType trackType, float volume)
        {
            int index = (int)trackType;

            if (index >= 0 && index < tracks.Length)
            {
                float clampedVolume = Mathf.Clamp01(volume);
                tracks[index].volume = clampedVolume;
                _trackTargetVolumes[index] = clampedVolume;
                tracks[index].ApplyVolume();
            }
        }
        
        public float GetTrackTargetVolume(AudioTrackType trackType)
        {
            int index = (int)trackType;
            
            if (index >= 0 && index < _trackTargetVolumes.Length)
                return _trackTargetVolumes[index];
            return 1f;
        }
        
        public void SetSpatialAudioVolume(float volume)
        {
            SpatialAudioEmitter.GlobalVolume = volume;
        }

        public float GetTrackVolume(AudioTrackType trackType)
        {
            int index = (int)trackType;
            if (index >= 0 && index < tracks.Length)
                return tracks[index].volume;
            return 0f;
        }

        public void Mute(bool mute)
        {
            foreach (AudioTrack track in tracks)
            {
                if (track.Source != null)
                    track.Source.mute = mute;
            }
        }

        private void Log(string msg) => DebugLogger.Log(msg, debug);
        private void LogWarning(string msg) => DebugLogger.LogWarning(msg, debug);
    }
}