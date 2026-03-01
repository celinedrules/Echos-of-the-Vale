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

        public float IdleTime => 2.0f; //Data.IdleTime;
        public float MoveTime => 2.0f; //Data.MoveTime;
        public float MoveSpeed => 1.0f; //Data.MoveSpeed;
        [field: SerializeField] public float RandomMoveRadius { get; set; } = 5.0f;

        public Vector2 OriginalPosition { get; private set; }
        
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
            OriginalPosition = Rigidbody.position;
            StateMachine.Initialize(IdleState);
        }

        public Vector2 GetRandomDirection()
        {
            int x, y;
            do
            {
                x = Random.Range(-1, 2);
                y = Random.Range(-1, 2);
            } while (x == 0 && y == 0);

            return new Vector2(x, y);
        }
        
        public float GetMoveSpeed() => MoveSpeed * ActiveSlowMultiplier;
        
        protected override void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector2 center = Application.isPlaying ? OriginalPosition : (Vector2)transform.position;
            Gizmos.DrawWireSphere(center, RandomMoveRadius);
        }
    }
}