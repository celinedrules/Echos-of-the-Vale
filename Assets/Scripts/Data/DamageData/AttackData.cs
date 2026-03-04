// Done
using System;
using Stats;
using Utilities.Enums;

namespace Data.DamageData
{
    [Serializable]
    public class AttackData
    {
        public int PhysicalDamage { get; set; }
        public int ElementalDamage { get; set; }
        public bool IsCritical { get; set; }
        public ElementType ElementType { get; set; }
        public ElementalEffectData EffectData { get; set; }

        public AttackData(EntityStats stats, DamageScaleData scaleData)
        {
            PhysicalDamage  = stats.GetPhysicalDamage(out bool critical, scaleData.Physical);
            ElementalDamage = stats.GetElementalDamage(out ElementType elementType, scaleData.Elemental);
            IsCritical = critical;
            ElementType = elementType;
            EffectData = new ElementalEffectData(stats, scaleData);
        }
    }
}