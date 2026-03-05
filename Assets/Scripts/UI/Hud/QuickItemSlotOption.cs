// Done
using InventorySystem;
using Managers;
using UI.Inventory;
using UnityEngine.EventSystems;

namespace UI.Hud
{
    public class QuickItemSlotOption : ItemSlot
    {
        private QuickItemSlot _currentQuickItemSlot;

        public void SetupOption(QuickItemSlot currentQuickItemSlot, InventoryItem item)
        {
            _currentQuickItemSlot = currentQuickItemSlot;
            UpdateSlot(item);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            _currentQuickItemSlot.SetupQuickSlotItem(ItemInSlot);
            UiManager.Instance.Hud.HideQuickItemOptions();
        }
    }
}