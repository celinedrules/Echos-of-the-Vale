// Done
using System;
using UnityEngine;

namespace Data.InventoryData
{
    [Serializable]
    public class InventoryGrid
    {
        public const int Columns = 5;
        
        [SerializeField] private int size = 10;
        [SerializeField] private ItemSlotData[] slots;

        public int Size
        {
            get => size;
            set
            {
                size = Mathf.Max(1, value);
                ResizeIfNeeded();
            }
        }

        public int Rows => Mathf.CeilToInt((float)size / Columns);
        public ItemSlotData[] Slots => slots;
        public InventoryGrid() => slots = new ItemSlotData[size];

        public ItemSlotData GetSlot(int column, int row)
        {
            int index = row * Columns + column;
            
            if (index >= size || index >= slots.Length)
                return default;
            
            return slots[index];
        }

        public void SetSlot(int column, int row, ItemSlotData value)
        {
            int index = row * Columns + column;
            
            if (index >= size || index >= slots.Length)
                return;
            
            slots[index] = value;
        }

        public bool IsValidSlot(int column, int row)
        {
            int index = row * Columns + column;
            return index < size;
        }

        private void ResizeIfNeeded()
        {
            if (slots == null || slots.Length != size)
            {
                ItemSlotData[] newSlots = new ItemSlotData[size];
                
                if (slots != null)
                {
                    int copyCount = Mathf.Min(slots.Length, size);
                    Array.Copy(slots, newSlots, copyCount);
                }
                
                slots = newSlots;
            }
        }
    }
}