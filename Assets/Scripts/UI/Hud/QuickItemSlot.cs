// Done
using InventorySystem;
using Managers;
using UI.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Hud
{
    public class QuickItemSlot : ItemSlot
    {
        [SerializeField] private int slotNumber;

        private Button _button;

        protected override void Awake()
        {
            base.Awake();
            _button = GetComponent<Button>();
        }

        public void SetupQuickSlotItem(InventoryItem item)
        {
            Inventory.SetQuickItemInSlot(slotNumber, item);
        }

        public void SimilulateButtonFeedback()
        {
            EventSystem.current.SetSelectedGameObject(_button.gameObject);
            ExecuteEvents.Execute(_button.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }
        
        public void UpdateQuickSlot(InventoryItem currentItemInSlot)
        {
            if (currentItemInSlot == null || currentItemInSlot.ItemData == null)
            {
                icon.sprite = defaultSprite;
                stackCount.text = "";
                return;
            }
            
            icon.sprite = currentItemInSlot.ItemData.ItemIcon;
            stackCount.text = currentItemInSlot.StackSize.ToString();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            UiManager.Instance.Hud.OpenQuickItemOptions(this, Rect);
        }
    }
}