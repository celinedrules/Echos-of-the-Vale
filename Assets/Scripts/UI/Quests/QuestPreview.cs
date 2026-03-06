// Done
using Data.QuestData;
using Managers;
using TMPro;
using UnityEngine;

namespace UI.Quests
{
    public class QuestPreview : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questName;
        [SerializeField] private TextMeshProUGUI questDescription;
        [SerializeField] private TextMeshProUGUI questGoal;
        [SerializeField] private QuestRewardSlot[] questReward;
        [SerializeField] private GameObject[] additionalObjects;

        private QuestData _previewQuest;
        private Quest _quest;
        
        public void SetupQuestPreview(QuestData questData)
        {
            _quest = transform.root.GetComponentInChildren<Quest>();
            _previewQuest = questData;
            
            EnableAdditionalObjects(true);
            EnableRewardObjects(false);
            
            questName.text = questData.QuestName;
            questDescription.text = questData.description;
            questGoal.text = questData.questGoal + " " + questData.RequiredAmount;
            
            for(int i =  0; i < questData.RewardItems.Length; i++)
            {
                questReward[i].gameObject.SetActive(true);
                questReward[i].UpdateSlot(questData.RewardItems[i]);
            }
        }

        public void AcceptQuest()
        {
            ClearQuestPreview();
            QuestManager.Instance.AcceptQuest(_previewQuest);
            _quest.UpdateQuestList();
        }

        public void ClearQuestPreview()
        {
            questName.text = "";
            questDescription.text = "";

            EnableAdditionalObjects(false);
            EnableRewardObjects(false);
        }
        
        private void EnableRewardObjects(bool enable)
        {
            foreach (QuestRewardSlot rewardSlot in questReward)
                rewardSlot.gameObject.SetActive(enable);
        }
        
        private void EnableAdditionalObjects( bool enable)
        {
            foreach (GameObject additionalObject in additionalObjects)
                additionalObject.SetActive(enable);
        }
    }
}