using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Enums;

namespace Data.DialogueData
{
    [System.Serializable]
    public class DialogueRow
    {
        private string RowTitle
        {
            get
            {
                string speakerName = speaker != null ? speaker.SpeakerName : "No Speaker";

                string previewText = IsChoiceResponseRow
                    ? playerChoiceAnswer
                    : GetFirstLine();

                if (string.IsNullOrWhiteSpace(previewText))
                    previewText = rowKind.ToString();

                previewText = GetTrimmedPreview(previewText, 50);

                return $"[{rowId}: {speakerName}] - {previewText}";
            }
        }

        private static string GetTrimmedPreview(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string trimmed = text.Trim();

            if (trimmed.Length <= maxLength)
                return trimmed;

            int lastSpaceIndex = trimmed.LastIndexOf(' ', maxLength);
            if (lastSpaceIndex > 0)
                return $"{trimmed.Substring(0, lastSpaceIndex)}...";

            return $"{trimmed.Substring(0, maxLength)}...";
        }

        [FoldoutGroup("$RowTitle", Expanded = false)]
        [BoxGroup("$RowTitle/Identity")]
        [SerializeField] private int rowId;

        [FoldoutGroup("$RowTitle", Expanded = false)]
        [BoxGroup("$RowTitle/Identity")]
        [SerializeField] private DialogueRowKind rowKind;

        [FoldoutGroup("$RowTitle", Expanded = false)]
        [BoxGroup("$RowTitle/Identity")]
        [SerializeField] private DialogueSpeakerData speaker;

        [FoldoutGroup("$RowTitle", Expanded = false)]
        [BoxGroup("$RowTitle/Content")]
        [TextArea]
        [ValidateInput(nameof(HasTextLines), "This row should have at least one text line.")]
        [SerializeField] private string[] textLines;

        [FoldoutGroup("$RowTitle", Expanded = false)]
        [BoxGroup("$RowTitle/Content")]
        [SerializeField] private Sprite portraitOverride;

        [FoldoutGroup("$RowTitle", Expanded = false)]
        [BoxGroup("$RowTitle/Interaction")]
        [FormerlySerializedAs("actionType")]
        [SerializeField] private DialogueRowAction rowAction;

        [FoldoutGroup("$RowTitle", Expanded = false)]
        [BoxGroup("$RowTitle/Choice Settings")]
        [ShowIf(nameof(IsChoiceResponseRow))]
        [LabelText("Choice Answer")]
        [ValidateInput(nameof(HasChoiceAnswerIfNeeded), "Choice response rows must have a Player Choice Answer.")]
        [SerializeField] private string playerChoiceAnswer;

        [FoldoutGroup("$RowTitle", Expanded = false)]
        [BoxGroup("$RowTitle/Choice Settings")]
        [ShowIf(nameof(IsChoicePromptRow))]
        [LabelText("Choice Row Ids")]
        [ValidateInput(nameof(HasChoiceRowsIfNeeded), "Choice prompt rows must define at least one Choice Row Id.")]
        [SerializeField] private int[] choiceRowIds;

        [FoldoutGroup("$RowTitle", Expanded = false)]
        [BoxGroup("$RowTitle/Audio")]
        [SerializeField] private AudioClip audioClip;

        [FoldoutGroup("$RowTitle", Expanded = false)]
        [BoxGroup("$RowTitle/Audio")]
        [SerializeField] private float audioStartTime;

        [FoldoutGroup("$RowTitle", Expanded = false)]
        [BoxGroup("$RowTitle/Flags")]
        [SerializeField] private bool dialogSkip;

        [FoldoutGroup("$RowTitle", Expanded = false)]
        [BoxGroup("$RowTitle/Flow")]
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