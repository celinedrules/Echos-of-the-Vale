using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.DialogueData
{
    [Serializable]
    public class DialogueBlackboardSpeakerNodeData
    {
        [SerializeField] private int nodeId;
        [SerializeField] private DialogueSpeakerData speaker;
        [SerializeField] private Vector2 position;
        [SerializeField] private List<int> targetRowIds = new();

        public int NodeId => nodeId;
        public DialogueSpeakerData Speaker => speaker;

        public Vector2 Position
        {
            get => position;
            set => position = value;
        }

        public List<int> TargetRowIds => targetRowIds;

        public DialogueBlackboardSpeakerNodeData(int nodeId, DialogueSpeakerData speaker, Vector2 position)
        {
            this.nodeId = nodeId;
            this.speaker = speaker;
            this.position = position;
        }
    }
}