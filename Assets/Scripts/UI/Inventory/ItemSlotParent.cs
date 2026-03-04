// Done
using System.Collections.Generic;
using InventorySystem;
using UnityEngine;

namespace UI.Inventory
{
    public class ItemSlotParent : MonoBehaviour
    {
        private ItemSlot[] _slots;

        public void UpdateSlots(List<InventoryItem> itemList)
        {
            if(_slots is not { Length: > 0})
                _slots = GetComponentsInChildren<ItemSlot>();
            
            for (int i = 0; i < _slots.Length; i++)
            {
                if (i < itemList.Count)
                    _slots[i].UpdateSlot(itemList[i]);
                else
                    _slots[i].UpdateSlot(null);
            }
        }
    }
}