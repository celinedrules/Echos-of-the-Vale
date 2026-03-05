using System;
using System.Collections.Generic;
using Core.Interfaces;
using Data.ItemData;
using Managers;
using Player;
using SaveSystem;
using UnityEngine;

namespace InventorySystem
{
    public class InventoryBase : MonoBehaviour, ISavable
    {
        public event Action OnInventoryChanged;

        [SerializeField] protected ItemDatabase itemDatabase;
        [SerializeField] private int maxInventorySize = 10;
        [SerializeField] private List<InventoryItem> itemList = new();

        protected PlayerController Player;

        protected int MaxInventorySize => maxInventorySize;
        public List<InventoryItem> ItemList => itemList;   
        
        protected virtual void Awake() => Player = GameManager.Instance.Player;

        public void TryUseItem(InventoryItem item)
        {
            InventoryItem consumable = itemList.Find(i => i == item);

            if (consumable == null)
                return;

            if (!consumable.ItemEffect.CanBeUsed(Player))
                return;

            consumable.ItemEffect.ExecuteEffect();

            if (consumable.StackSize > 1)
                consumable.RemoveFromStack();
            else
                RemoveOneItem(consumable);

            OnInventoryChanged?.Invoke();
        }

        public bool CanAddItem(InventoryItem item)
        {
            bool hasStackable = FindStackable(item) != null;
            return hasStackable || itemList.Count < maxInventorySize;
        }
        
        private InventoryItem FindStackable(InventoryItem itemToAdd) =>
            itemList.Find(i => i.ItemData == itemToAdd.ItemData && i.CanAddToStack());

        public void AddItem(InventoryItem item)
        {
            InventoryItem itemInInventory = FindStackable(item);

            if (itemInInventory != null)
                itemInInventory.AddToStack();
            else
                itemList.Add(item);
            
            // if (QuestManager.Exists)
            //     QuestManager.Instance.ReduceProgress(item.ItemData.SaveId);

            OnInventoryChanged?.Invoke();
        }
        
        public int GetItemAmount(ItemData itemData)
        {
            int total = 0;
            
            foreach (InventoryItem item in itemList)
            {
                if (item.ItemData == itemData)
                    total += item.StackSize;
            }
            
            return total;
        }
        
        public bool HasItemAmount(ItemData itemData, int amount)
        {
            int total = 0;
            
            foreach (InventoryItem item in itemList)
            {
                if (item.ItemData == itemData)
                    total += item.StackSize;
            }
            
            return total >= amount;
        }
        
        public void RemoveItemAmount(ItemData itemData, int amount)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                InventoryItem item = itemList[i];
                
                if(item.ItemData != itemData)
                    continue;
                
                int removedAmount = Mathf.Min(item.StackSize, amount);

                for (int j = 0; j < removedAmount; j++)
                {
                    RemoveOneItem(item);
                    amount--;
                    
                    if(amount <= 0)
                        break;
                }
            }
        }
        
        protected InventoryItem FindItem(InventoryItem itemToFind) => itemList.Find(i => i == itemToFind);

        public void RemoveOneItem(InventoryItem item)
        {
            InventoryItem itemInInventory = itemList.Find(i => i == item);

            if (itemInInventory.StackSize > 1)
                itemInInventory.RemoveFromStack();
            else
                itemList.Remove(item);

            OnInventoryChanged?.Invoke();
        }

        public void RemoveFullStack(InventoryItem itemToRemove)
        {
            for (int i = 0; i < itemToRemove.StackSize; i++)
                RemoveOneItem(itemToRemove);
        }
        
        protected InventoryItem FindSameItem(InventoryItem itemToFind) =>
            itemList.Find(i => i.ItemData == itemToFind.ItemData);

        protected void TriggerUpdateUi() => OnInventoryChanged?.Invoke();
        
        public virtual void LoadData(GameData gameData)
        {
            
        }

        public virtual void SaveData(ref GameData gameData)
        {
            
        }
    }
}