// Done
using System;
using Stats;
using UnityEngine;

namespace Data.DamageData
{
    [Serializable]
    public class ElementalEffectData
    {
        [field:SerializeField] public float ChillDuration { get; private set;}
        [field:SerializeField] public float ChillSlowMultiplier { get; private set; }
        [field:SerializeField] public float BurnDuration { get; private set; }
        [field:SerializeField] public float BurnDamage { get; private set; }
        [field:SerializeField] public float ShockDuration { get; private set; }
        [field:SerializeField] public float ShockDamage { get; private set; }
        [field:SerializeField] public float ShockCharge { get; private set; }

        public ElementalEffectData(EntityStats stats, DamageScaleData damageScale)
        {
            ChillDuration = damageScale.ChillDuration;
            ChillSlowMultiplier = damageScale.ChillSlowMultiplier;

            BurnDuration = damageScale.BurnDuration;
            BurnDamage = stats.OffenseStats.FireDamage.Value * damageScale.BurnDamageScale;
            
            ShockDuration = damageScale.ShockDuration;
            ShockDamage = stats.OffenseStats.LightningDamage.Value * damageScale.ShockDamageScale;
            ShockCharge = damageScale.ShockCharge;
        }
    }
}