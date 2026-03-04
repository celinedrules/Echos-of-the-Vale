// Done
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Stats.Groups
{
    [Serializable, InlineProperty, HideLabel]
    [GUIColor(1, 1, 1)]
    public class DefenseStats
    {
        [SerializeField]
        [FoldoutGroup("Armor"), Indent(1)]
        private Stat armor = new(0);

        [SerializeField]
        [FoldoutGroup("Evasion"), Indent(1)]
        private Stat evasion = new(0);

        [SerializeField]
        [FoldoutGroup("Fire Resistance"), Indent(1)]
        private Stat fireResistance = new(0);

        [SerializeField]
        [FoldoutGroup("Ice Resistance"), Indent(1)]
        private Stat iceResistance = new(0);

        [SerializeField]
        [FoldoutGroup("Lightning Resistance"), Indent(1)]
        private Stat lightningResistance = new(0);

        public Stat Armor
        {
            get => armor;
            set => armor = value;
        }
        public Stat Evasion
        {
            get => evasion;
            set => evasion = value;
        }
        public Stat FireResistance
        {
            get => fireResistance;
            set => fireResistance = value;
        }
        public Stat IceResistance
        {
            get => iceResistance;
            set => iceResistance = value;
        }
        public Stat LightningResistance
        {
            get => lightningResistance;
            set => lightningResistance = value;
        }

        public void Reset()
        {
            Armor.Value = 0;
            Evasion.Value = 0;
            FireResistance.Value = 0;
            IceResistance.Value = 0;
            LightningResistance.Value = 0;
        }
    }
}