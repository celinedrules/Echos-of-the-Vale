// Done
using System.Collections.Generic;
using InventorySystem;
using UnityEngine;

namespace UI.Inventory
{
    public class EquipSlotParent : MonoBehaviour
    {
        private EquipSlot[] _equipSlots;

        public void UpdateEquipmentSlots(List<InventoryEquipmentSlot> equipList)
        {
            if(_equipSlots == null  || _equipSlots.Length == 0 )
                _equipSlots = GetComponentsInChildren<EquipSlot>();
             
            for (int i = 0; i < _equipSlots.Length; i++)
            {
                InventoryEquipmentSlot playerEquipSlot = equipList[i];
                
                if(!playerEquipSlot.HasItem)
                    _equipSlots[i].UpdateSlot(null);
                else
                    _equipSlots[i].UpdateSlot(playerEquipSlot.EquippedItem);
            }
        }
    }
}