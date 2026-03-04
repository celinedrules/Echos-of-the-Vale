// Done
using InventorySystem;
using TMPro;
using UI.Common;
using UnityEngine;

namespace UI.Inventory
{
    public class Inventory : MonoBehaviour, IUiPanel
    {
        [SerializeField] private ItemSlotParent inventorySlotsParent;
        [SerializeField] private EquipSlotParent equipSlotParent;
        [SerializeField] private TextMeshProUGUI goldText; 
        
        private CanvasGroup _canvasGroup;
        private InventoryPlayer _inventory;
        
        public CanvasGroup CanvasGroup => _canvasGroup;
        public bool ShowMenuButtons => true;
        public bool ShowBackground => true;
        public bool DisablePlayerInput => true;
        public bool HasTooltips => true;

        private void Awake()
        {
            
            _inventory = FindFirstObjectByType<InventoryPlayer>();
            _inventory.OnInventoryChanged += UpdateUi;
        }

        private void Start() => _canvasGroup = GetComponent<CanvasGroup>();

        private void UpdateUi()
        {
            inventorySlotsParent.UpdateSlots(_inventory.ItemList); 
            equipSlotParent.UpdateEquipmentSlots(_inventory.EquipList);
            goldText.text = $"{_inventory.Gold:N0}g";
        }
        
        public void OnOpened()
        {
            UpdateUi();
        }
    }
}