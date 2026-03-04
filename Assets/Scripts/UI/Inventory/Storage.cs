// Done
using InventorySystem;
using UI.Common;
using UnityEngine;

namespace UI.Inventory
{
    public class Storage : MonoBehaviour, IUiPanel
    {
        [SerializeField] private ItemSlotParent storageParent;
        [SerializeField] private ItemSlotParent inventoryParent;
        [SerializeField] private ItemSlotParent materialStashParent;
     
        private InventoryStorage _storage;
        private InventoryPlayer _inventory;
        private CanvasGroup _canvasGroup;
        
        public CanvasGroup CanvasGroup => _canvasGroup;
        public bool ShowMenuButtons => false;
        public bool ShowBackground => true;
        public bool DisablePlayerInput => true;
        public bool HasTooltips => true;
        public InventoryPlayer InventoryPlayer => _inventory;
        public InventoryStorage InventoryStorage => _storage;
        
        private void Start() => _canvasGroup = GetComponent<CanvasGroup>();

        public void SetupStorage(InventoryStorage storage)
        {
            _storage = storage;
            _inventory = storage.PlayerInventory;
            _storage.OnInventoryChanged += UpdateUi;
            UpdateUi();
            
            StorageSlot[] storageSlots = GetComponentsInChildren<StorageSlot>();

            foreach (StorageSlot slot in storageSlots)
                slot.SetStorage( _storage);
        }

        public void UpdateUi()
        {
            if(!_storage)
                return;
            
            storageParent.UpdateSlots(_storage.ItemList);
            inventoryParent.UpdateSlots(_inventory.ItemList);
            materialStashParent.UpdateSlots(_storage.MaterialStash);
        }
        
        public void OnOpened()
        {
            UpdateUi();
        }
    }
}