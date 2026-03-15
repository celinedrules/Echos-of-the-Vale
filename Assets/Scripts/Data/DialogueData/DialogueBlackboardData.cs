using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.DialogueData
{
    [Serializable]
    public class DialogueBlackboardData
    {
        [SerializeField] private List<DialogueSpeakerData> speakers = new();
        [SerializeField] private List<DialogueBlackboardSpeakerNodeData> speakerNodes = new();

        public IReadOnlyList<DialogueSpeakerData> Speakers => speakers;
        public IReadOnlyList<DialogueBlackboardSpeakerNodeData> SpeakerNodes => speakerNodes;

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

        public DialogueBlackboardSpeakerNodeData AddSpeakerNode(int nodeId, DialogueSpeakerData speaker, Vector2 position)
        {
            if (speaker == null)
                return null;

            if (GetSpeakerNode(nodeId) != null)
                return null;

            DialogueBlackboardSpeakerNodeData node = new DialogueBlackboardSpeakerNodeData(nodeId, speaker, position);
            speakerNodes.Add(node);
            return node;
        }

        public bool RemoveSpeakerNode(int nodeId)
        {
            for (int i = speakerNodes.Count - 1; i >= 0; i--)
            {
                if (speakerNodes[i].NodeId != nodeId)
                    continue;

                speakerNodes.RemoveAt(i);
                return true;
            }

            return false;
        }

        public DialogueBlackboardSpeakerNodeData GetSpeakerNode(int nodeId)
        {
            for (int i = 0; i < speakerNodes.Count; i++)
            {
                if (speakerNodes[i].NodeId == nodeId)
                    return speakerNodes[i];
            }

            return null;
        }

        public bool HasSpeakerNode(int nodeId) => GetSpeakerNode(nodeId) != null;

        public int GetNextSpeakerNodeId()
        {
            int minNodeId = -2000;

            for (int i = 0; i < speakerNodes.Count; i++)
                minNodeId = Mathf.Min(minNodeId, speakerNodes[i].NodeId);

            return minNodeId - 1;
        }

        public bool AddSpeakerNodeLink(int nodeId, int targetRowId)
        {
            DialogueBlackboardSpeakerNodeData node = GetSpeakerNode(nodeId);
            if (node == null || targetRowId < 0)
                return false;

            if (node.TargetRowIds.Contains(targetRowId))
                return false;

            node.TargetRowIds.Add(targetRowId);
            node.TargetRowIds.Sort();
            return true;
        }

        public bool RemoveSpeakerNodeLink(int nodeId, int targetRowId)
        {
            DialogueBlackboardSpeakerNodeData node = GetSpeakerNode(nodeId);
            if (node == null)
                return false;

            return node.TargetRowIds.Remove(targetRowId);
        }

        public bool ClearSpeakerNodeLinks(int nodeId)
        {
            DialogueBlackboardSpeakerNodeData node = GetSpeakerNode(nodeId);
            if (node == null)
                return false;

            if (node.TargetRowIds.Count == 0)
                return false;

            node.TargetRowIds.Clear();
            return true;
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