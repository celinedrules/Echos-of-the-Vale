// Done
using System;
using Data.DamageData;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Enums;

namespace Data.SkillData
{
    [CreateAssetMenu(fileName = "Skill Data", menuName = "Echos of the Vale/Skill Data/Skills Data", order = 0)]
    public class SkillData : ScriptableObject
    {
        [BoxGroup("Skill Info")]
        [SerializeField] private SkillType skillType;

        [BoxGroup("Skill Info")]
        [SerializeField] private string displayName;

        [BoxGroup("Skill Info")]
        [SerializeField, Multiline(3)] private string description;

        [BoxGroup("Skill Info")]
        [SerializeField, PreviewField(64, ObjectFieldAlignment.Left)]
        private Sprite icon;

        [BoxGroup("Unlock & Upgrade")]
        [SerializeField] private bool unlockedByDefault;
        [BoxGroup("Unlock & Upgrade")]
        [SerializeField] private int cost;
        [BoxGroup("Unlock & Upgrade")]
        [SerializeField] private SkillUpgradeData upgradeData;
        
        public bool UnlockedByDefault => unlockedByDefault;
        public SkillType SkillType => skillType;
        public SkillUpgradeData UpgradeData => upgradeData;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public int Cost => cost;
    }

    [Serializable]
    public class SkillUpgradeData
    {
        [SerializeField] private SkillUpgradeType upgradeType;
        [SerializeField] private float cooldown;
        [FormerlySerializedAs("damageScale")] [SerializeField] private DamageScaleData damageScaleData;

        public SkillUpgradeType UpgradeType => upgradeType;
        public float Cooldown => cooldown;
        public DamageScaleData DamageScaleData => damageScaleData;
    }
}