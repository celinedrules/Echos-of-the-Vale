using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Enums;

namespace Data.DialogueData
{
    [System.Serializable]
    public class DialogueRow
    {
        [SerializeField] private int rowId;
        [SerializeField] private DialogueRowKind rowKind;
        [SerializeField] private DialogueSpeakerData speaker;

        [TextArea]
        [ValidateInput(nameof(HasTextLines), "This row should have at least one text line.")]
        [SerializeField] private string[] textLines;

        [SerializeField] private Sprite portraitOverride;

        [FormerlySerializedAs("actionType")]
        [SerializeField] private DialogueRowAction rowAction;

        [ShowIf(nameof(IsChoiceResponseRow))]
        [LabelText("Choice Answer")]
        [ValidateInput(nameof(HasChoiceAnswerIfNeeded), "Choice response rows must have a Player Choice Answer.")]
        [SerializeField] private string playerChoiceAnswer;

        [ShowIf(nameof(IsChoicePromptRow))]
        [LabelText("Choice Row Ids")]
        [ValidateInput(nameof(HasChoiceRowsIfNeeded), "Choice prompt rows must define at least one Choice Row Id.")]
        [SerializeField] private int[] choiceRowIds;

        [SerializeField] private AudioClip audioClip;
        [SerializeField] private float audioStartTime;
        [SerializeField] private bool dialogSkip;

        [ShowIf(nameof(UsesLeadsTo))]
        [ValidateInput(nameof(HasValidLeadsToValue), "Leads To must be -1 or a valid non-negative row ID.")]
        [SerializeField] private int leadsTo = -1;

        [SerializeField] private bool changeStartRowId;

        [ShowIf(nameof(changeStartRowId))]
        [SerializeField] private int newStartRowId;
        
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
        public bool ChangeStartRowId => changeStartRowId;
        public int NewStartRowId => newStartRowId;

        public bool IsLineRow => rowKind == DialogueRowKind.Line;
        public bool IsChoicePromptRow => rowKind == DialogueRowKind.ChoicePrompt;
        public bool IsChoiceResponseRow => rowKind == DialogueRowKind.ChoiceResponse;
        public bool UsesLeadsTo => rowKind != DialogueRowKind.ChoicePrompt;

        public string HeaderTitle
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

        public void SetRowId(int newRowId) => rowId = newRowId;
        public void SetLeadsTo(int newLeadsTo) => leadsTo = newLeadsTo;
        public void SetChoiceRowIds(int[] newChoiceRowIds) => choiceRowIds = newChoiceRowIds;

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

        private bool HasTextLines()
        {
            if (textLines == null || textLines.Length == 0)
                return false;

            foreach (string textLine in textLines)
            {
                if (!string.IsNullOrWhiteSpace(textLine))
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