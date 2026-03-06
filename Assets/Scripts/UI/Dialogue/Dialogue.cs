// Done
using System;
using System.Collections;
using Data.DialogueData;
using Managers;
using TMPro;
using UI.Common;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Enums;

namespace UI.Dialogue
{
    public class Dialogue : MonoBehaviour, IUiPanel
    {
        [SerializeField] private Image speakerPortrait;
        [SerializeField] private TextMeshProUGUI speakerName;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI[] dialogueChoicesText;
        [SerializeField] private float textSpeed = 0.1f;
        [SerializeField] private float choiceDelay = 0.5f;

        private Coroutine _typingRoutine;
        private string _fullTextToShow;

        private DialogueTable _table;
        private DialogueRow _currentRow;
        private DialogueRow[] _currentChoices;
        private DialogueRow _selectedChoice;
        private int _selectedChoiceIndex;
        private bool _waitingToConfirm;
        private int _startedTypingFrame;
        private DialogueNpcData _npcData;
        private CanvasGroup _canvasGroup;

        public CanvasGroup CanvasGroup => _canvasGroup;
        public bool ShowMenuButtons => false;
        public bool ShowBackground => false;
        public bool DisablePlayerInput => true;
        public bool HasTooltips => false;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            for (int i = 0; i < dialogueChoicesText.Length; i++)
            {
                DialogueChoiceHandler handler = dialogueChoicesText[i].GetComponent<DialogueChoiceHandler>();

                handler.Setup(i);
                handler.OnHover += SelectChoice;
                handler.OnClick += ConfirmChoice;
            }
        }

        public void SetupNpcData(DialogueNpcData npcData) => _npcData = npcData;

        /// <summary>
        /// Starts playing a dialogue from a table, beginning at the specified row ID.
        /// </summary>
        public void PlayDialogue(DialogueTable table, int startRowId)
        {
            _table = table;
            DialogueRow row = _table.GetRowById(startRowId);

            if (row == null)
            {
                Debug.LogWarning($"Row ID {startRowId} not found in table {table.TableName}");
                return;
            }

            PlayRow(row);
        }

        private void PlayRow(DialogueRow row)
        {
            _currentRow = row;

            // Resolve choice rows from IDs
            int[] choiceIds = row.ChoiceRowIds;
            if (choiceIds != null && choiceIds.Length > 0)
            {
                _currentChoices = new DialogueRow[choiceIds.Length];
                for (int i = 0; i < choiceIds.Length; i++)
                    _currentChoices[i] = _table.GetRowById(choiceIds[i]);
            }
            else
            {
                _currentChoices = null;
            }

            _fullTextToShow = row.GetRandomLine();
            _waitingToConfirm = false;
            _selectedChoice = null;

            HideAllChoices();

            Sprite portrait = row.GetPortrait();
            if (portrait != null)
                speakerPortrait.sprite = portrait;

            speakerName.text = row.Speaker != null ? row.Speaker.SpeakerName : "";

            if (_typingRoutine != null)
                StopCoroutine(_typingRoutine);

            _startedTypingFrame = Time.frameCount;
            _typingRoutine = StartCoroutine(TypeText(_fullTextToShow));
        }

        private void HandleNextAction()
        {
            switch (_currentRow.ActionType)
            {
                case DialogueActionType.None:
                    if (_currentRow.LeadsTo >= 0)
                    {
                        DialogueRow nextRow = _table.GetRowById(_currentRow.LeadsTo);
                        if (nextRow != null)
                        {
                            PlayRow(nextRow);
                            return;
                        }

                        Debug.LogWarning($"LeadsTo row ID {_currentRow.LeadsTo} not found in table {_table.TableName}");
                    }
                    break;
                case DialogueActionType.OpenQuest:
                    UiManager.Instance.Quest.SetupQuests(_npcData.Quests);
                    UiManager.Instance.OpenQuest();
                    break;
                case DialogueActionType.OpenShop:
                    UiManager.Instance.OpenMerchant();
                    break;
                case DialogueActionType.OpenStorage:
                    UiManager.Instance.OpenStorage();
                    break;
                case DialogueActionType.OpenCraft:
                    UiManager.Instance.OpenCraft();
                    break;
                case DialogueActionType.GetQuestReward:
                    QuestManager.Instance.TryGetQuestReward(_npcData.QuestTargetId);
                    UiManager.Instance.TryCloseActiveUi();
                    break;
                case DialogueActionType.PlayerChoice:
                    if (_selectedChoice == null)
                    {
                        _selectedChoiceIndex = 0;
                        StartCoroutine(ShowChoicesDelayed());
                    }
                    else
                    {
                        DialogueRow selectedChoice = _currentChoices[_selectedChoiceIndex];
                        _selectedChoice = null;
                        PlayRow(selectedChoice);
                    }
                    break;
                case DialogueActionType.CloseDialogue:
                    UiManager.Instance.TryCloseActiveUi();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void DialogueInteraction()
        {
            if (_startedTypingFrame == Time.frameCount)
                return;

            if (_typingRoutine != null)
            {
                CompleteTyping();
                return;
            }

            if (_waitingToConfirm)
            {
                _waitingToConfirm = false;
                HandleNextAction();
                return;
            }
        }

        private void CompleteTyping()
        {
            if (_typingRoutine != null)
                StopCoroutine(_typingRoutine);

            _typingRoutine = null;
            dialogueText.text = _fullTextToShow;
            OnTypingComplete();
        }

        private void OnTypingComplete()
        {
            if (_currentRow.ActionType == DialogueActionType.PlayerChoice)
            {
                HandleNextAction();
                return;
            }

            _waitingToConfirm = true;
        }

        private IEnumerator ShowChoicesDelayed()
        {
            yield return new WaitForSecondsRealtime(choiceDelay);
            ShowChoices();
            _waitingToConfirm = true;
        }

        private void ShowChoices()
        {
            if (_currentChoices == null) return;

            for (int i = 0; i < dialogueChoicesText.Length; i++)
            {
                if (i < _currentChoices.Length && _currentChoices[i] != null)
                {
                    DialogueRow choice = _currentChoices[i];
                    string choiceText = choice.PlayerChoiceAnswer;

                    dialogueChoicesText[i].gameObject.SetActive(true);
                    dialogueChoicesText[i].text = _selectedChoiceIndex == i
                        ? $"<color=green>{i + 1}) {choiceText}</color>"
                        : $"{i + 1}) {choiceText}";

                    if (choice.ActionType == DialogueActionType.GetQuestReward
                        && !QuestManager.Instance.CanTurnInAnyQuest(_npcData.QuestTargetId))
                        dialogueChoicesText[i].gameObject.SetActive(false);
                }
                else
                {
                    dialogueChoicesText[i].gameObject.SetActive(false);
                }
            }

            if (_currentChoices.Length > 0 && _currentChoices[_selectedChoiceIndex] != null)
                _selectedChoice = _currentChoices[_selectedChoiceIndex];
        }

        private void SelectChoice(int index)
        {
            _selectedChoiceIndex = index;
            ShowChoices();
        }

        private void ConfirmChoice(int index)
        {
            SelectChoice(index);
            DialogueInteraction();
        }

        private void HideAllChoices()
        {
            foreach (TextMeshProUGUI choice in dialogueChoicesText)
                choice.gameObject.SetActive(false);
        }

        public void NavigateChoices(int direction)
        {
            if (_currentChoices == null || _currentChoices.Length <= 1)
                return;

            _selectedChoiceIndex += direction;
            _selectedChoiceIndex = Mathf.Clamp(_selectedChoiceIndex, 0, _currentChoices.Length - 1);
            ShowChoices();
        }

        private IEnumerator TypeText(string text)
        {
            dialogueText.text = "";

            foreach (char letter in text)
            {
                dialogueText.text += letter;
                yield return new WaitForSecondsRealtime(textSpeed);
            }

            _typingRoutine = null;
            OnTypingComplete();
        }

        public void OnOpened()
        {
        }
    }
}