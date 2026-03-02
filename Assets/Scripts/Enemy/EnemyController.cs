using Control;
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
        [field: SerializeField] public AISensor Sensor { get; set; }

        private Transform _player;

        public float BattleMoveSpeed => 2.0f; //Data.BattleMoveSpeed;
        public float AttackDistance => 1.5f; //Data.AttackDistance;
        public float BattleTimeDuration => 5.0f; //Data.BattleTimeDuration;
        public float MinRetreatDistance => 5.0f; //Data.MinRetreatDistance;
        public Vector2 RetreatVelocity => Vector2.zero; //Data.RetreatVelocity;
        public Vector2 StunnedVelocity => new Vector2(7, 7); //Data.StunnedVelocity;
        public bool CanBeStunned { get; set; } = false; //Data.CanBeStunned;
        public int StartStunnedFrame => 20; //Data.StartStunnedFrame;
        public float StunnedFrameDuration => 0.25f; //Data.StunnedFrameDuration;
        public float IdleTime => 2.0f; //Data.IdleTime;
        public float MoveTime => 2.0f; //Data.MoveTime;
        public float MoveSpeed => 1.0f; //Data.MoveSpeed;
        public float RandomMoveRadius => 5.0f; //Data.RandomMoveRadius;

        public Vector2 OriginalPosition { get; private set; }
        
        public float ActiveSlowMultiplier { get; private set; } = 1.0f;

        public Transform PlayerTransform
        {
            get
            {
                if (!_player)
                    _player = Sensor.GetPlayerTransform();
                
                return _player;
            }
            set => _player = value;
        }
        
        public IdleState IdleState { get; protected set; }
        public MoveState MoveState { get; protected set; }
        public AttackState AttackState { get; protected set; }
        public BattleState BattleState { get; protected set; }
        public RetreatState RetreatState { get; protected set; }
        public StunnedState StunnedState { get; protected set; }
        public DeathState DeathState { get; protected set; }

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
            CanBeStunned = true; //Data.CanBeStunned;
        }

        protected override void Update()
        {
            base.Update();
            Sensor.SetFacingDirection(FacingDirection);
        }
        
        public bool PlayerDetected => Sensor.IsPlayerInRange();
        
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

        public void MoveTowardPlayer()
        {
            if(!PlayerTransform)
                return;
            
            Vector2 direction = ((Vector2)PlayerTransform.position - Rigidbody.position).normalized;
            SetVelocity(direction * BattleMoveSpeed);
        }
        
        public float GetMoveSpeed() => MoveSpeed * ActiveSlowMultiplier;
        
        protected override void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Vector2 center = Application.isPlaying ? OriginalPosition : transform.position;
            Gizmos.DrawWireSphere(center, RandomMoveRadius);

            Gizmos.color = Color.blue;
            Vector3 pos = transform.position;
            Vector2 moveDir = Vector2.zero;

            if (!Application.isPlaying || Rigidbody.linearVelocity.normalized == Vector2.zero)
            {
                moveDir = FacingDirection switch
                {
                    Direction.Up => Vector2.up,
                    Direction.Down => Vector2.down,
                    Direction.Left => Vector2.left,
                    Direction.Right => Vector2.right,
                    _ => Vector2.down
                };
            }
            else
            {
                moveDir = Rigidbody.linearVelocity.normalized;
            }

            if (moveDir.sqrMagnitude > 0.01f)
                Gizmos.DrawLine(pos, pos + (Vector3)(moveDir * AttackDistance));
            
        }
    }
}