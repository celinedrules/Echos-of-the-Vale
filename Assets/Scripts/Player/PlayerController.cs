using System;
using System.Collections;
using Core;
using StateMachine.States.PlayerStates;
using UnityEngine;

namespace Player
{
    public class PlayerController : Entity
    {
        private PlayerStateFactory _factory;
        private Coroutine _queuedAttackCo;
        
        public PlayerInputHandler Input { get; private set; }
        
        public IdleState IdleState { get; private set; }
        public MoveState MoveState { get; private set; }
        public BasicAttackState BasicAttackState { get; private set; }

        [field: SerializeField, Header("Movement Settings")]
        public float MoveSpeed { get; set; } = 5.0f;
        
        [field: SerializeField, Header("Attack Settings")]
        public Vector2[] AttackVelocity { get; set; }
        [field: SerializeField] public float AttackVelocityDuration { get; set; } = 0.1f;
        [field: SerializeField] public float ComboResetDuration { get; set; } = 1f;

        protected override void Awake()
        {
            base.Awake();
            
            Input = GetComponent<PlayerInputHandler>();
            
            _factory = new PlayerStateFactory(this, StateMachine);
            
            IdleState = _factory.Create<IdleState>("Idle");
            MoveState = _factory.Create<MoveState>("Move");
            BasicAttackState = _factory.Create<BasicAttackState>("BasicAttack");
        }

        protected void Start()
        {
            base.Start();
            
            StateMachine.Initialize(IdleState);
        }
        
        public void SetVelocity(float velocityX, float velocityY)
        {
            // if (_isKnockedBack)
            //     return;

            Rigidbody.linearVelocity = new Vector2(velocityX, velocityY);
            //HandleFlip(velocityX);
        }
        
        public void EnterAttackStateWithDelay()
        {
            if (_queuedAttackCo != null)
                StopCoroutine(_queuedAttackCo);

            _queuedAttackCo = StartCoroutine(EnterAttackStateWithDelayCo());
        }

        private IEnumerator EnterAttackStateWithDelayCo()
        {
            yield return new WaitForEndOfFrame();
            StateMachine.ChangeState(BasicAttackState);
        }
    }
}