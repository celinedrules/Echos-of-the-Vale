// Done
using System.Collections.Generic;
using Data.InventoryData;
using UnityEngine;

namespace InventorySystem
{
    public class InventoryMerchant : InventoryBase
    {
        private int _minItemsAmount = 4;
        private InventoryGrid _shopInventory;
        private InventoryPlayer _inventory;

        public void SetInventory(InventoryPlayer inventory) => _inventory = inventory;

        public void SetShopData(InventoryGrid shopInventory, int minItemsAmount)
        {
            _shopInventory = shopInventory;
            _minItemsAmount = minItemsAmount;
            FillShopList();
        }

        public void FillShopList()
        {
            if (_shopInventory == null)
                return;

            ItemList.Clear();

            List<InventoryItem> possibleItems = new();

            foreach (ItemSlotData slot in _shopInventory.Slots)
            {
                if (slot.Item == null)
                    continue;

                int randomizedStack = Random.Range(slot.Item.MinStackSizeAtShop, slot.Item.MaxStackSizeAtShop + 1);
                int finalStack = Mathf.Clamp(randomizedStack, 1, slot.Item.MaxStackSizeAtShop);

                InventoryItem itemToAdd = new InventoryItem(slot.Item)
                {
                    StackSize = finalStack
                };

                possibleItems.Add(itemToAdd);
            }

            int randomItemAmount = Random.Range(_minItemsAmount, MaxInventorySize + 1);
            int finalAmount = Mathf.Clamp(randomItemAmount, 1, possibleItems.Count);

            for (int i = 0; i < finalAmount; i++)
            {
                int randomIndex = Random.Range(0, possibleItems.Count);
                InventoryItem item = possibleItems[randomIndex];

                if (CanAddItem(item))
                {
                    possibleItems.Remove(item);
                    AddItem(item);
                }
            }

            TriggerUpdateUi();
        }

        public void TryBuyItem(InventoryItem itemToBuy, bool buyFullStack)
        {
            int amountToBuy = buyFullStack ? itemToBuy.StackSize : 1;

            for (int i = 0; i < amountToBuy; i++)
            {
                if (_inventory.Gold < itemToBuy.BuyPrice)
                {
                    Debug.Log("Not enough gold");
                    return;
                }

                if (_inventory.CanAddItem(itemToBuy))
                {
                    InventoryItem itemToAdd = new InventoryItem(itemToBuy.ItemData);
                    _inventory.AddItem(itemToAdd);
                }

                _inventory.Gold -= itemToBuy.BuyPrice;
                RemoveOneItem(itemToBuy);
            }

            TriggerUpdateUi();
        }

        public void TrySellItem(InventoryItem itemToSell, bool sellFullStack)
        {
            int amountToSell = sellFullStack ? itemToSell.StackSize : 1;

            for (int i = 0; i < amountToSell; i++)
            {
                int sellPrice = Mathf.FloorToInt(itemToSell.SellPrice);
                _inventory.Gold += sellPrice;
                _inventory.RemoveOneItem(itemToSell);
            }

            TriggerUpdateUi();
        }
    }
}