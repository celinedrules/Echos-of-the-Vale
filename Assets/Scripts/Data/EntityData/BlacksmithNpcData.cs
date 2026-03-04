// Done
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.EntityData
{
    public class BlacksmithNpcData : NpcData
    {
        [TabGroup("Craft Recipes")]
        [SerializeField, LabelText("Weapons")]
        private ItemData.ItemData[] weaponRecipes;

        [TabGroup("Craft Recipes")]
        [SerializeField, LabelText("Armor")]
        private ItemData.ItemData[] armorRecipes;

        [TabGroup("Craft Recipes")]
        [SerializeField, LabelText("Accessories")]
        private ItemData.ItemData[] accessoryRecipes;

        public ItemData.ItemData[] WeaponRecipes => weaponRecipes;
        public ItemData.ItemData[] ArmorRecipes => armorRecipes;
        public ItemData.ItemData[] AccessoryRecipes => accessoryRecipes;
    }
}