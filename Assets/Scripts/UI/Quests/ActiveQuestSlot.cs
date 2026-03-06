// Done
using Data.QuestData;
using InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Quests
{
    public class ActiveQuestSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questName;
        [SerializeField] private Image[] questRewardPreview;
        
        private QuestStatus _questInSlot;
        private ActiveQuestPreview _questPreview;
        private ActiveQuest _activeQuest;
        private bool _isSubQuest;

        public void SetupActiveQuestSlot(QuestStatus questStatus, ActiveQuest activeQuest, bool isSubQuest = false)
        {
            _questPreview = transform.root.GetComponentInChildren<ActiveQuestPreview>(true);
            _questInSlot = questStatus;
            _activeQuest = activeQuest;
            _isSubQuest = isSubQuest;
            
            questName.text = _questInSlot.QuestData.QuestName;

            InventoryItem[] rewards = _questInSlot.QuestData.RewardItems;
            
            foreach (Image icon in questRewardPreview)
                icon.gameObject.SetActive(false);

            for (int i = 0; i < rewards.Length; i++)
            {
                if(rewards[i] == null)
                    continue;
                
                Image slotIcon = questRewardPreview[i];
                slotIcon.gameObject.SetActive(true);
                slotIcon.sprite = rewards[i].ItemData.ItemIcon;
                slotIcon.GetComponentInChildren<TextMeshProUGUI>().text = rewards[i].StackSize.ToString();
            }
        }

        public void SetupPreview()
        {
            _questPreview.SetupQuestPreview(_questInSlot);

            if (_isSubQuest)
                return;
            
            if (_activeQuest != null && _questInSlot.QuestData.HasSubQuests)
                _activeQuest.ShowSubQuests(_questInSlot.QuestData);
            else
                _activeQuest?.ClearSubQuests();
        }
    }
}