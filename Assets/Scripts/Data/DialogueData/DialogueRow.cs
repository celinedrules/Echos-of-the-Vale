using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Enums;

namespace Data.DialogueData
{
    [System.Serializable]
    public class DialogueRow
    {
        [BoxGroup("Identity")]
        [SerializeField] private int rowId;

        [BoxGroup("Identity")]
        [SerializeField] private DialogueRowKind rowKind;

        [BoxGroup("Identity")]
        [SerializeField] private DialogueSpeakerData speaker;

        [BoxGroup("Identity")]
        [ShowInInspector, ReadOnly, PropertyOrder(-10)]
        [LabelText("Row Summary")]
        private string RowSummary => GetRowSummary();

        [BoxGroup("Content")]
        [TextArea]
        [ValidateInput(nameof(HasTextLines), "This row should have at least one text line.")]
        [SerializeField] private string[] textLines;

        [BoxGroup("Content")]
        [SerializeField] private Sprite portraitOverride;

        [BoxGroup("Interaction")]
        [FormerlySerializedAs("actionType")]
        [SerializeField] private DialogueRowAction rowAction;

        [BoxGroup("Choice Settings")]
        [ShowIf(nameof(IsChoiceResponseRow))]
        [LabelText("Choice Answer")]
        [ValidateInput(nameof(HasChoiceAnswerIfNeeded), "Choice response rows must have a Player Choice Answer.")]
        [SerializeField] private string playerChoiceAnswer;

        [BoxGroup("Choice Settings")]
        [ShowIf(nameof(IsChoicePromptRow))]
        [LabelText("Choice Row Ids")]
        [ValidateInput(nameof(HasChoiceRowsIfNeeded), "Choice prompt rows must define at least one Choice Row Id.")]
        [SerializeField] private int[] choiceRowIds;

        [BoxGroup("Audio")]
        [SerializeField] private AudioClip audioClip;

        [BoxGroup("Audio")]
        [SerializeField] private float audioStartTime;

        [BoxGroup("Flags")]
        [SerializeField] private bool dialogSkip;

        [BoxGroup("Flow")]
        [ShowIf(nameof(UsesLeadsTo))]
        [ValidateInput(nameof(HasValidLeadsToValue), "Leads To must be -1 or a valid non-negative row ID.")]
        [SerializeField] private int leadsTo = -1;

        public int RowId => rowId;
        public DialogueRowKind RowKind => rowKind;
        public DialogueSpeakerData Speaker => speaker;
        public string[] TextLines => textLines;
        public Sprite PortraitOverride => portraitOverride;
        public DialogueRowAction RowAction => rowAction;
        public string PlayerChoiceAnswer => playerChoiceAnswer;
        public int[] ChoiceRowIds => choiceRowIds;
        public AudioClip AudioClip => audioClip;
        public float AudioStartTime => audioStartTime;
        public bool DialogSkip => dialogSkip;
        public int LeadsTo => leadsTo;

        public bool IsLineRow => rowKind == DialogueRowKind.Line;
        public bool IsChoicePromptRow => rowKind == DialogueRowKind.ChoicePrompt;
        public bool IsChoiceResponseRow => rowKind == DialogueRowKind.ChoiceResponse;
        public bool UsesLeadsTo => rowKind != DialogueRowKind.ChoicePrompt;

        public void SetRowId(int newRowId) => rowId = newRowId;
        public void SetLeadsTo(int newLeadsTo) => leadsTo = newLeadsTo;
        public void SetChoiceRowIds(int[] newChoiceRowIds) => choiceRowIds = newChoiceRowIds;

        private string GetRowSummary()
        {
            return rowKind switch
            {
                DialogueRowKind.Line => "Line: shows dialogue text and optionally continues via Leads To.",
                DialogueRowKind.ChoicePrompt => "Choice Prompt: shows dialogue text and displays options from Choice Row Ids.",
                DialogueRowKind.ChoiceResponse => "Choice Response: uses Choice Answer as button text, then shows Text Lines after selection.",
                _ => string.Empty
            };
        }

        private bool HasTextLines()
        {
            if (textLines == null || textLines.Length == 0)
                return false;

            for (int i = 0; i < textLines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(textLines[i]))
                    return true;
            }

            return false;
        }

        private bool HasChoiceAnswerIfNeeded()
        {
            if (!IsChoiceResponseRow)
                return true;

            return !string.IsNullOrWhiteSpace(playerChoiceAnswer);
        }

        private bool HasChoiceRowsIfNeeded()
        {
            if (!IsChoicePromptRow)
                return true;

            return choiceRowIds != null && choiceRowIds.Length > 0;
        }

        private bool HasValidLeadsToValue()
        {
            if (!UsesLeadsTo)
                return true;

            return leadsTo >= -1;
        }

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