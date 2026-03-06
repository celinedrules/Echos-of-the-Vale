// Done
using Core.Interfaces;
using Data.QuestData;
using Managers;
using SaveSystem;
using UI.Common;
using UnityEngine;

namespace UI.Quests
{
    public class Quest : MonoBehaviour, IUiPanel, ISavable
    {
        [SerializeField] private QuestPreview questPreview;

        private GameData _gameData;
        private QuestSlot[] _questSlots;
        private CanvasGroup _canvasGroup;

        public CanvasGroup CanvasGroup => _canvasGroup;
        public QuestPreview Preview => questPreview;


        public bool ShowMenuButtons => false;
        public bool ShowBackground => true;
        public bool DisablePlayerInput => true;
        public bool HasTooltips => true;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _questSlots = GetComponentsInChildren<QuestSlot>(true);
        }

        public void SetupQuests(QuestData[] quests)
        {
            foreach (QuestSlot questSlot in _questSlots)
                questSlot.gameObject.SetActive(false);

            for (int i = 0; i < quests.Length; i++)
            {
                _questSlots[i].gameObject.SetActive(true);
                _questSlots[i].SetupQuestSlot(quests[i]);
            }

            questPreview.ClearQuestPreview();

            UpdateQuestList();
        }

        public void UpdateQuestList()
        {
            foreach (QuestSlot questSlot in _questSlots)
            {
                if (questSlot.gameObject.activeSelf && !CanAcceptQuest(questSlot.QuestInSlot))
                    questSlot.gameObject.SetActive(false);
            }
        }

        private bool CanAcceptQuest(QuestData questData)
        {
            bool questActive = QuestManager.Instance.QuestAccepted(questData);
            bool questCompleted = QuestManager.Instance.QuestIsCompleted(questData);

            if (questActive || questCompleted)
                return false;
            
            if (_gameData != null)
                return !_gameData.completedQuests.TryGetValue(questData.saveId, out questCompleted);
            
            return true;

        }

        public void OnOpened()
        {
        }

        public void SaveData(ref GameData gameData)
        {
        }

        public void LoadData(GameData gameData)
        {
            _gameData = gameData;
        }
    }
}