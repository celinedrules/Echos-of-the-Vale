// Done
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Stats.Groups
{
    [Serializable, InlineProperty, HideLabel]
    [GUIColor(1, 1, 1)]
    public class OffenseStats
    {
        [SerializeField]
        [FoldoutGroup("Attack Speed"), Indent(1)]
        private Stat attackSpeed = new(1);

        [SerializeField]
        [FoldoutGroup("Damage"), Indent(1)]
        private Stat damage = new(10);

        [SerializeField]
        [FoldoutGroup("Critical Power"), Indent(1)]
        private Stat criticalPower = new(0);

        [SerializeField]
        [FoldoutGroup("Critical Chance"), Indent(1)]
        private Stat criticalChance = new(0);

        [SerializeField]
        [FoldoutGroup("Armor Reduction"), Indent(1)]
        private Stat armorReduction = new(0);

        [SerializeField]
        [FoldoutGroup("Fire Damage"), Indent(1)]
        private Stat fireDamage = new(0);

        [SerializeField]
        [FoldoutGroup("Ice Damage"), Indent(1)]
        private Stat iceDamage = new(0);

        [SerializeField]
        [FoldoutGroup("Lightning Damage"), Indent(1)]
        private Stat lightningDamage = new(0);

        public Stat AttackSpeed { get => attackSpeed; set => attackSpeed = value; }
        public Stat Damage { get => damage; set => damage = value; }
        public Stat CriticalPower { get => criticalPower; set => criticalPower = value; }
        public Stat CriticalChance { get => criticalChance; set => criticalChance = value; }
        public Stat ArmorReduction { get => armorReduction; set => armorReduction = value; }
        public Stat FireDamage { get => fireDamage; set => fireDamage = value; }
        public Stat IceDamage { get => iceDamage; set => iceDamage = value; }
        public Stat LightningDamage { get => lightningDamage; set => lightningDamage = value; }

        public void Reset()
        {
            AttackSpeed.Value = 1;
            Damage.Value = 10;
            CriticalPower.Value = 0;
            CriticalChance.Value = 0;
            ArmorReduction.Value = 0;
            FireDamage.Value = 0;
            IceDamage.Value = 0;
            LightningDamage.Value = 0;
        }
    }
}