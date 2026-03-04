// Done
using InventorySystem;
using TMPro;
using UI.Common;
using UI.Inventory;
using UnityEngine;

namespace UI
{
    public class Merchant : MonoBehaviour, IUiPanel
    {
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private ItemSlotParent merchantSlotsParent;
        [SerializeField] private ItemSlotParent inventorySlotsParent;
        [SerializeField] private EquipSlotParent equipSlotsParent;
        
        private InventoryPlayer _inventory;
        private InventoryMerchant _merchant;
        
        private CanvasGroup _canvasGroup;

        public CanvasGroup CanvasGroup => _canvasGroup;
        public bool ShowMenuButtons => false;
        public bool ShowBackground => true;
        public bool DisablePlayerInput => true;
        public bool HasTooltips => true;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        
        public void SetupMerchantUi(InventoryMerchant merchant, InventoryPlayer inventory)
        {
            _inventory = inventory;
            _merchant = merchant;
            
            _inventory.OnInventoryChanged += UpdateSlotUi;
            _merchant.OnInventoryChanged += UpdateSlotUi;
            
            UpdateSlotUi();
            
            MerchantSlot[] merchantSlots = GetComponentsInChildren<MerchantSlot>();
            
            foreach (MerchantSlot slot in merchantSlots)
                slot.SetupMerchantUi(merchant);
        }

        private void UpdateSlotUi()
        {
            if(!_inventory)
                return;
            
            inventorySlotsParent.UpdateSlots(_inventory.ItemList);
            merchantSlotsParent.UpdateSlots(_merchant.ItemList);
            equipSlotsParent.UpdateEquipmentSlots(_inventory.EquipList);
            goldText.text = $"{_inventory.Gold:N0}g";
        }
        
        private void OnDestroy()
        {
            if (_inventory != null)
                _inventory.OnInventoryChanged -= UpdateSlotUi;
            if (_merchant != null)
                _merchant.OnInventoryChanged -= UpdateSlotUi;
        }
        
        public void OnOpened()
        {
            
        }
    }
}