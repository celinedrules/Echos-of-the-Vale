using UnityEngine;

namespace Data.StatsData
{
    [CreateAssetMenu(fileName = "Stats Runtime Data", menuName = "Echos of the Vale/Stat Data/Stats Runtime Data", order = 0)]
    public class StatsRuntimeData : ScriptableObject
    {
        [SerializeField] private int currentHealth;
        [SerializeField] private int healthRegen;
        [SerializeField] private int attackSpeed;
        [SerializeField] private int damage;
        [SerializeField] private int criticalPower;
        [SerializeField] private int criticalChance;
        [SerializeField] private int armorReduction;
        [SerializeField] private int fireDamage;
        [SerializeField] private int iceDamage;
        [SerializeField] private int lightningDamage;
        [SerializeField] private int armor;
        [SerializeField] private int evasion;
        [SerializeField] private int fireResistance;
        [SerializeField] private int iceResistance;
        [SerializeField] private int lightningResistance;

        [SerializeField] private bool hasValidData;

        public int CurrentHealth
        {
            get => currentHealth;
            set => currentHealth = value;
        }

        public int HealthRegen
        {
            get => healthRegen;
            set => healthRegen = value;
        }

        public int AttackSpeed
        {
            get => attackSpeed;
            set => attackSpeed = value;
        }

        public int ArmorReduction
        {
            get => armorReduction;
            set => armorReduction = value;
        }

        public int FireDamage
        {
            get => fireDamage;
            set => fireDamage = value;
        }

        public int IceDamage
        {
            get => iceDamage;
            set => iceDamage = value;
        }

        public int LightningDamage
        {
            get => lightningDamage;
            set => lightningDamage = value;
        }

        public bool HasValidData
        {
            get => hasValidData;
            set => hasValidData = value;
        }

        public void ResetToDefaults()
        {
            hasValidData = false;
            CurrentHealth = 0;
            HealthRegen = 0;
            AttackSpeed = 0;
            ArmorReduction = 0;
            FireDamage = 0;
            IceDamage = 0;
            LightningDamage = 0;
        }

#if UNITY_EDITOR
        private void OnEnable() => UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        private void OnDisable() => UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                ResetToDefaults();
        }
#endif
    }
}