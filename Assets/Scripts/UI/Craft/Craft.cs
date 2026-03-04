// Done
using Data.EntityData;
using Data.ItemData;
using InventorySystem;
using UI.Common;
using UI.Inventory;
using UnityEngine;

namespace UI
{
    public class Craft : MonoBehaviour, IUiPanel
    {
        [SerializeField] private ItemSlotParent inventoryParent;
        
        private InventoryPlayer _inventory;
        private CraftPreview _craftPreview;
        private CraftSlot[] _craftSlots;
        private CraftListButton[] _craftListButtons;

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

        public void SetupCraftUi(InventoryStorage storage, BlacksmithNpcData blacksmithData)
        {
            _inventory = storage.PlayerInventory;
            _inventory.OnInventoryChanged += UpdateUi;
            UpdateUi();
            
            _craftPreview = GetComponentInChildren<CraftPreview>();
            _craftPreview.SetupCraftPreview(storage);
            
            SetupCraftListButtons(blacksmithData);
        }
        
        private void SetupCraftListButtons(BlacksmithNpcData blacksmithData)
        {
            _craftSlots = GetComponentsInChildren<CraftSlot>();

            foreach (CraftSlot slot in _craftSlots)
                slot.gameObject.SetActive(false);

            _craftListButtons = GetComponentsInChildren<CraftListButton>();

            // Map each button to its recipe array by index order matching the hierarchy
            ItemData[][] recipeGroups =
            {
                blacksmithData.WeaponRecipes,
                blacksmithData.ArmorRecipes,
                blacksmithData.AccessoryRecipes,
                // Add ConsumableRecipes here when ready
            };

            for (int i = 0; i < _craftListButtons.Length; i++)
            {
                _craftListButtons[i].SetCraftsSlots(_craftSlots);

                if (i < recipeGroups.Length)
                    _craftListButtons[i].SetCraftRecipes(recipeGroups[i]);
            }
        }

        private void UpdateUi() => inventoryParent.UpdateSlots(_inventory.ItemList);
        
        public void OnOpened()
        {
            
        }
    }
}