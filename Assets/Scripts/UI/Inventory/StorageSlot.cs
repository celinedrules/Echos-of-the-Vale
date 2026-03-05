// Done
using System;
using InventorySystem;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Enums;

namespace UI.Inventory
{
    public class StorageSlot : ItemSlot
    {
        public enum StorageSlotType
        {
            StorageSlot,
            PlayerInventorySlot,
            MaterialStashSlot
        }

        [SerializeField] private StorageSlotType slotType;
        
        private InventoryStorage _storage;
        
        public StorageSlotType SlotType => slotType;
        
        public void SetStorage(InventoryStorage storage) => _storage = storage;

        public override void OnPointerDown(PointerEventData eventData)
        {
            if(ItemInSlot == null)
                return;

            // Update input
            bool transferAllItems = Input.GetKey(KeyCode.LeftControl);
            
            switch (slotType)
            {
                case StorageSlotType.StorageSlot:
                    _storage.FromStorageToPlayer(ItemInSlot, transferAllItems);
                    break;
                case StorageSlotType.PlayerInventorySlot:
                    if (ItemInSlot.ItemData.ItemType == ItemType.Material)
                        _storage.DepositMaterialFromPlayer(ItemInSlot, transferAllItems);
                    else
                        _storage.FromPlayerToStorage(ItemInSlot, transferAllItems);
                    break;
                case StorageSlotType.MaterialStashSlot:
                    _storage.WithdrawMaterialToPlayer(ItemInSlot, transferAllItems);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            UiManager.Instance.ItemTooltip.ShowTooltip(false, null);
        }
    }
}