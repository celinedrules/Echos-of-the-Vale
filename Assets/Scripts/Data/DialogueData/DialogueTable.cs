// Done
using System.Collections.Generic;
using UnityEngine;
using Utilities.Enums;

namespace Data.DialogueData
{
    [CreateAssetMenu(fileName = "DialogueTable", menuName = "Echos of the Vale/Dialogue Data/Dialogue Table")]
    public class DialogueTable : ScriptableObject
    {
        [SerializeField] private string tableName;
        [SerializeField] private List<DialogueRow> rows = new();

        public string TableName => tableName;
        public IReadOnlyList<DialogueRow> Rows => rows;

        public DialogueRow GetRow(int index)
        {
            if (index < 0 || index >= rows.Count)
                return null;

            return rows[index];
        }
        
        public DialogueRow GetRowById(int rowId)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].RowId == rowId)
                    return rows[i];
            }

            return null;
        }

        public DialogueRow FirstRow => rows.Count > 0 ? rows[0] : null;
        public int RowCount => rows.Count;
    }

    [System.Serializable]
    public class DialogueRow
    {
        [SerializeField] private int rowId;
        [SerializeField] private DialogueSpeakerData speaker;

        [TextArea]
        [SerializeField] private string[] textLines;

        [SerializeField] private Sprite portraitOverride;
        [SerializeField] private DialogueActionType actionType;

        [Header("Choice Settings")]
        [SerializeField] private string playerChoiceAnswer;
        [SerializeField] private int[] choiceRowIds;

        [Header("Audio")]
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private float audioStartTime;

        [Header("Flags")]
        [SerializeField] private bool dialogSkip;
        
        [Header("Flow")]
        [SerializeField] private int leadsTo = -1;

        public int RowId => rowId;
        public DialogueSpeakerData Speaker => speaker;
        public string[] TextLines => textLines;
        public Sprite PortraitOverride => portraitOverride;
        public DialogueActionType ActionType => actionType;
        public string PlayerChoiceAnswer => playerChoiceAnswer;
        public int[] ChoiceRowIds => choiceRowIds;
        public AudioClip AudioClip => audioClip;
        public float AudioStartTime => audioStartTime;
        public bool DialogSkip => dialogSkip;
        public int LeadsTo => leadsTo;

        /// <summary>
        /// Returns the portrait to display. Uses the override if set, otherwise falls back to the speaker's portrait.
        /// </summary>
        public Sprite GetPortrait()
        {
            if (portraitOverride != null)
                return portraitOverride;

            return speaker != null ? speaker.SpeakerPortrait : null;
        }

        public string GetFirstLine() => textLines is { Length: > 0 } ? textLines[0] : string.Empty;

        public string GetRandomLine() =>
            textLines is { Length: > 0 } ? textLines[Random.Range(0, textLines.Length)] : string.Empty;
    }
}