// Done
using System;
using Sirenix.OdinInspector;
using Stats.Groups;
using UnityEngine;

namespace Data.EntityData
{
    [Serializable]
    public class EntityStatsData
    {
        [HideInInspector]
        [SerializeField] private ResourceStats resourceStats = new();
        
        [HideInInspector]
        [SerializeField] private OffenseStats offenseStats = new();
        
        [HideInInspector]
        [SerializeField] private DefenseStats defenseStats = new();

        [ProgressBar(0, 100), ShowInInspector]
        [BoxGroup("Health")]
        public int MaxHealth
        {
            get => resourceStats.MaxHealth.Value;
            set => resourceStats.MaxHealth.Value = value;
        }
        
        [BoxGroup("Health")]
        [ProgressBar(0, 100), ShowInInspector]
        public int HealthRegen
        {
            get => resourceStats.HealthRegen.Value;
            set => resourceStats.HealthRegen.Value = value;
        }
        
        [BoxGroup("Attack")]
        [ProgressBar(0, 100), ShowInInspector]
        public int AttackSpeed
        {
            get => offenseStats.AttackSpeed.Value;
            set => offenseStats.AttackSpeed.Value = value;
        }
        
        [BoxGroup("Attack")]
        [ProgressBar(0, 100), ShowInInspector]
        public int Damage
        {
            get => offenseStats.Damage.Value;
            set => offenseStats.Damage.Value = value;
        }
        
        [BoxGroup("Defense")]
        [ProgressBar(0, 100), ShowInInspector]
        public int Armor
        {
            get => defenseStats.Armor.Value;
            set => defenseStats.Armor.Value = value;
        }
        
        public ResourceStats ResourceStats => resourceStats;
    }
}