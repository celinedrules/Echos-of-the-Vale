// Done
using InventorySystem;
using Managers;
using UI.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class MerchantSlot : ItemSlot
    {
        public enum MerchantSlotType
        {
            MerchantSlot,
            PlayerSlot
        }

        [SerializeField] private MerchantSlotType slotType; 
        
        private InventoryMerchant _merchant;
        
        public MerchantSlotType SlotType => slotType;
        
        public void SetupMerchantUi(InventoryMerchant merchant) => _merchant = merchant;

        public override void OnPointerDown(PointerEventData eventData)
        {
            if(ItemInSlot == null)
                return;
            
            bool rightClick  = eventData.button == PointerEventData.InputButton.Right;
            bool leftClick = eventData.button == PointerEventData.InputButton.Left;

            if (slotType == MerchantSlotType.PlayerSlot)
            {
                if (rightClick)
                {
                    bool buyFullStack = Input.GetKey(KeyCode.LeftControl);
                    _merchant.TrySellItem(ItemInSlot, buyFullStack);
                }
                else if (leftClick)
                {
                    base.OnPointerDown(eventData);
                }
            }
            else if (slotType == MerchantSlotType.MerchantSlot)
            {
                if(leftClick)
                    return;
                
                bool sellFullStack = Input.GetKey(KeyCode.LeftControl);
                _merchant.TryBuyItem(ItemInSlot, sellFullStack);
            }
            
            UiManager.Instance.ItemTooltip.ShowTooltip(false, null);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if(ItemInSlot == null)
                return;
            
            if(SlotType == MerchantSlotType.MerchantSlot)
                UiManager.Instance.ItemTooltip.ShowTooltip(true, Rect, ItemInSlot, true, true);
            else
                UiManager.Instance.ItemTooltip.ShowTooltip(true, Rect, ItemInSlot, false, true);
        }
    }
}