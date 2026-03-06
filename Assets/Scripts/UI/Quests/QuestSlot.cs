// Done
using Data.QuestData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Quests
{
    public class QuestSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questName;
        [SerializeField] private Image[] rewardQuickPreview;

        private QuestData _questInSlot;
        private QuestPreview _preview;
        
        public QuestData QuestInSlot => _questInSlot;

        public void SetupQuestSlot(QuestData questData)
        {
            _preview = transform.root.GetComponentInChildren<Quest>().Preview;
            _questInSlot = questData;
            questName.text = _questInSlot.QuestName;
            
            foreach (Image previewIcon in rewardQuickPreview)
                previewIcon.gameObject.SetActive(false);
            
            for(int i =  0; i < _questInSlot.RewardItems.Length; i++)
            {
                if(_questInSlot.RewardItems[i] == null || !_questInSlot.RewardItems[i].ItemData)
                    continue;
                
                Image slotIcon = rewardQuickPreview[i];
                slotIcon.gameObject.SetActive(true);
                slotIcon.sprite = _questInSlot.RewardItems[i].ItemData.ItemIcon;
                slotIcon.GetComponentInChildren<TextMeshProUGUI>().text = _questInSlot.RewardItems[i].StackSize.ToString();
            }
        }

        public void UpdateQuestPreview()
        {
            _preview.SetupQuestPreview(_questInSlot);
        }
    }
}