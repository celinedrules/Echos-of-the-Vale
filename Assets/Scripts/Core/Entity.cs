using System;
using System.Collections;
using Data.EntityData;
using Sirenix.OdinInspector;
using StateMachine.States;
using StateMachine.States.PlayerStates;
using Stats;
using UnityEngine;

namespace Core
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    
    public class Entity : MonoBehaviour
    {
        public event Action<int> OnDoingPhysicalDamage;
        
        [SerializeField, PropertyOrder(-10)]
        [ValidateInput(nameof(ValidateEntityData), "Wrong EntityData type for this ENTITY!")]
        [LabelText("$DataLabel")]
        protected EntityData entityData;
        
        protected StateMachine.StateMachine StateMachine;
        
        [ReadOnly, ShowInInspector, PropertyOrder(-1)]
        public string EntityName => entityData ? entityData.EntityName : string.Empty;
        
        private bool _isKnockedBack;
        private Coroutine _knockbackRoutine;
        
        public bool IsKnockedBack => _isKnockedBack;
        public EntityStats Stats { get; protected set; }
        public Animator Animator  { get; private set; }
        public Rigidbody2D Rigidbody { get; private set; }
        public Direction FacingDirection { get; private set; } = Direction.Down;
        
        public EntityHealth Health { get; protected set; }
        
        protected virtual Type RequiredDataType => typeof(NpcData);

        protected virtual void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
            Rigidbody = GetComponent<Rigidbody2D>();
            
            StateMachine = new StateMachine.StateMachine();
        }

        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
            StateMachine.UpdateActiveState();
        }
        
        protected virtual void InitializeStatsFromData()
        {
            if (entityData?.StatsData == null || Stats == null)
                return;

            // Copy values from the ScriptableObject data to the runtime stats
            Stats.ResourceStats.MaxHealth.Value = entityData.StatsData.MaxHealth;
            Stats.ResourceStats.HealthRegen.Value = entityData.StatsData.HealthRegen;
            Stats.OffenseStats.AttackSpeed.Value = entityData.StatsData.AttackSpeed;
            Stats.OffenseStats.Damage.Value = entityData.StatsData.Damage;
            Stats.DefenseStats.Armor.Value = entityData.StatsData.Armor;
        }
        
        public void SetVelocity(Vector2 velocity)
        {
            Rigidbody.linearVelocity = velocity;
            
            if (velocity == Vector2.zero)
                return;
            
            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
                FacingDirection = velocity.x > 0 ? Direction.Right : Direction.Left;
            else
                FacingDirection = velocity.y > 0 ? Direction.Up : Direction.Down;
        }
        
        public void Knockback(Vector2 knockbackPower, float knockbackTime)
        {
            if (_knockbackRoutine != null)
                StopCoroutine(_knockbackRoutine);

            _knockbackRoutine = StartCoroutine(KnockbackRoutine(knockbackPower, knockbackTime));
        }
        
        private IEnumerator KnockbackRoutine(Vector2 knockbackPower, float knockbackTime)
        {
            _isKnockedBack = true;
            Health.CanTakeDamage = false;
            Rigidbody.linearVelocity = knockbackPower;

            yield return new WaitForSeconds(knockbackTime);

            Rigidbody.linearVelocity = Vector2.zero;
            Health.CanTakeDamage = true;
            _isKnockedBack = false;
        }
        
        public virtual void Stun(bool knockback)
        {
        }
        
        public virtual void EntityDeath()
        {
        }

        private string DataLabel => RequiredDataType.Name.Replace("Data", "") is { Length: > 0 } label
            ? $"{label} Data"
            : "Entity Data";
        
        private bool ValidateEntityData(EntityData value, ref string errorMessage)
        {
            if (!value)
            {
                errorMessage = "EntityData cannot be null";
                return false;
            }

            if (RequiredDataType.IsInstanceOfType(value))
                return true;
            
            errorMessage = $"Expected {RequiredDataType.Name} but got {value.GetType().Name}";
            return false;

        }
        
        protected virtual void OnDrawGizmos()
        {
            
        }
    }
}