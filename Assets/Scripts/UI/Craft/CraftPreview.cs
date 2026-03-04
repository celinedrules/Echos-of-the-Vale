// Done
using Data.ItemData;
using InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CraftPreview : MonoBehaviour
    {
        [Header("Item Preview")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI itemInfo;
        [SerializeField] private TextMeshProUGUI buttonText;

        private InventoryItem _itemToCraft;
        private InventoryStorage _storage;
        private CraftPreviewSlot[] _craftPreviewSlots;

        public void SetupCraftPreview(InventoryStorage storage)
        {
            _storage = storage;
            _craftPreviewSlots = GetComponentsInChildren<CraftPreviewSlot>();

            foreach (CraftPreviewSlot slot in _craftPreviewSlots)
                slot.gameObject.SetActive(false);
        }
        
        public void UpdateCraftPreview(ItemData itemData)
        {
            _itemToCraft = new InventoryItem(itemData);

            icon.sprite = itemData.ItemIcon;
            itemName.text = itemData.ItemName;
            itemInfo.text = _itemToCraft.GetItemInfo();

            UpdateCraftPreviewSlots();
        }

        private void UpdateCraftPreviewSlots()
        {
            foreach (CraftPreviewSlot slot in _craftPreviewSlots)
                slot.gameObject.SetActive(false);
            
            for (int i = 0; i < _itemToCraft.ItemData.CraftRecipe.Length; i++)
            {
                InventoryItem requiredItem = _itemToCraft.ItemData.CraftRecipe[i];
                int availableAmount = _storage.GetAvailableAmountOf(requiredItem.ItemData);
                int requiredAmount = requiredItem.StackSize;
                
                _craftPreviewSlots[i].gameObject.SetActive(true);
                _craftPreviewSlots[i].SetupPreviewSlot(requiredItem.ItemData, availableAmount, requiredAmount);
            }
        }

        public void ConfirmCraft()
        {
            if(_itemToCraft == null)
            {
                buttonText.text = "Pick an Item!";
                return;
            }

            if (_storage.CanCraftItem(_itemToCraft))
                _storage.CraftItem(_itemToCraft);
            
            UpdateCraftPreviewSlots();
        }
    }
}