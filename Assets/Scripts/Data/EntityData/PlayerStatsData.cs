// Done
using System;
using Sirenix.OdinInspector;
using Stats.Groups;
using UnityEngine;

namespace Data.EntityData
{
    [Serializable]
    public class PlayerStatsData : EntityStatsData
    {
        [HideInInspector]
        [SerializeField] private MajorStats majorStats;
        
        [BoxGroup("Major")]
        [ProgressBar(0, 100), ShowInInspector]
        public int Strength
        {
            get => majorStats.Strength.Value;
            set => majorStats.Strength.Value = value;
        }
        
        [BoxGroup("Major")]
        [ProgressBar(0, 100), ShowInInspector]
        public int Agility
        {
            get => majorStats.Agility.Value;
            set => majorStats.Agility.Value = value;
        }
        
        [BoxGroup("Major")]
        [ProgressBar(0, 100), ShowInInspector]
        public int Intelligence
        {
            get => majorStats.Intelligence.Value;
            set => majorStats.Intelligence.Value = value;
        }
        
        [BoxGroup("Major")]
        [ProgressBar(0, 100), ShowInInspector]
        public int Vitality
        {
            get => majorStats.Vitality.Value;
            set => majorStats.Vitality.Value = value;
        }
    }
}