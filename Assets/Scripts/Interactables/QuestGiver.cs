// Done
using Core.Interfaces;
using Data.QuestData;
using UnityEngine;

namespace Interactables
{
    public class QuestGiver : Npc, IInteractable
    {
        [Header("Quest and Dialogue")]
        [SerializeField] private QuestData[] quests;
        
        protected override void Awake()
        {
            base.Awake();
            Animator.SetBool(IsBlacksmith, false);
        }
        
        public void Interact()
        {
            UiManager.Quest.SetupQuests(quests);
            UiManager.OpenQuest();
        }
    }
}