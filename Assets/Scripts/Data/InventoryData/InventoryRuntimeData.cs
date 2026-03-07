// Done
using System;
using System.Collections.Generic;
using Managers;
using Player;
using Sirenix.OdinInspector;
using UI.Inventory;
using UnityEngine;
using Utilities.Enums;

namespace Data.InventoryData
{
    [CreateAssetMenu(fileName = "Inventory Data", menuName = "Echos of the Vale/Inventory Data/Inventory Data")]
    public class InventoryRuntimeData : ScriptableObject
    {
        public event Action OnDataChanged;

        [Header("Gold")]
        [SerializeField] private int gold = 10000;

        [Header("Inventory Items")]
        [SerializeField] private List<InventoryItemEntry> items = new();

        [Header("Equipped Items")]
        [SerializeField] private List<EquippedItemEntry> equippedItems = new();

        [Header("Quick Slots")]
        [SerializeField] private string[] quickSlotItemIds = new string[2];

        [Header("Storage Items")]
        [SerializeField] private List<InventoryItemEntry> storageItems = new();

        [Header("Storage Materials")]
        [SerializeField] private List<InventoryItemEntry> storageMaterials = new();

        [Header("State")]
        [SerializeField] private bool hasValidData;

        public int Gold
        {
            get => gold;
            set
            {
                gold = value;
                OnDataChanged?.Invoke();
            }
        }

        public List<InventoryItemEntry> Items => items;
        public List<EquippedItemEntry> EquippedItems => equippedItems;
        public string[] QuickSlotItemIds => quickSlotItemIds;
        public List<InventoryItemEntry> StorageItems => storageItems;
        public List<InventoryItemEntry> StorageMaterials => storageMaterials;

        public bool HasValidData
        {
            get => hasValidData;
            private set => hasValidData = value;
        }

        public void ClearAll()
        {
            hasValidData = false;
            gold = 0;
            items.Clear();
            equippedItems.Clear();
            quickSlotItemIds = new string[2];
            storageItems.Clear();
            storageMaterials.Clear();
            OnDataChanged?.Invoke();
        }

        public void AddItem(string saveId, int amount = 1)
        {
            hasValidData = true;
            InventoryItemEntry existing = items.Find(i => i.saveId == saveId);

            if (existing != null)
                existing.stackSize += amount;
            else
                items.Add(new InventoryItemEntry { saveId = saveId, stackSize = amount });

            OnDataChanged?.Invoke();
        }

        public void RemoveItem(string saveId, int amount = 1)
        {
            var existing = items.Find(i => i.saveId == saveId);
            if (existing != null)
            {
                existing.stackSize -= amount;
                if (existing.stackSize <= 0)
                {
                    items.Remove(existing);
                }
            }

            OnDataChanged?.Invoke();
        }

        public void EquipItem(string saveId, ItemType slotType)
        {
            hasValidData = true;
            equippedItems.RemoveAll(e => e.slotType == slotType);
            equippedItems.Add(new EquippedItemEntry { saveId = saveId, slotType = slotType });
            OnDataChanged?.Invoke();
        }

        public void UnequipItem(ItemType slotType)
        {
            equippedItems.RemoveAll(e => e.slotType == slotType);
            OnDataChanged?.Invoke();
        }

        public void SetQuickSlot(int slotIndex, string saveId)
        {
            if (slotIndex >= 0 && slotIndex < quickSlotItemIds.Length)
            {
                quickSlotItemIds[slotIndex] = saveId;
                OnDataChanged?.Invoke();
            }
        }

        public void AddStorageItem(string saveId, int amount = 1)
        {
            var existing = storageItems.Find(i => i.saveId == saveId);
            if (existing != null)
            {
                existing.stackSize += amount;
            }
            else
            {
                storageItems.Add(new InventoryItemEntry { saveId = saveId, stackSize = amount });
            }

            OnDataChanged?.Invoke();
        }

        public void AddStorageMaterial(string saveId, int amount = 1)
        {
            var existing = storageMaterials.Find(i => i.saveId == saveId);
            if (existing != null)
            {
                existing.stackSize += amount;
            }
            else
            {
                storageMaterials.Add(new InventoryItemEntry { saveId = saveId, stackSize = amount });
            }

            OnDataChanged?.Invoke();
        }

        public void ClearStorage()
        {
            storageItems.Clear();
            storageMaterials.Clear();
            OnDataChanged?.Invoke();
        }

        // Reset to default values (useful for new game)
        public void ResetToDefaults()
        {
            hasValidData = false;
            gold = 10000;
            items.Clear();
            equippedItems.Clear();
            quickSlotItemIds = new string[2];
            storageItems.Clear();
            storageMaterials.Clear();
            OnDataChanged?.Invoke();
        }

#if UNITY_EDITOR
        [ButtonGroup("Debug")]
        [Button("Save Runtime Data", ButtonSizes.Medium)]
        private void DebugSaveRuntimeData()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Can only save runtime data while in Play Mode.");
                return;
            }

            PlayerController player = GameManager.Instance?.Player;

            if (!player)
            {
                Debug.LogWarning("GameManager.Player not available.");
                return;
            }

            player.Inventory?.SaveToRuntimeData();

            Storage storage = UiManager.Instance?.Storage;
            storage?.InventoryStorage?.SaveToRuntimeData();

            Debug.Log("Runtime data saved.");
        }

        [ButtonGroup("Debug")]
        [Button("Clear Runtime Data", ButtonSizes.Medium)]
        private void DebugClearRuntimeData()
        {
            ResetToDefaults();
            Debug.Log("Runtime data cleared.");
        }

        // Reset runtime data when exiting play mode in editor
        private void OnEnable() => UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        private void OnDisable() => UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                ResetToDefaults();
        }
#endif
    }
}