// Done
using InventorySystem;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Enums;

namespace UI.Inventory
{
    public class ItemSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Slot Setup")]
        [SerializeField] protected Sprite defaultSprite;
        [SerializeField] protected GameObject defaultIcon;
        [SerializeField] protected Image icon;
        [SerializeField] protected TextMeshProUGUI stackCount;

        protected InventoryPlayer Inventory;
        protected RectTransform Rect;

        public InventoryItem ItemInSlot { get; set; }

        protected virtual void Awake()
        {
            Inventory = FindFirstObjectByType<InventoryPlayer>();
            Rect = GetComponent<RectTransform>();
        }

        public void UpdateSlot(InventoryItem item)
        {
            ItemInSlot = item;
            if(defaultIcon)
                defaultIcon.SetActive(ItemInSlot == null);

            if (ItemInSlot == null)
            {
                stackCount.text = "";
                icon.sprite = defaultSprite;
                return;
            }
            
            Color color = Color.white;
            color.a = 0.95f;
            icon.color = color;
            icon.sprite = ItemInSlot.ItemData.ItemIcon;
            stackCount.text = item.StackSize > 1 ? ItemInSlot.StackSize.ToString() : "";
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (ItemInSlot == null)
                return;

            bool alternativeInput = Input.GetKey(KeyCode.LeftControl);
            
            if (alternativeInput)
            {
                Inventory.RemoveOneItem(ItemInSlot);
            }
            else
            {
                if(ItemInSlot.ItemData.ItemType == ItemType.Material)
                    return;

                if (ItemInSlot.ItemData.ItemType == ItemType.Consumable)
                {
                    Inventory.TryUseItem(ItemInSlot);
                }
                else
                {
                    Inventory.TryEquipItem(ItemInSlot);
                }
            }

            if (ItemInSlot == null)
                UiManager.Instance.ItemTooltip.ShowTooltip(false, null);
            else
                UiManager.Instance.ItemTooltip.ShowTooltip(true, Rect, ItemInSlot);
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (ItemInSlot == null || ItemInSlot.ItemData == null)
                return;

            UiManager.Instance.ItemTooltip.ShowTooltip(true, Rect, ItemInSlot);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UiManager.Instance.ItemTooltip.ShowTooltip(false, null);
        }
    }
}