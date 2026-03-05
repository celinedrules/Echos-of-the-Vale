// Done
using InventorySystem;
using TMPro;
using UnityEngine;

namespace UI.Common
{
    public class ItemTooltip : Tooltip
    {
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI itemType;
        [SerializeField] private TextMeshProUGUI itemInfo;
        [SerializeField] private TextMeshProUGUI itemPrice;
        [SerializeField] private Transform merchantInfo;
        [SerializeField] private Transform inventoryInfo;
        
        public void ShowTooltip(bool show, RectTransform targetRect, InventoryItem item, bool buyPrice = false, bool showMerchantInfo = false, bool showControls = true)
        {
            base.ShowTooltip(show, targetRect);
            
            if(showControls)
            {
                merchantInfo.gameObject.SetActive(showMerchantInfo);
                inventoryInfo.gameObject.SetActive(!showMerchantInfo);
            }
            else
            {
                merchantInfo.gameObject.SetActive(false);
                inventoryInfo.gameObject.SetActive(false);
            }
            
            int price = buyPrice ? item.BuyPrice : Mathf.FloorToInt(item.SellPrice);
            int totalPrice = price * item.StackSize;
            
            string fullStackPrice = $"Price: {price} x {item.StackSize} = {totalPrice}g.";
            string singleStackPrice = $"Price: {price}g.";

            if (item.ItemData == null)
            {
                int i = 0;
            }
            
            itemPrice.text = item.StackSize > 1 ? fullStackPrice : singleStackPrice;
            itemType.text = item.ItemData.ItemType.ToString();
            itemInfo.text = item.GetItemInfo();
            
            Color color = GetColorByRarity(item.ItemData.Rarity);
            itemName.text = GetColoredString(item.ItemData.ItemName, color);
        }

        private Color GetColorByRarity(int rarity)
        {
            return rarity switch
            {
                <= 100 => Color.white,
                <= 300 => Color.green,
                <= 600 => Color.blue,
                <= 850 => Color.purple,
                _ => Color.orange
            };
        }
    }
}