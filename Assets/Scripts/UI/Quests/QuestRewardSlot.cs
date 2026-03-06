// Done
using Managers;
using UI.Inventory;
using UnityEngine.EventSystems;

namespace UI.Quests
{
    public class QuestRewardSlot : ItemSlot
    {
        public override void OnPointerDown(PointerEventData eventData)
        {
            
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (ItemInSlot == null || ItemInSlot.ItemData == null)
                return;

            UiManager.Instance.ItemTooltip.ShowTooltip(true, Rect, ItemInSlot, showControls: false);
        }
    }
}