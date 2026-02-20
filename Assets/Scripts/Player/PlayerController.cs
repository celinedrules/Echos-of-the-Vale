using System;
using Core;
using StateMachine.States.PlayerStates;
using UnityEngine;

namespace Player
{
    public class PlayerController : Entity
    {
        private PlayerStateFactory _factory;
        
        public PlayerInputHandler Input { get; private set; }
        
        public IdleState IdleState { get; private set; }
        public MoveState MoveState { get; private set; }

        [field: SerializeField, Header("Movement Settings")]
        public float MoveSpeed { get; set; } = 5.0f;

        protected override void Awake()
        {
            base.Awake();
            
            Input = GetComponent<PlayerInputHandler>();
            
            _factory = new PlayerStateFactory(this, StateMachine);
            
            IdleState = _factory.Create<IdleState>("Idle");
            MoveState = _factory.Create<MoveState>("Move");
        }

        protected void Start()
        {
            base.Start();
            
            StateMachine.Initialize(IdleState);
        }
    }
}