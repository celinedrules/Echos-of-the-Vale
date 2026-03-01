using Core;
using StateMachine.States.EnemyStates;
using UnityEngine;

namespace Enemy
{
    public class EnemyController : Entity
    {
        protected EnemyStateFactory Factory;
        
        [field: SerializeField, Header("Player Detection")]
        public LayerMask PlayerLayer { get; set; }
        [field: SerializeField] public Transform PlayerCheck { get; set; }

        private Transform _player;

        public float IdleTime => 1.0f; //Data.IdleTime;
        public float MoveSpeed => 1.0f; //Data.MoveSpeed;
        
        public float ActiveSlowMultiplier { get; private set; } = 1.0f;

        // public Transform PlayerTransform
        // {
        //     get
        //     {
        //         if (!_player)
        //             _player = PlayerDetected().Transform;
        //         
        //         return _player;
        //     }
        //     set => _player = value;
        // }
        
        public IdleState IdleState { get; protected set; }
        public MoveState MoveState { get; protected set; }
        

        protected override void Awake()
        {
            base.Awake();
            
            Factory = new EnemyStateFactory(this, StateMachine);
        }

        protected override void Start()
        {
            base.Start();
            StateMachine.Initialize(IdleState);
        }

        public float GetMoveSpeed() => MoveSpeed * ActiveSlowMultiplier;
    }
}