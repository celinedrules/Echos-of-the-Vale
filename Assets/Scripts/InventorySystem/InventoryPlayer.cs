using System;
using System.Collections.Generic;
using Data.InventoryData;
using Data.ItemData;
using Managers;
using SaveSystem;
using Stats;
using UnityEngine;
using Utilities.Enums;

namespace InventorySystem
{
    public class InventoryPlayer : InventoryBase
    {
        public event Action<int> OnQuickSlotUse;
        
        [Header("Gold Info")]
        [SerializeField] private int gold = 10000;
        
        [Header("Equipment Info")]
        [SerializeField] private List<InventoryEquipmentSlot> equipList;
        
        [Header("Quick Item Slots")]
        [SerializeField] private InventoryItem[] quickItems = new InventoryItem[2];
        
        private InventoryRuntimeData RuntimeRuntimeData => GameManager.Instance.InventoryRuntimeData;

        public int Gold {get => gold; set => gold = value; }
        
        public List<InventoryEquipmentSlot> EquipList => equipList;
        public InventoryItem[] QuickItems => quickItems;
        
        private void Start()
        {
            if (Player.Stats is PlayerStats playerStats)
                playerStats.OnStatsChanged += TriggerUpdateUi;
            
            // Load from runtime data on start (handles scene transitions)
            LoadFromRuntimeData();
        }
        
        public void SetQuickItemInSlot(int slotNumber, InventoryItem inventoryItem)
        {
            quickItems[slotNumber - 1] = inventoryItem;
            TriggerUpdateUi();
        }
        
        public void TryUseQuickItem(int slotNumber)
        {
            int finalSlotNumber =  slotNumber - 1;
            InventoryItem itemToUse = quickItems[finalSlotNumber];
            
            if(itemToUse == null)
                return;
            
            TryUseItem(itemToUse);

            if (FindItem(itemToUse) == null)
                quickItems[finalSlotNumber] = FindSameItem(itemToUse);
            
            TriggerUpdateUi();
            OnQuickSlotUse?.Invoke(finalSlotNumber);
        }
        
        public void TryEquipItem(InventoryItem item)
        {
            InventoryItem inventoryItem = FindItem(item);
            List<InventoryEquipmentSlot> matchingSlots = equipList.FindAll(s => s.SlotType == item.ItemData.ItemType);

            foreach (InventoryEquipmentSlot slot in matchingSlots)
            {
                if (!slot.HasItem)
                {
                    EquipItem(inventoryItem, slot);
                    return;
                }
            }

            InventoryEquipmentSlot slotToReplace = matchingSlots[0];
            InventoryItem itemToUnEquip = slotToReplace.EquippedItem;

            UnequipItem(itemToUnEquip, true);
            EquipItem(inventoryItem, slotToReplace);
        }
        
        private void EquipItem(InventoryItem itemToEquip, InventoryEquipmentSlot slot)
        {
            float savedHealthPercentage = Player.Health.GetHealthPercentage();
            
            slot.EquippedItem = itemToEquip;
            slot.EquippedItem.AddModifiers(Player.Stats);
            slot.EquippedItem.AddItemEffect(Player);
            
            Player.Health.SetHealthToPercentage(savedHealthPercentage);

            RemoveOneItem(itemToEquip);
        }
        
        public void UnequipItem(InventoryItem itemToUnequip, bool replacingItem = false)
        {
            if (!CanAddItem(itemToUnequip) && !replacingItem)
            {
                Debug.Log($"Can't unequip {itemToUnequip.ItemData.ItemType}! No space in inventory!");
                return;
            }

            float savedHealthPercentage = Player.Health.GetHealthPercentage();

            InventoryEquipmentSlot slotToUnequip = equipList.Find(s => s.EquippedItem == itemToUnequip);
            
            if (slotToUnequip != null)
                slotToUnequip.EquippedItem = null;

            itemToUnequip.RemoveModifiers(Player.Stats);
            itemToUnequip.RemoveItemEffect();
            
            Player.Health.SetHealthToPercentage(savedHealthPercentage);
            AddItem(itemToUnequip);
        }
        
        private void OnDestroy()
        {
            if (Player.Stats is PlayerStats playerStats)
                playerStats.OnStatsChanged -= TriggerUpdateUi;
        }
        
        public void SaveToRuntimeData()
        {
            InventoryRuntimeData runtimeRuntimeData = RuntimeRuntimeData;
            
            if (runtimeRuntimeData == null)
                return;
        
            runtimeRuntimeData.Items.Clear();
            runtimeRuntimeData.EquippedItems.Clear();
            runtimeRuntimeData.Gold = Gold;
        
            // Save inventory items
            foreach (InventoryItem item in ItemList)
            {
                if (item == null || !item.ItemData) continue;
                runtimeRuntimeData.AddItem(item.ItemData.SaveId, item.StackSize);
            }
        
            // Save equipped items
            foreach (InventoryEquipmentSlot slot in equipList)
            {
                if (slot.HasItem)
                {
                    runtimeRuntimeData.EquipItem(slot.EquippedItem.ItemData.SaveId, slot.SlotType);
                }
            }
        
            // Save quick slots
            for (int i = 0; i < quickItems.Length; i++)
            {
                string saveId = quickItems[i]?.ItemData?.SaveId ?? "";
                runtimeRuntimeData.SetQuickSlot(i, saveId);
            }
        }
        
        private void LoadFromRuntimeData()
        {
            InventoryRuntimeData runtimeRuntimeData = RuntimeRuntimeData;
            
            if (runtimeRuntimeData == null || runtimeRuntimeData.Items.Count == 0 && runtimeRuntimeData.Gold == 10000) 
                return; // No saved data or default state
        
            Gold = runtimeRuntimeData.Gold;
            ItemList.Clear();
        
            // Clear current equipment
            foreach (InventoryEquipmentSlot slot in equipList)
            {
                if (slot.HasItem)
                {
                    slot.EquippedItem.RemoveModifiers(Player.Stats);
                    slot.EquippedItem.RemoveItemEffect();
                    slot.EquippedItem = null;
                }
            }
        
            // Load inventory items
            foreach (InventoryItemEntry entry in runtimeRuntimeData.Items)
            {
                ItemData itemData = itemDatabase.GetItemData(entry.saveId);
                if (!itemData)
                {
                    Debug.LogWarning($"Can't find item data for {entry.saveId}!");
                    continue;
                }
        
                for (int i = 0; i < entry.stackSize; i++)
                {
                    InventoryItem inventoryItem = new(itemData);
                    AddItem(inventoryItem);
                }
            }
        
            // Load equipped items
            foreach (EquippedItemEntry entry in runtimeRuntimeData.EquippedItems)
            {
                ItemData itemData = itemDatabase.GetItemData(entry.saveId);
                if (!itemData) continue;
        
                InventoryItem inventoryItem = new(itemData);
                InventoryEquipmentSlot slot = equipList.Find(s => s.SlotType == entry.slotType && !s.HasItem);
        
                if (slot != null)
                {
                    slot.EquippedItem = inventoryItem;
                    inventoryItem.AddModifiers(Player.Stats);
                    inventoryItem.AddItemEffect(Player);
                }
            }
        
            // Load quick slots
            for (int i = 0; i < runtimeRuntimeData.QuickSlotItemIds.Length && i < quickItems.Length; i++)
            {
                string saveId = runtimeRuntimeData.QuickSlotItemIds[i];
                if (!string.IsNullOrEmpty(saveId))
                {
                    quickItems[i] = ItemList.Find(item => item.ItemData.SaveId == saveId);
                }
            }
        
            TriggerUpdateUi();
        }

        public void ClearRuntimeData()
        {
            // Clear the ScriptableObject
            RuntimeRuntimeData?.ResetToDefaults();
        
            // Clear local inventory state
            Gold = 10000; // or whatever your default starting gold is
            ItemList.Clear();
        
            // Clear equipped items
            foreach (InventoryEquipmentSlot slot in equipList)
            {
                if (slot.HasItem)
                {
                    slot.EquippedItem.RemoveModifiers(Player.Stats);
                    slot.EquippedItem.RemoveItemEffect();
                    slot.EquippedItem = null;
                }
            }
        
            // Clear quick slots
            for (int i = 0; i < quickItems.Length; i++)
            {
                quickItems[i] = null;
            }
        
            TriggerUpdateUi();
        }

        public override void SaveData(ref GameData gameData)
        {
            SaveToRuntimeData();
            
            // gameData.gold = Gold;
            // gameData.inventory.Clear();
            // gameData.equipedItems.Clear();
            //
            // foreach (InventoryItem item in ItemList)
            // {
            //     if (item == null || !item.ItemData) 
            //         continue;
            //     
            //     string saveId = item.ItemData.SaveId;
            //
            //     gameData.inventory.TryAdd(saveId, 0);
            //     gameData.inventory[saveId] += item.StackSize;
            // }
            //
            // foreach (InventoryEquipmentSlot slot in equipList)
            // {
            //     if (slot.HasItem)
            //         gameData.equipedItems[slot.EquippedItem.ItemData.saveId] = slot.SlotType;
            // }
        }

        public override void LoadData(GameData gameData)
        {
            // Gold = gameData.gold;
            //
            // ItemList.Clear();
            //
            // foreach (InventoryEquipmentSlot slot in equipList)
            // {
            //     if (slot.HasItem)
            //     {
            //         slot.EquippedItem.RemoveModifiers(Player.Stats);
            //         slot.EquippedItem.RemoveItemEffect();
            //         slot.EquippedItem = null;
            //     }
            // }
            //
            // foreach ((string saveId, int stackSize) in gameData.inventory)
            // {
            //     ItemData itemData = itemDatabase.GetItemData(saveId);
            //     
            //     if (!itemData)
            //     {
            //         Debug.LogWarning($"Can't find save data for item {saveId}!");
            //         continue;
            //     }
            //     
            //     for(int i = 0; i < stackSize; i++)
            //     {
            //         InventoryItem inventoryItem = new(itemData);
            //         AddItem(inventoryItem);
            //     }
            // }
            //
            // foreach ((string saveId, ItemType itemType) in gameData.equipedItems)
            // {
            //     ItemData itemData = itemDatabase.GetItemData(saveId);
            //     InventoryItem inventoryItem = new(itemData);
            //     
            //     InventoryEquipmentSlot slot = equipList.Find(slot => slot.SlotType == itemType && !slot.HasItem);
            //
            //     if (slot == null)
            //         continue;
            //     
            //     slot.EquippedItem = inventoryItem;
            //     inventoryItem.AddModifiers(Player.Stats);
            //     inventoryItem.AddItemEffect(Player);
            // }
            //
            // TriggerUpdateUi();
            // SaveToRuntimeData();
        }
    }
}