// Done
using System;

namespace Data.InventoryData
{
    [Serializable]
    public struct ItemSlotData : IEquatable<ItemSlotData>
    {
        public ItemData.ItemData Item;
        public int Amount;

        public ItemSlotData(ItemData.ItemData item, int amount = 1)
        {
            Item = item;
            Amount = amount;
        }

        public bool Equals(ItemSlotData other) => Item == other.Item && Amount == other.Amount;
        public override bool Equals(object obj) => obj is ItemSlotData other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Item, Amount);

        public static bool operator ==(ItemSlotData left, ItemSlotData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ItemSlotData left, ItemSlotData right)
        {
            return !left.Equals(right);
        }
    }
}