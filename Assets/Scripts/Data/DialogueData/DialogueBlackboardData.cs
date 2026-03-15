using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.DialogueData
{
    [Serializable]
    public class DialogueBlackboardData
    {
        [SerializeField] private List<DialogueSpeakerData> speakers = new();

        public IReadOnlyList<DialogueSpeakerData> Speakers => speakers;

        public bool AddSpeaker(DialogueSpeakerData speaker)
        {
            if (speaker == null)
                return false;

            if (speakers.Contains(speaker))
                return false;

            speakers.Add(speaker);
            speakers.Sort((a, b) =>
                string.Compare(GetSpeakerSortName(a), GetSpeakerSortName(b), StringComparison.OrdinalIgnoreCase));

            return true;
        }

        public bool RemoveSpeaker(DialogueSpeakerData speaker)
        {
            if (speaker == null)
                return false;

            return speakers.Remove(speaker);
        }

        private static string GetSpeakerSortName(DialogueSpeakerData speaker)
        {
            if (speaker == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(speaker.SpeakerName))
                return speaker.SpeakerName.Trim();

            return speaker.name;
        }
    }
}