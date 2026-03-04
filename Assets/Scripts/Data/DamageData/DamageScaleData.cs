// Done
using System;
using UnityEngine;

namespace Data.DamageData
{
    [Serializable]
    public class DamageScaleData
    {
        [field: SerializeField, Header("Damage")]
        public float Physical { get; set; } = 1.0f;
        [field: SerializeField]
        public float Elemental { get; set; } = 1.0f;
        
        [field: SerializeField, Header("Chill")]
        public float ChillDuration { get; set; } = 3.0f;
        [field: SerializeField]
        public float ChillSlowMultiplier { get; set; } = 0.2f;
        
        [field: SerializeField, Header("Burn")]
        public float BurnDuration { get; set; } = 3.0f;
        [field: SerializeField]
        public float BurnDamageScale { get; set; } = 1.0f;
        
        [field: SerializeField, Header("Shock")]
        public float ShockDuration { get; set; } = 3.0f;
        [field: SerializeField]
        public float ShockDamageScale { get; set; } = 1.0f;
        [field: SerializeField]
        public float ShockCharge { get; set; } = 0.4f;
    }
}