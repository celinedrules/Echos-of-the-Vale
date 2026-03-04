// Done
using Data.ItemData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CraftSlot : MonoBehaviour
    {
        [SerializeField] private CraftPreview craftPreview;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI itemName;
        
        private ItemData _itemToCraft;

        public void SetupButton(ItemData craftData)
        {
            _itemToCraft = craftData;
            icon.sprite = craftData.ItemIcon;
            itemName.text = craftData.ItemName;
        }

        public void UpdateCraftPreview() => craftPreview.UpdateCraftPreview(_itemToCraft);
    }
}