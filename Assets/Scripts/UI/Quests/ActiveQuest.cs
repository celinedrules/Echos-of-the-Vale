// Done
using System.Collections.Generic;
using Data.QuestData;
using Managers;
using UI.Common;
using UnityEngine;

namespace UI.Quests
{
    public class ActiveQuest : MonoBehaviour, IUiPanel
    {
        [SerializeField] private ActiveQuestPreview activeQuestPreview;
        [SerializeField] private Transform mainSlotParent;
        [SerializeField] private GameObject subQuestPanel;
        [SerializeField] private Transform subSlotParent;

        private ActiveQuestSlot[] _mainQuestSlots;
        private ActiveQuestSlot[] _subQuestSlots;
        private CanvasGroup _canvasGroup;

        public CanvasGroup CanvasGroup => _canvasGroup;
        public bool ShowMenuButtons => true;
        public bool ShowBackground => true;
        public bool DisablePlayerInput => true;
        public bool HasTooltips => false;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _mainQuestSlots = mainSlotParent.GetComponentsInChildren<ActiveQuestSlot>(true);
            _subQuestSlots = subSlotParent.GetComponentsInChildren<ActiveQuestSlot>(true);
        }

        public void OnOpened()
        {
            List<QuestStatus> quests = QuestManager.Instance.ActiveQuests;

            // Hide all main slots
            foreach (ActiveQuestSlot questSlot in _mainQuestSlots)
                questSlot.gameObject.SetActive(false);

            // Hide all sub slots
            ClearSubQuests();
            subQuestPanel.SetActive(false);

            // Only show main quests (quests that are NOT sub quests of another quest)
            int slotIndex = 0;
            for (int i = 0; i < quests.Count; i++)
            {
                if (IsSubQuest(quests[i].QuestData, quests))
                    continue;

                if (slotIndex >= _mainQuestSlots.Length)
                    break;

                _mainQuestSlots[slotIndex].gameObject.SetActive(true);
                _mainQuestSlots[slotIndex].SetupActiveQuestSlot(quests[i], this);
                slotIndex++;
            }

            if (slotIndex > 0)
            {
                activeQuestPreview.gameObject.SetActive(true);
                _mainQuestSlots[0].SetupPreview();
            }
            else
            {
                activeQuestPreview.gameObject.SetActive(false);
            }
        }

        public void ShowSubQuests(QuestData mainQuest)
        {
            ClearSubQuests();

            if (!mainQuest.HasSubQuests)
            {
                subQuestPanel.SetActive(false);
                return;
            }

            subQuestPanel.SetActive(true);

            List<QuestStatus> allQuests = QuestManager.Instance.ActiveQuests;

            int slotIndex = 0;
            foreach (QuestData subQuestData in mainQuest.SubQuests)
            {
                if (mainQuest.SequentialSubQuests)
                {
                    if (QuestManager.Instance.QuestIsCompleted(subQuestData))
                        continue;

                    QuestStatus subStatus = allQuests.Find(q => q.QuestData == subQuestData);
                    if (subStatus == null)
                    {
                        Debug.Log($"Sub quest '{subQuestData.QuestName}' not found in active quests!");
                        continue;
                    }
                    
                    Debug.Log($"Sub quest '{subQuestData.QuestName}' was found in active quests!");
                    
                    

                    if (slotIndex >= _subQuestSlots.Length)
                        break;

                    _subQuestSlots[slotIndex].gameObject.SetActive(true);
                    _subQuestSlots[slotIndex].SetupActiveQuestSlot(subStatus, this, true);
                    break;
                }

                QuestStatus status = allQuests.Find(q => q.QuestData == subQuestData);
                if (status == null || slotIndex >= _subQuestSlots.Length)
                    continue;

                _subQuestSlots[slotIndex].gameObject.SetActive(true);
                _subQuestSlots[slotIndex].SetupActiveQuestSlot(status, this, true);
                slotIndex++;
            }
        }

        public void ClearSubQuests()
        {
            foreach (ActiveQuestSlot questSlot in _subQuestSlots)
                questSlot.gameObject.SetActive(false);

            subQuestPanel.SetActive(false);
        }

        private bool IsSubQuest(QuestData questData, List<QuestStatus> allQuests)
        {
            foreach (QuestStatus status in allQuests)
            {
                if (!status.QuestData.HasSubQuests)
                    continue;

                foreach (QuestData sub in status.QuestData.SubQuests)
                {
                    if (sub == questData)
                        return true;
                }
            }

            return false;
        }
    }
}