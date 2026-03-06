// Done
using Data.QuestData;
using Managers;
using TMPro;
using UnityEngine;
using Utilities.Enums;

namespace UI.Quests
{
    public class ActiveQuestPreview : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questName;
        [SerializeField] private TextMeshProUGUI questDescription;
        [SerializeField] private TextMeshProUGUI progress;
        [SerializeField] private QuestRewardSlot[] questReward;

        public void SetupQuestPreview(QuestStatus questStatus)
        {
            QuestData questData = questStatus.QuestData;

            questName.text = questData.QuestName;
            questDescription.text = questData.description;

            if (questData.QuestType == QuestType.Deliver)
            {
                progress.text = questData.questGoal + "\n" + QuestManager.Instance.GetDeliverProgressText(questStatus);
            }
            else
            {
                progress.text = questData.questGoal + " " + QuestManager.Instance.GetQuestProgress(questStatus) + " / " + questData.RequiredAmount;
            }

            foreach (QuestRewardSlot rewardSlot in questReward)
                rewardSlot.gameObject.SetActive(false);

            for (int i = 0; i < questData.RewardItems.Length; i++)
            {
                questReward[i].gameObject.SetActive(true);
                questReward[i].UpdateSlot(questData.RewardItems[i]);
            }
        }
    }
}