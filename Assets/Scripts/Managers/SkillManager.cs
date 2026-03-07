using System;
using System.Linq;
using System.Reflection;
using SkillSystem.Core;
using UI.SkillTree.Core;
using UnityEngine;
using Utilities;
using Utilities.Enums;

namespace Managers
{
    public class SkillManager : Singleton<SkillManager>
    {
        public event Action<int> OnSkillPointsChanged;

        [SerializeField] private int skillPoints;
        public int SkillPoints
        {
            get => skillPoints;
            set => skillPoints = value;
        }

        [SerializeField] private SkillTree skillTree;
        [SerializeField] private GameObject skills;

        public SkillBase[] AllSkills { get; private set; }
        public SkillTree SkillTree => skillTree;
        // public SkillDash Dash { get; private set; }
        // public SkillShard Shard { get; private set; }
        // public SkillSwordThrow SwordThrow { get; private set; }
        // public SkillTimeEcho TimeEcho { get; private set; }
        // public SkillDomainExpansion DomainExpansion { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            AllSkills = skills.GetComponents<SkillBase>();

            // Dash = GetSkill<SkillDash>();
            // Shard = GetSkill<SkillShard>();
            // SwordThrow = GetSkill<SkillSwordThrow>();
            // TimeEcho = GetSkill<SkillTimeEcho>();
            // DomainExpansion = GetSkill<SkillDomainExpansion>();
        }
        
        public SkillBase GetSkillByType(SkillType type)
        {
            switch (type)
            {
                // case SkillType.Dash:
                //     return Dash;
                // case SkillType.TimeEcho:
                //     return TimeEcho;
                // case SkillType.TimeShard:
                //     return Shard;
                // case SkillType.SwordThrow:
                //     return SwordThrow;
                // case SkillType.DomainExpansion:
                //     return DomainExpansion;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void ReduceAllSkillsCooldown(float amount)
        {
            foreach (SkillBase skill in AllSkills)
                skill.ReduceCooldownBy(amount);
        }

        private T GetSkill<T>() where T : SkillBase => AllSkills.OfType<T>().FirstOrDefault();

        private SkillBase[] GetAllSkills()
        {
            return GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => typeof(SkillBase).IsAssignableFrom(f.FieldType))
                .Select(f => (SkillBase)f.GetValue(this))
                .Where(skill => skill != null)
                .ToArray();
        }

        public void SetSkillPoints(int amount)
        {
            skillPoints = amount;
            OnSkillPointsChanged?.Invoke(skillPoints);
        }
        
        public void AddSkillPoints(int amount)
        {
            skillPoints += amount;
            OnSkillPointsChanged?.Invoke(skillPoints);
        }

        public void RemoveSkillPoints(int amount)
        {
            skillPoints -= amount;
            OnSkillPointsChanged?.Invoke(skillPoints);
        }
    }
}