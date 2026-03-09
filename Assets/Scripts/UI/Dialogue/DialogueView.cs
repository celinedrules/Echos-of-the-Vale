using System.Collections.Generic;
using Data.DialogueData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Enums;

namespace UI.Dialogue
{
    public class DialogueView : MonoBehaviour
    {
        [SerializeField] private Image speakerPortrait;
        [SerializeField] private TextMeshProUGUI speakerName;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Transform choicesRoot;

        private TextMeshProUGUI[] _dialogueChoicesText;

        public TextMeshProUGUI[] DialogueChoicesText
        {
            get
            {
                CacheChoiceTextsIfNeeded();
                return _dialogueChoicesText;
            }
        }

        private void Awake()
        {
            CacheChoiceTextsIfNeeded();
        }

        private void OnValidate()
        {
            CacheChoiceTextsIfNeeded();
        }

        public void SetSpeaker(DialogueRow row)
        {
            Sprite portrait = row.GetPortrait();
            if (portrait != null)
                speakerPortrait.sprite = portrait;

            speakerName.text = row.Speaker != null ? row.Speaker.SpeakerName : "";
        }

        public void SetDialogueText(string text)
        {
            dialogueText.text = text;
        }

        public void ClearDialogueText()
        {
            dialogueText.text = "";
        }

        public void HideAllChoices()
        {
            TextMeshProUGUI[] choiceTexts = DialogueChoicesText;

            foreach (TextMeshProUGUI choice in choiceTexts)
                choice.gameObject.SetActive(false);
        }

        public void ShowChoices(DialogueRow[] currentChoices, int selectedChoiceIndex, DialogueNpcData npcData)
        {
            TextMeshProUGUI[] choiceTexts = DialogueChoicesText;

            if (currentChoices == null)
            {
                HideAllChoices();
                return;
            }

            for (int i = 0; i < choiceTexts.Length; i++)
            {
                if (i >= currentChoices.Length || currentChoices[i] == null)
                {
                    choiceTexts[i].gameObject.SetActive(false);
                    continue;
                }

                DialogueRow choice = currentChoices[i];
                TextMeshProUGUI choiceTextUi = choiceTexts[i];

                if (choice.RowAction == DialogueRowAction.GetQuestReward &&
                    !Managers.QuestManager.Instance.CanTurnInAnyQuest(npcData.QuestTargetId))
                {
                    choiceTextUi.gameObject.SetActive(false);
                    continue;
                }

                string choiceText = choice.PlayerChoiceAnswer;
                choiceTextUi.gameObject.SetActive(true);
                choiceTextUi.text = selectedChoiceIndex == i
                    ? $"<color=green>{i + 1}) {choiceText}</color>"
                    : $"{i + 1}) {choiceText}";
            }
        }

        private void CacheChoiceTextsIfNeeded()
        {
            if (choicesRoot == null)
            {
                _dialogueChoicesText = System.Array.Empty<TextMeshProUGUI>();
                return;
            }

            List<TextMeshProUGUI> choiceTexts = new();

            for (int i = 0; i < choicesRoot.childCount; i++)
            {
                Transform child = choicesRoot.GetChild(i);
                TextMeshProUGUI textComponent = child.GetComponent<TextMeshProUGUI>();

                if (textComponent != null)
                    choiceTexts.Add(textComponent);
            }

            _dialogueChoicesText = choiceTexts.ToArray();
        }
    }
}