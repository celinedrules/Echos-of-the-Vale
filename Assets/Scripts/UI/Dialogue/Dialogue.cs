using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private DialogueView view;
        [SerializeField] private DialogueTypewriter typewriter;
        [SerializeField] private float choiceDelay = 0.5f;

        private DialogueTable _table;
        private DialogueRow _currentRow;
        private DialogueRow[] _currentChoices;
        private DialogueRow _selectedChoice;
        private int _selectedChoiceIndex;
        private bool _waitingToConfirm;
        private int _startedTypingFrame;
        private string _currentRowText;
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

        public void DialogueInteraction()
        {
            if (_startedTypingFrame == Time.frameCount)
                return;

            if (typewriter.IsTyping)
            {
                typewriter.CompleteInstantly();
                return;
            }

            if (_waitingToConfirm)
            {
                _waitingToConfirm = false;
                HandleNextAction();
            }
        }

        public void NavigateChoices(int direction)
        {
            if (_currentChoices == null || _currentChoices.Length <= 1)
                return;

            _selectedChoiceIndex += direction;
            _selectedChoiceIndex = Mathf.Clamp(_selectedChoiceIndex, 0, _currentChoices.Length - 1);
            ShowChoices();
        }

        public void OnOpened()
        {
        }

        private void SetupChoiceHandlers()
        {
            IReadOnlyList<DialogueView.ChoiceInstance> choiceInstances = view.ChoiceInstances;

            for (int i = 0; i < choiceInstances.Count; i++)
            {
                DialogueChoiceHandler handler = choiceInstances[i].Handler;
                if (handler == null)
                    continue;

                handler.Setup(i);
                handler.OnHover -= SelectChoice;
                handler.OnHover += SelectChoice;
                handler.OnClick -= ConfirmChoice;
                handler.OnClick += ConfirmChoice;
            }
        }

        private void PlayRow(DialogueRow row)
        {
            SetCurrentRow(row);
            ResolveCurrentChoices(row);
            view.ClearChoiceInstances();
            view.SetSpeaker(row);
            StartTypingCurrentRow();
        }

        private void SetCurrentRow(DialogueRow row)
        {
            _currentRow = row;
            _currentRowText = row.GetRandomLine();
            _waitingToConfirm = false;
            _selectedChoice = null;
        }

        private void ResolveCurrentChoices(DialogueRow row)
        {
            int[] choiceIds = row.ChoiceRowIds;

            if (row.RowKind != DialogueRowKind.ChoicePrompt || choiceIds == null || choiceIds.Length == 0)
            {
                _currentChoices = null;
                return;
            }

            _currentChoices = new DialogueRow[choiceIds.Length];
            for (int i = 0; i < choiceIds.Length; i++)
                _currentChoices[i] = _table.GetRowById(choiceIds[i]);
        }

        private void StartTypingCurrentRow()
        {
            _startedTypingFrame = Time.frameCount;
            typewriter.StartTyping(_currentRowText, OnTypingComplete);
        }

        private void HandleNextAction()
        {
            if (_currentRow.RowKind == DialogueRowKind.ChoicePrompt)
            {
                HandleChoicePromptRow();
                return;
            }

            ExecuteCurrentRowAction();
            TryAdvanceToNextRow();
        }

        private void HandleChoicePromptRow()
        {
            if (_selectedChoice == null)
            {
                _selectedChoiceIndex = 0;
                StartCoroutine(ShowChoicesDelayed());
                return;
            }

            DialogueRow selectedChoice = _currentChoices[_selectedChoiceIndex];
            _selectedChoice = null;
            PlayRow(selectedChoice);
        }
        

        private void ExecuteCurrentRowAction() => DialogueActionExecutor.Execute(_currentRow.RowAction, _npcData);

        private void TryAdvanceToNextRow()
        {
            if (_currentRow.RowAction == DialogueRowAction.CloseDialogue)
                return;

            if (_currentRow.LeadsTo < 0)
                return;

            DialogueRow nextRow = _table.GetRowById(_currentRow.LeadsTo);
            if (nextRow != null)
            {
                PlayRow(nextRow);
                return;
            }

            Debug.LogWarning($"LeadsTo row ID {_currentRow.LeadsTo} not found in table {_table.TableName}");
        }

        private void OnTypingComplete()
        {
            if (_currentRow.RowKind == DialogueRowKind.ChoicePrompt)
            {
                HandleNextAction();
                return;
            }

            _waitingToConfirm = true;
        }

        private IEnumerator ShowChoicesDelayed()
        {
            yield return new WaitForSecondsRealtime(choiceDelay);
            ShowChoices(true);
            _waitingToConfirm = true;
        }

        private void ShowChoices(bool rebuild = false)
        {
            if (rebuild)
            {
                view.RebuildChoices(_currentChoices, _selectedChoiceIndex, _npcData);
                SetupChoiceHandlers();
            }
            else
            {
                view.RefreshChoiceVisuals(_currentChoices, _selectedChoiceIndex, _npcData);
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
    }
}