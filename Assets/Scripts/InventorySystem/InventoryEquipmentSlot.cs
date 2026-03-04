// Done
using System;
using UnityEngine;
using Utilities.Enums;

namespace InventorySystem
{
    [Serializable]
    public class InventoryEquipmentSlot
    {
        [SerializeField] private ItemType slotType;
        [SerializeField] private InventoryItem equippedItem;
        
        public ItemType SlotType => slotType;
        public InventoryItem EquippedItem
        {
            get => equippedItem;
            set => equippedItem = value;
        }

        public bool HasItem => equippedItem != null && equippedItem.ItemData;
    }
}