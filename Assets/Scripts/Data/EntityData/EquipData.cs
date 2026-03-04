// Done
using System;
using InventorySystem;
using UnityEngine;

namespace Data.EntityData
{
    [Serializable]
    public class EquipData
    {
        [SerializeField] private InventoryItem weapon;
        [SerializeField] private InventoryItem armor;
        [SerializeField] private InventoryItem accessory1;
        [SerializeField] private InventoryItem accessory2;

        public InventoryItem Weapon => weapon;
        public InventoryItem Armor => armor;
        public InventoryItem Accessory1 => accessory1;
        public InventoryItem Accessory2 => accessory2;
    }
}