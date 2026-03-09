using System;
using Data.DialogueData;
using Managers;
using Utilities.Enums;

namespace UI.Dialogue
{
    public static class DialogueActionExecutor
    {
        public static void Execute(DialogueRowAction action, DialogueNpcData npcData)
        {
            switch (action)
            {
                case DialogueRowAction.None:
                    return;
                case DialogueRowAction.OpenQuest:
                    if (npcData == null)
                        throw new InvalidOperationException("DialogueNpcData is required for OpenQuest.");
                    UiManager.Instance.Quest.SetupQuests(npcData.Quests);
                    UiManager.Instance.OpenQuest();
                    return;
                case DialogueRowAction.OpenShop:
                    UiManager.Instance.OpenMerchant();
                    return;
                case DialogueRowAction.OpenStorage:
                    UiManager.Instance.OpenStorage();
                    return;
                case DialogueRowAction.OpenCraft:
                    UiManager.Instance.OpenCraft();
                    return;
                case DialogueRowAction.GetQuestReward:
                    if (npcData == null)
                        throw new InvalidOperationException("DialogueNpcData is required for GetQuestReward.");
                    QuestManager.Instance.TryGetQuestReward(npcData.QuestTargetId);
                    UiManager.Instance.TryCloseActiveUi();
                    return;
                case DialogueRowAction.CloseDialogue:
                    UiManager.Instance.TryCloseActiveUi();
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }
    }
}