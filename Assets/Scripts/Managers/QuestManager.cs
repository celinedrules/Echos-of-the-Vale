// Done
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Interfaces;
using Data.QuestData;
using InventorySystem;
using SaveSystem;
using UnityEngine;
using Utilities;
using Utilities.Enums;

namespace Managers
{
    public class QuestManager : Singleton<QuestManager>, ISavable
    {
        [SerializeField] private QuestDatabase questDatabase;
        [SerializeField] private List<QuestStatus> activeQuests;
        [SerializeField] private List<QuestStatus> completedQuests;

        private QuestRuntimeData RuntimeData => GameManager.Instance.QuestRuntimeData;
        private EntityDropManager _dropManager;
        private InventoryPlayer _inventory;
        
        public List<QuestStatus> ActiveQuests => activeQuests;

        private void Start()
        {
            _dropManager = GameManager.Instance.Player.GetComponent<EntityDropManager>();
            _inventory = GameManager.Instance.Player.GetComponent<InventoryPlayer>();
            
            if (RuntimeData.ActiveQuests.Count > 0 || RuntimeData.CompletedQuestIds.Count > 0)
                LoadFromRuntimeData();
        }

        public int GetQuestProgress(QuestStatus questStatus)
        {
            if (questStatus.QuestData.QuestType != QuestType.Deliver)
                return questStatus.CurrentAmount;
            
            int fulfilled = 0;
            foreach (InventoryItem required in questStatus.QuestData.ItemsToDeliver)
            {
                if (required?.ItemData != null && _inventory.GetItemAmount(required.ItemData) >= required.StackSize)
                    fulfilled++;
            }
            return fulfilled;
        }
        
        public int GetDeliverRequiredAmount(QuestStatus questStatus)
        {
            return questStatus.QuestData.ItemsToDeliver?.Length ?? 0;
        }
        
        public string GetDeliverProgressText(QuestStatus questStatus)
        {
            System.Text.StringBuilder sb = new();

            foreach (InventoryItem required in questStatus.QuestData.ItemsToDeliver)
            {
                if (required?.ItemData == null)
                    continue;

                int current = Mathf.Min(_inventory.GetItemAmount(required.ItemData), required.StackSize);
                sb.AppendLine($"  - {required.ItemData.ItemName}: {current} / {required.StackSize}");
            }

            return sb.ToString().TrimEnd();
        }
        
        public void CheckImmediateDeliverQuests()
        {
            List<QuestStatus> completable = new();

            foreach (QuestStatus questStatus in activeQuests)
            {
                if (questStatus.QuestData.QuestType != QuestType.Deliver)
                    continue;

                if (questStatus.QuestData.RewardType != RewardType.Immediate)
                    continue;

                if (HasAllDeliverItems(questStatus))
                {
                    RemoveAllDeliverItems(questStatus);
                    questStatus.AddQuestProgress(1);
                    completable.Add(questStatus);
                }
            }

            foreach (QuestStatus questStatus in completable)
            {
                GiveQuestReward(questStatus.QuestData);
                CompleteQuest(questStatus);
            }
        }
        
        public void AcceptQuest(QuestData questData)
        {
            activeQuests.Add(new QuestStatus(questData));
            
            if (questData.HasSubQuests)
            {
                if (questData.SequentialSubQuests)
                {
                    // Only add the first sub quest
                    QuestData firstSub = questData.SubQuests[0];
                    if (!QuestAccepted(firstSub) && !QuestIsCompleted(firstSub))
                        activeQuests.Add(new QuestStatus(firstSub));
                }
                else
                {
                    foreach (QuestData subQuest in questData.SubQuests)
                    {
                        if (!QuestAccepted(subQuest) && !QuestIsCompleted(subQuest))
                            activeQuests.Add(new QuestStatus(subQuest));
                    }
                }
            }
        }

        public bool QuestAccepted(QuestData questData)
        {
            if (!questData)
                return false;

            return activeQuests.Find(questStatus => questStatus.QuestData == questData) != null;
        }

        private void CompleteQuest(QuestStatus questStatus)
        {
            completedQuests.Add(questStatus);
            activeQuests.Remove(questStatus);
            
            ActivateNextSequentialSubQuest(questStatus.QuestData);
        }
        
        private void ActivateNextSequentialSubQuest(QuestData completedQuest)
        {
            // Find a parent quest that has this as a sequential sub quest
            foreach (QuestStatus parentStatus in activeQuests)
            {
                QuestData parent = parentStatus.QuestData;
                
                if (!parent.HasSubQuests || !parent.SequentialSubQuests)
                    continue;

                for (int i = 0; i < parent.SubQuests.Length; i++)
                {
                    if (parent.SubQuests[i] != completedQuest)
                        continue;

                    // Found the parent — activate the next sub quest if there is one
                    int nextIndex = i + 1;
                    if (nextIndex < parent.SubQuests.Length)
                    {
                        QuestData nextSub = parent.SubQuests[nextIndex];
                        if (!QuestAccepted(nextSub) && !QuestIsCompleted(nextSub))
                            activeQuests.Add(new QuestStatus(nextSub));
                    }
                    return;
                }
            }
        }

        public void AddProgress(string questTargetId, int amount = 1)
        {
            List<QuestStatus> rewardQuests = new();

            foreach (QuestStatus questStatus in activeQuests)
            {
                if (questStatus.QuestData.QuestTargetId != questTargetId)
                    continue;

                if (questStatus.QuestData.QuestType == QuestType.Deliver)
                    continue;
                
                if(!questStatus.CanGetReward())
                    questStatus.AddQuestProgress(amount);

                if (questStatus.QuestData.RewardType == RewardType.Immediate && questStatus.CanGetReward())
                    rewardQuests.Add(questStatus);
            }

            foreach (QuestStatus questStatus in rewardQuests)
            {
                GiveQuestReward(questStatus.QuestData);
                CompleteQuest(questStatus);
            }
        }
        
        public void ReduceProgress(string questTargetId, int amount = 1)
        {
            foreach (QuestStatus questStatus in activeQuests)
            {
                if (questStatus.QuestData.QuestTargetId != questTargetId)
                    continue;
        
                if (questStatus.QuestData.QuestType != QuestType.Deliver)
                    continue;

                questStatus.CurrentAmount = Mathf.Max(0, questStatus.CurrentAmount - amount);
            }
        }

        public void TryGetQuestReward(string npcTargetId)
        {
            List<QuestStatus> rewardQuests = new();

            foreach (QuestStatus questStatus in activeQuests)
            {
                if (questStatus.QuestData.TurnInTargetId != npcTargetId)
                    continue;

                if (questStatus.QuestData.QuestType == QuestType.Deliver)
                {
                    if (HasAllDeliverItems(questStatus))
                    {
                        RemoveAllDeliverItems(questStatus);
                        questStatus.AddQuestProgress(1); // Mark as complete
                    }
                }

                // Main quest with subquests — check all subquests are done
                if (questStatus.QuestData.HasSubQuests && !AllSubQuestsCompleted(questStatus.QuestData))
                    continue;
                
                if (questStatus.CanGetReward())
                    rewardQuests.Add(questStatus);
            }

            foreach (QuestStatus questStatus in rewardQuests)
            {
                GiveQuestReward(questStatus.QuestData);
                CompleteQuest(questStatus);
            }
        }
        
        private bool AllSubQuestsCompleted(QuestData questData)
        {
            foreach (QuestData subQuest in questData.SubQuests)
            {
                if (!QuestIsCompleted(subQuest))
                    return false;
            }
            return true;
        }
        
        private bool HasAllDeliverItems(QuestStatus questStatus)
        {
            foreach (InventoryItem required in questStatus.QuestData.ItemsToDeliver)
            {
                if (required?.ItemData == null)
                    continue;

                if (!_inventory.HasItemAmount(required.ItemData, required.StackSize))
                    return false;
            }
            
            return true;
        }
        
        private void RemoveAllDeliverItems(QuestStatus questStatus)
        {
            foreach (InventoryItem required in questStatus.QuestData.ItemsToDeliver)
            {
                if (required?.ItemData == null)
                    continue;

                _inventory.RemoveItemAmount(required.ItemData, required.StackSize);
            }
        }

        private void GiveQuestReward(QuestData questData)
        {
            foreach (InventoryItem rewardItem in questData.RewardItems)
            {
                if (rewardItem == null || !rewardItem.ItemData)
                    continue;

                _dropManager.CreateItemDrop(rewardItem.ItemData);
            }
        }
        
        public bool CanTurnInAnyQuest(string npcTargetId)
        {
            foreach (QuestStatus questStatus in activeQuests)
            {
                if (questStatus.QuestData.TurnInTargetId != npcTargetId)
                    continue;

                if (questStatus.QuestData.HasSubQuests && !AllSubQuestsCompleted(questStatus.QuestData))
                    continue;

                if (questStatus.QuestData.QuestType == QuestType.Deliver)
                {
                    if (HasAllDeliverItems(questStatus))
                        return true;
                    
                    continue;
                }
            
                if (questStatus.CanGetReward())
                    return true;
            }
            return false;
        }
        
        public bool QuestIsCompleted(QuestData questData)
        {
            if(!questData)
                return false;
            
            return completedQuests.Find(questStatus => questStatus.QuestData == questData) != null;
        }

        public void SaveData(ref GameData gameData)
        {
            gameData.activeQuests.Clear();

            foreach (QuestStatus questStatus in activeQuests)
                gameData.activeQuests.Add(questStatus.QuestData.saveId, questStatus.CurrentAmount);

            foreach (QuestStatus questStatus in completedQuests)
                gameData.completedQuests.Add(questStatus.QuestData.saveId, true);
        }

        public void LoadData(GameData gameData)
        {
            activeQuests.Clear();

            foreach ((string saveId, int amount) in gameData.activeQuests)
            {
                QuestData questData = questDatabase.GetQuestByID(saveId);

                if (!questData)
                    continue;

                QuestStatus questStatus = new(questData)
                {
                    CurrentAmount = amount
                };

                activeQuests.Add(questStatus);
            }
        }
        
        public void SaveToRuntimeData()
        {
            List<QuestRuntimeData.QuestEntry> entries = activeQuests.Select(q => new QuestRuntimeData.QuestEntry
            {
                saveId = q.QuestData.saveId,
                currentAmount = q.CurrentAmount
            }).ToList();

            RuntimeData.SetActiveQuests(entries);

            List<string> completedIds = completedQuests
                .Select(q => q.QuestData.saveId)
                .ToList();

            RuntimeData.SetCompletedQuests(completedIds);
        }

        private void LoadFromRuntimeData()
        {
            activeQuests.Clear();
            completedQuests.Clear();

            foreach (QuestRuntimeData.QuestEntry entry in RuntimeData.ActiveQuests)
            {
                QuestData questData = questDatabase.GetQuestByID(entry.saveId);
                
                if (!questData)
                    continue;

                QuestStatus status = new(questData) { CurrentAmount = entry.currentAmount };
                activeQuests.Add(status);
            }

            foreach (string saveId in RuntimeData.CompletedQuestIds)
            {
                QuestData questData = questDatabase.GetQuestByID(saveId);
                
                if (!questData) 
                    continue;

                QuestStatus status = new(questData) { CurrentAmount = questData.RequiredAmount };
                completedQuests.Add(status);
            }
        }
    }
}