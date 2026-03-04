// Done
using System;
using System.Reflection;
using Data.DamageData;
using Stats.Groups;
using UnityEngine;
using Utilities.Enums;

namespace Stats
{
    public class EntityStats
    {
        protected const int EvasionCap = 85;
        protected const float MitigationCap = 0.85f;
        protected int ResistanceCap = 75;
        protected virtual int ElementalBonus => 0;
        private ResourceStats resourceStats = new();
        private OffenseStats offenseStats = new();
        private DefenseStats defenseStats = new();

        public ResourceStats ResourceStats { get => resourceStats; set => resourceStats = value; }
        public OffenseStats OffenseStats { get => offenseStats; set => offenseStats = value; }
        public DefenseStats DefenseStats { get => defenseStats; set => defenseStats = value; }
        
        protected virtual void Awake()
        {
            
        }
        
        
        public virtual void ResetStats()
        {
            ResourceStats.Reset();
            ClearModifiers(ResourceStats);
            OffenseStats.Reset();
            ClearModifiers(OffenseStats);
            DefenseStats.Reset();
            ClearModifiers(DefenseStats);
            ClearBuffs();
        }

        private void ClearBuffs()
        {
            if (GetType() == typeof(PlayerStats))
            {
                ((PlayerStats)this).ClearBuffs();
            }
                
        }
        
        protected void ClearModifiers(object target)
        {
            if (target == null) return;

            Type type = target.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType != typeof(Stat))
                    continue;
                
                Stat stat = (Stat)field.GetValue(target);
                stat?.ClearModifiers();
            }
        }
        
        public AttackData GeAttackData(DamageScaleData scaleData) => new(this, scaleData);

        public virtual int GetElementalDamage(out ElementType elementType, float scaleFactor = 1)
        {
            int fireDamage = OffenseStats.FireDamage.Value;
            int iceDamage = OffenseStats.IceDamage.Value;
            int lightningDamage = OffenseStats.LightningDamage.Value;

            int highestDamage = Mathf.Max(fireDamage, Mathf.Max(iceDamage, lightningDamage));
            elementType = ElementType.Fire;

            if(iceDamage >= highestDamage)
            {
                highestDamage = iceDamage;
                elementType = ElementType.Ice;
            }
            
            if(lightningDamage >= highestDamage)
            {
                highestDamage = lightningDamage;
                elementType = ElementType.Lightning;
            }
            
            
            if (highestDamage <= 0)
            {
                elementType = ElementType.None;
                return 0;
            }
            
            float bonusFire = (elementType == ElementType.Fire) ? 0 : fireDamage * 0.5f;
            float bonusIce = (elementType == ElementType.Ice) ? 0 : iceDamage * 0.5f;
            float bonusLightning = (elementType == ElementType.Lightning ) ? 0 : lightningDamage * 0.5f;
            float weakerElementDamage = bonusFire + bonusIce + bonusLightning;
            int additionalBonus = ElementalBonus;
            int finalDamage = Mathf.RoundToInt((highestDamage + weakerElementDamage + additionalBonus) * scaleFactor);
            
            return finalDamage;
        }

        public virtual float GetElementalResistance(ElementType elementType)
        {
            float bonusResistance = 0;

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
        
        public virtual float GetArmorReduction()
        {
            float finalReduction = OffenseStats.ArmorReduction.Value / 100.0f;
            return finalReduction;
        }
        
        public virtual float GetArmorMitigation(float armorReduction)
        {
            int baseArmor = DefenseStats.Armor.Value;
            float reductionMultiplier = ClampArmorReduction(1 - armorReduction);
            float effectiveArmor = baseArmor * reductionMultiplier;
            return effectiveArmor;
        }
        
        public virtual int GetMaxHealth()
        {
            int baseHealth = ResourceStats.MaxHealth.Value;
            return baseHealth;
        }

        public virtual int GetEvasion()
        {
            int baseEvastion = DefenseStats.Evasion.Value;
            return ClampEvasion(baseEvastion);
        }
        
        public virtual int GetPhysicalDamage(out bool critical, float scaleFactor = 1)
        {
            int baseDamage = OffenseStats.Damage.Value;
            critical = false;
            return baseDamage * Mathf.RoundToInt(scaleFactor);
        }
        
        public virtual Stat GetStatByType(StatType statType)
        {
            return statType switch
            {
                StatType.MaxHealth => ResourceStats.MaxHealth,
                StatType.HealthRegen => ResourceStats.HealthRegen,
                StatType.AttackSpeed => OffenseStats.AttackSpeed,
                StatType.Damage => OffenseStats.Damage,
                StatType.CriticalChance => OffenseStats.CriticalChance,
                StatType.CriticalPower => OffenseStats.CriticalPower,
                StatType.ArmorReduction => OffenseStats.ArmorReduction,
                StatType.FireDamage => OffenseStats.FireDamage,
                StatType.IceDamage => OffenseStats.IceDamage,
                StatType.LightningDamage => OffenseStats.LightningDamage,
                StatType.Armor => DefenseStats.Armor,
                StatType.Evasion => DefenseStats.Evasion,
                StatType.FireResistance => DefenseStats.FireResistance,
                StatType.IceResistance => DefenseStats.IceResistance,
                StatType.LightningResistance => DefenseStats.LightningResistance,
                _ => null
            };
        }
        
        protected int ClampEvasion(int value) => Mathf.Clamp(value, 0, EvasionCap);
        protected float ClampMitigation(float value) => Mathf.Clamp(value, 0, MitigationCap);
        protected float ClampArmorReduction(float value) => Mathf.Clamp(value, 0, 1);
        protected float ClampResistance(float value) => Mathf.Clamp(value, 0, ResistanceCap);
    }
}