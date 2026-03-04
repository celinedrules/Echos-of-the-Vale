using System;
using Core.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Utilities.Enums;

namespace Core
{
    public class EntityHealth : MonoBehaviour, IDamageable
    {
        [Serializable]
        public struct KnockbackSettings
        {
            public float power;
            public float duration;
        }
        
        public event Action OnTakingDamage;
        public event Action OnHealthUpdate;
        
        // [SerializeField] private GameObject healthBar;
        // [SerializeField] private float healthBarFadeTime = 1.5f;
        //
        // [field: SerializeField] public EntityFx DamageEffect { get; set; }

        [Header("Knockback")]
        [SerializeField] protected KnockbackSettings normalDamageKnockback;
        [SerializeField] protected KnockbackSettings heavyDamageKnockback;
        
        [Header("Heavy Damage")]
        [SerializeField] private float heavyDamageThreshold = 0.3f;
        
        protected Entity entity;
        // private EntityDropManager _dropManager;
        // private Slider _healthBar;
        // private Timer _healthBarTimer;
        // private float _regenTimer;
        // private Coroutine _regenRoutine;
        
        [field: SerializeField, Header("Health")]
        public int CurrentHealth { get; set; }
        
        [SerializeField] private float regenInterval = 1.0f;
        [SerializeField] private bool canRegenerateHealth = true;
        [SerializeField] private bool alwaysRegenerate;
        
        // private EntityStats _entityStats;
        // private EntityStats EntityStats => _entityStats ??= _entity?.Stats;
        
        public bool CanTakeDamage { get; set; } = true;
        public bool IsDead { get; private set; }
        public float LastDamageTakenTime { get; private set; }
        // public bool MiniHealthBarActive
        // {
        //     get => _healthBar.transform.parent.gameObject.activeSelf;
        //     set => _healthBar?.transform.parent.gameObject.SetActive(value);
        // }

        private void Start()
        {
            entity = GetComponent<Entity>();
            // _dropManager = GetComponent<EntityDropManager>();
            // _entityStats = _entity?.Stats;
            // _healthBar = GetComponentInChildren<Slider>();
        }

        public void Initialize()
        {
            //_entityStats = _entity?.Stats;
            SetupHealth();
        }

        private void SetupHealth()
        {
            // if (_entityStats == null)
            //     return;
            //
            CurrentHealth = 100;
            // CurrentHealth = _entityStats.GetMaxHealth();
            // OnHealthUpdate += UpdateHealthBar;
            // //OnHealthUpdate += DisplayHealthBar;
            // //UpdateHealthBar();
            // OnHealthUpdate?.Invoke();
            // healthBar.SetActive(false);
            //
            // if (alwaysRegenerate)
            //     StartRegen(_entityStats.ResourceStats.HealthRegen.Value, true);
            //
            // if (_entityStats is PlayerStats playerStats)
            //     playerStats.LoadFromRuntimeData();
        }
        
        public virtual bool TakeDamage(int amount, int elementalDamage, ElementType elementType, Entity attacker)
        {
            if(IsDead || !CanTakeDamage)
                return false;

            if (AttackEvaded())
            {
                Debug.Log($"{gameObject.name} evaded the attack!");
                return false;
            }
            
            // float armorReduction = attacker?.Stats?.GetArmorReduction() ?? 0;
            //
            // float mitigation = _entityStats?.GetArmorMitigation(armorReduction) ?? 0;
            int physicalDamageTaken = 10;
            // int physicalDamageTaken = Mathf.RoundToInt(amount * (1 - mitigation));
            //
            // float resistance = _entityStats?.GetElementalResistance(elementType) ?? 0;
            int elementalDamageTaken = 0;
            // int elementalDamageTaken = Mathf.RoundToInt(elementalDamage * (1 - resistance));
            
            HandleDamageKnockback(attacker, physicalDamageTaken);
            ReduceHealth(physicalDamageTaken + elementalDamageTaken);

            LastDamageTakenTime = physicalDamageTaken + elementalDamageTaken;
            
            OnTakingDamage?.Invoke();
            
            return true;
        }
        
        private bool AttackEvaded()
        {
            // if (_entityStats == null)
            //     return false;
            //
            // return Random.Range(0, 100) < _entityStats.GetEvasion();

            return false;
        }
        
        // public void IncreaseHealth(int amount)
        // {
        //     if (IsDead)
        //         return;
        //
        //     CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, _entityStats.GetMaxHealth());
        //     OnHealthUpdate?.Invoke();
        // }
        
        public void ReduceHealth(int amount)
        {
            //DamageEffect?.Flash();

            CurrentHealth -= amount;
            OnHealthUpdate?.Invoke();

            if (CurrentHealth <= 0)
                Die();
        }
        
        protected virtual void Die()
        {
            IsDead = true;
            entity?.EntityDeath();
            //_dropManager?.DropItems();
        }
        
        // public float GetHealthPercentage()
        // {
        //     if (EntityStats == null)
        //         return 1;
        //     
        //     return CurrentHealth / (float)EntityStats.GetMaxHealth();
        // }

        // public void SetHealthToPercentage(float percentage)
        // {
        //     if(EntityStats == null)
        //         return;
        //     
        //     CurrentHealth = Mathf.RoundToInt(Mathf.Clamp01(percentage) * EntityStats.GetMaxHealth());
        //     //UpdateHealthBar();
        //     OnHealthUpdate?.Invoke();
        // }
        
        public void NotifyHealthChanged() => OnHealthUpdate?.Invoke();

        protected virtual void HandleDamageKnockback(Entity attacker, int finalDamage)
        {
           
        }
        
        // private bool IsHeavyDamage(int damage)
        // {
        //     if(_entityStats == null)
        //         return false;
        //     
        //     return (float)damage / _entityStats.GetMaxHealth() > heavyDamageThreshold;
        // }
    }
}