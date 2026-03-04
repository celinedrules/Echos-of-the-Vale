// Done
using Data.ItemData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CraftPreviewSlot : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameValue;

        public void SetupPreviewSlot(ItemData itemData, int availableAmount, int requiredAmount)
        {
            icon.sprite = itemData.ItemIcon;
            nameValue.text = $"{itemData.ItemName} x{availableAmount}/{requiredAmount}";
        }
    }
}