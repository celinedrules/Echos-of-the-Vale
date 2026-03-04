// Done
using Data.InventoryData;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.EntityData
{
    public class MerchantNpcData : NpcData
    {
        [TabGroup("Shop Inventory")]
        [SerializeField, HideLabel]
        private InventoryGrid shopInventory = new();

        [TabGroup("Shop Inventory")]
        [SerializeField] private int minItemsAmount = 4;

        public InventoryGrid ShopInventory => shopInventory;
        public int MinItemsAmount => minItemsAmount;
    }
}