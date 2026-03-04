// Done
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Stats.Groups
{
    [Serializable, InlineProperty, HideLabel]
    [GUIColor(1, 1, 1)]
    public class ResourceStats
    {
        [SerializeField]
        [FoldoutGroup("Max Health"), Indent(1)]
        private Stat maxHealth =  new(100);

        [SerializeField]
        [FoldoutGroup("Health Regen"), Indent(1)]
        private Stat healthRegen = new(0);

        public Stat MaxHealth { get => maxHealth; set => maxHealth = value; }
        public Stat HealthRegen { get => healthRegen; set => healthRegen = value; }

        public void Reset()
        {
            MaxHealth.Value = 100;
            HealthRegen.Value = 0;
        }
    }
}