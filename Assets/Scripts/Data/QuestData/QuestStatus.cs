// Done
using System;
using UnityEngine;

namespace Data.QuestData
{
    [Serializable]
    public class QuestStatus
    {
        [SerializeField] private QuestData questData;
        [SerializeField] private int currentAmount;
        [SerializeField] private bool canGetReward;
        
        public QuestData QuestData => questData;
        public int CurrentAmount
        {
            get => currentAmount;
            set => currentAmount = value;
        }

        public  QuestStatus(QuestData data)
        {
            questData = data;
        }

        public void AddQuestProgress(int amount = 1)
        {
            currentAmount += amount;
            canGetReward = CanGetReward();
        }
        
        public bool CanGetReward() => currentAmount >= questData.RequiredAmount;
    }
}