// Done
using Data.DamageData;
using Data.SkillData;
using Managers;
using Player;
using UnityEngine;
using Utilities.Enums;

namespace SkillSystem.Core
{
    public class SkillBase : MonoBehaviour
    {
        [Header("General Details")]
        [SerializeField] protected SkillType skillType;
        [SerializeField] protected SkillUpgradeType upgradeType;
        [SerializeField] protected float cooldown;
        
        private float _lastTimeUsed;

        public SkillType SkillType => skillType;
        public SkillUpgradeType SkillUpgradeType
        {
            get => upgradeType;
            set => upgradeType = value;
        }
        public PlayerController Player { get; private set; }
        public DamageScaleData DamageScaleData { get; private set; }
        
        protected virtual void Awake()
        {
            Player = GetComponentInParent<PlayerController>();
            _lastTimeUsed -= cooldown;
            DamageScaleData = new DamageScaleData();
        }

        public virtual void TryUseSkill()
        {
            
        }
        
        protected bool OnCooldown() => Time.time < _lastTimeUsed + cooldown;
        
        public void SetSkillOnCooldown()
        {
            UiManager.Instance.Hud.GetSkillSlot(skillType).StartCooldown(cooldown);
            _lastTimeUsed = Time.time;
        }

        public void ReduceCooldownBy(float cooldownReduction) => _lastTimeUsed -= cooldownReduction;
        public void ResetCooldown()
        {
            UiManager.Instance?.Hud?.GetSkillSlot(skillType)?.ResetCooldown();
            _lastTimeUsed = Time.time - cooldown;
        }

        protected bool Unlocked(SkillUpgradeType upgradeToCheck) => upgradeType == upgradeToCheck;
        
        public virtual bool CanUseSkill()
        {
            if (upgradeType == SkillUpgradeType.None)
                return false;
            
            return !OnCooldown();
        }

        public void UpgradeSkill(SkillData skillData)
        {
            SkillUpgradeData upgradeData = skillData.UpgradeData;
            upgradeType = upgradeData.UpgradeType;
            cooldown = upgradeData.Cooldown;
            DamageScaleData = upgradeData.DamageScaleData;
            
            if (UiManager.Instance != null && UiManager.Instance.Hud != null)
            {
                UiManager.Instance.Hud.GetSkillSlot(skillType)?.SetupSkillSlot(skillData);
            }
            
            ResetCooldown();
        }
    }
}