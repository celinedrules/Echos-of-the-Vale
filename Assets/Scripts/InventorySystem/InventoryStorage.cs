using System.Collections.Generic;
using System.Linq;
using Data.InventoryData;
using Data.ItemData;
using Managers;
using UnityEngine;

namespace InventorySystem
{
    public class InventoryStorage : InventoryBase
    {
        private InventoryPlayer _playerInventory;
        private List<InventoryItem> _materialStash = new();

        private InventoryRuntimeData RuntimeRuntimeData => GameManager.Instance.InventoryRuntimeData;

        public InventoryPlayer PlayerInventory => _playerInventory;
        public List<InventoryItem> MaterialStash => _materialStash;

        public void SetInventory(InventoryPlayer inventory)
        {
            _playerInventory = inventory;
            LoadFromRuntimeData();
        }

        public bool CanCraftItem(InventoryItem itemToCraft) =>
            HasEnoughMaterials(itemToCraft) && PlayerInventory.CanAddItem(itemToCraft);

        public void CraftItem(InventoryItem itemToCraft)
        {
            ConsumeItems(itemToCraft);
            PlayerInventory.AddItem(itemToCraft);
        }

        public void FromPlayerToStorage(InventoryItem item, bool transferAll)
        {
            int transferAmount = transferAll ? item.StackSize : 1;

            for (int i = 0; i < transferAmount; i++)
            {
                if (CanAddItem(item))
                {
                    InventoryItem itemToAdd = new(item.ItemData);
                    _playerInventory.RemoveOneItem(item);
                    AddItem(itemToAdd);

                    // if (QuestManager.Exists)
                    //     QuestManager.Instance.ReduceProgress(item.ItemData.SaveId);
                }
            }

            TriggerUpdateUi();
        }

        public void FromStorageToPlayer(InventoryItem item, bool transferAll)
        {
            int transferAmount = transferAll ? item.StackSize : 1;

            for (int i = 0; i < transferAmount; i++)
            {
                if (_playerInventory.CanAddItem(item))
                {
                    InventoryItem itemToAdd = new(item.ItemData);
                    RemoveOneItem(item);
                    _playerInventory.AddItem(itemToAdd);

                    // if (QuestManager.Exists)
                    //     QuestManager.Instance.AddProgress(item.ItemData.SaveId);
                }
            }

            TriggerUpdateUi();
        }

        private void AddMaterialToStash(InventoryItem item)
        {
            InventoryItem stackableItem = StackableInStash(item);

            if (stackableItem != null)
            {
                stackableItem.AddToStack();
            }
            else
            {
                InventoryItem itemToAdd = new(item.ItemData);
                _materialStash.Add(itemToAdd);
            }

            TriggerUpdateUi();
            _materialStash = _materialStash.OrderBy(i => i.ItemData.ItemName).ToList();
        }

        public void DepositMaterialFromPlayer(InventoryItem item, bool depositAll = false)
        {
            int amount = depositAll ? item.StackSize : 1;

            for (int i = 0; i < amount; i++)
            {
                InventoryItem materialToAdd = new(item.ItemData);
                _playerInventory.RemoveOneItem(item);
                AddMaterialToStash(materialToAdd);
            }
        }

        public void WithdrawMaterialToPlayer(InventoryItem item, bool withdrawAll = false)
        {
            int amount = withdrawAll ? item.StackSize : 1;

            for (int i = 0; i < amount; i++)
            {
                if (!_playerInventory.CanAddItem(item))
                    break;

                InventoryItem materialToAdd = new(item.ItemData);
                RemoveFromMaterialStash(item);
                _playerInventory.AddItem(materialToAdd);
            }

            TriggerUpdateUi();
        }

        private void RemoveFromMaterialStash(InventoryItem item)
        {
            InventoryItem itemInStash = _materialStash.Find(i => i == item);

            if (itemInStash.StackSize > 1)
                itemInStash.RemoveFromStack();
            else
                _materialStash.Remove(item);
        }

        private InventoryItem StackableInStash(InventoryItem item) =>
            _materialStash.Find(i => i.ItemData == item.ItemData && i.CanAddToStack());

        private void ConsumeItems(InventoryItem itemToCraft)
        {
            foreach (InventoryItem requiredItem in itemToCraft.ItemData.CraftRecipe)
            {
                int amountToConsume = requiredItem.StackSize;
                amountToConsume -= ConsumedItemsAmount(_playerInventory.ItemList, requiredItem);

                if (amountToConsume > 0)
                    amountToConsume -= ConsumedItemsAmount(ItemList, requiredItem);

                if (amountToConsume > 0)
                    amountToConsume -= ConsumedItemsAmount(MaterialStash, requiredItem);

                // if (QuestManager.Exists)
                //     QuestManager.Instance.ReduceProgress(requiredItem.ItemData.SaveId, requiredItem.StackSize);
            }
        }

        private int ConsumedItemsAmount(List<InventoryItem> itemsList, InventoryItem neededItem)
        {
            int amountNeeded = neededItem.StackSize;
            int consumedAmount = 0;
            List<InventoryItem> itemsToRemove = new();

            foreach (InventoryItem item in itemsList)
            {
                if (item.ItemData != neededItem.ItemData)
                    continue;

                int removedAmount = Mathf.Min(item.StackSize, amountNeeded - consumedAmount);
                item.StackSize -= removedAmount;
                consumedAmount += removedAmount;

                if (item.StackSize <= 0)
                    itemsToRemove.Add(item);

                if (consumedAmount >= amountNeeded)
                    break;
            }

            foreach (InventoryItem item in itemsToRemove)
                itemsList.Remove(item);

            return consumedAmount;
        }

        private bool HasEnoughMaterials(InventoryItem itemToCraft)
        {
            foreach (InventoryItem requiredItem in itemToCraft.ItemData.CraftRecipe)
            {
                if (GetAvailableAmountOf(requiredItem.ItemData) < requiredItem.StackSize)
                    return false;
            }

            return true;
        }

        public int GetAvailableAmountOf(ItemData requiredItem)
        {
            int amount = 0;

            foreach (InventoryItem item in _playerInventory.ItemList)
            {
                if (item.ItemData == requiredItem)
                    amount = amount + item.StackSize;
            }

            foreach (InventoryItem item in ItemList)
            {
                if (item.ItemData == requiredItem)
                    amount = amount + item.StackSize;
            }

            foreach (InventoryItem item in MaterialStash)
            {
                if (item.ItemData == requiredItem)
                    amount = amount + item.StackSize;
            }

            return amount;
        }

        // public override void SaveData(ref GameData gameData)
        // {
        //     SaveToRuntimeData();
        //
        //     base.SaveData(ref gameData);
        //
        //     gameData.storageItems.Clear();
        //     gameData.storageMaterials.Clear();
        //
        //     foreach (InventoryItem item in ItemList)
        //     {
        //         if (item == null || !item.ItemData)
        //             continue;
        //
        //         string saveId = item.ItemData.SaveId;
        //
        //         gameData.storageItems.TryAdd(saveId, 0);
        //         gameData.storageItems[saveId] += item.StackSize;
        //     }
        //
        //     foreach (InventoryItem item in MaterialStash)
        //     {
        //         if (item == null || !item.ItemData)
        //             continue;
        //
        //         string saveId = item.ItemData.SaveId;
        //
        //         gameData.storageMaterials.TryAdd(saveId, 0);
        //         gameData.storageMaterials[saveId] += item.StackSize;
        //     }
        // }

        // public override void LoadData(GameData gameData)
        // {
        //     ItemList.Clear();
        //     MaterialStash.Clear();
        //
        //     foreach ((string saveId, int stackSize) in gameData.storageItems)
        //     {
        //         ItemData itemData = itemDatabase.GetItemData(saveId);
        //
        //         if (!itemData)
        //         {
        //             Debug.LogWarning($"Can't find save data for item {saveId}!");
        //             continue;
        //         }
        //
        //         for (int i = 0; i < stackSize; i++)
        //         {
        //             InventoryItem inventoryItem = new(itemData);
        //             AddItem(inventoryItem);
        //         }
        //     }
        //
        //     foreach ((string saveId, int stackSize) in gameData.storageMaterials)
        //     {
        //         ItemData itemData = itemDatabase.GetItemData(saveId);
        //
        //         if (!itemData)
        //         {
        //             Debug.LogWarning($"Can't find save data for item {saveId}!");
        //             continue;
        //         }
        //
        //         for (int i = 0; i < stackSize; i++)
        //         {
        //             InventoryItem inventoryItem = new(itemData);
        //             AddMaterialToStash(inventoryItem);
        //         }
        //     }
        //
        //     SaveToRuntimeData();
        // }

        public void SaveToRuntimeData()
        {
            var runtimeData = RuntimeRuntimeData;

            if (runtimeData == null)
                return;

            runtimeData.StorageItems.Clear();
            runtimeData.StorageMaterials.Clear();

            // Save storage items
            foreach (InventoryItem item in ItemList)
            {
                if (item == null || !item.ItemData) continue;
                runtimeData.AddStorageItem(item.ItemData.SaveId, item.StackSize);
            }

            // Save material stash
            foreach (InventoryItem item in _materialStash)
            {
                if (item == null || !item.ItemData) continue;
                runtimeData.AddStorageMaterial(item.ItemData.SaveId, item.StackSize);
            }
        }

        private void LoadFromRuntimeData()
        {
            InventoryRuntimeData runtimeRuntimeData = RuntimeRuntimeData;

            if (runtimeRuntimeData == null || (runtimeRuntimeData.StorageItems.Count == 0 &&
                                               runtimeRuntimeData.StorageMaterials.Count == 0))
                return;

            ItemList.Clear();
            _materialStash.Clear();

            // Load storage items
            foreach (InventoryItemEntry entry in runtimeRuntimeData.StorageItems)
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

            // Load material stash
            foreach (InventoryItemEntry entry in runtimeRuntimeData.StorageMaterials)
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
                    AddMaterialToStash(inventoryItem);
                }
            }

            TriggerUpdateUi();
        }

        public void ClearRuntimeData()
        {
            RuntimeRuntimeData?.ClearStorage();

            ItemList.Clear();
            _materialStash.Clear();

            TriggerUpdateUi();
        }
    }
}