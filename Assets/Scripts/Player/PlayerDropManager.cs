// Done
using System.Collections.Generic;
using Core;
using InventorySystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Player
{
    public class PlayerDropManager : EntityDropManager
    {
        [Header("Drop Details")]
        [Range(0.0f, 100.0f)]
        [SerializeField] private float chanceToLoseItem = 90.0f;

        private InventoryPlayer _inventory;

        private void Awake()
        {
            _inventory = GetComponent<InventoryPlayer>();
        }

        public override void DropItems()
        {
            List<InventoryItem> inventoryCopy = new(_inventory.ItemList);
            List<InventoryEquipmentSlot> equipCopy = new(_inventory.EquipList);

            foreach (InventoryItem item in inventoryCopy)
            {
                if (Random.Range(0.0f, 100.0f) < chanceToLoseItem)
                {
                    CreateItemDrop(item.ItemData);
                    _inventory.RemoveFullStack(item);
                }
            }

            foreach (InventoryEquipmentSlot equipmentSlot in equipCopy)
            {
                if (Random.Range(0.0f, 100.0f) < chanceToLoseItem && equipmentSlot.HasItem)
                {
                    InventoryItem item = equipmentSlot.EquippedItem;
                    
                    CreateItemDrop(item.ItemData);
                    _inventory.UnequipItem(item);
                    _inventory.RemoveFullStack(item);
                }
            }
        }
    }
}