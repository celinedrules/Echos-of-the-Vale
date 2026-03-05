// Done
using System;
using System.Collections.Generic;
using Core.Attributes;
using Data.ItemEffects;
using Data.StatsData;
using Managers;
using Player;
using Sirenix.OdinInspector;
using Stats.Groups;
using UnityEngine;
using Utilities.Enums;
using Random = UnityEngine.Random;

namespace Stats
{
    public class PlayerStats : EntityStats
    {
        public event Action OnStatsChanged;

        private List<string> _activeBuffs = new();

        protected override int ElementalBonus => MajorStats.Intelligence.Value;

        [ColorFoldoutGroup("Major", .5f, .5f, 0f), Indent(1)]
        [SerializeField] private MajorStats majorStats = new();

        public MajorStats MajorStats
        {
            get => majorStats;
            set => majorStats = value;
        }

        public override void ResetStats()
        {
            base.ResetStats();
            MajorStats.Reset();
            ClearModifiers(MajorStats);
        }

        public float GetStatValue(StatType type)
        {
            return type switch
            {
                StatType.MaxHealth => ResourceStats.MaxHealth.Value,
                StatType.HealthRegen => ResourceStats.HealthRegen.Value,
                StatType.Strength => MajorStats.Strength.Value,
                StatType.Agility => MajorStats.Agility.Value,
                StatType.Intelligence => MajorStats.Intelligence.Value,
                StatType.Vitality => MajorStats.Vitality.Value,
                StatType.AttackSpeed => OffenseStats.AttackSpeed.Value, // * 100,
                StatType.Damage => GetBaseDamage(),
                StatType.CriticalChance => GetCriticalChance(),
                StatType.CriticalPower => GetCriticalPower(),
                StatType.ArmorReduction => OffenseStats.ArmorReduction.Value * 100,
                StatType.FireDamage => OffenseStats.FireDamage.Value,
                StatType.IceDamage => OffenseStats.IceDamage.Value,
                StatType.LightningDamage => OffenseStats.LightningDamage.Value,
                StatType.ElementalDamage => GetElementalDamage(out ElementType elementType, 1),
                StatType.Armor => GetBaseArmor(),
                StatType.Evasion => DefenseStats.Evasion.Value,
                StatType.FireResistance => GetElementalResistance(ElementType.Fire) * 100,
                StatType.IceResistance => GetElementalResistance(ElementType.Ice) * 100,
                StatType.LightningResistance => GetElementalResistance(ElementType.Lightning) * 100,
                _ => 0
            };
        }

        public override float GetElementalResistance(ElementType elementType)
        {
            float bonusResistance = MajorStats.Intelligence.Value * 0.5f;

            float baseResistance = elementType switch
            {
                ElementType.None => 0,
                ElementType.Fire => DefenseStats.FireResistance.Value,
                ElementType.Ice => DefenseStats.IceResistance.Value,
                ElementType.Lightning => DefenseStats.LightningResistance.Value,
                _ => throw new ArgumentOutOfRangeException(nameof(elementType), elementType, null)
            };

            float resistance = baseResistance + bonusResistance;
            float finalResistance = ClampResistance(resistance) / 100.0f;
            return finalResistance;
        }

        public override float GetArmorMitigation(float armorReduction)
        {
            int totalArmor = GetBaseArmor();
            float reductionMultiplier = ClampArmorReduction(1 - armorReduction);
            float effectiveArmor = totalArmor * reductionMultiplier;
            float mitigation = effectiveArmor / (effectiveArmor + 100f);
            return ClampMitigation(mitigation);
        }

        public int GetBaseArmor() => DefenseStats.Armor.Value + MajorStats.Vitality.Value;

        public override int GetMaxHealth()
        {
            int baseMaxHealth = base.GetMaxHealth();
            int bonusMaxHealth = MajorStats.Vitality.Value * 5;
            int finalMaxHealth = baseMaxHealth + bonusMaxHealth;
            return finalMaxHealth;
        }

        public override int GetEvasion()
        {
            int baseEvasion = DefenseStats.Evasion.Value;
            float bonusEvasion = MajorStats.Agility.Value * 0.5f;
            int totalEvasion = baseEvasion + Mathf.RoundToInt(bonusEvasion);
            return ClampEvasion(totalEvasion);
        }

        public override int GetPhysicalDamage(out bool critical, float scaleFactor = 1)
        {
            int baseDamage = GetBaseDamage();
            float criticalChance = GetCriticalChance();
            float criticalPower = GetCriticalPower();

            critical = Random.Range(0f, 100f) < criticalChance;
            float finalDamage = critical ? baseDamage * criticalPower : baseDamage;

            return Mathf.RoundToInt(finalDamage * scaleFactor);
        }

        public int GetBaseDamage() => OffenseStats.Damage.Value + MajorStats.Strength.Value;
        public float GetCriticalChance() => OffenseStats.CriticalChance.Value + MajorStats.Agility.Value * 0.3f;
        public float GetCriticalPower() => OffenseStats.CriticalPower.Value + MajorStats.Strength.Value * 0.5f;

        public override Stat GetStatByType(StatType statType)
        {
            return statType switch
            {
                StatType.Strength => MajorStats.Strength,
                StatType.Agility => MajorStats.Agility,
                StatType.Intelligence => MajorStats.Intelligence,
                StatType.Vitality => MajorStats.Vitality,
                _ => base.GetStatByType(statType)
            };
        }

        public bool CanApplyBuff(string source) => !_activeBuffs.Contains(source);

        public void ApplyBuff(BuffEffectData[] buffsToApply, float duration, string source)
        {
            _activeBuffs.Add(source);

            foreach (BuffEffectData buffEffectData in buffsToApply)
                GetStatByType(buffEffectData.type).AddModifier(source, buffEffectData.value);

            OnStatsChanged?.Invoke();

            Debug.Log($"Buff {source} applied for {duration} seconds");
            TimerManager.Instance.CreateTimer(duration, () =>
            {
                Debug.Log($"Buff {source} expired");
                foreach (BuffEffectData buffEffectData in buffsToApply)
                    GetStatByType(buffEffectData.type).RemoveModifier(source);

                _activeBuffs.Remove(source);
                OnStatsChanged?.Invoke();
            }, useUnscaledTime: true);
        }

        public void ClearBuffs()
        {
            _activeBuffs.Clear();
        }

        public void SaveToRuntimeData()
        {
            Debug.Log("Saving Stats");
            PlayerController player = GameManager.Instance.Player;
        
            if (!player)
                return;
        
            StatsRuntimeData runtimeData = GameManager.Instance.StatsRuntimeData;
            runtimeData.CurrentHealth = player.Health.CurrentHealth;
            runtimeData.HealthRegen = player.Stats.ResourceStats.HealthRegen.Value;
            runtimeData.AttackSpeed = player.Stats.OffenseStats.AttackSpeed.Value;
            runtimeData.ArmorReduction = player.Stats.OffenseStats.ArmorReduction.Value;
            runtimeData.FireDamage = player.Stats.OffenseStats.FireDamage.Value;
            runtimeData.IceDamage = player.Stats.OffenseStats.IceDamage.Value;
            runtimeData.LightningDamage = player.Stats.OffenseStats.LightningDamage.Value;
            runtimeData.HasValidData = true;
        }

        public void LoadFromRuntimeData()
        {
            StatsRuntimeData runtimeData = GameManager.Instance.StatsRuntimeData;
            
            if (!runtimeData.HasValidData)
                return;
            
            Debug.Log("Loading Stats");
            
            PlayerController player = GameManager.Instance.Player;
            
            if (!player)
                return;
            
            player.Health.CurrentHealth = runtimeData.CurrentHealth;
            player.Stats.ResourceStats.HealthRegen.Value = runtimeData.HealthRegen;
            player.Stats.OffenseStats.AttackSpeed.Value = runtimeData.AttackSpeed;
            player.Stats.OffenseStats.ArmorReduction.Value = runtimeData.ArmorReduction;
            player.Stats.OffenseStats.FireDamage.Value = runtimeData.FireDamage;
            player.Stats.OffenseStats.IceDamage.Value = runtimeData.IceDamage;
            player.Stats.OffenseStats.LightningDamage.Value = runtimeData.LightningDamage;
            
            player.Health.NotifyHealthChanged();
        }
    }
}