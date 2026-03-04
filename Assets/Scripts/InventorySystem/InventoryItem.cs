using System;
using System.Text;
using Data.ItemData;
using Data.ItemEffects;
using Player;
using Sirenix.OdinInspector;
using UI.Common;
using UnityEngine;
using Utilities.Enums;

namespace InventorySystem
{
    [Serializable, InlineProperty]
    public class InventoryItem
    {
        [HorizontalGroup("Item", 55), PreviewField(50), HideLabel, ShowInInspector, PropertyOrder(-1)]
        private Sprite Preview => itemData ? itemData.ItemIcon : null;

        [HorizontalGroup("Item"), VerticalGroup("Item/Details"), SerializeField, HideLabel]
        private ItemData itemData;

        [VerticalGroup("Item/Details"), ShowInInspector, HideLabel]
        private string ItemName => itemData ? itemData.ItemName : "No Item";

        [VerticalGroup("Item/Details"), ShowInInspector, HideLabel]
        private ItemType ItemType => itemData ? itemData.ItemType : ItemType.Material;

        [VerticalGroup("Item/Details"), SerializeField, LabelText("Stack Size:"), LabelWidth(70),
         PropertyOrder(10)]
        private int _stackSize = 1;
        
        private string _itemId;
        
        public ItemData ItemData
        {
            get => itemData;
            set => itemData = value;
        }
        public int StackSize
        {
            get => _stackSize;
            set => _stackSize = value;
        }
        
        public ItemModifier[] Modifiers { get; private set;}
        public ItemEffectData ItemEffect { get; private set; }
        public int BuyPrice { get; private set; }
        public float SellPrice { get; private set; }

        public InventoryItem(ItemData data)
        {
            itemData = data;
            Modifiers = (itemData as EquipmentData)?.Modifiers;
            _itemId = $"{itemData.ItemName} - {Guid.NewGuid()}";
            ItemEffect = itemData.ItemEffect;
            BuyPrice = itemData.ItemPrice;
            SellPrice = itemData.ItemPrice * 0.35f;
        }
        
        // public void AddModifiers(EntityStats playerStats)
        // {
        //     foreach (ItemModifier modifier in Modifiers)
        //     {
        //         Stat statToModify = playerStats.GetStatByType(modifier.StatType);
        //         statToModify.AddModifier(_itemId, modifier.Value);
        //     }
        // }
        //
        // public void RemoveModifiers(EntityStats playerStats)
        // {
        //     foreach (ItemModifier modifier in Modifiers)
        //     {
        //         Stat statToModify = playerStats.GetStatByType(modifier.StatType);
        //         statToModify.RemoveModifier(_itemId);
        //     }
        // }
        
        public void AddItemEffect(PlayerController player) => ItemEffect?.Subscribe(player);
        public void RemoveItemEffect() => ItemEffect?.Unsubscribe();
        public bool CanAddToStack() => _stackSize < itemData.MaxStackSize;
        public void AddToStack() => _stackSize++;
        public void RemoveFromStack() => _stackSize--;
        
        public string GetItemInfo()
        {
            StringBuilder sb = new();
            
            if (itemData.ItemType == ItemType.Material)
            {
                sb.AppendLine("");
                sb.AppendLine("Used for crafting.");
                sb.AppendLine("");
                sb.AppendLine("");
                return sb.ToString();
            }

            if (itemData.ItemType == ItemType.Consumable)
            {
                sb.AppendLine("");
                sb.AppendLine(itemData.ItemEffect.EffectDescription);
                sb.AppendLine("");
                sb.AppendLine("");
                return sb.ToString();
            }
            
            sb.AppendLine("");

            foreach (ItemModifier modifier in Modifiers)
            {
                // ??
                string modifierType = modifier.StatType.ToString();
                string modifierValue = Utils.IsPercentageStat(modifier.StatType)
                    ? $"{modifier.Value.ToString()}%"
                    : modifier.Value.ToString();
                sb.AppendLine($"+ {modifierValue} {modifierType}");
            }

            if (ItemEffect != null)
            {
                sb.AppendLine("");
                sb.AppendLine("Effect:");
                sb.AppendLine(ItemEffect.EffectDescription);
            }
            
            sb.AppendLine("");
            sb.AppendLine("");
            return sb.ToString();
        }
    }
}