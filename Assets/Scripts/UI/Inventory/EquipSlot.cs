// Done
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Enums;

namespace UI.Inventory
{
    public class EquipSlot : ItemSlot
    {
        [SerializeField] private ItemType slotType;

        private void OnValidate()
        {
            gameObject.name = $"EquipmentSlot -  {slotType.ToString()}";
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if(ItemInSlot == null)
                return;
            
            Inventory.UnequipItem(ItemInSlot);
        }
    }
}