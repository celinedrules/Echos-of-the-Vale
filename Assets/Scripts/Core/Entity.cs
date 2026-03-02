using System;
using System.Collections;
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
        protected StateMachine.StateMachine StateMachine;
        
        private bool _isKnockedBack;
        private Coroutine _knockbackRoutine;
        
        public Animator Animator  { get; private set; }
        public Rigidbody2D Rigidbody { get; private set; }
        public Direction FacingDirection { get; private set; } = Direction.Down;
        
        public EntityHealth Health { get; protected set; }

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
            Rigidbody.linearVelocity = knockbackPower;

            yield return new WaitForSeconds(knockbackTime);

            Rigidbody.linearVelocity = Vector2.zero;
            _isKnockedBack = false;
        }
        
        public virtual void Stun(bool knockback)
        {
        }
        
        public virtual void EntityDeath()
        {
        }

        protected virtual void OnDrawGizmos()
        {
            
        }
    }
}