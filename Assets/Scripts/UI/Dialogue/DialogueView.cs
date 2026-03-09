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
        [System.Serializable]
        public class ChoiceInstance
        {
            public GameObject Root;
            public DialogueChoiceHandler Handler;
            public TextMeshProUGUI Text;
        }

        [SerializeField] private Image speakerPortrait;
        [SerializeField] private TextMeshProUGUI speakerName;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Transform choicesRoot;
        [SerializeField] private GameObject choicePrefab;

        private readonly List<ChoiceInstance> _choiceInstances = new();

        public IReadOnlyList<ChoiceInstance> ChoiceInstances => _choiceInstances;

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

        public void RebuildChoices(DialogueRow[] currentChoices, int selectedChoiceIndex, DialogueNpcData npcData)
        {
            ClearChoiceInstances();

            if (currentChoices == null || currentChoices.Length == 0 || choicesRoot == null || choicePrefab == null)
                return;

            for (int i = 0; i < currentChoices.Length; i++)
            {
                DialogueRow choice = currentChoices[i];
                if (choice == null)
                    continue;

                GameObject choiceInstance = Instantiate(choicePrefab, choicesRoot);
                TextMeshProUGUI choiceTextUi = choiceInstance.GetComponentInChildren<TextMeshProUGUI>();
                DialogueChoiceHandler handler = choiceInstance.GetComponentInChildren<DialogueChoiceHandler>();

                if (choiceTextUi == null)
                {
                    Debug.LogWarning("Dialogue choice prefab is missing a TextMeshProUGUI component.");
                    Destroy(choiceInstance);
                    continue;
                }

                if (handler == null)
                {
                    Debug.LogWarning("Dialogue choice prefab is missing a DialogueChoiceHandler component.");
                    Destroy(choiceInstance);
                    continue;
                }

                choiceInstance.SetActive(true);

                if (choice.RowAction == DialogueRowAction.GetQuestReward &&
                    !Managers.QuestManager.Instance.CanTurnInAnyQuest(npcData.QuestTargetId))
                {
                    choiceInstance.SetActive(false);
                    _choiceInstances.Add(new ChoiceInstance
                    {
                        Root = choiceInstance,
                        Handler = handler,
                        Text = choiceTextUi
                    });
                    continue;
                }

                string choiceText = choice.PlayerChoiceAnswer;
                choiceTextUi.text = selectedChoiceIndex == i
                    ? $"<color=green>{i + 1}) {choiceText}</color>"
                    : $"{i + 1}) {choiceText}";

                _choiceInstances.Add(new ChoiceInstance
                {
                    Root = choiceInstance,
                    Handler = handler,
                    Text = choiceTextUi
                });
            }
        }

        public void RefreshChoiceVisuals(DialogueRow[] currentChoices, int selectedChoiceIndex, DialogueNpcData npcData)
        {
            if (currentChoices == null)
                return;

            for (int i = 0; i < _choiceInstances.Count; i++)
            {
                ChoiceInstance choiceInstance = _choiceInstances[i];

                if (choiceInstance == null || choiceInstance.Text == null || choiceInstance.Root == null)
                    continue;

                if (i >= currentChoices.Length || currentChoices[i] == null)
                {
                    choiceInstance.Root.SetActive(false);
                    continue;
                }

                DialogueRow choice = currentChoices[i];

                if (choice.RowAction == DialogueRowAction.GetQuestReward &&
                    !Managers.QuestManager.Instance.CanTurnInAnyQuest(npcData.QuestTargetId))
                {
                    choiceInstance.Root.SetActive(false);
                    continue;
                }

                choiceInstance.Root.SetActive(true);

                string choiceText = choice.PlayerChoiceAnswer;
                choiceInstance.Text.text = selectedChoiceIndex == i
                    ? $"<color=green>{i + 1}) {choiceText}</color>"
                    : $"{i + 1}) {choiceText}";
            }
        }
        
        public void ClearChoiceInstances()
        {
            for (int i = 0; i < _choiceInstances.Count; i++)
            {
                if (_choiceInstances[i].Root != null)
                    Destroy(_choiceInstances[i].Root);
            }

            _choiceInstances.Clear();
        }
    }
}