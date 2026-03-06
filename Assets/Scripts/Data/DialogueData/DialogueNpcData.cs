// Done
using System;
using UnityEngine;
using Utilities.Enums;

namespace Data.DialogueData
{
    [Serializable]
    public class DialogueNpcData
    {
        [SerializeField] private string questTargetId;
        [SerializeField] private RewardType npcRewardType;
        [SerializeField] private QuestData.QuestData[] quests;
        
        public string QuestTargetId => questTargetId;
        public RewardType NpcRewardType => npcRewardType;
        public QuestData.QuestData[] Quests => quests;
        
        public DialogueNpcData(string targetId, RewardType rewardType, QuestData.QuestData[] questData)
        {
            questTargetId = targetId;
            npcRewardType = rewardType;
            quests = questData;
        }
    }
}